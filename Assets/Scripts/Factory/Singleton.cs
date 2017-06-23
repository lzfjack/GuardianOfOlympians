using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//用于单实例化游戏工厂
public class Singleton<T> : MonoBehaviour where T: MonoBehaviour {
    //用于获取飞碟工厂
    protected static T instance;

	public static T Instance {
        get {
            if (instance == null) {
                instance = (T)FindObjectOfType(typeof(T));
                if (instance == null)
                    Debug.Log("Instance GameObject Factory Fail");
            }
            return instance;
        }
    }
}
