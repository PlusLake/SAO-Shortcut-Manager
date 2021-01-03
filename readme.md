# SAO Shortcut Manager

This software provides a SAO-like simple shortcut launcher.  
For people who don't know SAO (Sword Art Online, ソード・アート・オンライン), go check the Wikipedia below:  

https://ja.wikipedia.org/wiki/%E3%82%BD%E3%83%BC%E3%83%89%E3%82%A2%E3%83%BC%E3%83%88%E3%83%BB%E3%82%AA%E3%83%B3%E3%83%A9%E3%82%A4%E3%83%B3

## Screenshots and Manuals

TBD.

## How does it work?

The core of this software is [PrivateExtractIcons](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-privateextracticonsa) in Win32 API.  
It can extract original path of a Windows shortcut(＊.lnk files).  
(So maybe I'll try to find out equivalents in linux/gnome and make a linux ver.)  

```
[DllImport("User32.dll", CharSet = CharSet.Auto)]
internal static extern UInt32 PrivateExtractIcons(String lpszFile, int nIconIndex, int cxIcon, int cyIcon, IntPtr[] phicon, IntPtr[] piconid, UInt32 nIcons, UInt32 flags);

// example
static Bitmap whatever(string path) {
    IntPtr[] phicon = new IntPtr[] { IntPtr.Zero };
    PrivateExtractIcons(path, 0, 64, 64, phicon, new IntPtr[] { IntPtr.Zero }, 1, 0);
    // Use System.Drawing.Icon to convert to Bitmap
    return phicon[0] != IntPtr.Zero ? Icon.FromHandle(phicon[0]).ToBitmap() : null;
}
```

And UI is actually Windows Forms. (Nothing special)

## System requiements

TBD.  
*Only confirmed in windows7 currently.*

## Remarks

This software is actually created in around 2014 with VB.NET and remake it in 2020 with C#.NET.  
I am not deciding to add more functions on it but may fix bugs when bugs are found.

