using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfluxDBHost
{
    public class DBHoster
    {
        private static Lazy<DBHoster> _instance = new Lazy<DBHoster>(()=> new DBHoster());
        private readonly string CONST_DB_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfluxDB");

        public static DBHoster Instance { get => _instance.Value; }

        public event Action<string> OutputLineReceived;

        private Process _influxProc;
        //private Thread _influxOutputReaderThread1;
        //private Thread _influxOutputReaderThread2;

        private DBHoster() { }

        public void Start()
        {
            ProcessStartInfo psi = new ProcessStartInfo(Path.Combine(CONST_DB_PATH, "influxd.exe"), "-config influxdb.conf");
            psi.WorkingDirectory = CONST_DB_PATH;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            //psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            _influxProc = Process.Start(psi);

            //当主程序退出时，InfluxDB自动退出
            Job job = new Job();
            job.AddProcess(_influxProc.Handle);

            //StartOutputReaderThread();
            _influxProc.BeginErrorReadLine();
            _influxProc.BeginOutputReadLine();
            _influxProc.ErrorDataReceived += InfluxProc_ErrorDataReceived;
            _influxProc.OutputDataReceived += InfluxProc_OutputDataReceived;
        }

        public void Stop()
        {
            if (_influxProc != null)
            {
                _influxProc.Kill();

                _influxProc = null;
            }

            //StopOutputReaderThread();
        }

        #region ReadOutput

        //private void StartOutputReaderThread()
        //{
        //    _influxOutputReaderThread1 = new Thread(OutputReadWoker1);
        //    _influxOutputReaderThread1.Name = "InfluxOutputReaderThread1";
        //    _influxOutputReaderThread1.IsBackground = true;
        //    _influxOutputReaderThread1.Start();

        //    _influxOutputReaderThread2 = new Thread(OutputReadWoker2);
        //    _influxOutputReaderThread2.Name = "InfluxOutputReaderThread2";
        //    _influxOutputReaderThread2.IsBackground = true;
        //    _influxOutputReaderThread2.Start();
        //}

        //private void StopOutputReaderThread()
        //{
        //    if (_influxOutputReaderThread1 != null && _influxOutputReaderThread1.IsAlive)
        //    {
        //        _influxOutputReaderThread1.Join(1000);
        //        if (_influxOutputReaderThread1.IsAlive)
        //        {
        //            _influxOutputReaderThread1.Abort();
        //        }
        //        _influxOutputReaderThread1 = null;
        //    }

        //    if (_influxOutputReaderThread2 != null && _influxOutputReaderThread2.IsAlive)
        //    {
        //        _influxOutputReaderThread2.Join(1000);
        //        if (_influxOutputReaderThread2.IsAlive)
        //        {
        //            _influxOutputReaderThread2.Abort();
        //        }
        //        _influxOutputReaderThread2 = null;
        //    }
        //}

        //private void OutputReadWoker1()
        //{
        //    Process proc = _influxProc;
        //    if (proc == null)
        //        return;

        //    while (proc != null && !proc.HasExited)
        //    {
        //        string line = proc.StandardError.ReadLine();
        //        if (line != null)
        //        {
        //            OutputLineReceived?.Invoke(line);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //}

        //private void OutputReadWoker2()
        //{
        //    Process proc = _influxProc;
        //    if (proc == null)
        //        return;

        //    while (proc != null && !proc.HasExited)
        //    {
        //        string line = proc.StandardOutput.ReadLine();
        //        if (line != null)
        //        {
        //            OutputLineReceived?.Invoke(line);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //}

        #endregion



        private void InfluxProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputLineReceived?.Invoke(e.Data);
        }

        private void InfluxProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputLineReceived?.Invoke(e.Data);
        }

    }
}
