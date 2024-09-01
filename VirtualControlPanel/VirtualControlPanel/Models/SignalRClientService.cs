using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace VirtualControlPanel.Models;

public class SignalRClientService
{
    private HubConnection? _connection;
    
    public event Action<string?>? TitleReceived;
    public event Action<string, byte[]>? PmdgDataReceived;
    
    public async Task StartConnectionAsync(string? ipAddress, int? port, CancellationToken cancellationToken)
    {
        if (!IPAddress.TryParse(ipAddress, out IPAddress? address))
        {
            address = IPAddress.Loopback;
        }

        port ??= 2024;
        
        _connection = new HubConnectionBuilder()
            .WithUrl($"http://{address}:{port}/datahub")
            .WithAutomaticReconnect()
            .AddMessagePackProtocol()
            .Build();
        
        _connection.On<byte, byte[]>("SendPmdgData", ReceivePmdgDataMessage);
        _connection.On<string?>("SendTitle", ReceiveTitleMessage);
        
        try
        {
            await _connection.StartAsync(cancellationToken);
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    public async Task StopConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is null)
        {
            return;
        }

        try
        {
            await _connection.StopAsync(cancellationToken);
            ClearData();
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    private void ReceiveTitleMessage(string? title)
    {
        TitleReceived?.Invoke(title);
    }
    
    private void ReceivePmdgDataMessage(byte id, byte[] receivedMessage)
    {
        Cdu.DataRequestId dataRequestId = (Cdu.DataRequestId)id;
        
        switch (dataRequestId)
        {
            case Cdu.DataRequestId.Cdu0:
            case Cdu.DataRequestId.Cdu1:
            case Cdu.DataRequestId.Cdu2:
                string location = Cdu.Locations[(byte)dataRequestId - 1];
                PmdgDataReceived?.Invoke(location, receivedMessage);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(id), id, null);
        }
    }

    private void ClearData()
    {
        ReceiveTitleMessage(null);
        byte[] data = new byte[Cdu.ScreenStateSize];
        ReceivePmdgDataMessage((byte)Cdu.DataRequestId.Cdu0, data);
        ReceivePmdgDataMessage((byte)Cdu.DataRequestId.Cdu1, data);
        ReceivePmdgDataMessage((byte)Cdu.DataRequestId.Cdu2, data);
    }
}