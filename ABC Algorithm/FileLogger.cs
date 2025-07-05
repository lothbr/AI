using Serilog;

namespace ABC_Algorithm
{
    public static class FileLogger
    {
        public static void logInfo (string message ) 
        {
            Log.Information (message);
        }
        public static void LogError(string message, string ex)
        {
            Log.Error (string.Format("{0} {1}",message, ex));
        }
    }
}
