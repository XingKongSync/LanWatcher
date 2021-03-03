using Common;
using InfluxStreamSharp.DataModel;
using LanWatcher.UI.Config;
using LanWatcher.UI.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanWatcher.UI
{
    public class MainWindowVM : BindableBase
    {
        private LogTypeEnum _logType = LogTypeEnum.Core;
        private string _dashboardUrl = string.Empty;
        private DateTime _selectedDate = DateTime.Now;

        public ConfigEntity Config { get => ConfigManager.Instance.Value.Config; }

        public string DashboardUrl 
        {
            get => _dashboardUrl;
            set => SetProperty(ref _dashboardUrl, value);
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    DateTime beginTime = _selectedDate.Date;
                    string bTime = GetUTCDate(beginTime), eTime;
                    if (beginTime.Equals(DateTime.Today))
                    {
                        //eTime = "now()&refresh=60s";
                        eTime = "now%28%29&refresh=60s";
                    }
                    else
                    {
                        DateTime endTime = new DateTime(beginTime.Year, beginTime.Month, beginTime.Day, 23, 30, 0);
                        eTime = GetUTCDate(endTime).Replace(":", "%3A");
                    }
                    DashboardUrl = string.Format(Config.DashboardUrlFormat, bTime, eTime);
                }
            }
        }

        public LogTypeEnum LogType
        {
            get => _logType;
            set
            {
                if (SetProperty(ref _logType, value))
                {
                    switch (_logType)
                    {
                        case LogTypeEnum.Core:
                            CurrentLogs = LogManagerForUI.Instance.CoreLogs;
                            RaisePropertyChanged(nameof(CurrentLogs));
                            break;
                        case LogTypeEnum.DB:
                            CurrentLogs = LogManagerForUI.Instance.DbLogs;
                            RaisePropertyChanged(nameof(CurrentLogs));
                            break;
                        case LogTypeEnum.Scanner:
                            CurrentLogs = LogManagerForUI.Instance.ScannerLogs;
                            RaisePropertyChanged(nameof(CurrentLogs));
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public ObservableCollection<string> CurrentLogs { get; private set; } = LogManagerForUI.Instance.CoreLogs;

        public DelegateCommand OpenChronografCommand { get; private set; }
        public DelegateCommand SaveConfigCommand { get; private set; }
        public DelegateCommand TodayCommand { get; private set; }
        public DelegateCommand YesterdayCommand { get; private set; }
        public DelegateCommand TheDayBeforeYesterdayCommand { get; private set; }

        public MainWindowVM()
        {
            SelectedDate = DateTime.Now;
            ConfigManager.Instance.Value.ConfigReferenceChanged += Value_ConfigReferenceChanged;
            OpenChronografCommand = new DelegateCommand(OpenChronografHandler);
            SaveConfigCommand = new DelegateCommand(SaveConfigCommandHandler);

            TodayCommand = new DelegateCommand(() => SelectedDate = DateTime.Now);
            YesterdayCommand = new DelegateCommand(() => SelectedDate = DateTime.Now.AddDays(-1));
            TheDayBeforeYesterdayCommand = new DelegateCommand(() => DateTime.Now.AddDays(-2));
        }

        private void Value_ConfigReferenceChanged()
        {
            RaisePropertyChanged(nameof(Config));
        }

        private void OpenChronografHandler()
        {
            Process.Start(@"http://127.0.0.1:8888");
        }

        private void SaveConfigCommandHandler()
        {
            ConfigManager.Instance.Value.Save();
        }

        private string GetUTCDate(DateTime date)
        {
            DateTime utc = DateTimeConverter.ToUtcDateTime(date);
            return $"{ utc.ToString("yyyy-MM-dd")}T{utc.ToString("HH:mm:ss.fff")}Z";
        }

    }
}
