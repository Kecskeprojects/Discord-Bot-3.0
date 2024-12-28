using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace Discord_Bot.Windows;

public partial class BotWindow : Window
{
    private readonly Timer diagnosticsTimer;
    private bool AutoScroll = true;

    private readonly BotLogger logger;

    public BotWindow(BotLogger logger, Config config)
    {
        InitializeComponent();
        this.logger = logger;

        if (config.ShowDiagnostics)
        {
            diagnosticsTimer = new(1000) //1 second
            {
                AutoReset = true
            };
            diagnosticsTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            diagnosticsTimer.Start();
        }
        else
        {
            DiagnosticGrid.Visibility = Visibility.Hidden;
        }
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
            logger.Error("BotWindow.xaml.cs ScrollViewer_ScrollChanged", ex);
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
            logger.Error("BotWindow.xaml.cs ToolBar_Loaded", ex);
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
            logger.Error("BotWindow.xaml.cs ClearLog", ex);
        }
    }

    private void OpenLogFolder(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start("explorer.exe", Path.Combine(Directory.GetCurrentDirectory(), "Logs"));
        }
        catch (Exception ex)
        {
            logger.Error("BotWindow.xaml.cs OpenLogFolder", ex);
        }
    }

    private void OpenActiveLogFile(object sender, RoutedEventArgs e)
    {
        try
        {
            logger.LogToFile();
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), $"Logs\\logs[{DateTimeTools.CurrentDate()}].txt")))
            {
                Process.Start("notepad.exe", Path.Combine(Directory.GetCurrentDirectory(), $"Logs\\logs[{DateTimeTools.CurrentDate()}].txt"));
            }
        }
        catch (Exception ex)
        {
            logger.Error("BotWindow.xaml.cs OpenActiveLogFile", ex);
        }
    }

    public async void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        try
        {
            ProcessMetrics result = await ProcessTools.GetStatistics();
            Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, () =>
            {
                if (Application.Current.Windows.OfType<BotWindow>().FirstOrDefault() != null)
                {
                    BotWindow main = Application.Current.Windows.OfType<BotWindow>().First();
                    main.TotalCPUUsage.Content = $"{result.TotalCPUUsagePercent}%";
                    main.TotalRAMUsage.Content = $"{result.TotalRAMUsagePercent}%";
                    main.ThreadCount.Content = $"{result.ThreadCount}";
                    main.ChildProcessCount.Content = $"{result.ChildProcessCount}";
                    main.CPUUsage.Content = $"{result.CPUUsagePercent}%";
                    main.RAMUsage.Content = $"{result.RAMUsageInMB} MB";
                    main.ReservedRAM.Content = $"{result.ReservedRAMInMB} MB";
                }
            });
        }
        catch (Exception ex)
        {
            logger.Error("BotWindow.xaml.cs OnTimedEvent", ex);
        }
    }

    public static void ClearWindowLog()
    {
        Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, () =>
        {
            if (Application.Current.Windows.OfType<BotWindow>().FirstOrDefault() != null)
            {
                BotWindow main = Application.Current.Windows.OfType<BotWindow>().First();
                List<Inline> range = main.MainLogText.Inlines.TakeLast(20).ToList();
                main.MainLogText.Inlines.Clear();
                main.MainLogText.Inlines.AddRange(range);
            }
        });
    }

    public static void LogToWindow(Log log, System.Windows.Media.Brush color)
    {
        string mess = log.Content.Replace(":\t", ":    \t");
        Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, () =>
        {
            if (Application.Current.Windows.OfType<BotWindow>().FirstOrDefault() != null)
            {
                BotWindow main = Application.Current.Windows.OfType<BotWindow>().First();
                Run run = new(mess + "\n")
                {
                    Foreground = color
                };
                main.MainLogText.Inlines.Add(run);
            }
        });
    }
}
