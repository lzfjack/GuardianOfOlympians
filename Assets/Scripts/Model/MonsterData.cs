using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//对怪物预制体进行游戏工厂管理
public class MonsterData : MonoBehaviour {

    //下列两个参数在外部针对特定的怪物进行设置
    //怪物类型
    public int type;
    //发射武器的位置
    public GameObject weaponPos;

    //进入塔防
    public bool isInTower = false;
    //是否死亡
    public bool isDie = false;
    //生命值
    public int hp = 3;

    //近程怪物的行动目的地
    public Vector3 destination;

    public MonsterSource sourceCallback;

	// Use this for initialization
	void Start () {
        //对塔防触发事件进行订阅
        TowerTrigger.OnTowerEnter += EnterTower;

        //对进入技能范围事件进行订阅
        WeaponData.InSkillArea += EnterSkillArea;
	}

    private void EnterTower(GameObject tower, GameObject other) {
        //中心塔防进入触发器
        //即自己进入塔防
        if (!isInTower && other.name == gameObject.name) {
            //砸塔之前自己朝向塔
            gameObject.transform.LookAt(tower.transform.position);
            //对动画机进行设置
            gameObject.GetComponent<Animator>().SetBool("Attack", true);
            
            isInTower = true;
        }
    }

    private void EnterSkillArea(GameObject obj) {
        //确认是自己进入了技能范围
        //且尚未死亡
        if (obj.name == gameObject.name &&  isDie == false) {
            hp = 0;
            isDie = true;
            //相关怪源的怪物数量减少
            sourceCallback.MonsterDie();
            //开启协程
            StartCoroutine(Die());
        }
    }

    //怪物被武器击中
    private void OnCollisionEnter(Collision collision) {
        //碰撞物为武器且怪物尚未死亡
        if (collision.gameObject.name.Contains("Weapon") && isDie == false) {
            //生命值递减
            hp--;
            //生命值为0
            if (hp == 0) {
                isDie = true;
                //相关怪源的怪物数量减少
                sourceCallback.MonsterDie();
                //开启协程
                StartCoroutine(Die());
            }
            
        }
    }

    //死亡协程
    IEnumerator Die() {
        Animator ani = gameObject.GetComponent<Animator>();
        //对动画机进行设置
        ani.SetBool("Attack", false);
        //改为触发器，不知道是否会反复播放
        ani.SetTrigger("Death");
        //停止1s，等待动画播放完毕
        yield return new WaitForSeconds(2.5f);
        //通知工厂对自己进行回收
        Singleton<GameFactory>.Instance.FreeMonster(this);
    }
}
