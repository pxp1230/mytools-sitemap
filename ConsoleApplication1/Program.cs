using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConsoleApplication1
{
    class Program
    {
        static string host = "";
        static string date = "";
        static void Main(string[] args)
        {
            DirectoryInfo curDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            if (curDirectory.Exists)
            {
                host = "https://" + curDirectory.Name;
                date = DateTime.Now.ToString("yyyy-MM-dd");
                FileInfo sitemapTXT = new FileInfo(Path.Combine(curDirectory.FullName, "sitemap.xml"));
                StreamWriter writer = sitemapTXT.CreateText();
                writer.WriteLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"" xmlns:image=""http://www.google.com/schemas/sitemap-image/1.1"" xmlns:xhtml=""http://www.w3.org/1999/xhtml"">");
                writer.WriteLine(Wrap(host + "/index.html"));
                writer.WriteLine(Wrap(host + "/README.html"));
                Write(curDirectory, curDirectory, writer);//递归调用
                writer.WriteLine(@"</urlset>");
                writer.Close();
            }
            else
            {
                Console.WriteLine("无法生成sitemap.txt，请将sitemap.exe放在网站所在目录！");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
            Exit();
        }
        class _TableEle
        {
            public string date;
            public string file;
        }
        private static void Write(DirectoryInfo rootDirectory, DirectoryInfo curDirectory, StreamWriter writer)
        {
            FileInfo indexFile = new FileInfo(Path.Combine(curDirectory.FullName, "index.html"));
            string relativePath = curDirectory.FullName.Substring(rootDirectory.FullName.Length).Replace('\\', '/');
            if (indexFile.Exists)
            {
                StreamReader sr = new StreamReader(indexFile.FullName, Encoding.UTF8);
                string str = sr.ReadToEnd();
                sr.Close();
                int from = str.IndexOf("<body>"), to = str.IndexOf("</body>") + 7;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(str.Substring(from, to - from));
                XmlNodeList x = xmlDoc.DocumentElement.SelectNodes("//tr");
                foreach (XmlNode y in x)
                {
                    string date = y.ChildNodes[0].InnerText;
                    string file = y.ChildNodes[1].SelectSingleNode("a").Attributes["href"].Value;
                    writer.WriteLine(Wrap(host + UrlEncode(relativePath) + "/" + file));
                    file = UrlDecode(file);
                    if (date == "[目录]")
                    {
                        Write(rootDirectory, new FileInfo(Path.Combine(curDirectory.FullName, file)).Directory, writer);
                    }
                }
            }
        }
        static void Exit()
        {
            Process[] ps = Process.GetProcessesByName("cmd");
            if (ps.Length == 1)
                ps[0].Kill();
        }
        static string UrlDecode(string url)
        {
            return System.Net.WebUtility.UrlDecode(url);
        }
        static string UrlEncode(string relativePath)
        {
            return System.Net.WebUtility.UrlEncode(relativePath).Replace("%2F", "/").Replace("+", "%20");
        }
        static string Wrap(string raw)
        {
            return @"  <url>
    <loc>" + raw + @"</loc>
    <lastmod>" + date + @"</lastmod>
    <changefreq>weekly</changefreq>
    <priority>0.5</priority>
  </url>";
        }
    }
}
