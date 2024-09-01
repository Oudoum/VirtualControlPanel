using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using VirtualControlPanel.Models;
using VirtualControlPanel.ViewModels;

namespace VirtualControlPanel;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Ioc.Default.ConfigureServices(new ServiceCollection()
            .AddSingleton<IDialogService, DialogService>(provider => new DialogService(new DialogManager(new ViewLocator(), new DialogFactory().AddDialogHost()), provider.GetService))
            .AddSingleton<SignalRClientService>()
            .AddSingleton<MainViewModel>()
            .AddSingleton<HomeViewModel>()
            .AddSingleton<HelpViewModel>()
            .AddTransient<PmdgCduViewModel>()
            .BuildServiceProvider());
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        GC.KeepAlive(typeof(DialogService));
        IDialogService? dialogService = Ioc.Default.GetService<IDialogService>();
        MainViewModel? mainViewModel = Ioc.Default.GetService<MainViewModel>();

        if (mainViewModel is not null)
        {
            dialogService?.Show(null, mainViewModel);
        }
        
        base.OnFrameworkInitializationCompleted();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
        
        HomeViewModel? homeViewModel = Ioc.Default.GetService<HomeViewModel>();
        if (homeViewModel is not null)
        {
            await homeViewModel.Startup();
        }
    }
}