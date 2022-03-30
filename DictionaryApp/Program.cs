using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DictionaryApp
{
    class Program
    {
        [STAThread]

        static void Main(string[] args)
        {
            GetDataFromDB getData = new GetDataFromDB();
            getData.InitConnection();

            string command = " ";

            if (args.Count() > 0)
            {
                switch (args[0])
                {
                    case "создание":
                        getData.CreateDictionary();
                        Console.WriteLine("создание словаря проведено успешно");
                        break;

                    case "обновление":
                        getData.UpdateDictionary();
                        Console.WriteLine("дополнение словаря проведено успешно");
                        break;

                    case "очистить":
                        getData.ClearDictionary();
                        Console.WriteLine("очистка словаря проведена успешно");
                        break;

                    default:
                        break;
                }
            }
            else
            {
                while (command != "")
                {
                    //Console.WriteLine("Введите команду:");
                    command = Console.ReadLine();

                    List<string> words = getData.AdditWordFromDictionary(command);
                    foreach (string word in words)
                    {
                        Console.WriteLine(word);
                    }

                }
            }  
        }
    }
}
