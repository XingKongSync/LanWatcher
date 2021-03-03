using Common;
using InfluxDBHost;
using LanScanner;
using LanWatcher.Core;
using LanWatcher.UI.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LanWatcher.UI
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public Watcher WatcherCore { get; private set; }
        private System.Windows.Forms.NotifyIcon _notifyIcon = null;
        private MainWindow _mainWindow = null;

        private ConfigEntity Config { get => ConfigManager.Instance.Value.Config; }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            CreateTrayIcon();

            IEHelper.SetIE(IEHelper.IeVersion.标准ie11);

            WatcherCore = Watcher.Instance;
            
            ConfigManager.Instance.Value.ConfigReferenceChanged += Value_ConfigReferenceChanged;
            LogManager.LogService.Instance.Value.LogPrinted += LogManager_LogPrinted;
            WatcherCore.InfluxDBLogged += WatcherCore_InfluxDBLogged;
            WatcherCore.ScannerLogged += WatcherCore_ScannerLogged;

            ReloadConfig(Config);

            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow();
                Current.MainWindow = _mainWindow;
            }

            base.OnStartup(e);
            Task.Run(WatcherCore.Start);
            Task.Run(ChronografHost.ChronografHost.Instance.Start);

            //Task.Run(TestLog);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            LogManager.LogService.Instance.Value.LogError($"发生未处理的异常，异常类型：{e.ExceptionObject.GetType().Name}，原因：{ex?.Message}，堆栈：\r\n{ex?.StackTrace}");
        }

        //private void TestLog()
        //{
        //    var logger = LogManager.LogService.Instance.Value;
        //    int i = 0;
        //    while (true)
        //    {
        //        logger.LogDebug(i.ToString());
        //        i++;
        //        System.Threading.Thread.Sleep(500);
        //    }
        //}

        #region TrayHelper

        private void CreateTrayIcon()
        {
            if (_notifyIcon == null)
            {
                //Icon
                System.Reflection.Assembly ass = GetType().Assembly;
                System.IO.Stream icoStream = ass.GetManifestResourceStream("LanWatcher.UI.monitor.ico");
                System.Drawing.Icon icon = new System.Drawing.Icon(icoStream, 128, 128);

                _notifyIcon = new System.Windows.Forms.NotifyIcon();
                _notifyIcon.Icon = icon;
                _notifyIcon.Visible = true;

                System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("显示");
                System.Windows.Forms.MenuItem close = new System.Windows.Forms.MenuItem("退出");
                open.Click += Open_Click;
                close.Click += Close_Click;
                open.DefaultItem = true;

                _notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[] { open, close });
                _notifyIcon.DoubleClick += Open_Click;
            }
        }

        private void Close_Click(object sender, EventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.CanExit = true;
            }
            LogManager.LogService.Instance.Value.Dispose();
            Current.Shutdown();
        }

        private void Open_Click(object sender, EventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.Show();
                _mainWindow.Activate();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
            }
        }
        #endregion

        #region ConfigHelper

        private void Value_ConfigReferenceChanged()
        {
            ReloadConfig(Config);
        }

        private void ReloadConfig(ConfigEntity config)
        {
            WatcherCore.StartIP = config.StartIp;
            WatcherCore.StopIP = config.StopIp;

            config.PropertyChanged -= Config_PropertyChanged;
            config.PropertyChanged += Config_PropertyChanged;
        }

        private void Config_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ConfigEntity config = sender as ConfigEntity;
            if (e.PropertyName != null && config != null)
            {
                switch (e.PropertyName)
                {
                    case nameof(ConfigEntity.StartIp):
                        WatcherCore.StartIP = config.StartIp;
                        break;
                    case nameof(ConfigEntity.StopIp):
                        WatcherCore.StopIP = config.StopIp;
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region LogHelper


        private void WatcherCore_ScannerLogged(string obj)
        {
            LogManagerForUI.Instance.AddScannerLog(obj.Trim());
        }

        private void WatcherCore_InfluxDBLogged(string obj)
        {
            LogManagerForUI.Instance.AddDBLog(obj);
        }

        private void LogManager_LogPrinted(string obj)
        {
            LogManagerForUI.Instance.AddCoreLog(obj);
        }

        #endregion
    }
}
