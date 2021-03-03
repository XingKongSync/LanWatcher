using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LanWatcher.UI
{
    class LogManagerForUI : BindableBase
    {
        private static Lazy<LogManagerForUI> _instance = new Lazy<LogManagerForUI>(() => new LogManagerForUI());
        private ObservableCollection<string> _coreLogs = new ObservableCollection<string>();
        private ObservableCollection<string> _dbLogs = new ObservableCollection<string>();
        private ObservableCollection<string> _scannerLogs = new ObservableCollection<string>();
        private object _collctionLock = new object();

        public static LogManagerForUI Instance { get => _instance.Value; }
        public ObservableCollection<string> CoreLogs { get => _coreLogs; set => _coreLogs = value; }
        public ObservableCollection<string> DbLogs { get => _dbLogs; set => _dbLogs = value; }
        public ObservableCollection<string> ScannerLogs { get => _scannerLogs; set => _scannerLogs = value; }

        private LogManagerForUI()
        {
            BindingOperations.EnableCollectionSynchronization(CoreLogs, _collctionLock);
            BindingOperations.EnableCollectionSynchronization(DbLogs, _collctionLock);
            BindingOperations.EnableCollectionSynchronization(ScannerLogs, _collctionLock);
        }

        public void AddCoreLog(string log)
        {
            AddLog(CoreLogs, log);
        }

        public void AddDBLog(string log)
        {
            AddLog(DbLogs, log);
        }

        public void AddScannerLog(string log)
        {
            AddLog(ScannerLogs, log);
        }

        private void AddLog(ObservableCollection<string> collection, string log)
        {
            lock (_collctionLock)
            {
                collection.Add(log);
                if (collection.Count > 300)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        collection.RemoveAt(0);
                    }
                }
            }
        }
    }
}
