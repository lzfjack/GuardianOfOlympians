using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//适配器接口
public interface IActionManager {
    void Play(GameObject gameobject, Vector3 nextPos);
}


