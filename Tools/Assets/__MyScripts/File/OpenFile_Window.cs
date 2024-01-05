using System;
using System.Runtime.InteropServices;

public class OpenFile_Window
{
    /// <summary>
    /// 选择文件夹
    /// </summary>
    public static string ChooseWinFolder()
    {
        // 使用如下：
        OpenDialogDir ofn = new OpenDialogDir();
        ofn.pszDisplayName = new string(new char[2000]); ;     // 存放目录路径缓冲区
        ofn.title = "选择文件夹";// 标题
        IntPtr pidlPtr = WindowDll.SHBrowseForFolder(ofn);
        char[] charArray = new char[2000];
        for (int i = 0; i < 2000; i++)
            charArray[i] = '\0';
        WindowDll.SHGetPathFromIDList(pidlPtr, charArray);
        string fullDirPath = new String(charArray);
        return fullDirPath.Substring(0, fullDirPath.IndexOf('\0'));
    }

    /// <summary>
    /// 选择文件
    /// </summary>
    public static string ChooseWinFile()
    {
        OpenFileName OpenFileName = new OpenFileName();
        OpenFileName.structSize = Marshal.SizeOf(OpenFileName);
        OpenFileName.filter = "文件(*.*)\0*.*";
        OpenFileName.file = new string(new char[1024]);
        OpenFileName.maxFile = OpenFileName.file.Length;
        OpenFileName.fileTitle = new string(new char[64]);
        OpenFileName.maxFileTitle = OpenFileName.fileTitle.Length;
        OpenFileName.title = "选文件";
        OpenFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
        if (WindowDll.GetOpenFileName(OpenFileName))
            return OpenFileName.file.Trim('\0');
        else
            return null;
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct OpenFileName
{
    // 结构体定义
    public int structSize;
    public IntPtr dlgOwner;
    public IntPtr instance;
    public String filter;
    public String customFilter;
    public int maxCustFilter;
    public int filterIndex;
    public String file;
    public int maxFile;
    public String fileTitle;
    public int maxFileTitle;
    public String initialDir;  //打开路径
    public String title;
    public int flags;
    public short fileOffset;
    public short fileExtension;
    public String defExt;
    public IntPtr custData;
    public IntPtr hook;
    public String templateName;
    public IntPtr reservedPtr;
    public int reservedInt;
    public int flagsEx;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct OpenDialogDir
{
    // 结构体定义
    public IntPtr hwndOwner;
    public IntPtr pidlRoot;
    public string pszDisplayName;
    public string title;
    public UInt32 ulFlags;
    public IntPtr lpfno;
    public IntPtr lParam;
    public int iImage;
}

public class WindowDll
{
    // 链接指定系统函数，打开文件对话框
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    // 链接指定系统函数，另存为对话框
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

    [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern IntPtr SHBrowseForFolder([In, Out] OpenDialogDir ofn);

    [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool SHGetPathFromIDList([In] IntPtr pidl, [In, Out] char[] fileName);
}
