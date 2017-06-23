using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//挂载到出怪口使其成为一个独立模块
public class MonsterSource : MonoBehaviour, IMonsterCallBack {
    //指定生产哪一种怪
    public int type;
    //场记
    SceneController sceneController;

    int num = 0;

    bool on;

    private void Start() {
        //下述操作使得可以随意的添加怪源，怪源会自动加入管理器
        //获取场记引用
        sceneController = SSDirector.getInstance().currentSceneController as SceneController;
        //将自己加入场记怪源管理器
        sceneController.monsterSource.Add(this);
    }

    //开启协程
    public void FreshMonster() {
        on = true;
        if (gameObject.activeInHierarchy == true)
            StartCoroutine(CreateMonster());
    }

    //停止协程
    public void StopFresh() {
        on = false;
        if (gameObject.activeInHierarchy == true)
            StopCoroutine(CreateMonster());
    }

    //刷怪
    IEnumerator CreateMonster() {
        //等待2s动画播放，同时给玩家一些准备时间
        yield return new WaitForSeconds(3f);

        while (on == true) {
            switch (type) {
                case 1:
                    //普通攻击兵以怪物总量来进行生产，对是哪个乖源并不在意
                    if (num < 2) {
                        //生产怪源类型的怪物
                        sceneController.Factory.GetMonster(gameObject.transform.position, this, 1);
                        //增加怪物数量
                        num++;
                    }
                    break;
                case 2:
                    //远程兵以单塔为单位进行生产
                    if (num < 1) {
                        //生产怪源类型的怪物
                        sceneController.Factory.GetMonster(gameObject.transform.position, this, 2);
                        num++;
                    }
                        break;
            }

            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }
    }

    //怪物死亡通知函数执行
    public void MonsterDie() {
        num--;
    }

}
