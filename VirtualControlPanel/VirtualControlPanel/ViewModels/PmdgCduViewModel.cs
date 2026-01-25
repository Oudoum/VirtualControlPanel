using System;
using CommunityToolkit.Mvvm.ComponentModel;
using VirtualControlPanel.Models;
using VirtualControlPanel.Services;

namespace VirtualControlPanel.ViewModels;

public class PmdgCduViewModel : ObservableObject
{
    public string? Title { get; }
    public CduSettings CduSettings { get; private set; }
    public byte[] CduScreenData { get; private set; } = [];

    public event Action? ScreenUpdated;

    public PmdgCduViewModel(string? title, CduSettings? cduSettings, SignalRClientService signalRClientService)
    {
        Title = title;
        CduSettings = cduSettings ?? new CduSettings();
        signalRClientService.PmdgDataReceived += (location, cduScreenData) =>
        {
            if (location != Title)
            {
                return;
            }

            CduScreenData = cduScreenData;
            ScreenUpdated?.Invoke();
        };
    }
}