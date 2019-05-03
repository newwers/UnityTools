using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools.FileTool;

public class FileToolTest : MonoBehaviour {


    public string Path;
    public string Content;

    

    [ContextMenu("文件读取单元测试")]
    /// <summary>
    /// 文件读取单元测试
    /// </summary>
    void FileReadTest()
    {
        //给一个有文件夹,有文件的路径
        //Path = Application.dataPath + "/Resources/1.txt";
        //FileTools.ReadFile(Path,System.Text.Encoding.UTF8);
        //结果:正常读取

        //给一个有文件夹,有无文件的路径
        //Path = Application.dataPath + "/Resources/1.md";
        //FileTools.ReadFile(Path, System.Text.Encoding.UTF8);
        //结果:被try catch 捕获到异常

        //给一个无文件夹的路径
        //Path = Application.dataPath + "/Resource/1.md";
        //FileTools.ReadFile(Path, System.Text.Encoding.UTF8);
        //结果:被try catch 捕获到异常
    }


    [ContextMenu("文件写入单元测试")]
    /// <summary>
    /// 文件写入单元测试
    /// </summary>
	void FileWriteTest () {
        //FileExistTest();

        //传递一个有文件夹,没有文件的路径
        //Path = Application.dataPath + "/Resources/1.txt";
        //Content = "this is a test string";
        //FileTools.WriteFile(Path, Content, System.Text.Encoding.UTF8, false);
        //结果:创建一个新的文件,并写入

        //传递一个有文件夹,有文件的路径,覆盖写入
        //Path = Application.dataPath + "/Resources/1.txt";
        //Content = "this is a test string";
        //FileTools.WriteFile(Path, Content, System.Text.Encoding.UTF8, false);
        //结果:正常写入

        //传递一个有文件夹,有文件的路径,追加写入
        //Path = Application.dataPath + "/Resources/1.txt";
        //Content = "this is a append string";
        //FileTools.WriteFile(Path, Content, System.Text.Encoding.UTF8, true);
        //结果:正常写入

        //传递一个无文件夹的路径,覆盖写入
        //Path = Application.dataPath + "/Resource/1.txt";
        //Content = "this is a test string";
        //FileTools.WriteFile(Path, Content, System.Text.Encoding.UTF8, false);
        //结果:被try catch捕获到,文件写入失败
    }

    [ContextMenu("文件存在判断单元测试")]
    /// <summary>
    /// 文件存在判断单元测试
    /// </summary>
    void FileExistTest()
    {
        CheckCodeExecuteTime.StartCheck();
        //传递一个有文件夹,没有文件的路径
        Path = Application.dataPath + "/Resources/1.txt";
        FileTools.ExistFile(Path);//找不到

        //传递一个有文件夹,有文件的路径
        Path = Application.dataPath + "/Resources/文件写入文档.md";
        FileTools.ExistFile(Path);//找到

        //传递一个无文件夹的文件路径
        Path = Application.dataPath + "/Resource/1.md";
        FileTools.ExistFile(Path);//找不到

        CheckCodeExecuteTime.EndCheck();
    }
	
	
}
