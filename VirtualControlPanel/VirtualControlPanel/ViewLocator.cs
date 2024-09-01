using HanumanInstitute.MvvmDialogs.Avalonia;
using VirtualControlPanel.ViewModels;
using VirtualControlPanel.Views;

namespace VirtualControlPanel;

public class ViewLocator : StrongViewLocator
{
    public ViewLocator()
    {
        Register<MainViewModel, MainView, MainWindow>();
        Register<HomeViewModel, HomeView>();
        Register<HelpViewModel, HelpView>();
        Register<PmdgCduViewModel, PmdgCduView, PmdgCduWindow>();
    }
}