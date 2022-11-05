/*
 脚本生命周期
 */

namespace zdq.Behaviour
{
    //用抽象类的原因是,如果用接口,那么每个继承的都要实现函数,抽象类可以选择性的重写即可
    public abstract class ScriptBehaviour
    {
        /// <summary>
        /// 第一次创建时调用一次
        /// </summary>
        public virtual void Awake() { }
        /// <summary>
        /// 每次重新激活时,调用一次
        /// </summary>
        public virtual void OnEnable() { }
        /// <summary>
        /// 第一次创建时,在Awake后面调用一次
        /// Start的目的就是为了和Awake函数调用有个顺序的区分
        /// </summary>
        public virtual void Start() { }
        /// <summary>
        /// 每帧调用
        /// </summary>
        public virtual void Update(float frame) { }
        /// <summary>
        /// 隐藏或者销毁前调用一次
        /// </summary>
        public virtual void OnDisable() { }
        /// <summary>
        /// 销毁时调用一次
        /// </summary>
        public virtual void OnDestroy() { }
    }
}