using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeeView.Properties;
using System.Threading.Tasks;

namespace NeeView;

public partial class TaskTrayIconViewModel : ObservableObject
{
    public string ExitApplicationHeader => TextResources.GetString("CloseApplicationCommand.Menu");

    [RelayCommand]
    public async Task ShowWindow()
    {
        await AppState.Current.ShowWindow();
    }

    [RelayCommand]
    public void ExitApplication()
    {
        AppState.Current.Shutdown();
    }
}

