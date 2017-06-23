using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//早期版本
//简单的向指定位置transform
public class CCAction : SSAction {
    //巡逻目的地
    public Vector3 destination;

    //获取一个动作
    public static CCAction GetSSAction(Vector3 nextPos) {
        CCAction action = ScriptableObject.CreateInstance<CCAction>();
        action.destination = nextPos;
        return action;
    }

    //动作
    public override void Update() {
        //获取怪物的数据脚本
        MonsterData data = gameobject.GetComponent<MonsterData>();

        //到达目的地
        //进入塔防范围
        //或者怪物中途被消灭
        if (transform.position == destination ||
            data.isInTower == true ||
            data.isDie == true) {
            //销毁当前动作
            this.destory = true;
            //动作完成后，通知任务管理器
            this.callback.SSActionEvent(this);
        }
        //向目的地移动
        else {
            //运动学运动
            Vector3 nextStep = Vector3.MoveTowards(transform.position, destination, 0.005f);
            transform.position = nextStep;
        }
    }

    public override void Start() {
        base.Start();
    }
}
