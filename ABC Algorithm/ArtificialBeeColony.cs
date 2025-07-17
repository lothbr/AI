using ABC_Algorithm.BenchMarks;
using ABC_Algorithm.Objectives;

namespace ABC_Algorithm
{
    public  class ArtificialBeeColony
    {

        static int FoodSources = 10;        // Number of food sources (employed bees)
        static int D = 30;                 // Default dimension of problem (the number of optimization parameters)
        static int Limit = 100;            // Limit for abandonment
        static int MaxCycles = 500;        // Max iterations
        static double LowerBound = -5.12;  // Lower bound for solution
        static double UpperBound = 5.12;   // Upper bound for solution

        static BenchmarkType SelectedBenchmark = BenchmarkType.Weierstrass;

        static double[][] NewFoodSources = new double[FoodSources][]; // Array to hold food sources (solutions)
        static double[] Fitness = new double[FoodSources];
        static int[] Trial = new int[FoodSources];
        static Random rand = new Random();
        private readonly static ObjectiveFunction objectiveFunction = new(D, LowerBound, UpperBound, SelectedBenchmark);
        private readonly static InitializeABC initializeABC = new(objectiveFunction);
        public void Start()
        {
            try
            {
                #region Producing initial food source sites

                initializeABC.Initialize(FoodSources,D, ref NewFoodSources, ref Fitness, ref Trial,LowerBound, UpperBound);
                #endregion

                // Maximum number of cycles
                for (int cycle = 0; cycle < MaxCycles; cycle++)
                {
                    EmployedBeePhase();
                    OnlookerBeePhase();
                    ScoutBeePhase();
                    // Print best solution
                    int best = GetBestFoodIndex();
                    FileLogger.logInfo($"Cycle {cycle + 1}, Best fitness = {Fitness[best]:F5}");
                }
                int finalBest = GetBestFoodIndex();
                FileLogger.logInfo("Best solution found:");
                FileLogger.logInfo($"x = {string.Join(", ", NewFoodSources[finalBest])}, fitness = {Fitness[finalBest]:F5}");
            }
            catch (Exception ex)
            {

                FileLogger.LogError("Oti laana lori ", ex.StackTrace!);
            }
           
        }


        // Sending employed bees to the food source sites
        static void EmployedBeePhase()
        {
            for (int i = 0; i < FoodSources; i++)
            {
                double[] newSolution = GenerateNeighbor(NewFoodSources[i]); // each employed is aasociated to a food source to produce a solution
                double newFitness = objectiveFunction.CalculateObjective(newSolution);
                if (newFitness < Fitness[i]) // if new solution is better than current the fitness get updated.
                {
                    NewFoodSources[i] = newSolution;
                    Fitness[i] = newFitness;
                    Trial[i] = 0;
                }
                else
                {
                    Trial[i]++;
                }
            }
        }

        static void OnlookerBeePhase()
        {
            double maxFit = Fitness.Max();
            for (int t = 0; t < FoodSources; t++)
            {
                double r = rand.NextDouble();
                for (int i = 0; i < FoodSources; i++)
                {
                    double probability = Fitness[i] / Fitness.Sum(); //0.9 * (maxFit - Fitness[i]) / (maxFit) + 0.1;
                    if (r < probability)
                    {
                        double[] newSolution = GenerateNeighbor(NewFoodSources[i]);
                        double newFitness = objectiveFunction.CalculateObjective(newSolution);
                        if (newFitness < Fitness[i])
                        {
                            NewFoodSources[i] = newSolution;
                            Fitness[i] = newFitness;
                            Trial[i] = 0;
                        }
                        else
                        {
                            Trial[i]++;
                        }
                    }
                }
            }
        }

        static void ScoutBeePhase()
        {
            for (int i = 0; i < FoodSources; i++)
            {
                if (Trial[i] >= Limit)
                {
                    for (int j = 0; j < D; j++)
                        NewFoodSources[i][j] = LowerBound + rand.NextDouble() * (UpperBound - LowerBound);
                    Fitness[i] = objectiveFunction.CalculateObjective(NewFoodSources[i]);
                    Trial[i] = 0;
                }
            }
        }

        static double[] GenerateNeighbor(double[] current)
        {
            double[] newSolution = (double[])current.Clone();
            int j = rand.Next(D);
            int k;
            //The loop ensures that the selected index `k` is not the same as the current food source.
            do { k = rand.Next(FoodSources); } while (NewFoodSources[k] == current);
            double phi = (rand.NextDouble() * 2) - 1; // [-1,1]
            newSolution[j] = current[j] + phi * (current[j] - NewFoodSources[k][j]);
            newSolution[j] = Math.Max(LowerBound, Math.Min(UpperBound, newSolution[j]));
            return newSolution;
        }

        

        static int GetBestFoodIndex()
        {
            double minFit = Fitness.Min();
            return Array.IndexOf(Fitness, minFit);
        }
    }
}
