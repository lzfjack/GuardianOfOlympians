using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//玩家投掷动画帧事件
public class PlayerAniEvent : MonoBehaviour {

    //对投掷点事件进行发布订阅
    public delegate void ThrowPoint();
    public static event ThrowPoint OnThrowPoint;

    //帧事件
	private void ThrowWeapon() {
        //通知场记对武器进行投掷
        if (OnThrowPoint != null)
            OnThrowPoint();
    }
}
