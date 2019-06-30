using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// 管理程序的log日志,会将所有的log输出的本地的txt文件中.
/// 记得在正式发布的时候,把log输出去掉
/// </summary>
public class DebugHandle : MonoBehaviour {

    /// <summary>
    /// log文件路径
    /// </summary>
    public  string LogFilePath ;
    /// <summary>
    /// 报错的Log日志文件路径
    /// </summary>
    public  string ErrorLogFilePath ;
    /// <summary>
    /// 所有Log日志存放的list
    /// </summary>
    public List<string> AllLogList = new List<string>();
    /// <summary>
    /// 报错的Log日志存放的list
    /// </summary>
    public List<string> ErrorLogList = new List<string>();

    //执行顺序:Awake() -》 OnEnable() -》 Start()
	void OnEnable () {
        //初始化路径,不能在定义变量的时候进行初始化,因为Application.persistentDataPath这个是一个变量而不是一个固定的常量
        //注意事项,路径中不能带有不存在的文件夹名称,否则会创建文件失败,只会创建文件不会创建不存在的文件夹
        LogFilePath = Application.persistentDataPath + "/log.txt";
        ErrorLogFilePath = Application.persistentDataPath + "/ErrorLog.txt";
        Application.logMessageReceived +=DebugHandles;
	}

    private void OnDisable()
    {
        Application.logMessageReceived -= DebugHandles;
    }

    private void DebugHandles(string condition, string stackTrace, LogType type)
    {
        //AllLogList.Add($"condition:{condition},stackTrace:{stackTrace}");C# 6.0的语法可以使用$替换站位符的作用
        AllLogList.Add(string.Format("输出日志:{0},堆栈记录:{1}",condition,stackTrace));
        WriteLogFile(LogFilePath, AllLogList[AllLogList.Count - 1]);
        if (type == LogType.Error||type == LogType.Exception)
        {
            ErrorLogList.Add(string.Format("输出日志:{0},堆栈记录:{1}", condition, stackTrace));
            //写入最后一条log数据
            WriteLogFile(ErrorLogFilePath, ErrorLogList[ErrorLogList.Count - 1]);
        }
    }


    void Update () {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.LogError("press the T");
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            //Debug.Log和print都可以正常被打印
            print("press the y");
        }
	}

    /// <summary>
    /// 创建并向文件写入,如果文件存在,则删除已有的,
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="content">文件内容</param>
    public void CreateLogFile(string filePath,byte[] content)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        Stream stream;
        if (fileInfo.Exists)
        {
            fileInfo.Delete();
            stream = fileInfo.Create();

        }
        else
        {
            stream = fileInfo.Create();

        }
        stream.Write(content, 0, content.Length);
        //通常来将dispose会释放对象资源,表示不再调用对象,close表示关闭对象资源,在后面还可以调用.dispose包含Close
        stream.Close();
        stream.Dispose();
    }

    /// <summary>
    /// 向一个路径下的文件写入内容,追加的形式,默认的格式为UTF-8
    /// </summary>
    /// <param name="filePath">File path.</param>
    /// <param name="content">Content.</param>
    public void WriteLogFile(string filePath,string content)
    {
        if (CheckFileExists(filePath))
        {
            //这边的true就是表示追加
            StreamWriter streamWriter = new StreamWriter(filePath, true);
            streamWriter.Write(content, 0, content.Length);
            //这边需要记得释放资源,我没有释放资源导致日志只会输出一条,并且出现错误
            streamWriter.Close();
            streamWriter.Dispose();
        }
        else
        {
            //通过Encoding.UTF8.GetBytes 将string转换成byte[]
            //通过Encoding.UTF8.GetString() 将byte[] 转换成string
            CreateLogFile(filePath, Encoding.UTF8.GetBytes(content));

        }

    }

    /// <summary>
    /// 判断文件是否存在
    /// </summary>
    /// <returns><c>true</c>, if file exists was checked, <c>false</c> otherwise.</returns>
    /// <param name="filePath">File path.</param>
    public bool CheckFileExists(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        return fileInfo.Exists;
    }


    public string ReadFile(string filePath)
    {
        StreamReader streamReader = new StreamReader(filePath);
        string content = streamReader.ReadToEnd();
        streamReader.Close();
        streamReader.Dispose();
        return content;
    }


}
