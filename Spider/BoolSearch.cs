using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Spider
{
    public class BoolSearch
    {
        public static string pathForPorterFiles =
            "C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\porter\\";

        public void Search(string request)
        {
            var porter = new Porter();

            var textToProcess = request;
            var textAfterProcess = "";
            var startIndex = 0;
            var count = 0;
            List<string> words = new List<string>();

            for (var index = 0; index < textToProcess.Length; index++)
            {
                var symbol = textToProcess[index];

                if (symbol == ' ' || symbol == '!' || symbol == '?' || symbol == '.' || symbol == ',' ||
                    symbol == '(' || symbol == ')' || symbol == ':' || symbol == ';' || symbol == '_')
                {
                    var endIndex = index;
                    var word = "";
                    var temp = string.Copy(textToProcess);
                    var len = endIndex - startIndex;

                    if (len > 0)
                    {
                        count++;
                        word += temp.Substring(startIndex, len);
                        word = word.ToLower();

                        word = !porter.particles.Contains(word) ? porter.Stemm(word) : "";
                        textAfterProcess += word + " ";
                        startIndex = endIndex + 1;

                        if (!string.IsNullOrEmpty(word))
                        {
                            words.Add(word);
                        }
                    }
                }
            }

            Dictionary<string, int> results = new Dictionary<string, int>();

            DirectoryInfo dir = new DirectoryInfo(pathForPorterFiles);
            foreach (var item in dir.GetFiles())
            {
                int countWords = 0;
                var text = File.ReadAllText(pathForPorterFiles + item.Name);
                foreach (var word in words)
                {
                    if (text.Contains(word))
                    {
                        countWords++;
                    }
                }
                results.Add(item.Name, countWords);
            }


            Console.WriteLine("Булевый поиск: для запроса \"" + request + "\" составлен список совпадений: ");
            foreach (var result in results)
            {
                Console.WriteLine("в документе \"" + result.Key + "\" совпало слов из запроса: " +
                                  result.Value);
            }
        }
    }
}