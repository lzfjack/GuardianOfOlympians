using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//场景动画帧事件
public class SurroundingAniEvent : MonoBehaviour {

    //对场景动画播放完毕事件进行发布
    public delegate void SceneAniFin();
    public static event SceneAniFin OnSceneAniFin;

    bool isFIN = false;

    //帧事件
    private void AniFinish() {
        if (isFIN == false) {
            if (OnSceneAniFin != null)
                OnSceneAniFin();
            isFIN = true;
        }
    }
}
