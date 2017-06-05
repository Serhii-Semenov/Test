using Microsoft.VisualBasic.FileIO;
using ParsingCSV.Mapping;
using ParsingCSV.Provider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ParsingCSV.Parsing
{
    public class Parser
    {
        public static Parser Instance { get { return instance; } }
        private static readonly Parser instance = new Parser();
        private Parser() { Provider = ProviderDb.Instance; }

        public DataTable TableCSV { get; private set; }
        public bool MatchCSV { get; private set; }                                              //Checking file compliance
        public ObservableCollection<Param> Params { get; private set; }
        public ObservableCollection<ColmnMapping> ColMap { get; private set; }
        public readonly ParserParams ParamParser = new ParserParams();
        public string FileName { get; private set; }
        public ProviderDb Provider { get; private set; }

        #region public

        public string GetTableNameRelativelyFile()
        {
            return Regex.Replace(Path.GetFileNameWithoutExtension(FileName), @"\W", "");
        }

        public void Initializing(string FileName)
        {
            Params = new ObservableCollection<Param>();
            ColMap = new ObservableCollection<ColmnMapping>();
            TableCSV = new DataTable();

            this.FileName = FileName;
            MatchCSV = true;
            InitParams(); 
            GetTableCSV();
            GetColMap();
        }

        // Create table with ColMap optons
        public bool CreateTable()
        {
            return Provider.CreateTable();
        }

        #endregion

        #region private

        // Fill list ColmnMapping
        private void GetColMap()
        {
            foreach (DataColumn item in TableCSV.Columns)
            {
                var col = item.ColumnName;
                // DONE Try get param for this ColumnName, else Param[NoMapped]
                ColMap.Add(new ColmnMapping(col, TryGetParam(col), GetExamples(col), new Err()));
                // DONE To think about getExamles 
                // TODO To think about getError 
            }
        }

        // Auto-select
        private Param TryGetParam(string col)
        {
            var p = new Param();
            switch (col)
            {
                case "SKU":
                    p = Params[1];
                    break;
                case "Manufacture":
                    p = Params[2];
                    break;
                case "Cost":
                    p = Params[3];
                    break;
                case "Weight":
                    p = Params[4];
                    break;
                case "Title":
                case "Description":
                    p = Params[5];
                    break;
                case "Color":
                case "Size":
                    p = Params[6];
                    break;
                case "Shipping":
                    p = Params[7];
                    break;
                default:
                    p = Params[0];
                    break;
            }
            return p;
        }

        public string CheckBeforeLoadingMap()
        {
            string s = "";
            foreach(var v in ColMap)
            {
                // unique columns NAME
                var unique = (from a in ColMap
                              where a.ColmnName == v.ColmnName
                              select a.ColmnName).Distinct().OrderBy(b => b);
                if (unique.Count() == 1)
                {
                    ColMap.FirstOrDefault(a => a.ColmnName == v.ColmnName).Errors.OnlyOne = "";
                }
                else
                {
                    s += " " + v.ColmnName + " Не уникально!";
                }

                // Имя колонки на симфолы
                // string pattern = @"[0-9a-zA-Z_]";
                string pattern = @"\w+";
                var v1 = Regex.IsMatch(v.ColmnName, pattern, RegexOptions.IgnoreCase);
                var v2 = Regex.IsMatch(v.ColmnName, @"\w+\s+\w*");
                if (!v1 && v2)
                {
                    s += " " + v.ColmnName + " Некорректный.";
                }

            }
            return s; 
        }

        public void GetErrorsInColMap()
        {
            for (int i = 0; i < ColMap.Count; i++)
            {
                ColMap[i].GetError();
            }
        }

        public string ChekColMap()
        {
            // TODO
            bool error = false;
            int requiredField = 0;

            // error for unique
            foreach (var v in ColMap)
            {
                // Done UNIQUE
                if (v.Parameter.OnlyOne)
                {
                    var unique = (from a in ColMap
                                  where a.Parameter.NameParam == v.Parameter.NameParam
                                  select a.ColmnName).Distinct().OrderBy(b => b);
                    if (unique.Count() == 1)
                    {
                        ColMap.FirstOrDefault(a => a.ColmnName == v.ColmnName).Errors.OnlyOne = "";
                    }
                    else
                    {
                        error = true;
                        ColMap.FirstOrDefault(a => a.ColmnName == v.ColmnName).Errors.OnlyOne = "не уникальное";
                    }
                }
                // TODO RequiredField
                if (v.Parameter.RequiredField)
                    requiredField++;

                if (v.Parameter.Id == 0) return "NoMapped параметров не должно быть";
            }

            GetErrorsInColMap();
            if (requiredField == 3) return !error ? "" : "Error is exist";
            if (requiredField < 3)
                return "Обязательные поля SCU, Brand, Price - не в полном объёме";
            else if (requiredField > 3) return "Много обязательных полей SCU, Brand, Price.";
            return "";
        }

        // SCU + Brand = UNIQUE
        private bool CheckScuBrandUnique()
        {
            // TODO
            return true;
        }

        #region Load Example 
        private string GetExamples(string col)
        {
            // TODO
            string result = "";
            string[] s = new string[] { "", "", "", "", "", "" };
            // Get column from table
            var q = from w in TableCSV.AsEnumerable()
                    select w.Field<string>(col);
            int i = 0;
            bool flag = true;
            foreach (string item in q)
            {
                if (string.IsNullOrWhiteSpace(s[i]))
                {
                    foreach (var v in s)
                    {
                        if (item == v)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        s[i++] = item;
                    }
                }
                if (i == 6)
                {
                    flag = false;
                    break;
                }
                flag = true;
            }

            for (int j = 0; j < ((flag) ? i : i - 1); j++)
            {
                if (!string.IsNullOrWhiteSpace(s[j]))
                {
                    result += ((j == 0) ? "" : " / ") + s[j];
                }
            }
            if (!flag) result += " / ...";
            return result;
        }

        #endregion

        public class ColComparer : IEqualityComparer<DataRow>
        {
            public string ColumnName { get; set; }
            public ColComparer(string s)
            {
                ColumnName = s;
            }
            public bool Equals(DataRow x, DataRow y)
            {
                return x.Field<string>(ColumnName) == y.Field<string>(ColumnName);
            }
            public int GetHashCode(DataRow obj)
            {
                return obj.ToString().GetHashCode();
            }
        }
        
        // Fill Params of existing parameters
        private void InitParams()
        {
            // hardcord // todo add to db
            Params = new ObservableCollection<Param> {
                new Param() { Id = 0, NameParam = "NoMapped", OnlyOne = false, RequiredField = false, LoadToDb = false, queryAdditive = "" },
                new Param() { Id = 1, NameParam = "SKU", OnlyOne = true, RequiredField = true, LoadToDb = true, queryAdditive = "NVARCHAR(50) NOT NULL," },
                new Param() { Id = 2, NameParam = "Brand", OnlyOne = true, RequiredField = true, LoadToDb = true, queryAdditive = "NVARCHAR(50) NOT NULL," },
                new Param() { Id = 3, NameParam = "Price",  OnlyOne = true, RequiredField = true, LoadToDb = true, queryAdditive = "SMALLMONEY NOT NULL," },
                new Param() { Id = 4, NameParam = "Weight", OnlyOne = true, RequiredField = false, LoadToDb = true, queryAdditive = "SMALLMONEY NULL," },
                new Param() { Id = 5, NameParam = "Feature", OnlyOne = false, RequiredField = false, LoadToDb = true, queryAdditive = "NVARCHAR(150) NULL," },
                new Param() { Id = 6, NameParam = "Product parameter", OnlyOne = false, RequiredField = false, LoadToDb = true, queryAdditive = "NVARCHAR(150) NULL," },
                new Param() { Id = 7, NameParam = "Ignore", OnlyOne = false, RequiredField = false, LoadToDb = false, queryAdditive = "" }
            };
        }

        // Fill table from file
        private void GetTableCSV()
        {
            using (var parser = new TextFieldParser(FileName))
            {
                try
                {
                    parser.TextFieldType = FieldType.Delimited;
                    // Get Header Columns
                    var header = parser.ReadLine();

                    var HeaderColumnsList = MappingRow(header);
                    foreach (var item in HeaderColumnsList)
                    {
                        TableCSV.Columns.Add(item);
                    }

                    // Get Body Table
                    // int k = 0;
                    while (!parser.EndOfData /*&& k < 5*/)
                    {
                        var row = MappingRow(parser.ReadLine());
                        AddARow(row);
                        // k++;
                    }
                }
                catch (Exception)
                {
                    MatchCSV = false;
                    //throw;
                }
            }
        }

        // Add new Row in Table
        private void AddARow(List<string> row)
        {
            try
            {
                var newRow = TableCSV.NewRow();
                for (int i = 0; i < row.Count; i++)
                    newRow[i] = row[i];
                TableCSV.Rows.Add(newRow);
            }
            catch (Exception)
            {
                MatchCSV = false;
                // throw;
            }

        }

        // Сплитим строку
        private List<string> MappingRow(string str)
        {
            return new List<string>(str.Split(ParamParser.stringSeparators, StringSplitOptions.None));
        }

        #endregion

    }
}
