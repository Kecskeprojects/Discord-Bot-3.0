using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces;
using Discord_Bot.Tools.NativeTools;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace Discord_Bot.Windows;

public sealed partial class BotWindow : Window, IDisposable
{
    private readonly System.Timers.Timer? diagnosticsTimer;
    private bool autoScroll = true;

    private readonly BotLogger logger;

    public BotWindow(BotLogger logger, IConfig config)
    {
        InitializeComponent();
        this.logger = logger;
        logger.LogAdded += OnLogAdded;

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
            if (e.Source is not ScrollViewer scrollViewer)
            {
                return;
            }

            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                autoScroll = scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight;
            }

            // Content scroll event : autoscroll eventually
            if (autoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and autoscroll mode set
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
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
            if (sender is not ToolBar toolBar)
            {
                return;
            }

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
            _ = Process.Start("explorer.exe", Path.Combine(Directory.GetCurrentDirectory(), "Logs"));
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
                _ = Process.Start("notepad.exe", Path.Combine(Directory.GetCurrentDirectory(), $"Logs\\logs[{DateTimeTools.CurrentDate()}].txt"));
            }
        }
        catch (Exception ex)
        {
            logger.Error("BotWindow.xaml.cs OpenActiveLogFile", ex);
        }
    }

    public async void OnTimedEvent(object? source, ElapsedEventArgs e)
    {
        try
        {
            ProcessMetrics result = await ProcessTools.GetStatistics();
            _ = (Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, () =>
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
            }));
        }
        catch (Exception ex)
        {
            logger.Error("BotWindow.xaml.cs OnTimedEvent", ex);
        }
    }

    public static void ClearWindowLog()
    {
        _ = (Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, () =>
        {
            if (Application.Current.Windows.OfType<BotWindow>().FirstOrDefault() != null)
            {
                BotWindow main = Application.Current.Windows.OfType<BotWindow>().First();
                List<Inline> range = main.MainLogText.Inlines.TakeLast(20).ToList();
                main.MainLogText.Inlines.Clear();
                main.MainLogText.Inlines.AddRange(range);
            }
        }));
    }

    //Raised on whichever thread logged, so the line is added through the window's dispatcher
    private void OnLogAdded(object? sender, Log log)
    {
        string mess = log.Content.Replace(":\t", ":    \t");
        SolidColorBrush color = GetLogColor(log.Type);
        _ = Dispatcher.BeginInvoke(DispatcherPriority.DataBind, () =>
        {
            Run run = new(mess + "\n")
            {
                Foreground = color
            };
            MainLogText.Inlines.Add(run);
        });
    }

    private static SolidColorBrush GetLogColor(LogTypeEnum type)
    {
        return type switch
        {
            LogTypeEnum.Query => Brushes.DarkCyan,
            LogTypeEnum.Client => Brushes.DarkGray,
            LogTypeEnum.Audio => Brushes.DarkOrange,
            LogTypeEnum.Error => Brushes.Red,
            LogTypeEnum.Warning => Brushes.Yellow,
            _ => Brushes.White
        };
    }

    //WPF never disposes a window, so the timer is disposed when the window closes
    protected override void OnClosed(EventArgs e)
    {
        logger.LogAdded -= OnLogAdded;
        Dispose();
        base.OnClosed(e);
    }

    //The timer only exists when diagnostics are shown
    public void Dispose()
    {
        diagnosticsTimer?.Dispose();
    }
}
