using System;
using System.Runtime.InteropServices;

public class OpenFile_Window
{
    /// <summary>
    /// ѡ���ļ���
    /// </summary>
    public static string ChooseWinFolder()
    {
        // ʹ�����£�
        OpenDialogDir ofn = new OpenDialogDir();
        ofn.pszDisplayName = new string(new char[2000]); ;     // ���Ŀ¼·��������
        ofn.title = "ѡ���ļ���";// ����
        IntPtr pidlPtr = WindowDll.SHBrowseForFolder(ofn);
        char[] charArray = new char[2000];
        for (int i = 0; i < 2000; i++)
            charArray[i] = '\0';
        WindowDll.SHGetPathFromIDList(pidlPtr, charArray);
        string fullDirPath = new String(charArray);
        return fullDirPath.Substring(0, fullDirPath.IndexOf('\0'));
    }

    /// <summary>
    /// ѡ���ļ�
    /// </summary>
    public static string ChooseWinFile()
    {
        OpenFileName OpenFileName = new OpenFileName();
        OpenFileName.structSize = Marshal.SizeOf(OpenFileName);
        OpenFileName.filter = "�ļ�(*.*)\0*.*";
        OpenFileName.file = new string(new char[1024]);
        OpenFileName.maxFile = OpenFileName.file.Length;
        OpenFileName.fileTitle = new string(new char[64]);
        OpenFileName.maxFileTitle = OpenFileName.fileTitle.Length;
        OpenFileName.title = "ѡ�ļ�";
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
    // �ṹ�嶨��
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
    public String initialDir;  //��·��
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
    // �ṹ�嶨��
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
    // ����ָ��ϵͳ���������ļ��Ի���
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    // ����ָ��ϵͳ���������Ϊ�Ի���
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

    [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern IntPtr SHBrowseForFolder([In, Out] OpenDialogDir ofn);

    [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool SHGetPathFromIDList([In] IntPtr pidl, [In, Out] char[] fileName);
}
