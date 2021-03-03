using InfluxDBHost;
using InfluxStreamSharp.DataModel;
using InfluxStreamSharp.Influx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LanWatcher.Core
{
    public class Watcher
    {
        public static Watcher Instance { get => _instance.Value; }
        private static Lazy<Watcher> _instance = new Lazy<Watcher>(() => new Watcher());

        private static readonly string DB_Url = @"http://127.0.0.1:8086";
        private static readonly string DB_UserName = "";
        private static readonly string DB_Pwd = "";
        private static readonly string DB_DbName = "LanWatcher";
        private static readonly int DB_RetentionHours = 24 * 30;//Keep data for a month

        private const int CONST_RETRY_INTERVAL = 5000;

        public string StartIP { get; set; } = "10.0.9.1";
        public string StopIP { get; set; } = "10.0.9.254";
        public event Action<string> InfluxDBLogged;
        public event Action<string> ScannerLogged;

        private LanScanner.Scanner _scanner;
        private Timer _timer;
        private volatile bool _isRunning = false;
        private List<ScanRecord> _recordCache = new List<ScanRecord>();

        private LogManager.LogService _logger = LogManager.LogService.Instance.Value;

        private Watcher() 
        {
            _timer = new Timer();
            _timer.AutoReset = false;
            _timer.Elapsed += Timer_Elapsed;
            _timer.Interval = CalcNextTickInterval();

            _scanner = new LanScanner.Scanner();

            _scanner.LineScanned += Scanner_LineScanned;
            DBHoster.Instance.OutputLineReceived += Instance_OutputLineReceived;
        }

        private void Scanner_LineScanned(string obj)
        {
            ScannerLogged?.Invoke(obj);
        }

        private void Instance_OutputLineReceived(string obj)
        {
            InfluxDBLogged?.Invoke(obj);
        }

        public void Start()
        {
            _logger.LogDebug("LanWatcher 正在初始化...");

            _isRunning = true;
            DBHoster.Instance.Start();
            WriteService.Instance.Value.Start();

            var influx = InfluxService.Instance.Value;
            bool initSuccess = false;
            while (!initSuccess)
            {
                try
                {
                    influx.InitAsync(
                         DB_Url,
                         DB_UserName,
                         DB_Pwd,
                         DB_DbName,
                         DB_RetentionHours
                     ).Wait();
                    initSuccess = true;
                }
                catch (Exception)
                {
                    _logger.LogError("InfluxDB 初始化失败，即将重试...");
                }
            }
 

            _timer.Start();

            _logger.LogDebug("LanWatcher 初始化完成");
        }

        public void Stop()
        {
            _logger.LogDebug("LanWatcher 正在停止...");
            _isRunning = false;
            _timer.Stop();
            WriteService.Instance.Value.Stop();
            DBHoster.Instance.Stop();
            _logger.LogDebug("LanWatcher 已停止...");
        }

        private double CalcNextTickInterval()
        {
            //return 5000;

            DateTime now = DateTime.Now;

            DateTime next = now.AddMinutes(30 - now.Minute % 30).AddSeconds(now.Second * -1);

            return next.Subtract(now).TotalMilliseconds;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool scanSuccess = false;
            try
            {
                _logger.LogDebug("开始扫描...");

                int liveCount = 0;
                List<LanScanner.ScanResult> result = _scanner.Scan(StartIP, StopIP);
                if (result != null)
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        ScanRecord record;
                        if (_recordCache.Count <= i)
                        {
                            record = new ScanRecord();
                            _recordCache.Add(record);
                        }
                        else
                        {
                            record = _recordCache[i];
                        }
                        record.Fit(result[i]);
                        //统计一下在线数量
                        if (record.Status?.Contains("alive") == true)
                        {
                            liveCount++;
                        }

                        var point = ModelTransformer.Convert(record);
                        WriteService.Instance.Value.Enqueue(point);
                    }
                }
                _logger.LogDebug("扫描完成...在线数量：" + liveCount);

                scanSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("扫描过程中发生错误，原因：" + ex.Message);
            }

            if (_isRunning)
            {
                _timer.Interval = scanSuccess ? CalcNextTickInterval() : CONST_RETRY_INTERVAL;
                _timer.Start();
            }
        }

    }
}
