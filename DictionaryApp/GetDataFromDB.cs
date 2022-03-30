using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;

namespace DictionaryApp
{
    /// <summary>
    /// Класс для взаимодействия с базой данных
    /// </summary>
    public partial class GetDataFromDB
    {
        SqlConnection sqlConnection;
        SqlCommand cmd;

        /// <summary>
        /// Структура для создания словаря
        /// </summary>
        public struct dictionary
        {
            public List<string> words;
            public List<int> frequency;
        }

        /// <summary>
        /// Инициализировать подключение
        /// </summary>
        public void InitConnection()
        {
            DataTable tableRes = new DataTable();

            sqlConnection = new SqlConnection("Data Source=localhost; Integrated Security=True;User ID=client;Password=client");
            sqlConnection.Open();

            // Создать объект Command.
            cmd = new SqlCommand();
            cmd.Connection = sqlConnection;
        }

        public static string setQueryDictionary(dictionary dictionary)
        {
            string strRes;
            strRes = @"INSERT INTO [dictionary].[dbo].[words] ([name] ,[freq])  VALUES ";

            for (int i=0; i<dictionary.words.Count(); i++)
            {
                strRes = strRes + "("+ "'" + dictionary.words[i] + "'" + "," + dictionary.frequency[i] + "),";
            }
            strRes = strRes.Remove(strRes.Length - 1, 1);
            return strRes;
        }

        public static string updateQueryDictionary(dictionary dictionary)
        {
            string strRes;
            strRes = @"CREATE TABLE #temporary
                     (name NVARCHAR(15),
                     freq INT)";

            strRes += @"INSERT INTO #temporary VALUES ";
            for (int i = 0; i < dictionary.words.Count(); i++)
            {
                strRes = strRes + "(" + "'" + dictionary.words[i] + "'" + "," + dictionary.frequency[i] + "),";
            }
            strRes = strRes.Remove(strRes.Length - 1, 1);

            strRes += @"MERGE[dictionary].[dbo].[words] AS target
                        USING(SELECT * FROM #temporary) AS source 
                        ON(target.[name] = source.[name])
                        WHEN MATCHED
                        THEN UPDATE SET target.freq = target.freq + source.freq
                        WHEN NOT MATCHED
                        THEN INSERT VALUES(source.name, source.freq);";

            return strRes;
        }

        /// <summary>
        /// Создание словаря
        /// </summary>
        public void CreateDictionary()
        {
            dictionary dictionary = new dictionary();
            dictionary.words = new List<string>();
            dictionary.frequency = new List<int>();

            string path = @"c:\temp\MyTest.txt";

            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    path = openFileDialog.FileName;
                }
            }

            string[] readText = File.ReadAllLines(path);
            foreach (string s in readText)
            {
                string[] subs = s.Split(' ');
                foreach (var sub in subs)
                {
                    int index = dictionary.words.LastIndexOf(sub);
                    if (index == -1)
                    {
                        dictionary.words.Add(sub);
                        dictionary.frequency.Add(1);
                    }
                    else
                    {
                        dictionary.frequency[index]++;
                    }
                }
            }

            for (int i = dictionary.words.Count() - 1 ; i >= 0; i--)
            {
                if (dictionary.frequency[i] < 3 || dictionary.words[i].Count() < 3)
                {
                    dictionary.words.RemoveAt(i);
                    dictionary.frequency.RemoveAt(i);
                }
            }

            string query = setQueryDictionary(dictionary);
            cmd.CommandText = query;

            int rowCount = cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Очистка словаря
        /// </summary>
        public void ClearDictionary()
        {
            string query = @"DELETE FROM [dictionary].[dbo].[words]";
            cmd.CommandText = query;

            int rowCount = cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Автодополнения
        /// </summary>
        public List<string> AdditWordFromDictionary(string abc)
        {
            List<string> list = new List<string>();
            string query = @"SELECT TOP (5) [name] FROM [dictionary].[dbo].[words]
            WHERE [name] LIKE " + "'" + abc + "%" + "' " + "ORDER BY [freq] DESC";
            cmd.CommandText = query;

            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string row = (string)reader.GetValue(0);

                        list.Add(row);
                    }
                }
                reader.Close();
            }

            return list;
        }

        /// <summary>
        /// Дополнение словаря
        /// </summary>
        public void UpdateDictionary()
        {
            dictionary dictionary = new dictionary();
            dictionary.words = new List<string>();
            dictionary.frequency = new List<int>();

            string path = @"c:\temp\MyTest.txt"; // файл по умолчанию

            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    path = openFileDialog.FileName;
                }
            }

            string[] readText = File.ReadAllLines(path);
            foreach (string s in readText)
            {
                string[] subs = s.Split(' ');
                foreach (var sub in subs)
                {
                    int index = dictionary.words.LastIndexOf(sub);
                    if (index == -1)
                    {
                        dictionary.words.Add(sub);
                        dictionary.frequency.Add(1);
                    }
                    else
                    {
                        dictionary.frequency[index]++;
                    }
                }
            }

            for (int i = dictionary.words.Count() - 1; i >= 0; i--)
            {
                if (dictionary.frequency[i] < 3 || dictionary.words[i].Count() < 3 || dictionary.words[i].Count() > 15)
                {
                    dictionary.words.RemoveAt(i);
                    dictionary.frequency.RemoveAt(i);
                }
            }

            string query = updateQueryDictionary(dictionary);
            cmd.CommandText = query;

            int rowCount = cmd.ExecuteNonQuery();
        }

    }


}
