using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronografHost
{
    public class ChronografHost
    {
        //SELECT count(DISTINCT("MAC")) AS "Count" FROM "LanWatcher"."LanWatcher_rp"."scan_record" WHERE time > :dashboardTime: AND time< :upperDashboardTime: AND "Status"='alive' GROUP BY time(30m) FILL(previous)

        private static Lazy<ChronografHost> _instance = new Lazy<ChronografHost>(() => new ChronografHost());
        private readonly string CONST_CHRON_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Chronograf");

        public static ChronografHost Instance { get => _instance.Value; }

        private Process _chronografProc;

        private ChronografHost() { }

        public void Start()
        {
            ProcessStartInfo psi = new ProcessStartInfo(Path.Combine(CONST_CHRON_PATH, "chronograf.exe"));
            psi.WorkingDirectory = CONST_CHRON_PATH;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            _chronografProc = Process.Start(psi);
            _chronografProc.BeginErrorReadLine();
            _chronografProc.BeginOutputReadLine();

            Job job = new Job();
            job.AddProcess(_chronografProc.Handle);
        }

        public void Stop()
        {
            if (_chronografProc != null)
            {
                _chronografProc.Kill();
                _chronografProc = null;
            }
        }



    }
}
