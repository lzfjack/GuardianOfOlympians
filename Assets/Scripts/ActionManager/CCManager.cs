using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCManager : SSActionManager, ISSActionCallback, IActionManager {
    //场记
    public SceneController sceneController;

    //地图管理器
    private MapController mapController;

    // Use this for initialization
    public void Start() {
        //获取场记
        sceneController = SSDirector.getInstance().currentSceneController as SceneController;
        sceneController.actionManager = this;

        //获取地图管理器
        mapController = Singleton<MapController>.Instance;
    }

    // Update is called once per frame
    public void Update() {
        base.Update();
    }

    public void SSActionEvent(SSAction source, SSActionEventType events = SSActionEventType.Competeted, int intParam = 0, string strParam = null, object objectParam = null) {
        //完成动作
        MonsterData data = source.gameobject.GetComponent<MonsterData>();

        //细想怪物无非两种状态
        //1. 被攻击致死后，销毁当前动作是动作管理器的职责，不应该在这里对回收逻辑进行处理
       
        //2. 怪物未死，完成动作但是未到达塔防范围，继续添加动作
        if (data.isDie == false && data.isInTower == false) {
            //获取当前下一步的位置
            Vector3 desPos;
            //是否已经到达了目的地
            //if (source.gameobject.transform.position != data.destination)
            //    desPos = mapController.GetNextPos(source.gameobject.transform.position, data.destination);
            //else
            desPos = sceneController.mainTower.transform.position;
            //获取动作
            CCAction movetoAction = CCAction.GetSSAction(desPos);
            //改变朝向
            source.gameobject.transform.LookAt(desPos);
            //怪物开始行进
            RunAction(source.gameobject, movetoAction, this);
        }

    }

    //动作管理器适配器模式接口
    public void Play(GameObject gameobject, Vector3 nextPos) {
        //获取当前下一步的位置
        //Vector3 desPos = mapController.GetNextPos(gameobject.transform.position, nextPos);

        //获取动作
         CCAction movetoAction = CCAction.GetSSAction(nextPos);
        //改变物体运动朝向
        gameobject.transform.LookAt(nextPos, Vector3.up);

        //关闭怪物攻击状态
        gameobject.GetComponent<Animator>().SetBool("Attack", false);
        //怪物开始行进
        RunAction(gameobject, movetoAction, this);
    }

}
