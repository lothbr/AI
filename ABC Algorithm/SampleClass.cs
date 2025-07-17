using ABC_Algorithm.BenchMarks;

namespace ABC_Algorithm
{
    public class SampleClass
    {
        private int FoodNumber = 10;        // Number of food sources
        private int D = 30;                 // Dimension of problem
        private int Limit = 100;            // Limit for abandonment
        private int MaxCycles = 500;        // Max iterations
        private double LowerBound = -5.12;  // Lower bound
        private double UpperBound = 5.12;   // Upper bound
        private BenchmarkType SelectedBenchmark = BenchmarkType.Weierstrass;

        private double[][] Foods;
        private double[] Fitness;
        private int[] Trial;
        private Random rand = new Random();

        public SampleClass(int foodNumber = 10, int maxCycles = 500, int limit = 100, int dimension = 30)
        {
            FoodNumber = foodNumber;
            MaxCycles = maxCycles;
            Limit = limit;
            D = dimension;
            Foods = new double[FoodNumber][];
            Fitness = new double[FoodNumber];
            Trial = new int[FoodNumber];
        }

        public void Start()
        {
            try
            {
                ConfigureBenchmark();
                Initialize();
                for (int cycle = 0; cycle < MaxCycles; cycle++)
                {
                    EmployedBeePhase();
                    OnlookerBeePhase();
                    ScoutBeePhase();
                    int best = GetBestFoodIndex();
                    FileLogger.logInfo($"Cycle {cycle + 1}, Best fitness = {Fitness[best]:F5}");
                }
                int finalBest = GetBestFoodIndex();
                FileLogger.logInfo("Best solution found:");
                FileLogger.logInfo($"x = {string.Join(", ", Foods[finalBest])}, fitness = {Fitness[finalBest]:F5}");
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"Error in ABC execution: {ex.Message}", ex.StackTrace);
            }
        }

        private void ConfigureBenchmark()
        {
            // Same as your implementation
            switch (SelectedBenchmark)
            {
                case BenchmarkType.Sphere:
                case BenchmarkType.RotatedElliptic:
                case BenchmarkType.RotatedBentCigar:
                case BenchmarkType.RotatedDiscus:
                case BenchmarkType.DifferentPowers:
                    LowerBound = -100;
                    UpperBound = 100;
                    break;
                case BenchmarkType.Rosenbrock:
                    LowerBound = -30;
                    UpperBound = 30;
                    break;
                case BenchmarkType.SchafferF7:
                    LowerBound = -100;
                    UpperBound = 100;
                    break;
                case BenchmarkType.Ackley:
                    LowerBound = -32.768;
                    UpperBound = 32.768;
                    break;
                case BenchmarkType.Weierstrass:
                    LowerBound = -0.5;
                    UpperBound = 0.5;
                    break;
                case BenchmarkType.Griewank:
                    LowerBound = -600;
                    UpperBound = 600;
                    break;
                case BenchmarkType.Rastrigin:
                    LowerBound = -5.12;
                    UpperBound = 5.12;
                    break;
				case BenchmarkType.RotatedKatsuura:
					LowerBound = -5;
					UpperBound = 5;
					break;
            }
        }

        private void Initialize()
        {
            for (int i = 0; i < FoodNumber; i++)
            {
                Foods[i] = new double[D];
                for (int j = 0; j < D; j++)
                    Foods[i][j] = LowerBound + rand.NextDouble() * (UpperBound - LowerBound);
                Fitness[i] = 1.0 / (1.0 + CalculateObjective(Foods[i])); // Transform for minimization
                Trial[i] = 0;
            }
        }

        private void EmployedBeePhase()
        {
            for (int i = 0; i < FoodNumber; i++)
            {
                double[] newSolution = GenerateNeighbor(Foods[i]);
                double newObjective = CalculateObjective(newSolution);
                double newFitness = 1.0 / (1.0 + newObjective);
                if (newFitness > Fitness[i]) // Compare fitness (higher is better)
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

        private void OnlookerBeePhase()
        {
            double sumFitness = Fitness.Sum();
            double[] probabilities = Fitness.Select(f => f / sumFitness).ToArray();
            int onlooker = 0;
            while (onlooker < FoodNumber)
            {
                for (int i = 0; i < FoodNumber; i++)
                {
                    if (rand.NextDouble() < probabilities[i])
                    {
                        double[] newSolution = GenerateNeighbor(Foods[i]);
                        double newObjective = CalculateObjective(newSolution);
                        double newFitness = 1.0 / (1.0 + newObjective);
                        if (newFitness > Fitness[i])
                        {
                            Foods[i] = newSolution;
                            Fitness[i] = newFitness;
                            Trial[i] = 0;
                        }
                        else
                        {
                            Trial[i]++;
                        }
                        onlooker++;
                        break;
                    }
                }
            }
        }

        private void ScoutBeePhase()
        {
            int maxTrialIndex = Array.IndexOf(Trial, Trial.Max());
            if (Trial[maxTrialIndex] >= Limit)
            {
                for (int j = 0; j < D; j++)
                    Foods[maxTrialIndex][j] = LowerBound + rand.NextDouble() * (UpperBound - LowerBound);
                Fitness[maxTrialIndex] = 1.0 / (1.0 + CalculateObjective(Foods[maxTrialIndex]));
                Trial[maxTrialIndex] = 0;
            }
        }

        private double[] GenerateNeighbor(double[] current)
        {
            double[] newSolution = (double[])current.Clone();
            int j = rand.Next(D);
            int k;
            do { k = rand.Next(FoodNumber); } while (k == Array.IndexOf(Foods, current));
            double phi = (rand.NextDouble() * 2) - 1; // [-1,1]
            newSolution[j] = current[j] + phi * (current[j] - Foods[k][j]);
            newSolution[j] = Math.Max(LowerBound, Math.Min(UpperBound, newSolution[j]));
            return newSolution;
        }

        private double CalculateObjective(double[] solution)
        {
            // Same as your CalculateFitness, renamed to clarify it returns objective value
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
                    double sumCigar = solution[0] * solution[0];
                    for (int i = 1; i < solution.Length; i++)
                        sumCigar += 1e6 * solution[i] * solution[i];
                    return sumCigar;
                case BenchmarkType.RotatedDiscus:
                    double sumDiscus = 1e6 * Math.Pow(solution[0], 2);
                    for (int i = 1; i < solution.Length; i++)
                        sumDiscus += solution[i] * solution[i];
                    return sumDiscus;
                case BenchmarkType.DifferentPowers:
                    int n1 = solution.Length;
                    double sumPowers = 0.0;
                    for (int i = 0; i < n1; i++)
                    {
                        double exponent = 2.0 + (4.0 * i / (n1 - 1.0)); // Fixed i-1 issue
                        sumPowers += Math.Pow(Math.Abs(solution[i]), exponent);
                    }
                    return sumPowers;
                case BenchmarkType.Rosenbrock:
                    double sumRosenbrock = 0;
                    for (int i = 0; i < D - 1; i++)
                        sumRosenbrock += 100 * Math.Pow(solution[i + 1] - solution[i] * solution[i], 2) + Math.Pow(solution[i] - 1, 2);
                    return sumRosenbrock;
                case BenchmarkType.SchafferF7:
                    double sumSchafferF7 = 0.0;
                    for (int i = 0; i < solution.Length - 1; i++)
                    {
                        double xi = solution[i];
                        double xi1 = solution[i + 1];
                        double temp = Math.Sqrt(xi * xi + xi1 * xi1);
                        sumSchafferF7 += Math.Pow(temp, 0.5) + Math.Pow(Math.Sin(50 * Math.Pow(temp, 0.2)), 2);
                    }
                    return Math.Pow(sumSchafferF7 / (solution.Length - 1), 2);
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
                    double sumWeierstrass = 0.0;
                    for (int i = 0; i < solution.Length; i++)
                    {
                        for (int k = 0; k <= kMax; k++)
                        {
                            sumWeierstrass += Math.Pow(a, k) * Math.Cos(2 * Math.PI * Math.Pow(b, k) * (solution[i] + 0.5));
                        }
                    }
                    double sum2 = 0.0;
                    for (int k = 0; k <= kMax; k++)
                    {
                        sum2 += Math.Pow(a, k) * Math.Cos(2 * Math.PI * Math.Pow(b, k) * 0.5);
                    }
                    return sumWeierstrass - (solution.Length * sum2);
                case BenchmarkType.Rastrigin:
                    return 10 * D + solution.Sum(x => x * x - 10 * Math.Cos(2 * Math.PI * x));
                //case BenchmarkType.RotatedKatsuura:
                //	{
                //		double[] z = LambdaTransform(MatrixMultiply(solution, M), o);
                //		double prod = 1.0;
                //		for (int i = 0; i < z.Length; i++)
                //		{
                //			double sum = 0.0;
                //			for (int j = 1; j <= 32; j++)
                //			{
                //				sum += Math.Abs(Math.Pow(2, j) * z[i] - Math.Round(Math.Pow(2, j) * z[i])) / Math.Pow(2, j);
                //			}
                //			prod *= Math.Pow(1.0 + (i + 1) * sum, 10.0 / Math.Pow(D, 1.2));
                //		}
                //		return (prod - 1.0) * (10.0 / D / D);
                //	}
                default:
                    return double.MaxValue;
            }
        }

        private int GetBestFoodIndex()
        {
            double maxFit = Fitness.Max(); // Higher fitness is better
            return Array.IndexOf(Fitness, maxFit);
        }
    }
}