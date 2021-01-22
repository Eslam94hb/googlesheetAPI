using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CollectingDataFromWebPage
{
    static class Program
    {
        static void Main()
        {
            var URL = "https://stackoverflow.com/questions/4785623/how-can-i-make-an-application-in-c-sharp-collect-data-from-a-website";
            HttpClient httpClient = new HttpClient();
            var data = httpClient.GetStringAsync(URL);
            //WebClient webClient = new WebClient();
            //var data = webClient.DownloadString(URL);
            //Console.WriteLine(data.Result);
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(data.Result);
            var node = htmlDocument.DocumentNode.Descendants("li").Where(c => c.GetAttributeValue("id", "") == "comment-5300835");
            Console.WriteLine(node.First().Descendants("span").ElementAt(1).InnerText);
        }
    }
}
