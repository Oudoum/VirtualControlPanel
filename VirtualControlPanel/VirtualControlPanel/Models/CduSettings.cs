namespace VirtualControlPanel.Models;

public class CduSettings
{
    public int PositionX { get; set; }
    public int PositionY { get; set; }

    public double Width { get; set; } = 660;
    public double Height { get; set; } = 550;
    
    public double CharacterSize { get; set; } = 50;

    public double MarginTop { get; set; }
    public double MarginBottom { get; set; }
    public double MarginLeft { get; set; }
    public double MarginRight { get; set; }

    public double GridWidth { get; set; } = 620;
    public double GridHeight { get; set; } = 550;

    public double ScaleX { get; set; } = 1;
    public double ScaleY { get; set; } = 1;
}