using Logger;
using ParsingCSV.Mapping;
using ParsingCSV.Parsing;
using ParsingCSV.Provider;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System;

namespace maptool
{
    public partial class MainWindow : Window
    {
        public Parser Parser { get; private set; }
        public string FileNameCsv { get; private set; }

        public ObservableCollection<Param> Params { get; private set; }
        public ObservableCollection<ColmnMapping> collectionColMap { get; private set; }

        #region Logger
        ILogger Log;
        private bool LogInfo { set; get; }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            Parser = Parser.Instance;
            InitiaState();
            InitLogger();
        }

        private void InitLogger()
        {
            //exLogger.IsExpanded = true;
            LogInfo = true;
            Log = WPFLogger.Instance;
            WPFLogger.Instance.Initialize((ListBox)lbxLog);
        }

        private void btnСhooseFile_Click(object sender, RoutedEventArgs e)
        {
            if (LogInfo) Log.Info("btnСhooseFile_Click");

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";

            var result = dlg.ShowDialog();

            if (result != true || System.IO.Path.GetExtension(dlg.FileName) != ".csv")
            {
                MessageBox.Show("Выберите файл с расширением .csv", "Открытие неудалось", MessageBoxButton.OK, MessageBoxImage.Information);
                if (LogInfo) Log.Info("Try open file - " + dlg.FileName);
                return;
            }
            FileNameCsv = dlg.FileName;
            lblFileName.Content = FileNameCsv;

            btnLoadFileToMapping.IsEnabled = true;
        }

        private async void btnLoadFileToMapping_Click(object sender, RoutedEventArgs e)
        {
            StateMapping();
            
            await Task.Run(() => Parser.Initializing(FileNameCsv));
            if (Parser.MatchCSV != true)
            {
                lblErrorMsg.Content = "Файл не соответствует формату или поврежден, или имена колонок совпадают";
                return;
            }
            else lblErrorMsg.Content = "";
            // TODO
            lblErrorMsg.Content = Parser.CheckBeforeLoadingMap();
            InitCollectionColMap();
        }

        private void StateMapping()
        {
            exMappin.IsExpanded = true;
            exMappin.IsEnabled = true;
            lblFileInfo.Content = Path.GetFileNameWithoutExtension(FileNameCsv);
            btnLoadFileToMapping.IsEnabled = false;
            exСhooseFile.IsExpanded = false;
        }

        private void InitCollectionColMap()
        {
            this.Params = Parser.Params;
            collectionColMap = Parser.ColMap;

            DataContext = null;
            DataContext = this;
        }

        private async void btnOk_Click(object sender, RoutedEventArgs e)
        {
            // TODO Check
            string answer = Parser.ChekColMap();
            if (answer != "")
            {
                lblErrorMsg.Visibility = Visibility.Visible;
                lblErrorMsg.Content = answer;
                InitCollectionColMap();
                return;
            }
            else lblErrorMsg.Visibility = Visibility.Hidden;
            InitCollectionColMap();

            // for testing
            // return;

            // Done Load Table to DB
            bool b = true;
            await Task.Run(() => b = Parser.CreateTable()); 

            // IF everything Ok
            var w = new ViewPagination(Parser.Instance.GetTableNameRelativelyFile());
            w.ShowDialog();
        }

        private void InitiaState()
        {
            lblErrorMsg.Visibility = Visibility.Visible;
            exСhooseFile.IsExpanded = true;
            exMappin.IsExpanded = false;
            exMappin.IsEnabled = false;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            InitiaState();
        }
    }
}