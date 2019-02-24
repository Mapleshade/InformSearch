using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HtmlAgilityPack;

namespace Spider
{
    class Program
    {
        private static List<string> initialFileNames;

        private static string pathForInitialFiles =
            "C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\original\\";

        private static string pathForPorterFiles =
            "C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\porter\\";

        static void Main(string[] args)
        {
            initialFileNames = new List<string>();
            XPath();
            CopyFiles(pathForInitialFiles, pathForPorterFiles);
            DoPorter();
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

        private static HtmlDocument GetHtmlDocument(string url)
        {
            var web = new HtmlWeb();
            return web.Load(url);
        }

        private static void SaveIntoFile(string namePost, string textPost, string path)
        {
            //   var fullPath = "C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\original\\";
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

        private static void DoPorter()
        {
            var porter = new Porter();
            

            foreach (var fileName in initialFileNames)
            {
                var textToProcess = File.ReadAllText(pathForPorterFiles + fileName);
                var textAfterProcess = "";
                var startIndex = 0;
                for (var index = 0; index < textToProcess.Length; index++)
                {
                    var symbol = textToProcess[index];

                    if (symbol == ' ' || symbol == '!' || symbol == '?' || symbol == '.' || symbol == ',')
                    {
                        var endIndex = index;
                        var word = "";
                        var temp = string.Copy(textToProcess);
                        var len = endIndex - startIndex;

                        if (len > 0)
                        {
                            word += temp.Substring(startIndex, len);
                            Console.WriteLine(word);
                            if (!porter.particles.Contains(word))
                            {
                                Console.WriteLine(word + " in if");
                                word = porter.Stemm(word);
                            }
                            textAfterProcess += word + " ";
                            startIndex = endIndex + 1;
                        }
                    }
                }

                File.WriteAllText(pathForPorterFiles + fileName, textAfterProcess);
            }
        }
    }
}