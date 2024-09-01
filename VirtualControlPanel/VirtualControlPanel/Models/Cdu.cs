using System;
using System.Runtime.InteropServices;

namespace VirtualControlPanel.Models;

public static class Cdu
{
    private const string Name = "CDU";
    
    private const string Left = Name + "Left";
    private const string Right = Name + "Right";
    private const string Center = Name + "Center";

    public static string[] Locations  => [Left, Right, Center];
    
    // CDU Screen Cell Structure
    //

    // CDU Screen Data Structure
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct Screen
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public Row[] Columns;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct Row
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public Cell[] Rows;

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
            public struct Cell
            {
                [MarshalAs(UnmanagedType.U1)]
                public byte Symbol;
                public Color Color; // any of CDU_COLOR_ defines
                public Flags Flags;  // a combination of CDU_FLAG_ bits
            }
        }

        [MarshalAs(UnmanagedType.I1)]
        public bool Powered; // true if CDU is powered
    }
    
    private const int CduColumns = 24;
    private const int CduRows = 14;
    public const int CduCells = CduColumns * CduRows;
    public const int ScreenStateSize = CduCells * 3 + 1; // 3 = Symbol + Color + Flags | 1 = Powered

    public struct ScreenBytes
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ScreenStateSize)]
        public byte[] Data;
    }
    
    public struct CduData(int id, Screen screen)
    {
        public int Id = id;
        public Screen Screen = screen;
    }

    // CDU Screen Cell Colors
    public enum Color : byte
    {
        White,
        Cyan,
        Green,
        Magenta,
        Amber,
        Red
    }

    // CDU Screen Cell flags
    [Flags]
    public enum Flags : byte
    {
        Default = 0x00,
        SmallFont = 0x01, // small font, including that used for line headers 
        Reverse = 0x02,   // character background is highlighted in reverse video
        Unused = 0x04     // dimmed character color indicating inop/unused entries
    }
    
    public enum DataRequestId
    {
        Cdu0 = 1,
        Cdu1,
        Cdu2
    }
}