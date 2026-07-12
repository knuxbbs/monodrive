using System;
using System.IO;
using System.Runtime.CompilerServices;
using Gtk;
using MonoDrive.Application.Interfaces;
using Task = System.Threading.Tasks.Task;

[assembly: InternalsVisibleTo("MonoDrive.Gtk.Test")]

namespace MonoDrive.Gtk
{
    public class MainWindow
    {
        private readonly IMainWindowPresenter _mainWindowPresenter;
        private readonly IFolderPicker _folderPicker;
        private string _selectedFolderPath;

        private ApplicationWindow _window;
        internal Label UserLabel { get; private set; }
        internal Label FolderLabel { get; private set; }
        internal Button ChooseFolderButton { get; private set; }
        internal Button LoginButton { get; private set; }
        internal Button SyncButton { get; private set; }
        private ProgressBar ProgressBar { get; set; }

        public MainWindow(IMainWindowPresenter mainWindowPresenter, IFolderPicker folderPicker)
        {
            _mainWindowPresenter = mainWindowPresenter;
            _folderPicker = folderPicker;
        }

        public void EnsureInitialized(global::Gtk.Application app)
        {
            _window = ApplicationWindow.New(app);
            _window.Title = "knuxbbs Open Drive";
            _window.SetDefaultSize(480, 240);

            var layout = Box.New(Orientation.Vertical, 8);
            layout.SetMarginStart(8);
            layout.SetMarginEnd(8);
            layout.SetMarginTop(8);
            layout.SetMarginBottom(8);

            UserLabel = Label.New("[userEmail]");
            UserLabel.SetHalign(Align.Start);
            layout.Append(UserLabel);

            FolderLabel = Label.New("[syncRootFolder]");
            FolderLabel.SetHalign(Align.Start);
            layout.Append(FolderLabel);

            ChooseFolderButton = Button.NewWithLabel("Escolher pasta…");
            ChooseFolderButton.OnClicked += ChooseFolderButton_Clicked;
            layout.Append(ChooseFolderButton);

            LoginButton = Button.NewWithLabel("Login");
            LoginButton.OnClicked += LoginButton_Clicked;
            layout.Append(LoginButton);

            SyncButton = Button.NewWithLabel("Sync");
            SyncButton.Sensitive = false;
            SyncButton.OnClicked += SyncButton_Clicked;
            layout.Append(SyncButton);

            ProgressBar = ProgressBar.New();
            ProgressBar.ShowText = true;
            ProgressBar.Text = "Aguardando sincronização";
            layout.Append(ProgressBar);

            _window.Child = layout;

            _ = LoadContentAsync();
        }

        private async Task LoadContentAsync()
        {
            UserLabel.SetText(await _mainWindowPresenter.GetUserEmail());

            var localRootDirectory = await _mainWindowPresenter.GetLocalRootDirectory();

            if (!string.IsNullOrEmpty(localRootDirectory))
                SetSelectedFolder(localRootDirectory);
        }

        public void Show() => _window.Present();

        public async void ChooseFolderButton_Clicked(Button sender, EventArgs args)
        {
            try
            {
                var path = await _folderPicker.PickFolderAsync(_window, _selectedFolderPath);
                if (!string.IsNullOrEmpty(path))
                    SetSelectedFolder(path);
            }
            catch (Exception ex)
            {
                ProgressBar.Text = $"Erro ao selecionar pasta: {ex.Message}";
            }
        }

        private void SetSelectedFolder(string path)
        {
            _selectedFolderPath = path;
            FolderLabel.SetText(path);
            SyncButton.Sensitive = Directory.Exists(path);
        }

        public async void LoginButton_Clicked(Button sender, EventArgs args)
        {
            try
            {
                UserLabel.SetText(await _mainWindowPresenter.GetUserEmail());
            }
            catch (Exception ex)
            {
                ProgressBar.Text = $"Erro ao fazer login: {ex.Message}";
            }
        }

        private async void SyncButton_Clicked(Button sender, EventArgs args)
        {
            try
            {
                if (!Directory.Exists(_selectedFolderPath))
                {
                    ProgressBar.Text = "Diretório inválido.";
                    return;
                }

                ProgressBar.Text = "Sincronizando...";
                await _mainWindowPresenter.Sync(_selectedFolderPath);
                ProgressBar.Text = "Sincronização concluída.";
            }
            catch (Exception ex)
            {
                ProgressBar.Text = $"Erro na sincronização: {ex.Message}";
            }
        }
    }
}
