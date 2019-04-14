using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using HtmlAgilityPack;

namespace Spider
{
    class Program
    {
        private static List<string> initialFileNames;

        private static HashSet<string> words;

        private static string pathForInitialFiles =
            "C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\original\\";

        public static string pathForPorterFiles =
            "C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\porter\\";

        private static string pathForInvertIndexFiles =
            "C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\invertIndex\\";


        private static Dictionary<string, HashSet<string>> invertIndexes;

        private static Dictionary<string, int> countsWordsInDocuments;

        private static void Main(string[] args)
        {
            initialFileNames = new List<string>();
            words = new HashSet<string>();
            invertIndexes = new Dictionary<string, HashSet<string>>();
            countsWordsInDocuments = new Dictionary<string, int>();

//            XPath();
//            CopyFiles(pathForInitialFiles, pathForPorterFiles);
//            DoPorter();
//            DoTFIDF();
//            DoVecSearch();
              //VectorSearch("мотоцикл зимой ");
             // Console.WriteLine();
             // BoolSearch("мотоцикл зимой ");
             PageRank rank = new PageRank();
             rank.DoPageRank();
        }

        private static void XPath()
        {
            var doc = GetHtmlDocument("https://mototraveller.ru");

            foreach (HtmlNode row in doc.DocumentNode.SelectNodes("//div[@id]"))
            {
                if (row.Id.Contains("post-"))
                {
                    var link = doc.DocumentNode.SelectSingleNode(
                        "//div[@id='" + row.Id + "']/h2[@class='entry-title']/a");
                    var namePost = link.InnerText;
                    var pref = link.GetAttributeValue("href", "default");
                    var postPage = GetHtmlDocument(pref);
                    var text = "";
                    foreach (var selectNode in postPage.DocumentNode.SelectNodes(
                        "//div[@id='" + row.Id + "']//p[@style='text-align: justify;']/em/text() | //div[@id='" +
                        row.Id + "']//p[@style='text-align: justify;']/text() | //div[@id='" + row.Id +
                        "']//p[@style='text-align: justify;']/a/text() | //div[@id='" + row.Id +
                        "']//p[@style='text-align: justify;']/strong/text()| //div[@id='" + row.Id +
                        "']//p[not(@*)]/text() | //div[@id='" + row.Id + "']//p[not(@*)]/a/text()"))
                    {
                        var str = selectNode.InnerText;
                        str.Replace("\xa0", " ");
                        str.Replace("ðŸ™‚", " ");

                        if (str.Trim().Length == 0)
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(text))
                        {
                            text += str;
                        }
                        else if (text.EndsWith(".") || text.EndsWith("?") || text.EndsWith("!") ||
                                 text.EndsWith("â€¦"))
                        {
                            text += "\n" + str;
                        }
                        else
                        {
                            text += " " + str;
                        }
                    }

                    SaveIntoFile(namePost, text, pathForInitialFiles);
                }
            }
        }

        public static HtmlDocument GetHtmlDocument(string url)
        {
            var web = new HtmlWeb();
            return web.Load(url);
        }

        private static void SaveIntoFile(string namePost, string textPost, string path)
        {
            var fullPath = path;
            if (namePost.EndsWith("."))
            {
                fullPath += namePost + "txt";
                if (path == pathForInitialFiles)
                {
                    initialFileNames.Add(namePost + "txt");
                }
            }
            else
            {
                fullPath += namePost + ".txt";
                if (path == pathForInitialFiles)
                {
                    initialFileNames.Add(namePost + ".txt");
                }
            }

            File.WriteAllText(fullPath, textPost);
        }

        private static void CopyFiles(string sourcePath, string destinationPath)
        {
            foreach (var fileName in initialFileNames)
            {
                File.Copy(sourcePath + fileName, destinationPath + fileName, true);
            }
        }

        public static void DoPorter() //todo: не помешает добавить чистку от лишних пробелов, а то лагает чутка
        {
            var porter = new Porter();

            foreach (var fileName in initialFileNames)
            {
                var textToProcess = File.ReadAllText(pathForPorterFiles + fileName);
                var textAfterProcess = "";
                var startIndex = 0;
                var count = 0;

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
                                if (invertIndexes.Keys.Contains(word))
                                {
                                    invertIndexes[word].Add(fileName);
                                }
                                else
                                {
                                    var linksForWord = new HashSet<string>
                                        {fileName}; //todo: нужна ссылка или имя фйала?
                                    invertIndexes.Add(word, linksForWord);
                                }
                            }
                        }
                    }
                }

                countsWordsInDocuments.Add(fileName, count);
                File.WriteAllText(pathForPorterFiles + fileName, textAfterProcess);
                WriteInverIndexesInFile();
            }
        }


        private static void WriteInverIndexesInFile()
        {
            var str = "";
            foreach (var invertIndexesKey in invertIndexes.Keys)
            {
                str += invertIndexesKey + ":\n";
                str += "Встречается в файлах:" + "\n";
                str = invertIndexes[invertIndexesKey].Aggregate(str, (current, link) => current + (link + "\n"));
                str += "\n";
            }

            File.WriteAllText(pathForInvertIndexFiles + "inverted Indexes.txt", str);
        }

        private static void DoTFIDF()
        {
            var tfidf = new TFIDF();
            tfidf.Init(invertIndexes, countsWordsInDocuments);
        }

        private static void DoVecSearch()
        {
            var search = new VecSearch();
            search.Init(invertIndexes, initialFileNames, TFIDF.TF, TFIDF.IDF);
        }

        private static void VectorSearch(string request)
        {
            var search = new VecSearch();
            search.Search(request);
        }
        
        private static void BoolSearch(string request)
        {
            var search = new BoolSearch();
            search.Search(request);
        }
    }
}