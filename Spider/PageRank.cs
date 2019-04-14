using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using HtmlAgilityPack;

namespace Spider
{
    public class PageRank
    {
        Dictionary<string, int> IndexesOfLinks = new Dictionary<string, int>();

        private static string pathForPageRankFiles =
            "C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\rankFiles\\";

        public void DoPageRank()
        {
            var doc = Program.GetHtmlDocument("https://mototraveller.ru");

            //считаем все ссылки на странице
            var rows = doc.DocumentNode.SelectNodes("//div[@id]");
            int number = 0;
            for (var index = 0; index < rows.Count; index++)
            {
                HtmlNode row = rows[index];
                if (row.Id.Contains("post-"))
                {
                    var link = doc.DocumentNode.SelectSingleNode(
                        "//div[@id='" + row.Id + "']/h2[@class='entry-title']/a");
                    var namePost = link.InnerText;
                    var pref = link.GetAttributeValue("href", "default");
                    var postPage = Program.GetHtmlDocument(pref);
                    var links = postPage.DocumentNode.SelectNodes(
                        "//a ");
                    IndexesOfLinks.Add(namePost, number);
                    number++;
                }
            }


            var inc = new int[IndexesOfLinks.Count, IndexesOfLinks.Count];

            foreach (HtmlNode row in doc.DocumentNode.SelectNodes("//div[@id]"))
            {
                if (row.Id.Contains("post-"))
                {
                    var link = doc.DocumentNode.SelectSingleNode(
                        "//div[@id='" + row.Id + "']/h2[@class='entry-title']/a");
                    var namePost = link.InnerText;
                    var pref = link.GetAttributeValue("href", "default");
                    var postPage = Program.GetHtmlDocument(pref);
                    var links = postPage.DocumentNode.SelectNodes(
                        "//a ");
                    foreach (var link1 in links)
                    {
                        if (IndexesOfLinks.Keys.Contains(link1.InnerText) && namePost != link1.InnerText)
                        {
                            inc[IndexesOfLinks[namePost], IndexesOfLinks[link1.InnerText]] = 1;
                        }
                    }
                }
            }

            var currentIterPR = new Dictionary<int, double>();
            var previousIterPr = new Dictionary<int, double>();

            for (int i = 0; i < IndexesOfLinks.Count; i++)
            {
                currentIterPR.Add(i, 1 / (double) IndexesOfLinks.Count);
                previousIterPr.Add(i, 1 / (double) IndexesOfLinks.Count);
            }

            for (int iter = 0; iter < 10; iter++)
            {
                for (int j = 0; j < IndexesOfLinks.Count; j++)
                {
                    currentIterPR[j] = 0;
                    for (int i = 0; i < IndexesOfLinks.Count; i++)
                    {
                        if (inc[i, j] == 1)
                        {
                            int count = 0;
                            for (int k = 0; k < IndexesOfLinks.Count; k++)
                            {
                                count += inc[i, k];
                            }

                            currentIterPR[j] += previousIterPr[i] / count;
                        }
                    }
                }

                previousIterPr = currentIterPR;
            }

            var str = "";
            foreach (var d in IndexesOfLinks)
            {
                str += "PageRank страницы \"" + d.Key + "\" равен: " + currentIterPR[d.Value] + "\n";
                File.WriteAllText(pathForPageRankFiles + "PageRank.txt", str);
            }

            for (var index0 = 0; index0 < inc.GetLength(0); index0++)
            {
                for (var index1 = 0; index1 < inc.GetLength(1); index1++)
                {
                    var i = inc[index0, index1];
                    Console.Write(i + " ");
                }

                Console.WriteLine();
            }
        }
    }
}