using ABC_Algorithm.BenchMarks;

namespace ABC_Algorithm
{
    public  class ArtificialBeeColony
    {

        static int FoodNumber = 10;        // Number of food sources (employed bees)
        static int D = 30;                 // Default dimension of problem
        static int Limit = 100;            // Limit for abandonment
        static int MaxCycles = 500;        // Max iterations
        static double LowerBound = -5.12;  // Lower bound for solution
        static double UpperBound = 5.12;   // Upper bound for solution

        static BenchmarkType SelectedBenchmark = BenchmarkType.Rastrigin;

        static double[][] Foods = new double[FoodNumber][];
        static double[] Fitness = new double[FoodNumber];
        static int[] Trial = new int[FoodNumber];
        static Random rand = new Random();

        public void Start()
        {
            Initialize();
            ConfigureBenchmark();
            // Maximum number of cycles
            for (int cycle = 0; cycle < MaxCycles; cycle++)
            {
                EmployedBeePhase();
                OnlookerBeePhase();
                ScoutBeePhase();
                // Print best solution
                int best = GetBestFoodIndex();
                Console.WriteLine($"Cycle {cycle + 1}, Best fitness = {Fitness[best]:F5}");
            }
            int finalBest = GetBestFoodIndex();
            Console.WriteLine("Best solution found:");
            Console.WriteLine($"x = {string.Join(", ", Foods[finalBest])}, fitness = {Fitness[finalBest]:F5}");
        }


        static void ConfigureBenchmark()
        {
            switch (SelectedBenchmark)
            {
                case BenchmarkType.Sphere:
                case BenchmarkType.RotatedElliptic:
                    D = 30;
                    LowerBound = -100;
                    UpperBound = 100;
                    break;
                case BenchmarkType.RotatedBentCigar:
                    D = 30;
                    LowerBound = -100;
                    UpperBound = 100;
                    break;
                case BenchmarkType.RotatedDiscus:
                    D = 30;
                    LowerBound = -100;
                    UpperBound = 100;
                    break;
                case BenchmarkType.DifferentPowers:
                    D = 30;
                    LowerBound = -100;
                    UpperBound = 100;
                    break;
                case BenchmarkType.Rosenbrock:
                    D = 30;
                    LowerBound = -30;
                    UpperBound = 30;
                    break;
                case BenchmarkType.SchafferF7:
                    D = 30;
                    LowerBound = -100;
                    UpperBound = 100;
                    break;
                case BenchmarkType.Ackley:
                    D = 30;
                    LowerBound = -32.768;
                    UpperBound = 32.768;
                    break;
                case BenchmarkType.Weierstrass:
                    D = 30;
                    LowerBound = -0.5;
                    UpperBound = 0.5;
                    break;
                case BenchmarkType.Griewank:
                    D = 30;
                    LowerBound = -600;
                    UpperBound = 600;
                    break;
                case BenchmarkType.Rastrigin:
                    D = 30;
                    LowerBound = -5.12;
                    UpperBound = 5.12;
                    break;
            }
        }

        static void Initialize()
        {
            for (int i = 0; i < FoodNumber; i++)
            {
                Foods[i] = new double[D];
                for (int j = 0; j < D; j++)
                    Foods[i][j] = LowerBound + rand.NextDouble() * (UpperBound - LowerBound);
                Fitness[i] = CalculateFitness(Foods[i]);
                Trial[i] = 0;
            }
        }

        static void EmployedBeePhase()
        {
            for (int i = 0; i < FoodNumber; i++)
            {
                double[] newSolution = GenerateNeighbor(Foods[i]);
                double newFitness = CalculateFitness(newSolution);
                if (newFitness < Fitness[i])
                {
                    Foods[i] = newSolution;
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
            for (int t = 0; t < FoodNumber; t++)
            {
                double r = rand.NextDouble();
                for (int i = 0; i < FoodNumber; i++)
                {
                    double prob = 0.9 * (maxFit - Fitness[i]) / (maxFit) + 0.1;
                    if (r < prob)
                    {
                        double[] newSolution = GenerateNeighbor(Foods[i]);
                        double newFitness = CalculateFitness(newSolution);
                        if (newFitness < Fitness[i])
                        {
                            Foods[i] = newSolution;
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
            for (int i = 0; i < FoodNumber; i++)
            {
                if (Trial[i] >= Limit)
                {
                    for (int j = 0; j < D; j++)
                        Foods[i][j] = LowerBound + rand.NextDouble() * (UpperBound - LowerBound);
                    Fitness[i] = CalculateFitness(Foods[i]);
                    Trial[i] = 0;
                }
            }
        }

        static double[] GenerateNeighbor(double[] current)
        {
            double[] newSolution = (double[])current.Clone();
            int j = rand.Next(D);
            int k;
            do { k = rand.Next(FoodNumber); } while (Foods[k] == current);

            double phi = (rand.NextDouble() * 2) - 1; // [-1,1]
            newSolution[j] = current[j] + phi * (current[j] - Foods[k][j]);
            newSolution[j] = Math.Max(LowerBound, Math.Min(UpperBound, newSolution[j]));
            return newSolution;
        }

        static double CalculateFitness(double[] solution)
        {
            switch (SelectedBenchmark)
            {
                case BenchmarkType.Sphere:
                    return solution.Sum(x => x * x);

                case BenchmarkType.RotatedElliptic:
                    int n = solution.Length;
                    double sum = 0.0;
                    for (int i = 0; i < n; i++)
                    {
                        double factor = Math.Pow(1e6, i / (double)(n - 1));
                        sum += factor * solution[i] * solution[i];
                    }
                    return sum;
                
                case BenchmarkType.RotatedBentCigar:
                    double sum = solution[0] * solution[0];
                    for (int i = 1; i < solution.Length; i++)
                        sum += 1e6 * solution[i] * solution[i];
                    return sum;

                case BenchmarkType.RotatedDiscus:
                    double sum = 1e6 * solution[0] * solution[0];
                    for (int i = 1; i < solution.Length; i++)
                        sum += solution[i] * solution[i];
                    return sum;

                case BenchmarkType.DifferentPowers:
                    int n = solution.Length;
                    double sum = 0.0;
                    for (int i = 0; i < n; i++)
                    {
                        double exponent = 2.0 + 4.0 * i / (n - 1.0);
                        sum += Math.Pow(Math.Abs(solution[i]), exponent);
                    }
                    return sum;

                case BenchmarkType.Rosenbrock:
                    double sum = 0;
                    for (int i = 0; i < D - 1; i++)
                        sum += 100 * Math.Pow(solution[i + 1] - solution[i] * solution[i], 2) + Math.Pow(solution[i] - 1, 2);
                    return sum;
                
                case BenchmarkType.SchafferF7:
                    double sum = 0.0;
                    for (int i = 0; i < solution.Length - 1; i++)
                        {
                            double xi = solution[i];
                            double xi1 = solution[i + 1];
                            double temp = Math.Sqrt(xi * xi + xi1 * xi1);
                            sum += Math.Pow(temp, 0.5) + Math.Pow(Math.Sin(50 * Math.Pow(temp, 0.2)), 2);
                        }
                    return Math.Pow(sum / (solution.Length - 1), 2);

                case BenchmarkType.Ackley:
                    double sumSq = solution.Sum(x => x * x);
                    double sumCos = solution.Sum(x => Math.Cos(2 * Math.PI * x));
                    return -20 * Math.Exp(-0.2 * Math.Sqrt(sumSq / D)) - Math.Exp(sumCos / D) + 20 + Math.E;
                
                case BenchmarkType.Griewank:
                    double sumG = solution.Sum(x => x * x) / 4000.0;
                    double prod = 1;
                    for (int i = 0; i < D; i++)
                        prod *= Math.Cos(solution[i] / Math.Sqrt(i + 1));
                    return 1 + sumG - prod;

                case BenchmarkType.Weierstrass:
                    int kMax = 20;
                    double a = 0.5;
                    double b = 3.0;
                    int n = solution.Length;

                    double sum = 0.0;
                    for (int i = 0; i < n; i++)
                    {
                        for (int k = 0; k <= kMax; k++)
                        {
                            sum += Math.Pow(a, k) * Math.Cos(2 * Math.PI * Math.Pow(b, k) * (x[i] + 0.5));
                        }
                    }

                    double sum2 = 0.0;
                    for (int k = 0; k <= kMax; k++)
                    {
                        sum2 += Math.Pow(a, k) * Math.Cos(Math.PI * Math.Pow(b, k));
                    }

                    return sum - n * sum2;

                case BenchmarkType.Rastrigin:
                    return 10 * D + solution.Sum(x => x * x - 10 * Math.Cos(2 * Math.PI * x));

                default:
                    return double.MaxValue;
            }
        }

        static int GetBestFoodIndex()
        {
            double minFit = Fitness.Min();
            return Array.IndexOf(Fitness, minFit);
        }
    }
}
