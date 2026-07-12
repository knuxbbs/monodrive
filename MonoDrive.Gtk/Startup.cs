using System;
using Microsoft.Extensions.Logging;

namespace MonoDrive.Gtk
{
    public class Startup
    {
        private readonly MainWindow _mainWindow;
        private readonly ILogger<Startup> _logger;

        public Startup(MainWindow mainWindow, ILogger<Startup> logger)
        {
            _mainWindow = mainWindow;
            _logger = logger;
        }

        public void Run()
        {
            var app = global::Gtk.Application.New("com.knuxbbs.monodrive", Gio.ApplicationFlags.FlagsNone);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            app.OnActivate += (_, _) =>
            {
                _mainWindow.EnsureInitialized(app);
                _mainWindow.Show();
            };

            app.RunWithSynchronizationContext(null);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            if (args.ExceptionObject is Exception unhandledException)
            {
                _logger.LogError(unhandledException, unhandledException.Message);
            }
        }
    }
}