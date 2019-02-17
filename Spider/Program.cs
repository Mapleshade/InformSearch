using System.IO;
using System.Reflection;
using HtmlAgilityPack;

namespace Spider
{
    class Program
    {
        public static void Main(string[] args)
        {
            XPath();
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

                    SaveIntoFile(namePost, text);
                }
            }
        }

        private static HtmlDocument GetHtmlDocument(string url)
        {
            var web = new HtmlWeb();
            return web.Load(url);
        }

        private static void SaveIntoFile(string namePost, string textPost)
        {
            var fullPath = "C:\\Users\\Elina\\RiderProjects\\IS\\Spider\\Spider\\outputs\\";
            if (namePost.EndsWith("."))
            {
                fullPath += namePost + "txt";
            }
            else
            {
                fullPath += namePost + ".txt";
            }
            File.WriteAllText(fullPath, textPost);
        }
    }
}