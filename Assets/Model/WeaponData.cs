using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//挂载到投掷物
//对投掷物的碰撞情况等进行回收管理
public class WeaponData : MonoBehaviour {

    //在外对武器类型进行指定
    public int type;

    //由外部进行指定
    public ParticleSystem boomparticle;

    //对进入武器攻击范围进行发布
    public delegate void SkillArea(GameObject obj);
    public static SkillArea InSkillArea;

    private void Update() {
        //如果高度小于0，则进行回收
        if (gameObject.transform.position.y < 0)
            Singleton<GameFactory>.Instance.FreeWeapon(this);
    }

    private void OnCollisionEnter(Collision collision) {
        switch (type) {
            case 1:
                StartCoroutine(ShowEffect());
                break;
            case 2:
                Singleton<GameFactory>.Instance.FreeWeapon(this);
                break;
        }
    }

    //对进入技能范围事件进行发布
    private void OnTriggerEnter(Collider other) {
        if (type >= 3)
            //发布
            if (InSkillArea != null)
                InSkillArea(other.gameObject);
    }

    //武器二效果处理
    IEnumerator ShowEffect() {
        //对武器碰撞的粒子效果进行播放
        boomparticle.Play();
        //等待播放完毕
        yield return new WaitForSeconds(0.1f);
        //再对武器进行回收
        Singleton<GameFactory>.Instance.FreeWeapon(this);
    }

    //武器三效果处理
    public IEnumerator ShowCrowAround() {
        //等待乌鸦飞行动画播放完毕
        yield return new WaitForSeconds(3f);
        //再对武器进行回收
        Singleton<GameFactory>.Instance.FreeWeapon(this);
    }

    //武器四效果处理
    public IEnumerator ShowLightEffect() {
        //等待粒子播放完毕
        yield return new WaitForSeconds(3f);
        //再对武器进行回收
        Singleton<GameFactory>.Instance.FreeWeapon(this);
    }
}
