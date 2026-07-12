using System.Threading.Tasks;
using Gtk;

namespace MonoDrive.Gtk;

public interface IFolderPicker
{
    Task<string> PickFolderAsync(Window parent, string initialPath);
}
