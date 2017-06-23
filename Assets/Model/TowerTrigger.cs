using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//管理所有塔的触发事件
//因为碰撞体、刚体、触发器都挂在该脚本的gameobject上，所以统一处理
public class TowerTrigger : MonoBehaviour {

    //塔的生命值
    //塔的生命受怪物攻击影响
    //怪物攻击分为两种
    //1. 近战攻击
    //2. 远程攻击
    int TowerHp = 100;

    //塔是否死亡
    bool die = false;

    //塔防血条
    public GameObject TowerBlood;
    Image BloodImage;

    void Start() {
        BloodImage = TowerBlood.GetComponent<Image>();
        //塔防占地
        StartCoroutine(Singleton<MapController>.Instance.CoverMap(gameObject.transform.position.x - 0.25f, gameObject.transform.position.z + 0.2f, 0.4f));
    }

    //对塔防触发事件进行发布订阅
    public delegate void TowerEnter(GameObject sender, GameObject enter);
    public static event TowerEnter OnTowerEnter;

    //对游戏结束事件进行发布订阅
    public delegate void GameFail();
    public static event GameFail OnGameFail;

    //下面的设计存在的不足，没有对武器攻击的判定进行统一
    //而是分为碰撞与触发两者同时进行检测

    //对触发事件进行发布
    private void OnTriggerEnter(Collider other) {
        //怪物进入塔防范围，广播信息   
        if (other.gameObject.name.Contains("Monster") && OnTowerEnter != null)
            OnTowerEnter(gameObject, other.gameObject);

        //对近程武器攻击进行触发检测
        if (other.gameObject.name.Contains("MWeapon") && die == false) {
            TowerHp--;
            BloodImage.fillAmount = TowerHp / 100f;
            if (TowerHp <= 0) {
                die = true;
                //游戏结束，对场景控制器进行通知
                if (OnGameFail != null)
                    OnGameFail();
            }
        }
    }

    private void OnCollisionEnter(Collision other) {
        //对远程武器攻击进行触发检测
        if (other.gameObject.name.Contains("MWeapon") && die == false) {
            TowerHp--;
            BloodImage.fillAmount = TowerHp / 100f;
            if (TowerHp <= 0) {
                die = true;
                //游戏结束，对场景控制器进行通知
                if (OnGameFail != null)
                    OnGameFail();
            }
        }
    }
}
