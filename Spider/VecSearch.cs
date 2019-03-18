using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace Spider
{
    public class VecSearch
    {
        private double[,] vectors;
        
        private static string pathForVectorSearchFiles =
            "C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\vectorSearch\\";

        public void Init(Dictionary<string, HashSet<string>> invertIndexes, List<string> initialFileNames, IDictionary<string, List<double>> TF, Dictionary<string, double> IDF)
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
                        str += vectors[i,j] + " - " + "в документе №" + i + "\n";
                    }
                    str += "\n";
                }
            }

            File.WriteAllText(pathForVectorSearchFiles + "vectorSearch.txt", str);
        }
    }
}