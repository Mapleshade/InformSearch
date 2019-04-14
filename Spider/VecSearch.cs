using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;

namespace Spider
{
    public class VecSearch
    {
        private double[,] vectors;

        private static string pathForVectorSearchFiles =
            "C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\vectorSearch\\";

        public void Init(Dictionary<string, HashSet<string>> invertIndexes, List<string> initialFileNames,
            IDictionary<string, List<double>> TF, Dictionary<string, double> IDF)
        {
            vectors = new double[initialFileNames.Count, TF.Keys.Count];

            for (var indexOfDoc = 0; indexOfDoc < initialFileNames.Count; indexOfDoc++)
            {
                var initialFileName = initialFileNames[indexOfDoc];

                for (var indexOfWord = 0; indexOfWord < invertIndexes.Keys.Count; indexOfWord++)
                {
                    var word = invertIndexes.Keys.ToList()[indexOfWord];
                    var indexDocWithThisWord = 0;
                    foreach (var document in invertIndexes[word])
                    {
                        if (initialFileName == document)
                        {
                            vectors[indexOfDoc, indexOfWord] = TF[word][indexDocWithThisWord] * IDF[word];
                            indexDocWithThisWord++;
                        }
                    }
                }
            }

            WriteToFile(initialFileNames);
        }

        private void WriteToFile(List<string> initialFileNames)
        {
            var str = "";
            foreach (var document in initialFileNames)
            {
                str += document + ":\n";
                foreach (var i in Enumerable.Range(0, vectors.GetLength(0)))
                {
                    foreach (var j in Enumerable.Range(0, vectors.GetLength(1)))
                    {
                        str += "vector: " + "\n";
                        str += vectors[i, j] + " - " + "в документе №" + i + "\n";
                    }

                    str += "\n";
                }
            }

            File.WriteAllText(pathForVectorSearchFiles + "vectorSearch.txt", str);
        }

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
            
            var file = File.ReadLines("C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\TFIDF\\TFIDF.txt");
            Dictionary<string, double> terms = new Dictionary<string, double>();

            int i = 0;
            string w = "";
            bool readNext = false;
            foreach (var line in file)
            {
                if (i == 0 || readNext && line.Length>1)
                {
                    w = line.Substring(0, line.Length - 1);
                    readNext = false;
                }

                string num = "";
                if (line.StartsWith("IDF:"))
                {
                    readNext = true;
                 num  =  line.Substring(5);
                 var idf = double.Parse(num);
                 
                 if(!terms.ContainsKey(w))
                 terms.Add(w, idf);
                 else
                 {
                     terms.Add(w + " ", idf);
                 }
                }

                i++;
            }
            
            List<double> vectorRequest = new List<double>();
            foreach (var term in terms)
            {
                if (words.Contains(term.Key))
                {
                    vectorRequest.Add(term.Value);
                }
                else
                {
                    vectorRequest.Add(0);
                }
            }

            int j = 0;
            bool readN = false;
            var vectorsFile = File.ReadLines("C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\vectorSearch\\vectorSearch.txt");
            Dictionary<string, List<double>> vectorsInDocuments = new Dictionary<string, List<double>>();
            List<double> tempVector = new List<double>();
            string tempLine = "";
            foreach (var line in vectorsFile)
            {
                if (line.Contains("vector:"))
                {
                    readN = true;
                    continue;
                }

                if (readN)
                {
                    readN = false;
                    if (line.EndsWith(j.ToString()))
                    {
                        var num = double.Parse(line.Substring(0, line.IndexOf("-") - 1));
                        tempVector.Add(num);
                    }
                }

                if (line.Contains(".txt"))
                {
                    vectorsInDocuments.Add(tempLine, tempVector);
                    tempVector = new List<double>();
                    tempLine = line.Substring(0, line.Length - 5);
                    j++;
                }
            }

            Dictionary<string, double> results = new Dictionary<string, double>();
            foreach (var vector in vectorsInDocuments)
            {
                var result = 0d;
                var len1 = 0d;
                var len2 = 0d;
                
                for (int k = 0; k < vector.Value.Count; k++)
                {
                    result += vector.Value[k] * vectorRequest[k];
                    len1 += vector.Value[k] * vector.Value[k];
                    len2 += vectorRequest[k];
                }

                result /= Math.Sqrt(len1);
                result /= Math.Sqrt(len2);
                results.Add(vector.Key, result);
                
            }

            Console.WriteLine("для запроса \"" + request +  "\" составлен список совпадений: ");
            foreach (var result in results)
            {
                Console.WriteLine("вероятность найдти информацию по запросу в документе \"" + result.Key + "\" равна: " + result.Value * 100);
                
            }
        }
        
        
    }
}