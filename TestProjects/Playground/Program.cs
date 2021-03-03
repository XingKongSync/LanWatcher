using InfluxDBHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            TestLanScanner();
            //TestDBHost();
            //TestLanWatcherCore();
            //TestScan();

            Console.WriteLine("End");
            Console.ReadLine();
        }

        #region DBHost

        private static void TestDBHost()
        {
            DBHoster dbHoster = DBHoster.Instance;

            dbHoster.OutputLineReceived += DbHoster_OutputLineReceived;
            dbHoster.Start();

            Console.ReadLine();

            dbHoster.Stop();
        }

        private static void DbHoster_OutputLineReceived(string line)
        {
            Console.WriteLine(line);
        }

        #endregion

        private static void TestLanScanner()
        {
            LanScanner.Scanner scanner = new LanScanner.Scanner();
            //scanner.LogConsole += Scanner_LogConsole;
            scanner.LineScanned += Scanner_LineScanned;
            var result = scanner.Scan("10.0.9.1", "10.0.9.254");
        }

        private static void Scanner_LogConsole(string obj)
        {
            Console.Write(obj);
        }

        private static void Scanner_LineScanned(string obj)
        {
            Console.WriteLine(obj);
        }

        private static void TestLanWatcherCore()
        {
            LanWatcher.Core.Watcher.Instance.Start();
        }

        private static void TestScan()
        {
            TestScan ts = new TestScan();
            var r = ts.DeserilizeAdsOutput(File.OpenText(@"D:\Work\Projects\LanWatcher\LanScanner\AdvancedScanner\1.txt"));
        }
    }
}
