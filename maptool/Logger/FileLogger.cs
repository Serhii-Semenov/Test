using System.IO;

namespace Logger
{
    public class FileLogger : ILogger
    {
        public static FileLogger Instance { get { return instance; } }
        private static int number { get; set; }
        private static readonly FileLogger instance = new FileLogger();
        private string filename;
        private FileLogger() { }

        public void Initialize(string filename)
        {
            this.filename = filename;
            number = 1;
        }

        public void Debug(string message)
        {
            File.AppendAllText(filename, string.Format("({0, 4})Debug : {1}\n", number++, message));
        }

        public void Error(string message)
        {
            File.AppendAllText(filename, string.Format("({0, 4})Error : {1}\n", number++, message));
        }

        public void Info(string message)
        {
            File.AppendAllText(filename, string.Format("({0, 4})Info : {1}\n", number++, message));
        }
    }
}
