using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAniEvent : MonoBehaviour {

    public delegate void SendArrow(GameObject sender);
    public static event SendArrow OnSendArrow;

    public GameObject weapon;

    //帧事件
    private void ThrowArrow() {
        //将箭隐匿
        weapon.SetActive(false);
        //当到达投掷点时
        //通知场记对武器进行投掷
        if (OnSendArrow != null)
            OnSendArrow(gameObject);
    }

    //拔剑的动画帧事件
    //将箭重新显现
    private void ShowArrow() {
        //激活武器
        weapon.SetActive(true);
    }
}
