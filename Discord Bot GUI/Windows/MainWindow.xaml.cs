using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Tools;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Discord_Bot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Timer MainTimer;
        private bool AutoScroll = true;

        private readonly Logging logger;

        public MainWindow(Logging logger)
        {
            InitializeComponent();
            this.logger = logger;
            MainTimer = new(1000) { AutoReset = true }; //1 second
            MainTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            MainTimer.Start();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                // User scroll event : set or unset autoscroll mode
                if (e.ExtentHeightChange == 0)
                {   // Content unchanged : user scroll event
                    AutoScroll = (e.Source as ScrollViewer).VerticalOffset == (e.Source as ScrollViewer).ScrollableHeight;
                }

                // Content scroll event : autoscroll eventually
                if (AutoScroll && e.ExtentHeightChange != 0)
                {   // Content changed and autoscroll mode set
                    (e.Source as ScrollViewer).ScrollToVerticalOffset((e.Source as ScrollViewer).ExtentHeight);
                }
            }
            catch (Exception ex)
            {
                logger.Error("MainWindow.xaml.cs ScrollViewer_ScrollChanged", ex.ToString());
            }
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ToolBar toolBar = sender as ToolBar;
                if (toolBar.Template.FindName("OverflowGrid", toolBar) is FrameworkElement overflowGrid)
                {
                    overflowGrid.Visibility = Visibility.Collapsed;
                }

                if (toolBar.Template.FindName("MainPanelBorder", toolBar) is FrameworkElement mainPanelBorder)
                {
                    mainPanelBorder.Margin = new Thickness(0);
                }
            }
            catch (Exception ex)
            {
                logger.Error("MainWindow.xaml.cs ToolBar_Loaded", ex.ToString());
            }
        }

        private void ClearLog(object sender, RoutedEventArgs e)
        {
            try
            {
                MainLogText.Inlines.Clear();
            }
            catch (Exception ex)
            {
                logger.Error("MainWindow.xaml.cs ClearLog", ex.ToString());
            }
        }

        public async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                ProcessMetrics result = await ProcessTools.GetStatistics();
                Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, () =>
                {
                    if (Application.Current.MainWindow != null)
                    {
                        MainWindow main = Application.Current.MainWindow as MainWindow;
                        main.TotalCPUUsage.Content = $"{result.TotalCPUUsagePercent}%";
                        main.TotalRAMUsage.Content = $"{result.TotalRAMUsagePercent}%";
                        main.ThreadCount.Content = $"{result.ThreadCount}";
                        main.ChildProcessCount.Content = $"{result.ChildProcessCount}";
                        main.CPUUsage.Content = $"{result.CPUUsagePercent}%";
                        main.RAMUsage.Content = $"{result.RAMUsageInMB} MB";
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Error("MainWindow.xaml.cs OnTimedEvent", ex.ToString());
            }
        }
    }
}
