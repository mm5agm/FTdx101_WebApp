using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Hosting;

namespace FTdx101_WebApp.Services
{
    public class SystemTrayService : IHostedService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private Thread? _thread;
        private NotifyIcon? _notifyIcon;

        public SystemTrayService(IHostApplicationLifetime lifetime)
        {
            _lifetime = lifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _thread = new Thread(RunTray) { IsBackground = true, Name = "SysTray" };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
            return Task.CompletedTask;
        }

        private void RunTray()
        {
            Application.EnableVisualStyles();

            var menu = new ContextMenuStrip();

            var openItem = new ToolStripMenuItem("Open in browser");
            openItem.Click += (_, _) => OpenBrowser();

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (_, _) =>
            {
                _notifyIcon?.Dispose();
                _lifetime.StopApplication();
                Application.ExitThread();
            };

            menu.Items.Add(openItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);

            _notifyIcon = new NotifyIcon
            {
                Icon = LoadIcon(),
                Text = "FTdx101 WebApp",
                ContextMenuStrip = menu,
                Visible = true
            };

            _notifyIcon.DoubleClick += (_, _) => OpenBrowser();

            _lifetime.ApplicationStopping.Register(() =>
            {
                _notifyIcon?.Dispose();
                Application.ExitThread();
            });

            Application.Run();
        }

        private static void OpenBrowser()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "http://localhost:8080",
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private static Icon LoadIcon()
        {
            var icoPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "favicon.ico");
            if (File.Exists(icoPath))
                return new Icon(icoPath);
            return SystemIcons.Application;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _notifyIcon?.Dispose();
            return Task.CompletedTask;
        }
    }
}
