using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//最终的动作管理器CCActionManager的父类
public class SSActionManager : MonoBehaviour {
    //用字典对动作根据每个object唯一的instanceID进行存储
    protected Dictionary<int, SSAction> actions = new Dictionary<int, SSAction>();
    //等待被添加的动作
    protected List<SSAction> waitingAdd = new List<SSAction>();
    //等待被删除的动作
    protected List<int> waitingDelete = new List<int>();

    //添加动作
    public void RunAction(GameObject gameobject, SSAction action, ISSActionCallback manager) {
        action.gameobject = gameobject;
        action.transform = gameobject.transform;
        action.callback = manager;
        waitingAdd.Add(action);
        action.Start();
    }

    //动作管理器每一帧都对需要执行的动作进行执行
    protected void Update () {
        //首先将等待添加链表中动作加入动作字典中
        foreach (SSAction action in waitingAdd)
            actions[action.GetInstanceID()] = action;
        //清空等待添加链表
        waitingAdd.Clear();

        //依次执行当前词典中需要执行的所有动作
        foreach(KeyValuePair<int, SSAction> kv in actions) {
            SSAction action = kv.Value;
            if (action.destory)
                waitingDelete.Add(action.GetInstanceID());
            else if (action.enable) {
                action.Update();
            }
        }

        //依次对等待删除链表中的动作进行删除
        foreach(int key in waitingDelete) {
            SSAction action = actions[key];
            //从词典中移除动作
            actions.Remove(key);
            //对动作进行销毁
            //组合动作之所以要写OnDestory，原因就在这里
            //组合动作被唯一的记录在词典中，销毁时，需要额外对其中包含的子动作进行销毁回收
            DestroyObject(action);
        }
        //清空等待删除链表
        waitingDelete.Clear();
	}


    //对所有动作进行清理
    public void Clear() {
        //清理等待链表
        foreach (SSAction action in waitingAdd)
            DestroyObject(action);
        //清空等待添加链表
        waitingAdd.Clear();

        //清理字典
        foreach (KeyValuePair<int, SSAction> kv in actions) {
            SSAction action = kv.Value;
            DestroyObject(action);
        }
        actions.Clear();

        //等待删除的必然还存在与字典，随字典清理而清理，故只需清空等待删除链表即可
        waitingDelete.Clear();
    }
}
