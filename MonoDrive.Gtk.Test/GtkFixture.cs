namespace MonoDrive.Gtk.Test;

/// <summary>
/// Mantém um único Gtk.Application rodando em thread dedicada durante toda a suíte.
/// GTK não suporta múltiplas aplicações por processo, então os testes compartilham este.
/// </summary>
public sealed class GtkFixture : IDisposable
{
    private readonly global::Gtk.Application _app;
    private readonly Thread _gtkThread;
    private readonly ManualResetEventSlim _appReady = new(false);

    public bool DisplayAvailable { get; }

    public GtkFixture()
    {
        DisplayAvailable = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"))
                           || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));

        if (!DisplayAvailable) return;

        global::Gtk.Module.Initialize();
        GirCore.Integration.Initialize();

        _app = global::Gtk.Application.New("org.monodrive.testapp", Gio.ApplicationFlags.FlagsNone);
        _app.OnActivate += (sender, _) =>
        {
            // Janela invisível mantém o app vivo entre os testes
            var win = global::Gtk.ApplicationWindow.New((global::Gtk.Application)sender);
            win.Title = "Test Host";
            _appReady.Set();
        };

        _gtkThread = new Thread(() => _app.RunWithSynchronizationContext(null))
        {
            IsBackground = true,
            Name = "GtkMainLoop"
        };
        _gtkThread.Start();
        _appReady.Wait(TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Agenda uma ação no thread do loop principal do GLib e aguarda sua conclusão.
    /// </summary>
    public Task<T> RunOnGtkThread<T>(Func<global::Gtk.Application, TaskCompletionSource<T>, Task> setup)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        GLib.Functions.IdleAdd(GLib.Constants.PRIORITY_DEFAULT_IDLE, () =>
        {
            _ = setup(_app, tcs);
            return false;
        });

        return tcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
    }

    public void Dispose()
    {
        _app.Quit();
        _gtkThread?.Join(TimeSpan.FromSeconds(3));
    }
}

[CollectionDefinition("Gtk")]
public class GtkCollection : ICollectionFixture<GtkFixture>;
