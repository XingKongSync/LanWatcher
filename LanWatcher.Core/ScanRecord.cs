using InfluxStreamSharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanWatcher.Core
{
    [InfluxModel("scan_record")]
    public class ScanRecord
    {
        [InfluxModel(InfluxFieldType.Tag)]
        public string Status;

        [InfluxModel(InfluxFieldType.Value)]
        public string Name;

        [InfluxModel(InfluxFieldType.Tag)]
        public string IP;

        [InfluxModel(InfluxFieldType.Value)]
        public string MAC;

        public void Fit(LanScanner.ScanResult r)
        {
            Status = r.Status;
            Name = r.Name;
            IP = r.IP;
            MAC = r.MAC;
        }
    }
}
