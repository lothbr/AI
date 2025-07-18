using ABC_Algorithm.Objectives;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ABC_Algorithm
{
    /// <summary>
    /// The InitializeABC class provides methods for initializing the food sources in the Artificial Bee Colony (ABC) algorithm.
    /// The initialization phase sets each solution parameter using the formula:
    /// xij = xj_min + rand(0, 1) × (xj_max - xj_min),
    /// where i = 1...SN (number of food sources) and j = 1...D (number of optimization parameters).
    /// Additionally, counters tracking the number of trials for each solution are reset to 0 during this phase.
    /// </summary>
    public class InitializeABC
    {
        private readonly Random rand = new(); // Single Random instance
        private readonly ObjectiveFunction _objectiveFunction;
        public InitializeABC(ObjectiveFunction objectiveFunction)
        {
            _objectiveFunction = objectiveFunction ?? throw new ArgumentNullException(nameof(objectiveFunction), "Objective function cannot be null.");
        }

        public  void Initialize(int foodSource, int d, ref double[][] foodSources, ref double[] Fitness, ref int[] trials, double lowerBound, double upperBound)
        {
            // Ensure foodSources is allocated
            if (foodSources == null || foodSources.Length != foodSource || foodSources.Any(row => row == null || row.Length != d))
            {
                throw new ArgumentException("foodSources must be pre-allocated with dimensions [foodSource, d]");
            }

            for (int i = 0; i < foodSource; i++)
            {
                for (int j = 0; j < d; j++)
                {
                    foodSources[i][j] = lowerBound + rand.NextDouble() * (upperBound - lowerBound);
                }
                Fitness[i] = _objectiveFunction.EvaluateObjectiveCost(foodSources[i]);
                trials[i] = 0; // Reset trial counter
            }
        }
    }
}
