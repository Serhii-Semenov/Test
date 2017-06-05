using Logger;
using ParsingCSV.Mapping;
using ParsingCSV.Parsing;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;

namespace ParsingCSV.Provider
{
    public class ProviderDb
    {
        const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Base\Documents\test\maptool\Junior.mdf;Integrated Security=True;Connect Timeout=30";

        public static ProviderDb Instance { get { return instance; } }
        private static readonly ProviderDb instance = new ProviderDb();
        private ProviderDb() { }

        private static ILogger Log = FileLogger.Instance;

        static ProviderDb()
        {
            FileLogger.Instance.Initialize("log.txt");
        }

        private SqlConnection Connect()
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public bool CreateTable()
        {
            bool b = true;
            b = ExecuteQuery(GetQueryToCreateColumns(Parser.Instance.ColMap, GetTableName()));
            int rowsCount = Parser.Instance.TableCSV.Rows.Count;
            bool flag = false;
            int i = 0, s = 500;
            int j = (i + s) > rowsCount ? rowsCount : i + s;
            for (;;)
            {
                b = ExecuteQuery(GetQueryToFillTable(i,j));
                if (flag || j == rowsCount) break;
                i = j;
                j += s;
                if (j > rowsCount)
                {
                    j = rowsCount;
                    flag = true;
                    // throw
                }
            }
            return b;
        }

        private string GetTableName()
        {
            return Parser.Instance.GetTableNameRelativelyFile();
        }

        private string GetQueryToCreateColumns(ObservableCollection<ColmnMapping> tableSettings, string tableName)
        {
            CheckedTableExistToDelete(tableName);
            string query = string.Format(
                    "CREATE TABLE [dbo].[{0}]"
                    , tableName);
            query += "\n(\n";
            query += "\t[Id] INT NOT NULL PRIMARY KEY IDENTITY(1, 1),\n";
            foreach (var c in tableSettings)
            {
                if (c.Parameter.LoadToDb != true) continue;
                query += "\t";
                query += string.Format(
                    "[{0}] {1}"
                    , c.ColmnName, c.Parameter.queryAdditive);
                query += "\n";
            }
            query += ") ";
            return query;
        }

        private string GetQueryToFillTable(int begin, int end)
        {
            var dt = Parser.Instance.TableCSV;
            var colMap = Parser.Instance.ColMap;
            string tableName = GetTableName();
            bool flag = false;

            string query = string.Format(
                    @"INSERT INTO dbo.{0} (",
                    tableName);
            foreach (var c in colMap)
            {
                if (c.Parameter.LoadToDb != true) continue;
                if (flag) query += ", ";
                flag = true;
                query += c.ColmnName;
            }
            query += ") VALUES\n";
            for (int k = begin; k < end; k++)
            {
                flag = false;
                query += "(";
                for (int i = 0; i < colMap.Count; i++)
                {
                    if (colMap[i].Parameter.LoadToDb != true) continue;
                    if (flag) query += ", ";
                    flag = true;
                    if (colMap[i].Parameter.Id == 3 || colMap[i].Parameter.Id == 4)
                    {
                        query += dt.Rows[k][i].ToString();
                    }
                    else
                    {
                        query += string.Format("'{0}'", dt.Rows[k][i].ToString());
                    }
                }
                query += "),\n";
            }
            return query.Substring(0, query.Length - 2);
        }

        private bool ExecuteQuery(string query)
        {
            bool b = true;
            using (var con = Connect())
            {
                var cmd = new SqlCommand(query, con);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception err)
                {
                    Log.Error(err.Message);
                    b = false;
                    // throw;
                }
            }
            return b;
        }

        private void FillTable()
        {
            var tableName = GetTableName();
            var table = Parser.Instance.TableCSV;
            var tableSettings = Parser.Instance.ColMap;
            string query = string.Format(
                    "CREATE TABLE [dbo].[{0}]"
                    , tableName);
            query += "\n(\n";
            query += "\t[Id] INT NOT NULL PRIMARY KEY IDENTITY(1, 1),\n";
            foreach (var c in tableSettings)
            {
                if (c.Parameter.LoadToDb != true) continue;
                query += "\t";
                query += string.Format(
                    "[{0}] {1}"
                    , c.ColmnName, c.Parameter.queryAdditive);
                query += "\n";
            }
            query += ") ";

        }

        public void CheckedTableExistToDelete(string tableName)
        {
            string query = string.Format(
                @"IF OBJECT_ID(N'dbo.{0}', N'U') IS NOT NULL
                DROP TABLE dbo.{0};", tableName);
            query += "\n";
            using (var con = Connect())
            {
                var cmd = new SqlCommand(query, con);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception err)
                {
                    Log.Error(err.Message);
                    throw;
                }
            }
        }

        public DataTable Pagination(string tbName)
        {
            DataTable dt = new DataTable();
            SqlDataReader reader = null;
            string query = string.Format("SELECT * FROM {0}", tbName);

            using (var con = Connect())
            {
                var cmd = new SqlCommand(query, con);
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows) // если есть данные
                    {
                        // выводим названия столбцов
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            dt.Columns.Add(reader.GetName(i));
                        }

                        while (reader.Read()) // построчно считываем данные
                        {
                            var nr = dt.NewRow();
                            int i = 0;
                            foreach(var v in dt.Columns)
                            {
                                nr[v.ToString()] = reader.GetValue(i++);
                            }
                            dt.Rows.Add(nr);
                        }
                    }
                }
                catch (Exception err)
                {
                    Log.Error(err.Message);
                    throw;
                }
                finally
                {
                    reader.Close();
                }
            }
            return dt;
        }
    }
}