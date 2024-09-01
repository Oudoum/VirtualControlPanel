using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using VirtualControlPanel.Models;

namespace VirtualControlPanel.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;

    private readonly SignalRClientService _signalRClient;

    private readonly PmdgCduViewModel[] _pmdgCduViewModels = new PmdgCduViewModel[3];

    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private const string SettingsFile = "settings.json";

    public Settings Settings { get; }

    public HomeViewModel(IDialogService dialogService, SignalRClientService signalRClient)
    {
        _dialogService = dialogService;
        _signalRClient = signalRClient;

        for (int i = 0; i < 3; i++)
        {
            string title = Cdu.Locations[i];
            CduSettings? cduSettings = null;
            try
            {
                string text = File.ReadAllText(title + ".json");
                cduSettings = JsonSerializer.Deserialize<CduSettings>(text);
                _pmdgCduViewModels[i] = new PmdgCduViewModel(title, cduSettings, _signalRClient);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                _pmdgCduViewModels[i] = new PmdgCduViewModel(title, cduSettings ?? new CduSettings(), _signalRClient);
            }
        }

        try
        {
            string text = File.ReadAllText(SettingsFile);
            Settings = JsonSerializer.Deserialize<Settings>(text) ?? new Settings();
        }
        catch (Exception)
        {
            // ignored
        }
        finally
        {
            Settings ??= new Settings();
        }

        _signalRClient.TitleReceived += title => Title = title;
    }

    public async Task Startup()
    {
        if (Settings.IsAutoStart)
        {
            await StartStopConnectionCommand.ExecuteAsync(null);
        }

        OpenPmdgCduLeftCommand.Execute(Settings.IsCduLeftEnabled);
        OpenPmdgCduRightCommand.Execute(Settings.IsCduRightEnabled);
        OpenPmdgCduCenterCommand.Execute(Settings.IsCduCenterEnabled);
    }

    [ObservableProperty]
    private string? _title;

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                string json = JsonSerializer.Serialize(_pmdgCduViewModels[i].CduSettings, _jsonOptions);
                await File.WriteAllTextAsync(Cdu.Locations[i] + ".json", json);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        try
        {
            string json = JsonSerializer.Serialize(Settings, _jsonOptions);
            await File.WriteAllTextAsync(SettingsFile, json);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    [ObservableProperty]
    private bool _isStarted;

    [RelayCommand]
    private async Task StartStopConnection(CancellationToken cancellationToken)
    {
        if (IsStarted)
        {
            await _signalRClient.StopConnectionAsync(cancellationToken);
            IsStarted = false;
            return;
        }

        await _signalRClient.StartConnectionAsync(Settings.IpAddress, Settings.Port, cancellationToken);
        IsStarted = true;
    }
    
    private void OpenPmdgCdu(int index, bool isEnabled)
    {
        PmdgCduViewModel dialogViewModel = _pmdgCduViewModels[index];
        if (isEnabled)
        {
            _dialogService.Show(null, dialogViewModel);
            return;
        }
        
        _dialogService.Close(dialogViewModel);
    }

    [RelayCommand]
    private void OpenPmdgCduLeft(bool isEnabled) => OpenPmdgCdu(0, isEnabled);
    
    [RelayCommand]
    private void OpenPmdgCduRight(bool isEnabled) => OpenPmdgCdu(1, isEnabled);
    
    [RelayCommand]
    private void OpenPmdgCduCenter(bool isEnabled) => OpenPmdgCdu(2, isEnabled);
}