using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.MVVM.ViewModel.Window;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.View;

public partial class ApplicationInfoSubWindow : Window, IMenuWindow<ApplicationInfoWindowModel>
{
    public ApplicationInfoSubWindow()
    {
        InitializeComponent();
    }
}
