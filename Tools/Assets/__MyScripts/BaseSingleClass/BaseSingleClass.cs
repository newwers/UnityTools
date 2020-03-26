//单例基类
//目的:统一管理单例实现,方便代码书写,减少重复编写
//知识点:静态 static 泛型
public class BaseSingleClass<T> where T : new()//约束子类必须有无参构造函数,结构体没有无参构造函数
{
    private static T mInstance;

    public static T Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new T();
            }
            return mInstance;
        }
    }

}




