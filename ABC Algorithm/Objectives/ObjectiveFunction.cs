using ABC_Algorithm.BenchMarks;

namespace ABC_Algorithm.Objectives
{
    public class ObjectiveFunction
    {
        private int OptimizationParameters; // Default dimension of problem
        private double LowerBound; // Default lower bound for parameters
        private double UpperBound; // Default upper bound for parameters
        private BenchmarkType SelectedBenchmark; // Selected benchmark function
        public ObjectiveFunction(int OptimizationParameters, double LowerBound, double UpperBound, BenchmarkType SelectedBenchmark)
        {
            this.OptimizationParameters = OptimizationParameters;
            this.LowerBound = LowerBound;
            this.UpperBound = UpperBound;
            this.SelectedBenchmark = SelectedBenchmark;
            ConfigureBenchmark();
        }
        public void ConfigureBenchmark()
        {
            switch (SelectedBenchmark)
            {
                case BenchmarkType.Sphere:
                case BenchmarkType.RotatedElliptic:
                    OptimizationParameters = 30;
                    LowerBound = -100;
                    UpperBound = 100;
                    break;
                case BenchmarkType.RotatedBentCigar:
                    OptimizationParameters = 30;
                    LowerBound = -100;
                    UpperBound = 100;
                    break;
                case BenchmarkType.RotatedDiscus:
                    OptimizationParameters = 30;
                    LowerBound = -100;
                    UpperBound = 100;
                    break;
                case BenchmarkType.DifferentPowers:
                    OptimizationParameters = 30;
                    LowerBound = -100;
                    UpperBound = 100;
                    break;
                case BenchmarkType.Rosenbrock:
                    OptimizationParameters = 30;
                    LowerBound = -30;
                    UpperBound = 30;
                    break;
                case BenchmarkType.SchafferF7:
                    OptimizationParameters = 30;
                    LowerBound = -100;
                    UpperBound = 100;
                    break;
                case BenchmarkType.Ackley:
                    OptimizationParameters = 30;
                    LowerBound = -32.768;
                    UpperBound = 32.768;
                    break;
                case BenchmarkType.Weierstrass:
                    OptimizationParameters = 30;
                    LowerBound = -0.5;
                    UpperBound = 0.5;
                    break;
                case BenchmarkType.Griewank:
                    OptimizationParameters = 30;
                    LowerBound = -600;
                    UpperBound = 600;
                    break;
                case BenchmarkType.Rastrigin:
                    OptimizationParameters = 30;
                    LowerBound = -5.12;
                    UpperBound = 5.12;
                    break;
				case BenchmarkType.RotatedKatsuura:
					OptimizationParameters = 30;
					LowerBound = -5;
					UpperBound = 5;
					break;
            }
        }
        public double CalculateObjective(double[] solution)
        {
            switch (SelectedBenchmark)
            {
                case BenchmarkType.Sphere:
                    return solution.Sum(x => x * x);

                case BenchmarkType.RotatedElliptic:
                    int n = solution.Length;
                    var sum = 0.0;
                    for (int i = 0; i < n; i++)
                    {
                        double factor = Math.Pow(1e6, i / (double)(n - 1));
                        sum += factor * solution[i] * solution[i];
                    }
                    return sum;

                case BenchmarkType.RotatedBentCigar:
                    var sumCigar = solution[0] * solution[0];
                    for (int i = 1; i < solution.Length; i++)
                        sumCigar += 1e6 * solution[i] * solution[i];
                    return sumCigar;

                case BenchmarkType.RotatedDiscus:
                    var sumDiscuss = 1e6 * Math.Pow(solution[0], 2);
                    for (int i = 1; i < solution.Length; i++)
                        sumDiscuss += solution[i] * solution[i];
                    return sumDiscuss;

                case BenchmarkType.DifferentPowers:
                    int n1 = solution.Length;
                    var sumPowers = 0.0;
                    for (int i = 0; i < n1; i++)
                    {
                        double exponent = 2.0 + (4.0 * (i - 1 / (n1 - 1.0)));
                        sumPowers += Math.Pow(Math.Abs(solution[i]), exponent);
                    }
                    return sumPowers;

                case BenchmarkType.Rosenbrock:
                    double sumRosenbrock = 0;
                    for (int i = 0; i < OptimizationParameters - 1; i++)
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
                    return -20 * Math.Exp(-0.2 * Math.Sqrt(sumSq / OptimizationParameters)) - Math.Exp(sumCos / OptimizationParameters) + 20 + Math.E;

                case BenchmarkType.Griewank:
                    double sumG = solution.Sum(x => x * x) / 4000.0;
                    double prod = 1;
                    for (int i = 0; i < OptimizationParameters; i++)
                        prod *= Math.Cos(solution[i] / Math.Sqrt(i + 1));
                    return 1 + sumG - prod;

                case BenchmarkType.Weierstrass:
                    int kMax = 20;
                    double a = 0.5;
                    double b = 3.0;
                    int len = solution.Length;
                    FileLogger.logInfo(string.Format("Weierstrass Function got here  \n solution Length ={0}", len));
                    double sumWeierstrass = 0.0;

                    for (int i = 0; i < len; i++)
                    {
                        for (int k = 0; k <= kMax; k++)
                        {
                            sumWeierstrass += Math.Pow(a, k) * Math.Cos(2 * Math.PI * Math.Pow(b, k) * (solution[i] + 0.5));
                        }
                    }
                    FileLogger.logInfo(string.Format("Evaluating sumWeierstrass got here  ={0}", sumWeierstrass));

                    double sum2 = 0.0;
                    for (int k = 0; k <= kMax; k++)
                    {
                        sum2 += Math.Pow(a, k) * Math.Cos(2 * Math.PI * Math.Pow(b, k) * 0.5);
                    }

                    FileLogger.logInfo(string.Format("Return Result  got here  ={0}", sumWeierstrass - (len * sum2)));

                    return sumWeierstrass - (len * sum2);

                case BenchmarkType.Rastrigin:
                    return 10 * OptimizationParameters + solution.Sum(x => x * x - 10 * Math.Cos(2 * Math.PI * x));

				case BenchmarkType.RotatedKatsuura:
                    {
                        int D = solution.Length;

                        // Inline Lambda transform: z[i] = 0.5 * x[i]
                        double[] z = new double[D];
                        for (int i = 0; i < D; i++)
                        {
                            z[i] = 0.5 * solution[i];
                        }

                        double prod = 1.0;

                        for (int i = 0; i < D; i++)
                        {
                            double sum = 0.0;

                            for (int j = 1; j <= 32; j++)
                            {
                                double term = Math.Pow(2, j) * z[i];
                                sum += Math.Abs(term - Math.Round(term)) / Math.Pow(2, j);
                            }

                            prod *= Math.Pow(1.0 + (i + 1) * sum, 10.0 / Math.Pow(D, 1.2));
                        }

                        double result = (prod - 1.0) * (10.0 / (D * D));
                        return result;
                    }

					
                default:
                    return double.MaxValue;
            }
        }
    }
}
