using Elibre.CAD.StitcherApi.ActionLogger;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace CollectingDataFromWebPage
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            var URL = "https://www.youtube.com/watch?v=G4irK9WnmEo";
            //var URL = "https://www.youtube.com/watch?v=1IWCqAzwLnM";
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            openFile.DefaultExt = ".txt";
            openFile.Filter = "Text Files (*.txt)|*.txt|Dictionary Files (*.dic)|*.dic";
            List<string> urls = new List<string>();
            string DirectoryName = "";
            string UrlsWithNoAds = "Urls With No Ads.txt";
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryName = Path.GetDirectoryName(openFile.FileName);
                string[] lines = System.IO.File.ReadAllLines(openFile.FileName);
                for (int i = 0; i < lines.Length; i++)
                {
                    urls.Add(lines[i].Trim());
                }
            }
            //urls.Add(URL);
            //urls.Insert(0, URL);
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            var options = new ChromeOptions()
            {
                PageLoadStrategy = PageLoadStrategy.Eager,
                DebuggerAddress = "localhost:9222"
                //BinaryLocation= @"C:\Users\Eslam\AppData\Local\Google\Chrome\Application\chrome.exe",            
            };
            var userData = @"C:\Users\Eslam\Desktop\Script";
            //options.AddAdditionalCapability("debuggerAddress", "localhost:9222");
            options.AddArgument("ignore-certificate-errors");
            options.AddArgument("user-data-dir=" + userData);
            var driver = new ChromeDriver(driverService, options);


            for (int k = 0; k < urls.Count; k++)
            {
                bool hasAds = true;
                bool hasAtLeastOneAd = false;
                URL = urls[k];
                driver.SwitchTo().NewWindow(WindowType.Window);
                driver.Navigate().GoToUrl(URL);
                var t = driver.Title;

                Thread.Sleep(1000);
                for (int i = 0; i < 2; i++)
                {
                    if (!hasAds) continue;
                    IWebElement AdButton = null;
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    do
                    {
                        try
                        {
                            AdButton = driver.FindElement(By.ClassName(CssClasses.CssAdUrl));
                        }
                        catch
                        {
                            AdButton = null;
                            if (stopwatch.ElapsedMilliseconds > 5000) { hasAds = false; break; }
                        }
                    } while (AdButton == null);

                    var play = driver.FindElement(By.ClassName(CssClasses.CssPlayBtn));
                    play.Click();
                    if (AdButton != null)
                    {
                        hasAtLeastOneAd = true;
                        var AddNo = driver.FindElement(By.ClassName(CssClasses.CssAdNoOfNo)).Text;
                        var AdDuration = driver.FindElement(By.ClassName(CssClasses.CssAdDuration)).Text;

                        AdButton.Click();
                        play.Click();

                        while (AddNo == CssClasses.FirstAd)
                        {
                            Thread.Sleep(250);
                            do
                            {
                                try
                                {
                                    AddNo = driver.FindElement(By.ClassName(CssClasses.CssAdNoOfNo)).Text;
                                }
                                catch
                                {
                                    AddNo = null;
                                }
                            } while (AddNo == null);
                        }
                        if (i == 1)
                        {
                            var SkipBtn = driver.FindElement(By.ClassName(CssClasses.CssSkipAd));

                            while (!SkipBtn.Displayed)
                            {
                                Thread.Sleep(250);
                            }
                            SkipBtn.Click();
                            Thread.Sleep(1000);
                            play.Click();
                        }
                    }
                }
                Console.WriteLine(URL + "successful");
                if (!hasAds && !hasAtLeastOneAd)
                {
                    Console.WriteLine(URL + "failed");

                    ExceptionLogger.LogUrl(URL, DirectoryName, UrlsWithNoAds);
                    driver.Close();
                    driver = new ChromeDriver(driverService, options);
                    //driver.Quit();
                }
                /*if (hasAtLeastOneAd) */Thread.Sleep(240000);
            }
            MessageBox.Show("program completed");
        }
    }
    class YouTubeVideoInfo
    {
        public string url { get; set; }
        public int NoOfViewes { get; set; }
        public string Ad1_url { get; set; }
        public string Ad2_url { get; set; }
    }
    static class CssClasses
    {
        public static string CssAdUrl = "ytp-ad-visit-advertiser-button";
        public static string CssPlayBtn = "ytp-play-button";
        public static string CssAdNoOfNo = "ytp-ad-simple-ad-badge";
        public static string CssAdDuration = "ytp-ad-duration-remaining";
        public static string FirstAd = "Ad 1 of 2 ·";
        public static string CssSkipAd = "ytp-ad-skip-button";
    }
}
