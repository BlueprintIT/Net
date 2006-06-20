using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace BlueprintIT.Shell
{
  /// <summary>
  /// Enables extraction of icons for any file type from
  /// the Shell.
  /// </summary>
  public class FileDetails
  {
    private static IDictionary<string, FileDetails> details = new Dictionary<string, FileDetails>();
    private static IDictionary<string, int> icons = new Dictionary<string, int>();
    private static ImageList smallIcons = new ImageList();
    private static ImageList largeIcons = new ImageList();
    private static string[] units = { "", "KB", "MB", "GB", "TB" };

    static FileDetails()
    {
      smallIcons.ColorDepth = ColorDepth.Depth32Bit;
      smallIcons.ImageSize = new Size(16, 16);
      largeIcons.ColorDepth = ColorDepth.Depth32Bit;
      largeIcons.ImageSize = new Size(48, 48);
    }

    public static ImageList SmallIcons
    {
      get
      {
        return FileDetails.smallIcons;
      }
    }

    public static ImageList LargeIcons
    {
      get
      {
        return FileDetails.largeIcons;
      }
    }

    public static FileDetails GetFileDetails(FileInfo file)
    {
      if (details.ContainsKey(file.FullName))
        return details[file.FullName];
      FileDetails icon = new FileDetails(file);
      details[file.FullName] = icon;
      return icon;
    }

    public static FileDetails GetFileDetails(string filename)
    {
      return GetFileDetails(new FileInfo(filename));
    }

    private static int GetIconIndex(FileDetails file)
    {
      string extension = file.Extension;
      if (icons.ContainsKey(file.Extension))
        return icons[file.Extension];

      SHGetFileInfoConstants flags = SHGetFileInfoConstants.SHGFI_ICON |
                                     SHGetFileInfoConstants.SHGFI_SMALLICON;

      SHFILEINFO shfi = new SHFILEINFO();
      uint shfiSize = (uint)Marshal.SizeOf(shfi.GetType());

      Icon icon = null;
      int index = -1;
      int ret = SHGetFileInfo(file.Path, 0, ref shfi, shfiSize, (uint)(flags));
      if ((ret != 0) && (shfi.hIcon != IntPtr.Zero))
      {
        icon = Icon.FromHandle(shfi.hIcon);
        SmallIcons.Images.Add(icon);
        index = SmallIcons.Images.Count - 1;
      }

      flags = SHGetFileInfoConstants.SHGFI_ICON |
              SHGetFileInfoConstants.SHGFI_SHELLICONSIZE;

      shfi = new SHFILEINFO();
      shfiSize = (uint)Marshal.SizeOf(shfi.GetType());

      ret = SHGetFileInfo(file.Path, 0, ref shfi, shfiSize, (uint)(flags));
      if ((ret != 0) && (shfi.hIcon != IntPtr.Zero))
      {
        icon = Icon.FromHandle(shfi.hIcon);
        LargeIcons.Images.Add(icon);
        if (index < 0) 
        {
          SmallIcons.Images.Add(icon);
          index = SmallIcons.Images.Count - 1;
        }
      }
      else if (icon != null)
        LargeIcons.Images.Add(icon);

      if ((extension == "EXE") ||
          (extension == "LNK"))
        return index;

      if (extension.Length > 0)
        icons[extension] = index;
      return index;
    }

    #region UnmanagedCode
    private const int MAX_PATH = 260;

    [StructLayout(LayoutKind.Sequential)]
    private struct SHFILEINFO
    {
      public IntPtr hIcon;
      public int iIcon;
      public int dwAttributes;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
      public string szDisplayName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
      public string szTypeName;
    }

    [DllImport("shell32")]
    private static extern int SHGetFileInfo(
       string pszPath,
       int dwFileAttributes,
       ref SHFILEINFO psfi,
       uint cbFileInfo,
       uint uFlags);

    [DllImport("user32.dll")]
    private static extern int DestroyIcon(IntPtr hIcon);

    private const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
    private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
    private const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
    private const int FORMAT_MESSAGE_FROM_STRING = 0x400;
    private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
    private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
    private const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xFF;
    [DllImport("kernel32")]
    private extern static int FormatMessage(
       int dwFlags,
       IntPtr lpSource,
       int dwMessageId,
       int dwLanguageId,
       string lpBuffer,
       uint nSize,
       int argumentsLong);

    [DllImport("kernel32")]
    private extern static int GetLastError();
    #endregion

    #region Enumerations
    /// <summary>
    /// Flags which control FileIcon behaviour
    /// </summary>
    [Flags]
    public enum SHGetFileInfoConstants : int
    {
      /// <summary>
      /// Get icon.  Combine with SHGFI_LARGEICON, SHGFI_SMALLICON,
      /// SHGFI_OPENICON, SHGFI_SHELLICONSIZE, SHGFI_LINKOVERLAY,
      /// SHGFI_SELECTED, SHGFI_ADDOVERLAYS to specify icon
      /// size.
      /// </summary>
      SHGFI_ICON = 0x100,
      /// <summary>
      /// Get the Display name of the file.
      /// </summary>
      SHGFI_DISPLAYNAME = 0x200,
      /// <summary>
      /// Get the TypeName of the file.
      /// </summary>
      SHGFI_TYPENAME = 0x400,
      /// <summary>
      /// Get the attributes of the file.
      /// </summary>
      SHGFI_ATTRIBUTES = 0x800,
      /// <summary>
      /// Get the icon location (not used in this class)
      /// </summary>
      SHGFI_ICONLOCATION = 0x1000,
      /// <summary>
      /// Get the exe type (not used in this class)
      /// </summary>
      SHGFI_EXETYPE = 0x2000,
      /// <summary>
      /// Get the index of the icon in the System Image List (use
      /// vbAccelerator SystemImageList class instead)
      /// </summary>
      SHGFI_SYSICONINDEX = 0x4000,
      /// <summary>
      /// Put a link overlay on icon 
      /// </summary>
      SHGFI_LINKOVERLAY = 0x8000,
      /// <summary>
      /// Show icon in selected state 
      /// </summary>
      SHGFI_SELECTED = 0x10000,
      /// <summary>
      /// Get only specified attributes (not supported in this class)
      /// </summary>
      SHGFI_ATTR_SPECIFIED = 0x20000,
      /// <summary>
      /// get large icon 
      /// </summary>
      SHGFI_LARGEICON = 0x0,
      /// <summary>
      /// get small icon 
      /// </summary>
      SHGFI_SMALLICON = 0x1,
      /// <summary>
      /// get open icon 
      /// </summary>
      SHGFI_OPENICON = 0x2,
      /// <summary>
      /// get shell size icon 
      /// </summary>
      SHGFI_SHELLICONSIZE = 0x4,
      //SHGFI_PIDL = 0x8,                  // pszPath is a pidl 
      /// <summary>
      /// Use passed dwFileAttribute
      /// </summary>
      SHGFI_USEFILEATTRIBUTES = 0x10,
      /// <summary>
      /// Apply the appropriate overlays
      /// </summary>
      SHGFI_ADDOVERLAYS = 0x000000020,
      /// <summary>
      /// Get the index of the overlay (not used in this class)
      /// </summary>
      SHGFI_OVERLAYINDEX = 0x000000040
    }
    #endregion

    private FileInfo file;
    private string displayName;
    private string typeName;
    private int iconIndex;

    private FileDetails(FileInfo file)
    {
      this.file = file;

      typeName = "";
      displayName = "";

      SHGetFileInfoConstants flags = SHGetFileInfoConstants.SHGFI_DISPLAYNAME |
                                     SHGetFileInfoConstants.SHGFI_TYPENAME;

      SHFILEINFO shfi = new SHFILEINFO();
      uint shfiSize = (uint)Marshal.SizeOf(shfi.GetType());

      int ret = SHGetFileInfo(Path, 0, ref shfi, shfiSize, (uint)(flags));
      if (ret != 0)
      {
        typeName = shfi.szTypeName;
        displayName = shfi.szDisplayName;
      }

      iconIndex = GetIconIndex(this);
    }

    public FileInfo File
    {
      get
      {
        return file;
      }
    }

    public string Path
    {
      get
      {
        return file.FullName;
      }
    }

    public string FileName
    {
      get
      {
        return file.Name;
      }
    }

    public string Extension
    {
      get
      {
        string ext = file.Name;
        if (ext.LastIndexOf(".") > 0)
        {
          return ext.Substring(ext.LastIndexOf(".") + 1).ToUpper();
        }
        return "";
      }
    }

    public string ReadableSize
    {
      get
      {
        float size = file.Length;
        int unit = 0;
        while ((size >= 800) && (unit < (units.Length-1)))
        {
          size /= 1024;
          unit++;
        }
        string text;
        if (size > 10)
        {
          text = size.ToString("N0");
        }
        else
        {
          text = size.ToString("N2");
        }
        if (units[unit].Length > 0)
          text += " " + units[unit];
        return text;
      }
    }

    public int IconIndex
    {
      get
      {
        return iconIndex;
      }
    }

    public string DisplayName
    {
      get
      {
        return displayName;
      }
    }

    public string TypeName
    {
      get
      {
        return typeName;
      }
    }
  }
}