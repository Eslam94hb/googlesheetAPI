using Elibre.CAD.StitcherApi.ActionLogger;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YoutubeScraper
{
    enum Status { SUCCESS, HALF_SUCCESS, FAILED }
    enum ProcessType { VideoLink, ChannelLink }
    class Automation
    {
        ProcessType ProcessType;
        Status status;
        public event EventHandler<string> LinkProcessed;
        public event EventHandler<string> NoOutOfNo;
        public event EventHandler<string> Timer;
        string _url;
        ChromeDriver driver;
        List<string> _urls = new List<string>();
        int TimeBtweenEachVideo { get; set; }
        int TargetAdsNo { get; set; }
        public Automation(List<string> urls, ProcessType processType, int timeBtweenEachVideo, int targetAdsNo)
        {
            TimeBtweenEachVideo = timeBtweenEachVideo;
            TargetAdsNo = targetAdsNo;
            _urls = urls;
            ProcessType = processType;
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
            driver = new ChromeDriver(driverService, options);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(90);
        }
        public void Run()
        {
            List<VideoStatus> videoStatuses = new List<VideoStatus>();
            for (int k = 0; k < _urls.Count; k++)
            {
                //var windows = driver.WindowHandles;
                try
                {
                    NoOutOfNo.Invoke(this, $"{k + 1} / {_urls.Count}");

                    _url = _urls[k];
                    driver.SwitchTo().NewWindow(WindowType.Window);
                    driver.Navigate().GoToUrl(_url);
                    var Title = driver.Title;

                    string windowHandle = driver.CurrentWindowHandle;
                    VideoStatus videoStatus = new VideoStatus(_url, Title, windowHandle);
                    Thread.Sleep(1000);

                    LoopOn2Ads(videoStatus);

                    videoStatuses.Add(videoStatus);
                    Print();
                    Thread.Sleep(ToMilliSecond(TimeBtweenEachVideo));

                    //if (k != _urls.Count - 1)
                    //{
                    //    //int timeInSec = TimeBtweenEachVideo * 60;
                    //    //for (int i = 1; i < TimeBtweenEachVideo; i++)
                    //    //{
                    //    //    Timer.Invoke(this, i.ToString())
                    //    //}
                    //    Thread.Sleep(ToMilliSecond(TimeBtweenEachVideo));
                    //}

                }
                catch (Exception e)
                {
                    status = Status.FAILED;
                    Print();
                    LinkProcessed?.Invoke(this, "\r\n \r\n errorrrrrrrrrrrrr \r\n" + e.Message + " \r\n" + e.StackTrace + " \r\n \r\n");
                    //Task.Run(() => MessageBox.Show(e.Message, _url));
                    Thread.Sleep(ToMilliSecond(TimeBtweenEachVideo));
                    continue;
                }
            }

            try
            {
                Refreshes(videoStatuses);
            }
            catch (Exception e)
            {
                LinkProcessed?.Invoke(this, "\r\n \r\n errorrrrrrrrrrrrr \r\n" + e.Message + " \r\n" + e.StackTrace + " \r\n \r\n");
            }
            foreach (var item in videoStatuses)
            {
                LinkProcessed?.Invoke(this, $"{item.NoOfAds} Ads: {item.URL} \r\n");
            }
            LinkProcessed?.Invoke(this, "program completed \r\n");
        }
        void Refreshes(List<VideoStatus> videoStatuses)
        {
            LinkProcessed?.Invoke(this, "\r\n \r\n Refreshes Process started  \r\n \r\n");
            var remained = videoStatuses.Where(x => x.NoOfAds < TargetAdsNo).ToList();
            var lowstnumber = 0;
            if (remained.Count > 0) lowstnumber = remained.Min(x => x.NoOfAds);
            var remainedAdNo = TargetAdsNo - lowstnumber;

            for (int k = 0; k < remainedAdNo; k++)
            {
                LinkProcessed?.Invoke(this, $"\r\n \r\n Refresh {k + 1}/{remainedAdNo}   \r\n \r\n");
                foreach (var Window in remained)
                {
                    LinkProcessed?.Invoke(this, $"\r\n \r\n Refresh {Window.URL} {Window.Title}   \r\n \r\n");
                    try
                    {
                        driver.SwitchTo().Window(Window.CurrentWindowHandle);
                        Thread.Sleep(250);
                        driver.Navigate().Refresh();

                        Thread.Sleep(1000);

                        LoopOn2Ads(Window);

                        //if (remained.IndexOf(Window) != _urls.Count - 1)
                        Thread.Sleep(ToMilliSecond(TimeBtweenEachVideo));
                    }
                    catch (Exception e)
                    {
                        LinkProcessed?.Invoke(this, "\r\n \r\n errorrrrrrrrrrrrr \r\n" + e.Message + " \r\n" + e.StackTrace + " \r\n \r\n");
                        Thread.Sleep(ToMilliSecond(TimeBtweenEachVideo));
                        continue;
                    }


                }
                remained = remained.Where(x => x.NoOfAds < TargetAdsNo).ToList();
            }
        }
        void LoopOn2Ads(VideoStatus videoStatus)
        {
            bool hasAds = true;
            for (int i = 0; i < 2; i++)
            {
                //if (!hasAds) continue;
                IWebElement AdButton = GetButton(StaticStrings.CssAdUrl);
                if (AdButton == null) hasAds = false;
                var play = driver.FindElement(By.ClassName(StaticStrings.CssPlayBtn));
                Click(play);//play.Click();

                if (!SetStatus(i, hasAds)) break;

                Click(AdButton); /*AdButton.Click();*/
                driver.SwitchTo().Window(videoStatus.CurrentWindowHandle);
                Click(play); /*play.Click();*/

                videoStatus.NoOfAds += 1;

                WaitUntilNextAd();

                if (i == 1)
                {
                    SkipAd();
                    Thread.Sleep(1000);
                    Click(play);/* play.Click();*/ //stop video
                }
            }
        }
        void Click(IWebElement btn)
        {
            while (!(btn.Displayed && btn.Enabled))
            {
                Thread.Sleep(250);
            }
            btn.Click();
        }
       
        IWebElement GetButton(string className)
        {
            IWebElement AdButton = null;
            Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
                try
                {
                    AdButton = driver.FindElement(By.ClassName(className));
                }
                catch
                {
                    AdButton = null;
                    if (stopwatch.ElapsedMilliseconds > 5000) { break; }
                }
            } while (AdButton == null);
            return AdButton;
        }

        bool SetStatus(int i, bool hasAds)
        {
            if (i == 0 && !hasAds)
            {
                status = Status.FAILED;
                return false;
            }
            else if (i == 1 && !hasAds)
            {
                status = Status.HALF_SUCCESS;
                return false;
            }
            else if (i == 1 && hasAds)
            {
                status = Status.SUCCESS;
                return true;
            }
            return true;
        }
        void WaitUntilNextAd()
        {
            var AddNo = driver.FindElement(By.ClassName(StaticStrings.CssAdNoOfNo)).Text;
            while (AddNo == StaticStrings.FirstAd)
            {
                Thread.Sleep(250);
                do
                {
                    try
                    {
                        AddNo = driver.FindElement(By.ClassName(StaticStrings.CssAdNoOfNo)).Text;
                    }
                    catch
                    {
                        AddNo = null;
                    }
                } while (AddNo == null);
            }
        }
        void SkipAd()
        {
            var SkipBtn = GetButton(StaticStrings.CssSkipAd);
            if (SkipBtn == null) return;
            while (!SkipBtn.Displayed)
            {
                Thread.Sleep(250);
            }
            SkipBtn.Click();
        }
        void Print()
        {
            switch (status)
            {
                case Status.SUCCESS:
                    LinkProcessed?.Invoke(this, "success: " + _url + " \r\n");
                    break;
                case Status.HALF_SUCCESS:
                    LinkProcessed?.Invoke(this, "half Sucess: " + _url + " \r\n");
                    break;
                case Status.FAILED:
                    LinkProcessed?.Invoke(this, "failed: " + _url + " \r\n");
                    break;
                default:
                    break;
            }
        }
        int ToMilliSecond(int min)
        {
            return min * 60000;
        }
    }
    class VideoStatus
    {
        public VideoStatus(string currentWindowHandle)
        {
            CurrentWindowHandle = currentWindowHandle;
        }
        public VideoStatus(/*Status status, */string currentWindowHandle, int noOfAds)
        {
            //this.status = status;
            CurrentWindowHandle = currentWindowHandle;
            NoOfAds = noOfAds;
        }

        public VideoStatus(string uRL, string title, string currentWindowHandle)
        {
            URL = uRL;
            Title = title;
            CurrentWindowHandle = currentWindowHandle;
        }

        //public Status status { get; set; }
        public string URL { get; set; }
        public string Title { get; set; }
        public string CurrentWindowHandle { get; set; }
        public int NoOfAds { get; set; }
    }
}