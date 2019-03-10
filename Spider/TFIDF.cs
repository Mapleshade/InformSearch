using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Spider
{
    public class TFIDF
    {
        private IDictionary<string, IEnumerable<int>> counts;
        private IDictionary<string, List<double>> TF;
        private Dictionary<string, double> IDF;

        private static string pathForTFIDFIndexFiles =
            "C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\TFIDF\\";

        /// <summary>
        ///     Инициализировать сервис
        /// </summary>
        public void Init(Dictionary<string, IEnumerable<string>> invertIndexes,
            Dictionary<string, int> countsWordsInDocuments)
        {
            counts = new Dictionary<string, IEnumerable<int>>();
            TF = new Dictionary<string, List<double>>();
            IDF = new Dictionary<string, double>();
            foreach (var word in invertIndexes.Keys)
            {
                CountWordInDocuments(word, invertIndexes[word]);
                var TFs = new List<double>();
                for (var i = 0; i < invertIndexes[word].Count(); i++)
                {
                    TFs.Add(FindTFInOneDocument(word, i, countsWordsInDocuments[invertIndexes[word].ToArray()[i]]));
                }

                TF.Add(word, TFs);
                IDF.Add(word, FindIDF(word, countsWordsInDocuments.Count, invertIndexes[word].Count()));
            }

            WriteToFile();
        }

        /// <summary>
        /// посчитать все употребления слова в документе
        /// </summary>
        /// <param name="word">слово</param>
        /// <param name="document">исходный документ</param>
        /// <returns>количество употреблений слова в документе</returns>
        private int CountWordInDocument(string word, string document)
        {
            //счетчик 
            var count = 0;
            var startIndex = 0;
            var porter = new Porter();

            //для всех символов в тексте
            for (var index = 0; index < document.Length; index++)
            {
                var symbol = document[index];

                if (symbol == ' ' || symbol == '!' || symbol == '?' || symbol == '.' || symbol == ',' ||
                    symbol == '(' || symbol == ')' || symbol == ':' || symbol == ';' || symbol == '_')
                {
                    var endIndex = index;
                    var wordFromText = "";
                    var temp = string.Copy(document);
                    var len = endIndex - startIndex;

                    if (len > 0)
                    {
                        wordFromText += temp.Substring(startIndex, len);
                        wordFromText = wordFromText.ToLower();

                        wordFromText = !porter.particles.Contains(wordFromText) ? porter.Stemm(wordFromText) : "";
                        startIndex = endIndex + 1;

                        if (wordFromText == word)
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// посчитать вхождения слова в каждом документе
        /// </summary>
        /// <param name="word">слово</param>
        /// <param name="links">документы, содержащие это слово</param>
        private void CountWordInDocuments(string word, IEnumerable<string> links)
        {
            var countsLocal = links.Select(link => File.ReadAllText(Program.pathForPorterFiles + link))
                .Select(document => CountWordInDocument(word, document)).ToList();

            counts.Add(word, countsLocal);
        }

        /// <summary>
        ///     посчитать TF слова в одном документе
        /// </summary>
        /// <param name="word">слово</param>
        /// <param name="idDocument">айдишник документа, в котором встречается слово</param>
        /// <param name="countAllWordsInDocument">всего слов в документе</param>
        /// <returns>частота слова в документе</returns>
        private double FindTFInOneDocument(string word, int idDocument, int countAllWordsInDocument)
        {
            return counts[word].ToArray()[idDocument] / countAllWordsInDocument;
        }

        /// <summary>
        ///     посчитать IDF слова в документах
        /// </summary>
        /// <param name="word">слово</param>
        /// <param name="countAllDocuments">количество всех документов</param>
        /// <param name="countDocumentsWithWord">количество документов, в которых встречается слово</param>
        /// <returns></returns>
        private double FindIDF(string word, int countAllDocuments, int countDocumentsWithWord)
        {
            return Math.Log10(countAllDocuments / countDocumentsWithWord);
        }

        private void WriteToFile()
        {
            var str = "";
            foreach (var word in TF.Keys)
            {
                str += word + ":\n";
                str += "TF: " + "\n";
                for (var i = 0; i < TF[word].Count; i++)
                {
                    str += TF[word][i] + " - " + "в документе №" + i;
                }

                str += "\n";
                str += "IDF: " + IDF[word];
                str += "\n";
            }

            File.WriteAllText(pathForTFIDFIndexFiles + "TFIDF.txt", str);
        }
    }
}