using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YoutubeScraper
{
    public partial class Form1 : Form
    {
        CancellationTokenSource tokenSource;
        List<string> urls = new List<string>();
        string DirectoryName = "";
        public Form1()
        {
            InitializeComponent();
            Label.CheckForIllegalCrossThreadCalls = false;
            TextBox.CheckForIllegalCrossThreadCalls = false;
            tokenSource = new CancellationTokenSource();
        }

        private void button1_Click(object sender, EventArgs e)
        {          
            CancellationToken token = tokenSource.Token;
            Task task = new Task(() =>
            {
                Automation automation = new Automation(urls, GetProcessType(), GetTimeSpanInMin(), (int)AdsNo.Value);
                automation.LinkProcessed += WriteToTextBox;
                automation.NoOutOfNo += WriteToLabel;
                automation.Run();
            }, token);
            task.Start();
        }
        ProcessType GetProcessType()
        {
            if (rdoVideoLink.Checked) return ProcessType.VideoLink;
            else if (rdoChannelLinks.Checked) return ProcessType.ChannelLink;
            else return ProcessType.VideoLink;
        }
        private void WriteToTextBox(object sender, string e)
        {
            txtProgress.AppendText(e);
        }
        private void WriteToLabel(object sender, string e)
        {
            string name = "YoutubeScraper";
            this.Text = name + " " + e;
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            openFile.DefaultExt = ".txt";
            openFile.Filter = "Text Files (*.txt)|*.txt|Dictionary Files (*.dic)|*.dic";
            string FileName = "";
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileName = openFile.FileName;
                DirectoryName = Path.GetDirectoryName(openFile.FileName);
                string[] lines = System.IO.File.ReadAllLines(openFile.FileName);
                for (int i = 0; i < lines.Length; i++)
                {
                    urls.Add(lines[i].Trim());
                }
            }
            urls = urls.Where(x => !string.IsNullOrEmpty(x)).ToList();
            textBoxFileLocation.Text = FileName;
            lblDetectedLinks.Text = urls.Count.ToString() + " Links Detected";
            lblDetectedLinks.Visible = true;
        }
        List<string> GetUrls() => urls;
        string GetDirectoryName() => DirectoryName;

        private void btnStop_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            textBoxFileLocation.Text = "";
            lblDetectedLinks.Text = "";
            lblDetectedLinks.Visible = false;
            txtProgress.Clear();
        }
        int GetTimeSpanInMin()
        {
            return (int)TimeInMin.Value;
        }
    }
}
