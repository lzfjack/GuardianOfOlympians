using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//边塔动画播放事件
public class grow : MonoBehaviour {
    
    //边动画播放完毕事件进行发布
    public delegate void PlayCompleted();
    public static event PlayCompleted OnPlay;

    //播放动画
    public void Play () {
        StartCoroutine(Grow());
	}
	
	//协程使得动画有足够的播放时间
    IEnumerator Grow() {
        // 为了不同时渲染，进行随机世间等待
        float second = Random.Range(0f, 0.8f);
         yield return new WaitForSeconds(second);

        //将塔防激活
        gameObject.SetActive(true);
        //等待动画播放完毕s
        yield return new WaitForSeconds(5.64f);
  
        //希望所有动画播放完毕后对地形事件处理器进行通知
        if (OnPlay != null)
            OnPlay();
    }

}
