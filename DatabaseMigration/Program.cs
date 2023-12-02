using System.Data;
using System.Data.SQLite;
using System.Text;

namespace DatabaseMigration
{
    internal class Program
    {
        protected static SQLiteConnection Sqlite_conn;

        static void Main()
        {
            if (!Directory.Exists("Export"))
            {
                Directory.CreateDirectory("Export");
            }
            Console.WriteLine("Give the location for the sqlite database file:");
            string route = Console.ReadLine();
            if (File.Exists(route))
            {
                Sqlite_conn = new($"Data Source={route}; Version = 3; New = True; Compress = True; ");

                Sqlite_conn.Open();

                Sqlite_conn.StateChange += new StateChangeEventHandler(OpenConnection);

                Console.WriteLine("Do you want to export the tables into txts? (y/n)");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    string[] queries = File.ReadAllLines("sql\\base_requests.txt");

                    foreach (string query in queries)
                    {
                        try
                        {
                            DBManagement(query, "txt");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                else
                {
                    string[] queries = File.ReadAllLines("sql\\migration.txt").Where(x => !x.StartsWith("--")).ToArray();

                    string[] tableNames = File.ReadAllLines("sql\\tables.txt").Where(x => !x.StartsWith("--")).ToArray();

                    for (int i = 0; i < queries.Length; i++)
                    {
                        try
                        {
                            DBManagement(queries[i], "sql", tableNames[i]);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }
        }

        public static void DBManagement(string query, string format, string tableName = null)
        {
            DataTable result = ManualDBManagement(query);

            DataTable table = result;

            int maxwidth = table.Columns.Count;
            string text = "";

            foreach (DataRow row in table.Rows)
            {
                text += $"INSERT INTO {tableName} VALUES ('";
                for (int i = 0; i < maxwidth; i++)
                {
                    text += row[i].ToString();
                    if (i + 1 != maxwidth)
                    {
                        text += "\', \'";
                    }
                }
                text += "');\n";
            }
            text = text.Replace("'NULL'", "NULL");
            StreamWriter writer = new($"Export\\{tableName ?? table.TableName}.{format}", false, Encoding.UTF8);
            writer.WriteLine(text);
            writer.Close();
        }

        protected static DataTable ManualDBManagement(string query)
        {
            DataTable table = new();

            SQLiteCommand sqlite_cmd;

            sqlite_cmd = Sqlite_conn.CreateCommand();

            sqlite_cmd.CommandText = query;

            SQLiteDataReader reader = sqlite_cmd.ExecuteReader();

            table.Load(reader);

            return table;
        }
        protected static void OpenConnection(object sender, StateChangeEventArgs e)
        {
            if (Sqlite_conn.State == ConnectionState.Closed)
            {
                Sqlite_conn.Open();
            }
        }
    }
}
