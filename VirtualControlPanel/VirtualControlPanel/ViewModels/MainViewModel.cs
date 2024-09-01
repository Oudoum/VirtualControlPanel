using CommunityToolkit.Mvvm.ComponentModel;

namespace VirtualControlPanel.ViewModels;

public class MainViewModel : ObservableObject
{
    public HomeViewModel HomeViewModel { get; }
    public HelpViewModel HelpViewModel { get; }
    
    public MainViewModel(HomeViewModel homeViewModel, HelpViewModel helpViewModel)
    {
        HomeViewModel = homeViewModel;
        HelpViewModel = helpViewModel;
    }
}