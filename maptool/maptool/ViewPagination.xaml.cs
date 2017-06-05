using ParsingCSV.Provider;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace maptool
{
    /// <summary>
    /// Interaction logic for ViewPagination.xaml
    /// </summary>
    public partial class ViewPagination : Window
    {
        public DataTable dt { get; private set; } = new DataTable();
        public DataTable PaginationTable { get; private set; } = new DataTable();
        public ProviderDb Provider { get; private set; }
        public int pageNumber { get; set; } = 0;        // Current page
        public int pages { get; set; }                  // Page count
        public readonly int pageSize = 50;              // Number of elements per page

        public ViewPagination()
        {
            InitializeComponent();
        }

        public ViewPagination(string tableName)
        {
            InitializeComponent();
            Initial(tableName);  
        }

        private void Initial(string s)
        {
            Provider = ProviderDb.Instance;
            dt = Provider.Pagination(s);
            GetColumnPaginationTable();
            Pagination(0);
        }

        private void GetColumnPaginationTable()
        {
            for (int i = 1; i < dt.Columns.Count; i++)
            {
                PaginationTable.Columns.Add(dt.Columns[i].ColumnName);
            }
        }

        private void Pagination(int _pageNumber)
        {          
            int count = 0;                  // Total elements
            count = dt.Rows.Count;
            pages = dt.Rows.Count / pageSize;
            lbMaxPage.Content = "... " + pages.ToString();
            if (count > pageNumber * pageSize)
            {
                var query = dt.AsEnumerable().Skip(pageNumber * pageSize).Take(pageSize);
                PaginationTable.Rows.Clear();
                foreach (var v in query)
                {
                    var nr = PaginationTable.NewRow();
                    int i = 0;
                    foreach (var w in PaginationTable.Columns)
                    {
                        nr[w.ToString()] = v.ItemArray[++i];
                    }
                    PaginationTable.Rows.Add(nr);
                }
                dgTable.ItemsSource = PaginationTable.AsDataView();
            }
        }

        private void btnOlder_Click(object sender, RoutedEventArgs e)
        {
            if (pageNumber >= 1) Pagination(--pageNumber);
            tbPag.Text = pageNumber.ToString();
        }

        private void btnNewer_Click(object sender, RoutedEventArgs e)
        {
            if (pageNumber < (dt.Rows.Count / pageSize)) Pagination(++pageNumber);
            tbPag.Text = pageNumber.ToString();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int i;
            TextBox textBox = (TextBox)sender;
            if (int.TryParse(textBox.Text, out i))
            {
                if (i >= 0 && i < (pages))
                {
                    pageNumber = i;
                    Pagination(pageNumber);
                }
            }
        }

        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            pageNumber = pages;
            tbPag.Text = pageNumber.ToString();
            Pagination(pageNumber);
        }

        private void btnOlder_Click_1(object sender, RoutedEventArgs e)
        {
            pageNumber = 0;
            tbPag.Text = pageNumber.ToString();
            Pagination(pageNumber);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
