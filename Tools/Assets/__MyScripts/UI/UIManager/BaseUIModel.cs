using System;

/// <summary>
/// model基类,每个界面的model都是一个单例,
/// 这样不用挂载在主界面上,进行设置和获取的时候都保证设置的是唯一的对象
/// 原理:model类在继承了此类后,根据继承时把自己当泛型传递过来,然后进行实例化对象并返回
/// 这么设计的目的:在刚打开主界面时,如果没有打开某个界面,那么对这个界面的model层操作都没有效果
/// 例如进入游戏后,要添加背包里面的物品,如果没有打开过背包那么就无法添加
/// todo:但是没有调用那么就没有生成单例对象,还是不能正常储存所以需要在一开始将所有model初始化一次
/// </summary>
public class BaseUIModel<T>
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Activator.CreateInstance<T>();//为传递进来的泛型构造对象
            }
            return instance;
        }
    }
}
