

using System;
using System.Text;
/// <summary>
/// 定义发送消息的格式
/// 接收的第一个字节代表长度
/// 模块(byte) + 指令(byte) + 后面内容长度(4个byte) + 内容
/// 1个byte的范围是从0-255
/// 4个byte相当于1个int类型 长度从-2,147,483,648 到 2,147,483,647 
/// 0到2147483647 是{0,0,0,0}到{255,255,255,127}  
/// -2147483648 到-1 是从{0,0,0,128} 到{255,255,255,255}
/// 相当于256进制
/// 模块 + 指令 可以组成256*256 = 65536种操作指令
/// </summary>
public class MessageCommand  {

    /// <summary>
    /// 模块从0-255
    /// </summary>
    private byte m_Module;
    /// <summary>
    /// 指令从0-255
    /// </summary>
    private byte m_Order;
    /// <summary>
    /// 内容大小
    /// </summary>
    private int m_Size;

    /// <summary>
    /// 存放接收到的指令
    /// </summary>
    private byte[] m_Message;

    public byte Module
    {
        get
        {
            return m_Module;
        }

        set
        {
            m_Module = value;
        }
    }

    public byte Order
    {
        get
        {
            return m_Order;
        }

        set
        {
            m_Order = value;
        }
    }

    public int Size
    {
        get
        {
            return m_Size;
        }

        set
        {
            m_Size = value;
        }
    }

    public byte[] Message
    {
        get
        {
            return m_Message;
        }

        set
        {
            m_Message = value;
        }
    }

    public MessageCommand(byte model, byte order, int size)
    {
        Module = model;
        Order = order;
        Size = size;

        Message = new byte[size];
    }

    public MessageCommand(byte model, byte order, byte[] message)
    {
        Module = model;
        Order = order;
        
        Message = message;
        Size = Message.Length;
    }

    #region 数据类型获取

    

    /// <summary>
    /// 读取字节数组的索引
    /// </summary>
    private int m_readIndex;

    /// <summary>
    /// 读取有符号整数
    /// int 4字节
    /// </summary>
    /// <returns></returns>
    public int GetInt()
    {
        int value =  BitConverter.ToInt32(Message, m_readIndex);
        //Convert.ToInt32()这个只能转字节,没有字节数组
        m_readIndex += 4;

        return value;
    }

    /// <summary>
    /// 获取字符串
    /// 由于字符串长度为止,所以约定在开头存放一个4字节作为长度
    /// </summary>
    /// <returns></returns>
    public string GetString()
    {
        //先读取前面4个长度字节作为数组长度
        int length = GetInt();
        string value = BitConverter.ToString(Message, m_readIndex, length);
        m_readIndex += length;

        return value;
    }

    /// <summary>
    /// 获取浮点数
    /// 浮点数float 32位 4字节
    /// </summary>
    /// <returns></returns>
    public float GetFloat()
    {
        float value = BitConverter.ToSingle(Message, m_readIndex);
        m_readIndex += 4;

        return value;
    }

    #endregion

    #region 数据类型写入

    


    /// <summary>
    /// 写入字节数组的索引
    /// </summary>
    private int m_writeIndex;

    /// <summary>
    /// 写入浮点数保存到要发送的数组里
    /// </summary>
    /// <param name="value"></param>
    public void WriteFloat(float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);

        //Array.Copy(bytes,Message,)数组的赋值,但是不能指定开始位置
        Buffer.BlockCopy(bytes, 0, Message, m_readIndex, 4);

        m_readIndex += 4;
    }

    public void WriteInt(int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);

        Buffer.BlockCopy(bytes, 0, Message, m_readIndex, 4);

        m_readIndex += 4;
    }

    /// <summary>
    /// 写入字符串,约定开头4字节为字符串长度
    /// </summary>
    /// <param name="value"></param>
    public void WriteString(String value)
    {
        //写入字符串长度
        WriteInt(value.Length);

        byte[] bytes = Encoding.UTF8.GetBytes(value);

        Buffer.BlockCopy(bytes, 0, Message, m_readIndex, 4);

        m_readIndex += bytes.Length;
    }

    #endregion


}

/// <summary>
/// 模块命令
/// </summary>
public static class ModelCmd
{
    /// <summary>
    /// 0开头模块,作用:进行测试
    /// </summary>
    public static byte TestModelCmd = 0;
}

/// <summary>
/// 指令命令
/// </summary>
public static class OrderCmd
{
    /// <summary>
    /// 测试输出HellowWorld指令
    /// </summary>
    public static byte HelloWorldOrderCmd = 0;
}
