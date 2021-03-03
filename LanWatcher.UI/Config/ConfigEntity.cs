using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanWatcher.UI.Config
{
    public class ConfigEntity : BindableBase
    {
        private string _dashboardUrlFormat = @"http://127.0.0.1:8888/sources/1/dashboards/1?present=true&lower={0}&upper={1}";
        private string _startIp = "10.0.9.1";
        private string _stopIp = "10.0.9.254";

        public string DashboardUrlFormat { get => _dashboardUrlFormat; set => SetProperty(ref _dashboardUrlFormat, value); }
        public string StartIp { get => _startIp; set => SetProperty(ref _startIp, value); }
        public string StopIp { get => _stopIp; set => SetProperty(ref _stopIp, value); }
    }
}
