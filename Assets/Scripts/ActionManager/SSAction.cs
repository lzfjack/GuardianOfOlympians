using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSAction : ScriptableObject {

    public bool enable = true;
    public bool destory = false;

    //执行动作的对象
    public GameObject gameobject { get; set; }
    //动作对象的Transform组件
    public Transform transform { get; set; }
    //动作对象的RigidBody组件
    public Rigidbody rigidbody { get; set; }
    //动作归属的管理器
    public ISSActionCallback callback { get; set; }

    //防止用户自己new对象
    protected SSAction() { }

    //将Start和Update定义为虚函数
    //在子类中对函数进行重新定义以实现多态
    public virtual void Start() { }

    public virtual void Update() { }
}
