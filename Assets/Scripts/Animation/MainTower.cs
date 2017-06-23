using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//主塔动画播放事件
public class MainTower : MonoBehaviour {

    //主塔动画播放完毕事件进行发布
    public delegate void TowerAniFin();
    public static event TowerAniFin OnTowerAniFin;

    #region PRIVATE_MEMBER

    private bool DT_didAnimate = false;
    private bool DT_didFinishPlaying = false;

    #endregion PRIVATE_MEMBER

    #region PUBLIC_MEMBER

    public GameObject mainTower;

    public bool DidFinishAnimation {
        get {
            return DT_didFinishPlaying;
        }
    }

    #endregion PUBLIC_MEMBER

    #region PUBLIC_METHODS

    public void Play() {
        //若尚未播放动画
        if (!DT_didAnimate) {
            //采用协程对动画进行播放
            StartCoroutine(DidFinishPlayingCoroutine());
            DT_didAnimate = true;
        }
    }

    #endregion //PUBLIC_METHODS

    #region PRIVATE_METHODS

    private IEnumerator DidFinishPlayingCoroutine() {
        //等待0.3s
        yield return new WaitForSeconds(0.3f);
        //对主塔模型进行激活
        mainTower.SetActive(true);
        //等待1s
        yield return new WaitForSeconds(5f);
        //确认动画播放完毕
        DT_didFinishPlaying = true;
        //进入游戏状态
        if (OnTowerAniFin != null)
            OnTowerAniFin();
        yield return new WaitForEndOfFrame();
    }

    #endregion //PRIVATE_METHODS
}
