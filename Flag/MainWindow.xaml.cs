// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUICommunity.Common.Helpers;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;
using System.Threading.Tasks;
using Windows.ApplicationModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Flag
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private AppWindow m_AppWindow;
        public MainWindow()
        {
            this.InitializeComponent();
            m_AppWindow = WindowHelper.GetAppWindowForCurrentWindow(this);
            m_AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            this.Title = "Flag";
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            var builder = new AppNotificationBuilder()
                .AddText("Flag is dead")
                .AddButton(new AppNotificationButton("Rot!").SetInvokeUri(new Uri("http://scr1w.dlut.edu.cn/")));
            var notificationManager = AppNotificationManager.Default;
            notificationManager.Show(builder.BuildNotification());
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            CheckUpdate();
        }

        private void CheckUpdate()
        {
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            Task.Run(async () =>
            {
                try
                {
                    var ver = await UpdateHelper.CheckUpdateAsync("IShiraiKurokoI", "Flag", new Version(string.Format("{0}.{1}.{2}.{3}",
                            Package.Current.Id.Version.Major,
                            Package.Current.Id.Version.Minor,
                            Package.Current.Id.Version.Build,
                            Package.Current.Id.Version.Revision)));

                    if (ver.IsExistNewVersion)
                    {
                        dispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, async () =>
                        {
                            ContentDialog dialog = new ContentDialog();
                            dialog.XamlRoot = this.Content.XamlRoot;
                            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                            dialog.Title = "发现新版本！";
                            dialog.PrimaryButtonText = "前往更新";
                            dialog.CloseButtonText = "暂不更新";
                            dialog.DefaultButton = ContentDialogButton.Primary;
                            dialog.Content = $"检测到新版本：V{ver.TagName}\n发布时间：{ver.PublishedAt}";
                            var result = await dialog.ShowAsync();
                            if (result == ContentDialogResult.Primary)
                            {
                                await Windows.System.Launcher.LaunchUriAsync(new Uri(ver.HtmlUrl.ToString()));
                            }
                        });
                    }
                    else
                    {
                        var builder = new AppNotificationBuilder()
                            .AddText($"您当前使用的是最新版本！");
                        var notificationManager = AppNotificationManager.Default;
                        notificationManager.Show(builder.BuildNotification());
                    }
                }
                catch (Exception e)
                {
                    var builder = new AppNotificationBuilder()
                        .AddText($"检查更新失败：{e.Message}");
                    var notificationManager = AppNotificationManager.Default;
                    notificationManager.Show(builder.BuildNotification());
                }
            });
        }
    }
}
