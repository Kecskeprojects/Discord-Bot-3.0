using Discord_Bot.Core.Logger;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Discord_Bot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static TextBlock MainStaticLog { get; private set; }
        private bool AutoScroll = true;

        private readonly Logging logger;

        public MainWindow(Logging logger)
        {
            InitializeComponent();
            this.logger = logger;
            MainStaticLog = MainLogText;
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
            catch(Exception ex)
            {
                logger.Error("MainWindow.xaml.cs ToolBar_Loaded", ex.ToString());
            }
        }

        private void WriteLog(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Log("Logged action!\nNew Line");
            }
            catch(Exception ex)
            {
                logger.Error("MainWindow.xaml.cs WriteLog", ex.ToString());
            }
        }
    }
}
