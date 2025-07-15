using UnityEngine;


//单例基类
//目的:统一管理单例实现,方便代码书写,减少重复编写
//知识点:静态 static 泛型
public class BaseSingleClass<T> where T : new()//约束子类必须有无参构造函数,结构体没有无参构造函数
{
    #region Single
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
    #endregion


}

/// <summary>
/// 针对需要Mono运行生命周期函数,或者需要挂载在场景中
/// 这边需要继承MonoBehaviour 并且限制T类型,否则报错,这边就限制需要继承MonoBehaviour,不过只要继承这个类,就相当于继承MonoBehaviour
/// 
/// 注意:针对单例场景切换回来后,重复存在问题,单例物体不能重复加载,需要单独提取出来,而且单例身上引用的组件,也需要一起设置为dontDestroy才行,否则场景重复加载时,丢失引用
/// </summary>
/// <typeparam name="T"></typeparam>
public class BaseMonoSingleClass<T> : MonoBehaviour where T : MonoBehaviour
{
    #region Single
    /// <summary>
    /// 不要在Awake方法中访问其他单例实例（初始化顺序不确定）
    /// </summary>
    protected virtual void Awake()
    {
        if (mInstance)//如果已经有了该对象,销毁新创建的对象
        {
            Destroy(gameObject);
            return;
        }
        mInstance = this as T;
        DontDestroyOnLoad(this.gameObject);
    }

    protected static T mInstance;

    public static T Instance
    {
        get
        {
            if (mInstance == null)
            {
                var t = Object.FindFirstObjectByType<T>();
                if (t != null)
                {
                    mInstance = t;
                }
                else
                {
                    GameObject go = new GameObject(typeof(T).ToString());
                    mInstance = go.AddComponent<T>();
                }
            }
            UnityEngine.Object.DontDestroyOnLoad(mInstance.gameObject);
            return mInstance;
        }
    }
    #endregion

}




