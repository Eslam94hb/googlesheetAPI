using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elibre.CAD.StitcherApi.ActionLogger
{

    public static class ExceptionLogger
    {
        static string FilePath { get; set; } = @"D:\";
        static string FileName = "Log.txt";
        static object obj = new object();
        public static void SetTextFilePath(string _FilePath, string _FileName = "Log.txt")
        {
            FilePath = _FilePath;
            FileName = _FileName;
            //string TxtFileFullPath = FilePath + "/" + FileName;

            //if (File.Exists(TxtFileFullPath))
            //{
            //    File.Delete(TxtFileFullPath);
            //}
        }
        public static void Log(string Messsage, Exception _exception)
        {
            string TxtFileFullPath = FilePath + "/" + FileName;
            if (Directory.Exists(FilePath))
            {
                using (System.IO.StreamWriter w = System.IO.File.AppendText(TxtFileFullPath))
                {
                    w.Write("\r\nLog Entry : ");
                    w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                    w.WriteLine("{0}", Messsage);
                    w.WriteLine("  :{0}", _exception.Message);
                    w.WriteLine("  :{0}", _exception.StackTrace);
                    w.WriteLine("===============================================");
                    w.WriteLine("===============================================");
                }
            }
        }
        public static void Log(string Messsage)
        {
            string TxtFileFullPath = FilePath + "/" + FileName;
            if (Directory.Exists(FilePath))
            {
                lock (obj)
                {
                    using (System.IO.StreamWriter w = System.IO.File.AppendText(TxtFileFullPath))
                    {
                        w.Write("\r\nLog Entry : ");
                        w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                        w.WriteLine("{0}", Messsage);
                        w.WriteLine("===============================================");
                        w.WriteLine("===============================================");
                    }
                }
            }
        }
        public static void Log(string Messsage, string _FilePath)
        {
            string TxtFileFullPath = _FilePath + "/" + FileName;
            if (Directory.Exists(_FilePath))
            {
                lock (obj)
                {
                    using (System.IO.StreamWriter w = System.IO.File.AppendText(TxtFileFullPath))
                    {
                        w.Write("\r\nLog Entry : ");
                        w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                        w.WriteLine("{0}", Messsage);
                        w.WriteLine("===============================================");
                        w.WriteLine("===============================================");
                    }
                }
            }
        }
        public static void LogUrl(string Messsage, string _FilePath, string _FileName)
        {
            string TxtFileFullPath = _FilePath + "/" + _FileName;
            if (Directory.Exists(_FilePath))
            {
                lock (obj)
                {
                    using (System.IO.StreamWriter w = System.IO.File.AppendText(TxtFileFullPath))
                    {
                        w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                        w.WriteLine("{0}", Messsage);
                        w.WriteLine("===============================================");
                    }
                }
            }
        }
    }
}
