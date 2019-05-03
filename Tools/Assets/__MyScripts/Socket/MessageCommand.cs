

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
    private byte m_Model;
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
    private string m_Message;

    public byte Model
    {
        get
        {
            return m_Model;
        }

        set
        {
            m_Model = value;
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

    public string Message
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
        Model = model;
        Order = order;
        Size = size;
    }

    public MessageCommand(byte model, byte order, string message)
    {
        Model = model;
        Order = order;
        
        Message = message;
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        Size = messageBytes.Length;
    }
    //public byte[] Content;


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
