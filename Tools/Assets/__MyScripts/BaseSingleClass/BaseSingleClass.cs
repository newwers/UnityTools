using UnityEngine;


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

/// <summary>
/// 针对需要Mono运行生命周期函数,或者需要挂载在场景中
/// 这边需要继承MonoBehaviour 并且限制T类型,否则报错,这边就限制需要继承MonoBehaviour,不过只要继承这个类,就相当于继承MonoBehaviour
/// </summary>
/// <typeparam name="T"></typeparam>
public class BaseMonoSingleClass<T> : MonoBehaviour  where T:MonoBehaviour
{
    private static T mInstance;

    public static T Instance
    {
        get
        {
            if (mInstance == null)
            {
                GameObject go = new GameObject(typeof(T).ToString());
                UnityEngine.Object.DontDestroyOnLoad(go);
                mInstance = go.AddComponent<T>();
            }
            return mInstance;
        }
    }
}




