using Gtk;
using MonoDrive.Application.Interfaces;
using NSubstitute;

namespace MonoDrive.Gtk.Test;

[Collection("Gtk")]
public class MainWindowGtkTests(GtkFixture fixture)
{
    private static (IMainWindowPresenter presenter, IFolderPicker folderPicker) CreateMocks(
        string email = "user@example.com",
        string localRoot = null)
    {
        var presenter = Substitute.For<IMainWindowPresenter>();
        presenter.GetUserEmail().Returns(email);
        presenter.GetLocalRootDirectory().Returns(Task.FromResult(localRoot));
        var folderPicker = Substitute.For<IFolderPicker>();
        return (presenter, folderPicker);
    }

    [SkippableFact]
    public async Task LoginAsync_UpdatesUserLabel()
    {
        Skip.If(!fixture.DisplayAvailable, "Requer display (execute com: xvfb-run dotnet test)");
        var (presenter, folderPicker) = CreateMocks(email: "login@example.com");

        var result = await fixture.RunOnGtkThread<string>((app, tcs) =>
        {
            var window = new MainWindow(presenter, folderPicker);
            window.EnsureInitialized(app);

            window.LoginButton_Clicked(window.LoginButton, EventArgs.Empty);

            tcs.SetResult(window.UserLabel.GetText());
            return Task.CompletedTask;
        });

        Assert.Equal("login@example.com", result);
    }

    [SkippableFact]
    public async Task ChooseFolderAsync_WhenPickerReturnsValidPath_EnablesSyncButton()
    {
        Skip.If(!fixture.DisplayAvailable, "Requer display (execute com: xvfb-run dotnet test)");
        var validPath = Path.GetTempPath();
        var (presenter, folderPicker) = CreateMocks();
        folderPicker.PickFolderAsync(Arg.Any<Window>(), Arg.Any<string>())
            .Returns(Task.FromResult(validPath));

        var result = await fixture.RunOnGtkThread<bool>((app, tcs) =>
        {
            var window = new MainWindow(presenter, folderPicker);
            window.EnsureInitialized(app);

            window.ChooseFolderButton_Clicked(window.ChooseFolderButton, EventArgs.Empty);

            tcs.SetResult(window.SyncButton.Sensitive);
            return Task.CompletedTask;
        });

        Assert.True(result);
    }

    [SkippableFact]
    public async Task ChooseFolderAsync_WhenPickerReturnsValidPath_UpdatesFolderLabel()
    {
        Skip.If(!fixture.DisplayAvailable, "Requer display (execute com: xvfb-run dotnet test)");
        var validPath = Path.GetTempPath();
        var (presenter, folderPicker) = CreateMocks();
        folderPicker.PickFolderAsync(Arg.Any<Window>(), Arg.Any<string>())
            .Returns(Task.FromResult(validPath));

        var result = await fixture.RunOnGtkThread<string>((app, tcs) =>
        {
            var window = new MainWindow(presenter, folderPicker);
            window.EnsureInitialized(app);

            window.ChooseFolderButton_Clicked(window.ChooseFolderButton, EventArgs.Empty);

            tcs.SetResult(window.FolderLabel.GetText());
            return Task.CompletedTask;
        });

        Assert.Equal(validPath, result);
    }

    [SkippableFact]
    public async Task ChooseFolderAsync_WhenPickerReturnsNull_KeepsSyncButtonDisabled()
    {
        Skip.If(!fixture.DisplayAvailable, "Requer display (execute com: xvfb-run dotnet test)");
        var (presenter, folderPicker) = CreateMocks();
        folderPicker.PickFolderAsync(Arg.Any<Window>(), Arg.Any<string>())
            .Returns(Task.FromResult<string>(null));

        var result = await fixture.RunOnGtkThread<bool>((app, tcs) =>
        {
            var window = new MainWindow(presenter, folderPicker);
            window.EnsureInitialized(app);

            window.ChooseFolderButton_Clicked(window.ChooseFolderButton, EventArgs.Empty);

            tcs.SetResult(window.SyncButton.Sensitive);
            return Task.CompletedTask;
        });

        Assert.False(result);
    }

    [SkippableFact]
    public async Task OnLoad_WithSavedFolder_EnablesSyncButton()
    {
        Skip.If(!fixture.DisplayAvailable, "Requer display (execute com: xvfb-run dotnet test)");
        var savedPath = Path.GetTempPath();
        var (presenter, folderPicker) = CreateMocks(localRoot: savedPath);

        var result = await fixture.RunOnGtkThread<bool>((app, tcs) =>
        {
            var window = new MainWindow(presenter, folderPicker);
            window.EnsureInitialized(app);

            tcs.SetResult(window.SyncButton.Sensitive);
            return Task.CompletedTask;
        });

        Assert.True(result);
    }
}