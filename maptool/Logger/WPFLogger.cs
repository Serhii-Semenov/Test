using System.Windows.Controls;

namespace Logger
{
    public class WPFLogger : ILogger
    {
        public static WPFLogger Instance { get { return instance; } }

        private static int number { get; set; }

        private static readonly WPFLogger instance = new WPFLogger();
        private ListBox objName;
        private WPFLogger() { }

        public void Initialize(ListBox objName)
        {
            this.objName = objName;
            number = 1;
        }

        public void Debug(string message) { objName.Items.Add(string.Format("({0, 4})DEBUG : {1}", number++, message)); }

        public void Error(string message) { objName.Items.Add(string.Format("({0, 4})ERROR : {1}", number++, message)); }

        public void Info(string message) { objName.Items.Add(string.Format("({0, 4})INFO : {1}", number++, message)); }

    }
}