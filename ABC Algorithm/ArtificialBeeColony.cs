using ABC_Algorithm.BenchMarks;
using ABC_Algorithm.Objectives;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Numerics;

namespace ABC_Algorithm
{
    public  class ArtificialBeeColony
    {

        private readonly int FoodSources;        // Number of food sources (employed bees)
        private readonly int D;                 // Default dimension of problem (the number of optimization parameters)
        private readonly int Limit;            // Limit for abandonment
        private readonly int MaxCycles;        // Max iterations
        private const double LowerBound = -100;  // Lower bound for solution
        private const double UpperBound = 100;   // Upper bound for solution
        private const double Tolerance = 1e-8; // Tolerance error rate (1 x 10^-8)

        private readonly BenchmarkType SelectedBenchmark;

        private readonly double[][] NewFoodSources; // Array to hold food sources (solutions)
        private readonly double[] Fitness;
        private readonly int[] Trial;
        private readonly Random rand = new();
        private readonly ObjectiveFunction objectiveFunction;
        private readonly InitializeABC initializeABC;
        private readonly IConfiguration _configuration;
        public ArtificialBeeColony(int foodSources, int dimensions, int limit, int maxCycles, BenchmarkType benchmarkType, IConfiguration configuration)
        {
            _configuration = configuration; 
            FoodSources = foodSources;
            D = dimensions;
            Limit = limit;
            MaxCycles = maxCycles;
            SelectedBenchmark = benchmarkType;
            // Initialize arrays
            NewFoodSources = new double[FoodSources][];
            Fitness = new double[FoodSources];
            Trial = new int[FoodSources];
            for (int i = 0; i < FoodSources; i++) NewFoodSources[i] = new double[D]; // Initialize each food source with an array of size D
            objectiveFunction = new(D, LowerBound, UpperBound, SelectedBenchmark);
            // Initialize each food source with an array of size D
            initializeABC = new(objectiveFunction);
            // Allocate food sources
            initializeABC.Initialize(FoodSources, D, ref NewFoodSources, ref Fitness, ref Trial, LowerBound, UpperBound);
            ExcelPackage.License.SetNonCommercialPersonal("Abraham Olaoluwa");


        }
        public void Start()
        {
            try
            {   //initialize the array of 
                List<double> BestObjectives = new List<double>();

                FileLogger.logInfo($"Parameters: FoodSources={FoodSources}, Dimensions={D}, Limit={Limit}, MaxCycles={MaxCycles}, LowerBound={LowerBound}, UpperBound={UpperBound}, Tolerance={Tolerance}, Benchmark={SelectedBenchmark}");
               
                //string excelPath = _configuration.GetConnectionString("ExcelPath") + $"{SelectedBenchmark.ToString()} {D}.xlsx";
                //string excelPath = $"{SelectedBenchmark.ToString()} {D}.xlsx";
                //FileInfo file = new (excelPath);
                //if (file.Exists) file.Delete();
                //using var package = new ExcelPackage(file);
                //var worksheet = package.Workbook.Worksheets.Add($"{SelectedBenchmark.ToString()} {D}");

                //// Headers
                //worksheet.Cells[1, 1].Value = "Colony Size";
                //worksheet.Cells[1, 2].Value = "Dimension";
                //worksheet.Cells[1, 3].Value = "Benchmark";
                //worksheet.Cells[1, 4].Value = "Best Fitness";
                //worksheet.Cells[1, 5].Value = "Optimized Solutions";
                //worksheet.Cells[1, 6].Value = "Mean";
                //worksheet.Cells[1, 7].Value = "Standard Deviation";
                //int row = 2;
                // Maximum number of cycles
                for (int cycle = 0; cycle < MaxCycles; cycle++)
                {
                    // Employed bees search for food sources
                    EmployedBeePhase();

                    // Each employed bee shares its information with the onlooker bees
                    //After all employed bees complete their searches, they share their information related to the nectar amounts
                    //and the positions of their sources with the onlooker bees on the dance area
                    // Onlooker bees select food sources based on fitness
                    OnlookerBeePhase();
                    // Scout bees search for new food sources
                    ScoutBeePhase();
                    // Print best solution

                    int best = GetBestFoodIndex();

                    BestObjectives.Add(Fitness[best]);


                    if (Fitness[best] <= Tolerance)
                    {
                        FileLogger.logInfo($"Tolerance met at cycle {cycle + 1}. Best objective = {Fitness[best]:F15}");
                        break;
                    }
                    FileLogger.logInfo($"{SelectedBenchmark.ToString()} Cycle {cycle + 1}, Best fitness (Objective Function) = {Fitness[best]:F15}, Optimized Solutions = {JsonConvert.SerializeObject(NewFoodSources[best].ToList())}");

                }

                //double mean = BestObjectives.Average();
                //double stdDev = Math.Sqrt(BestObjectives.Select(x => Math.Pow(x - mean, 2)).Average());

                double finalBest = GetClosestToZero([..BestObjectives]);
                //var index = Array.IndexOf(Fitness, finalBest);
                FileLogger.logInfo($"Best solution found: fitness = {finalBest:F15}");
                //FileLogger.logInfo($"x = {string.Join(", ", NewFoodSources[index])}, fitness = {finalBest:F5}");

                //worksheet.Cells[row, 1].Value = FoodSources;
                //worksheet.Cells[row, 2].Value = D;
                //worksheet.Cells[row, 3].Value = SelectedBenchmark.ToString();
                //worksheet.Cells[row, 4].Value = $"{Fitness[finalBest]:F5}";
                //worksheet.Cells[row, 5].Value = bstObj;
                //worksheet.Cells[row, 6].Value = mean;
                //worksheet.Cells[row, 7].Value = stdDev;

                //package.Save(); // Save Excel file
            }
            catch (Exception ex)
            {

                FileLogger.LogError("Oti laana lori ", ex.StackTrace!);
            }
           
        }


        // Sending employed bees to the food source sites (the number of food source sites is equal to the number of employed bees.)
        public void EmployedBeePhase() 
        {
            for (int i = 0; i < FoodSources; i++)
            {
                // Each employed is aasociated to a food source to produce a solution
                //An employed bee produces a modification on the position of the food source and finds a neighboring food source (vi)
                double[] newSolution = GenerateNeighbor(NewFoodSources[i]); 

                // Evaluate the new solution quality (fi, is the cost value of the solution) using the Objective Function
                // which include the boundaries previously set while initializing the objective function.
                double fi = objectiveFunction.EvaluateObjectiveCost(newSolution);

                // Calculate the fitness of the new solution
                double newFitness = (fi >= 0) ?  1.0 / (1.0 + fi) : (1 + Math.Abs(fi));

                // A greedy selection process begins
                // If the new solution is better than the current solution, update the food source and reset the trial counter.
                if (newFitness > Fitness[i])
                {
                    NewFoodSources[i] = newSolution; // the new solution found by the employed bee
                    Fitness[i] = newFitness;  // the fitness values representing the nectar amount of the food sources
                    Trial[i] = 0;
                }
                else
                {
                    Trial[i]++;
                }
            }
        }

        void OnlookerBeePhase()
        {
            //An onlooker bee evaluates the nectar information taken from all employed bees and chooses a food source site with a probability related to its nectar amount
            //The probability of selecting a food source is calculated as the ratio of the nectar amount of the food source to the total nectar amount of all food sources.
            // The probability is defined as follows: Pi = fi / Σfi, where i = 1, 2, . . . SN (number of food sources) and SN is the number of employed bees.
            double sumFitness = Fitness.Sum();
            double[] probability = [..Fitness.Select(f => f / sumFitness)];

            for (int t = 0; t < FoodSources; t++)
            {
                //a random real number within the range [0,1] is generated for each source
                //r is a uniformly distributed random number in the range [0, 1]. 
                double r = rand.NextDouble();

                for (int i = 0; i < FoodSources; i++)
                {
                    if (r > probability[i]) // if the random number is greater than the probability of the food source
                    {
                        // The onlooker bee produces a new solution by perturbing the current food source
                        // and evaluates the new solution.
                        // If the new solution is better than the current one, it updates the food source.
                        double[] newSolution = GenerateNeighbor(NewFoodSources[i]);

                        // Evaluate the new solution quality (fi, is the cost value of the solution) using the Objective Function
                        // which include the boundaries previously set while initializing the objective function.
                        double fi = objectiveFunction.EvaluateObjectiveCost(newSolution);

                        // Calculate the fitness of the new solution
                        double newFitness = (fi >= 0) ? 1.0 / (1.0 + fi) : (1 + Math.Abs(fi));

                        // A greedy selection process begins
                        // If the new solution is better than the current solution, update the food source and reset the trial counter.
                        if (newFitness > Fitness[i])
                        {
                            NewFoodSources[i] = newSolution; // the new solution found by the onlooker bee
                            Fitness[i] = newFitness; // the fitness values representing the nectar amount of the food sources
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

        void ScoutBeePhase()
        {
            for (int i = 0; i < FoodSources; i++)
            {
                //If the value of the counter is greater than the control parameter of the ABC algorithm, known as the ‘‘limit”
                //the food source is abandoned and a new food source is generated randomly.
                if (Trial[i] >= Limit)
                {
                    for (int j = 0; j < D; j++)
                        NewFoodSources[i][j] = LowerBound + rand.NextDouble() * (UpperBound - LowerBound); // Generate new random solution

                    // Evaluate the new solution quality (fi, is the cost value of the solution) using the Objective Function
                    // which include the boundaries previously set while initializing the objective function.
                    double fi = objectiveFunction.EvaluateObjectiveCost(NewFoodSources[i]);
                    Fitness[i]= (fi >= 0) ? 1.0 / (1.0 + fi) : (1 + Math.Abs(fi));
                    Trial[i] = 0;
                }
            }
        }

        double[] GenerateNeighbor(double[] current)
        {
            double[] newSolution = (double[])current.Clone();
            //j is a random integer in the range [1,D]
            int j = rand.Next(D);
            //k 2 { 1, 2, . . .SN} is a randomly chosen index that has to be different from current food source (i).
            int k;
            //The loop ensures that the selected index `k` is not the same as the current food source.
            do { k = rand.Next(FoodSources); } while (NewFoodSources[k] == current);

            //phi (0ij) is a uniformly distributed real random number in the range [-1, 1].
            double phi = (rand.NextDouble() * 2) - 1; 

            // Generate new solution by perturbing the current solution
            newSolution[j] = current[j] + phi * (current[j] - NewFoodSources[k][j]);

            // Ensure the new solution is within bounds
            newSolution[j] = Math.Max(LowerBound, Math.Min(UpperBound, newSolution[j]));
            return newSolution;
        }

        

        int GetBestFoodIndex()
        {
            double minFit = Fitness.Min();
            return Array.IndexOf(Fitness, minFit);
        }

        //method that selects the value closest to zero from a given array of fitness (objective function)
        static double GetClosestToZero(double[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("Input array must not be null or empty.");

            double closest = values[0];

            foreach (double value in values)
            {
                if (Math.Abs(value) < Math.Abs(closest) ||
                   (Math.Abs(value) == Math.Abs(closest) && value > closest)) // Prefer positive if tie
                {
                    closest = value;
                }
            }

            return closest;
        }

    }
}
