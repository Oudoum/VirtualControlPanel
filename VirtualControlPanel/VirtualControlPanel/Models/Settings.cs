namespace VirtualControlPanel.Models;

public class Settings
{
    public string? IpAddress { get; set; } = "127.0.0.1";
    public int? Port { get; set; } = 2024;
    
    public bool IsAutoStart { get; set; }
    
    public bool IsCduLeftEnabled { get; set; }
    public bool IsCduRightEnabled { get; set; }
    public bool IsCduCenterEnabled { get; set; }
}