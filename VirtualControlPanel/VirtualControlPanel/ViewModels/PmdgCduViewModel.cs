using CommunityToolkit.Mvvm.ComponentModel;
using VirtualControlPanel.Models;

namespace VirtualControlPanel.ViewModels;

public class PmdgCduViewModel(string? title, CduSettings? cduSettings, SignalRClientService signalRClientService) : ObservableObject
{
    public string? Title { get; private set; } = title;
    public CduSettings CduSettings { get; private set; } = cduSettings ?? new CduSettings();

    public SignalRClientService SignalRClientService { get; private set; } = signalRClientService;
}