using System.IO;
using System.Threading.Tasks;
using Gtk;

namespace MonoDrive.Gtk;

public class GtkFolderPicker : IFolderPicker
{
    public async Task<string> PickFolderAsync(Window parent, string initialPath)
    {
        var dialog = FileDialog.New();
        dialog.SetTitle("Selecione a pasta de sincronização");

        if (!string.IsNullOrEmpty(initialPath) && Directory.Exists(initialPath))
            dialog.SetInitialFolder(Gio.FileHelper.NewForPath(initialPath));

        var folder = await dialog.SelectFolderAsync(parent);
        return folder?.GetPath() ?? string.Empty;
    }
}
