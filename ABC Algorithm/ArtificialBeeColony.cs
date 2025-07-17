using ABC_Algorithm.BenchMarks;
using ABC_Algorithm.Objectives;
using OfficeOpenXml;

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
            //for (int i = 0; i < FoodSources; i++) NewFoodSources[i] = new double[D]; // Initialize each food source with an array of size D
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
            {   
                FileLogger.logInfo($"Parameters: FoodSources={FoodSources}, Dimensions={D}, Limit={Limit}, MaxCycles={MaxCycles}, LowerBound={LowerBound}, UpperBound={UpperBound}, Tolerance={Tolerance}, Benchmark={SelectedBenchmark}");
               
                //string excelPath = _configuration.GetConnectionString("ExcelPath") + $"{SelectedBenchmark.ToString()} {D}.xlsx";
                string excelPath = $"{SelectedBenchmark.ToString()} {D}.xlsx";
                FileInfo file = new (excelPath);
                if (file.Exists) file.Delete();
                using var package = new ExcelPackage(file);
                var worksheet = package.Workbook.Worksheets.Add($"{SelectedBenchmark.ToString()} {D}");

                // Headers
                worksheet.Cells[1, 1].Value = "Cycle";
                worksheet.Cells[1, 2].Value = "Benchmark";
                worksheet.Cells[1, 3].Value = "Best Fitness";
                worksheet.Cells[1, 4].Value = "Best Objective";
                int row = 2;
                // Maximum number of cycles
                for (int cycle = 0; cycle < MaxCycles; cycle++)
                {
                    EmployedBeePhase();
                    OnlookerBeePhase();
                    ScoutBeePhase();
                    // Print best solution
                    int best = GetBestFoodIndex();
                    //double bestObjective = 1.0 / Fitness[best] - 1; // Reverse fitness to get objective
                    double error = Math.Abs(best);

                    worksheet.Cells[row, 1].Value = cycle + 1;
                    worksheet.Cells[row, 2].Value = SelectedBenchmark.ToString();
                    worksheet.Cells[row, 3].Value = Fitness[best];
                    worksheet.Cells[row, 4].Value = best;
                    row++;
                    if (error <= Tolerance)
                    {
                        FileLogger.logInfo($"Tolerance met at cycle {cycle + 1}. Best objective = {best:F15}");
                        break;
                    }
                    FileLogger.logInfo($"{SelectedBenchmark.ToString()} Cycle {cycle + 1}, Best fitness = {Fitness[best]:F5}, Objective = {best:F15}");

                }
                package.Save(); // Save Excel file
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
        public void EmployedBeePhase() 
        {
            for (int i = 0; i < FoodSources; i++)
            {
                double[] newSolution = GenerateNeighbor(NewFoodSources[i]); // each employed is aasociated to a food source to produce a solution
                double newFitness = objectiveFunction.EvaluateObjectiveCost(newSolution);
                if (newFitness > Fitness[i]) // if new solution is better than current the fitness get updated.
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

        void OnlookerBeePhase()
        {
            double sumFitness = Fitness.Sum();
            double[] probability = [..Fitness.Select(f => f / sumFitness)];
            for (int t = 0; t < FoodSources; t++)
            {
                double r = rand.NextDouble();
                for (int i = 0; i < FoodSources; i++)
                {
                    if (r < probability[i])
                    {
                        double[] newSolution = GenerateNeighbor(NewFoodSources[i]);
                        double newObjective = objectiveFunction.EvaluateObjectiveCost(newSolution);
                        double newFitness = 1.0 / (1.0 + newObjective);
                        if (newFitness > Fitness[i])
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

        void ScoutBeePhase()
        {
            for (int i = 0; i < FoodSources; i++)
            {
                if (Trial[i] >= Limit)
                {
                    for (int j = 0; j < D; j++)
                        NewFoodSources[i][j] = LowerBound + rand.NextDouble() * (UpperBound - LowerBound);
                    Fitness[i] = objectiveFunction.EvaluateObjectiveCost(NewFoodSources[i]);
                    Trial[i] = 0;
                }
            }
        }

        double[] GenerateNeighbor(double[] current)
        {
            double[] newSolution = (double[])current.Clone();
            int j = rand.Next(D);
            int k;
            //The loop ensures that the selected index `k` is not the same as the current food source.
            do { k = rand.Next(FoodSources); } while (k == Array.IndexOf(NewFoodSources, current));
            double phi = (rand.NextDouble() * 2) - 1; // [-1,1]
            newSolution[j] = current[j] + phi * (current[j] - NewFoodSources[k][j]);
            newSolution[j] = Math.Max(LowerBound, Math.Min(UpperBound, newSolution[j]));
            return newSolution;
        }

        

        int GetBestFoodIndex()
        {
            double minFit = Fitness.Min();
            return Array.IndexOf(Fitness, minFit);
        }
       
    }
}
