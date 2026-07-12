using System;
using System.IO;
using MonoDrive.Application.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace MonoDrive.Gtk
{
    public class MainWindow
    {
        private readonly IMainWindowPresenter _mainWindowPresenter;
        private global::Gtk.ApplicationWindow _window;
        private global::Gtk.Label _userLabel;
        private global::Gtk.Label _folderLabel;
        private global::Gtk.Button _chooseFolderButton;
        private global::Gtk.Button _syncButton;
        private global::Gtk.ProgressBar _progressBar;
        private string _selectedFolderPath;

        public MainWindow(IMainWindowPresenter mainWindowPresenter)
        {
            _mainWindowPresenter = mainWindowPresenter;
        }

        public void EnsureInitialized(global::Gtk.Application app)
        {
            if (_window != null)
            {
                return;
            }

            _window = global::Gtk.ApplicationWindow.New(app);
            _window.Title = "knuxbbs Open Drive";
            _window.SetDefaultSize(480, 240);

            var layout = global::Gtk.Box.New(global::Gtk.Orientation.Vertical, 8);
            layout.SetMarginStart(8);
            layout.SetMarginEnd(8);
            layout.SetMarginTop(8);
            layout.SetMarginBottom(8);

            _userLabel = global::Gtk.Label.New("[userEmail]");
            _userLabel.SetHalign(global::Gtk.Align.Start);
            layout.Append(_userLabel);

            _folderLabel = global::Gtk.Label.New("[syncRootFolder]");
            _folderLabel.SetHalign(global::Gtk.Align.Start);
            layout.Append(_folderLabel);

            _chooseFolderButton = global::Gtk.Button.NewWithLabel("Escolher pasta…");
            _chooseFolderButton.OnClicked += ChooseFolderButton_Clicked;
            layout.Append(_chooseFolderButton);

            var loginButton = global::Gtk.Button.NewWithLabel("Login");
            loginButton.OnClicked += LoginButton_Clicked;
            layout.Append(loginButton);

            _syncButton = global::Gtk.Button.NewWithLabel("Sync");
            _syncButton.Sensitive = false;
            _syncButton.OnClicked += SyncButton_Clicked;
            layout.Append(_syncButton);

            _progressBar = global::Gtk.ProgressBar.New();
            _progressBar.ShowText = true;
            _progressBar.Text = "Aguardando sincronização";
            layout.Append(_progressBar);

            _window.Child = layout;

            _ = LoadContentAsync();
        }

        private async Task LoadContentAsync()
        {
            _userLabel.SetText(await _mainWindowPresenter.GetUserEmail());

            var localRootDirectory = await _mainWindowPresenter.GetLocalRootDirectory();

            if (!string.IsNullOrEmpty(localRootDirectory))
            {
                SetSelectedFolder(localRootDirectory);
            }
        }

        public void Show() => _window?.Present();

        private async void ChooseFolderButton_Clicked(global::Gtk.Button sender, EventArgs args)
        {
            try
            {
                var dialog = global::Gtk.FileDialog.New();
                dialog.SetTitle("Selecione a pasta de sincronização");

                if (!string.IsNullOrEmpty(_selectedFolderPath) && Directory.Exists(_selectedFolderPath))
                    dialog.SetInitialFolder(Gio.FileHelper.NewForPath(_selectedFolderPath));

                var folder = await dialog.SelectFolderAsync(_window);
                var path = folder?.GetPath();

                if (!string.IsNullOrEmpty(path))
                    SetSelectedFolder(path);
            }
            catch (Exception ex)
            {
                _progressBar.Text = $"Erro ao selecionar pasta: {ex.Message}";
            }
        }

        private void SetSelectedFolder(string path)
        {
            _selectedFolderPath = path;
            _folderLabel.SetText(path);
            _syncButton.Sensitive = Directory.Exists(path);
        }

        private async void LoginButton_Clicked(global::Gtk.Button sender, EventArgs args)
        {
            try
            {
                _userLabel.SetText(await _mainWindowPresenter.GetUserEmail());
            }
            catch (Exception ex)
            {
                _progressBar.Text = $"Erro ao fazer login: {ex.Message}";
            }
        }

        private async void SyncButton_Clicked(global::Gtk.Button sender, EventArgs args)
        {
            try
            {
                if (!Directory.Exists(_selectedFolderPath))
                {
                    _progressBar.Text = "Diretório inválido.";
                    return;
                }

                _progressBar.Text = "Sincronizando...";
                await _mainWindowPresenter.Sync(_selectedFolderPath);
                _progressBar.Text = "Sincronização concluída.";
            }
            catch (Exception ex)
            {
                _progressBar.Text = $"Erro na sincronização: {ex.Message}";
            }
        }
    }
}