using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LanWatcher.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool CanExit = false;

        public MainWindowVM VM { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            VM = new MainWindowVM();
            IsVisibleChanged += MainWindow_IsVisibleChanged;

            //UpdateDatePickerBlackout();
            DataContext = VM;
        }

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                UpdateDatePickerBlackout();
            }
        }

        private void UpdateDatePickerBlackout()
        {
            DatePicker.BlackoutDates.Clear();
            DatePicker.BlackoutDates.Add(new CalendarDateRange(DateTime.Now.AddDays(1), DateTime.MaxValue));
        }

        private void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName?.Equals(nameof(VM.DashboardUrl)) == true && IsLoaded)
            {
                TryNavigate();
            }
        }

        private bool TryNavigate()
        {
            if (!string.IsNullOrWhiteSpace(VM?.DashboardUrl))
            {
                try
                {
                    wbDashboard.Source = new Uri(VM.DashboardUrl);
                    return true;
                }
                catch (Exception ex)
                {
                    LogManager.LogService.Instance.Value.LogError($"导航到页面失败，Url：{VM?.DashboardUrl}，原因：{ex.Message}");
                }
            }
            return false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CanExit)
            {
                e.Cancel = true;

                Hide();
            }
        }

        private void btRefresh_Click(object sender, RoutedEventArgs e)
        {
            TryNavigate();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VM.PropertyChanged += VM_PropertyChanged;
            TryNavigate();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            VM.PropertyChanged -= VM_PropertyChanged;
        }

        private void btDate_Click(object sender, RoutedEventArgs e)
        {
            UpdateDatePickerBlackout();
        }
    }
}
