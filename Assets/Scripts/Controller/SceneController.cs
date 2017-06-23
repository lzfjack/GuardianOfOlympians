using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Vuforia;

//游戏状态
public enum GameState {
    Guidence, Target_Detected, SCANNING, RENDERING, ANIMATION, Gaming, Fail, Win
}

//场景控制器
public class SceneController : MonoBehaviour, ISceneController, IUserAction {

    //对状态变更事件进行发布
    public delegate void ChangeState(GameState state);
    public static event ChangeState OnChangeState;
    //管理当前游戏状态
    GameState currentState = GameState.Guidence;

    #region VUFORIA

    //Image Target
    public GameObject Image;
    //Smart Terrain
    public GameObject smartTerrain;
    //控制对Smart Terrain的追踪检测
    private ReconstructionBehaviour ReconstructionBehaviour;
    //对检测事件注册回调
    private SmartTerrainEventHandler STEventHandler;
    //追踪状态回调处理器
    private SmartTerrainTrackableEventHandler STTrackableHandler;
    //Smart Terrain surface
    public SurfaceBehaviour SmartSurface;
    
    #endregion VUFORIA

    #region PUBLIC_REFERENCE
    
    //玩家
    public GameObject player;
    //玩家手部位置，后续需要在相应位置生成投掷武器
    public GameObject playerHand;
    //玩家出场的粒子效果
    public GameObject playerDisplayEffect;

    //塔防位置
    public GameObject mainTower;
    //游戏工厂
    public GameFactory Factory;
    //动作管理器
    public IActionManager actionManager;
    //怪源管理器
    public List<MonsterSource> monsterSource = new List<MonsterSource>();

    #endregion PUBLIC_REFERENCE

    #region PRIVATE_REFERENCE

    //完成动画播放的数量
    private int aniCompleted = 0;
    //玩家动画控制器
    private Animator playerAni;
    //塔防的动画组件
    private MainTower towerAnim;
    //对鼠标点击的坐标进行暂存
    private Vector3 HitPos;
    //记录按下的技能键
    private Button currentPressedButton;
    //记录按下的技能类型
    private int skillType = -1;
    //记录投掷动画是否完成
    private bool throwFin = true;

    #endregion PRIVATE_REFERENCE

    #region UI

    //UI射线碰撞检测
    public GameObject canvas;
    private EventSystem eventSystem;
    private GraphicRaycaster RaycastInCanvas;

    #endregion UI

    #region DIRECTOR_INTERFACE

    //加载资源
    public void LoadResouces() {
        //挂载UserAction
        //gameObject.AddComponent<UserGUI>();
        //挂载相机设置
        gameObject.AddComponent<CameraSettings>();
    }

    //初始化
    public void Initial() {
        //初始化英雄
        InitialHero();

        //对ImageTarget的追踪进行关闭
        Image.GetComponent<ImageTargetBehaviour>().enabled = false;

        //Smart Terrain 
        //获取ReconstructionBehaviour
        ReconstructionBehaviour = smartTerrain.GetComponent<ReconstructionBehaviour>();
        //获取Event Handler
        STEventHandler = smartTerrain.GetComponent<SmartTerrainEventHandler>();
 
        //获取UI射线碰撞检测引用
        RaycastInCanvas = canvas.GetComponent<GraphicRaycaster>();

        //获取塔楼动画组件
        towerAnim = Image.GetComponentInChildren<MainTower>();

        //观察者模式
        //对图片识别进行订阅
        DT_ImageTargetEventHandler.OnFoundImage += ScanScene;

        //对surface识别进行订阅
        SmartTerrainEventHandler.OnFoundSurface += surfaceFound;

        //对场景动画播放完毕进行订阅
        SurroundingAniEvent.OnSceneAniFin += changjingAniFin;

        //对边塔动画播放完毕进行订阅
        grow.OnPlay += AniCompleted;

        //对主塔动画播放完毕进行订阅
        MainTower.OnTowerAniFin += StartGame;

        //对玩家的投掷动画帧事件进行订阅
        PlayerAniEvent.OnThrowPoint += CreateWeapon;

        //对远程怪物的投掷动画帧事件进行订阅
        MonsterAniEvent.OnSendArrow += CreateArrow;

        //对游戏失败事件进行订阅
        TowerTrigger.OnGameFail += GameFail;
    }

    //用选择的英雄进行初始化
    void InitialHero() {
        //将保留的英雄赋予player
        player = SSDirector.getInstance().player;
        //对玩家的动画组件进行获取
        playerAni = player.GetComponent<Animator>();
        //对玩家动画事件脚本组件进行挂载
        player.AddComponent<PlayerAniEvent>();
        //只能通过遍历来寻找手的位置
        Transform[] childs = player.GetComponentsInChildren<Transform>();
        foreach (Transform transform in childs) {
            if (transform.gameObject.name == "Hand_Right_jnt") {
                playerHand = transform.gameObject;
                break;
            }
        }

        //将player置于MainTower的子对象
        player.transform.SetParent(mainTower.transform);
        //修改属性
        player.transform.localPosition = new Vector3(3.7f, 13.34f, -4.04f);
        player.transform.eulerAngles = new Vector3(0, 180, 0);
        player.transform.localScale = new Vector3(2.6f, 1.8f, 2.6f);
    }

    #endregion DIRECTOR_INTERFACE

    #region USER_INTERFACE
    
    //获取游戏状态
    public GameState GetState() {
        return currentState;
    }

    //获取技能状态
    public int GetSkill() {
        return skillType;
    }

    //获取技能释放状态
    public bool GetThrowFin() {
        return throwFin;
    }

    //按钮点击事件
    //通过按钮点击事件对游戏流程进行控制
    public void ButtonDown(int evenType) {
        //根据点击事件分类处理
        switch (evenType) {
            //开始游戏
            case 0:
                //启用图像追踪
                Image.GetComponent<ImageTargetBehaviour>().enabled = true;
                //进入Image Target识别阶段
                currentState = GameState.Target_Detected;
                break;

            //场地渲染
            case 1:
                //进入渲染阶段
                currentState = GameState.RENDERING;
                //对地面进行溶解动画渲染
                if ((ReconstructionBehaviour != null) && (ReconstructionBehaviour.Reconstruction != null)) {
                    //停止更新，但不去除
                    STEventHandler.StopUpdate();
                    SmartSurface.gameObject.transform.FindChild("chang_jing").gameObject.SetActive(true);
                }
                break;

            //游戏正式开始
            case 2:
                //显示地图占用情况
                //Singleton<MapController>.Instance.show();
                //激活英雄
                player.SetActive(true);
                //播放英雄的出场的粒子动画
                playerDisplayEffect.SetActive(true);
                //启动怪源
                foreach (MonsterSource source in monsterSource) {
                    source.gameObject.SetActive(true);
                    //激活怪源、刷新怪物
                    source.gameObject.GetComponent<Animator>().SetTrigger("open");
                    source.FreshMonster();
                }
                break;

            //退出游戏
            case 3:
                SSDirector.getInstance().NextScene();
                break;
        }
    }

    //对技能点击事件进行处理
    //先简单处理
    //每次点选技能，必须释放后才能进行技能点选
    public void SkillDown(int type, Button button) {
            //记录技能
            currentPressedButton = button;
            skillType = type;
    }

    //对UI射线碰撞进行检测
    bool CheckGuiRaycastObjects() {
        PointerEventData eventData = new PointerEventData(eventSystem);
        eventData.pressPosition = Input.mousePosition;
        eventData.position = Input.mousePosition;
        List<RaycastResult> list = new List<RaycastResult>();
        RaycastInCanvas.Raycast(eventData, list);
        return list.Count > 0;
    }


    //点击投掷事件
    public void Throw(Vector3 Mouse) {
        //创建射线;从摄像机发射一条经过鼠标当前位置的射线  
        Ray ray = Camera.main.ScreenPointToRay(Mouse);
        //对射线进行碰撞检测
        RaycastHit hitInfo;
        //只有碰撞到了物体，才进行投掷物发射
        if (Physics.Raycast(ray, out hitInfo)) {
            //需要对发射到UI上的射线进行过滤
            if (CheckGuiRaycastObjects() == false && throwFin == true) {
                //设置技能释放状态为未释放完毕
                throwFin = false;
                //改变玩家的位置
                //使得玩家朝向鼠标点击位置的等高处
                player.transform.LookAt(new Vector3(hitInfo.point.x, player.transform.position.y, hitInfo.point.z));
                //将player的动画状态置为单手投掷
                playerAni.SetTrigger("Throw");
                //对该次点击的鼠标位置进行记录
                HitPos = hitInfo.point;
                //点击的一刻即技能释放
                if (skillType != -1) {
                    //灭活按钮
                    currentPressedButton.interactable = false;
                    //去除点选
                    currentPressedButton.gameObject.transform.parent.gameObject.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                    //冷却技能
                    StartCoroutine(CoolSkill(currentPressedButton));
                }
            }
        }
    }

    #endregion USER_INTERFACE

    #region Reg_FUNC

    //检测到imageTarget
    void ScanScene() {
        currentState = GameState.SCANNING;
    }

    //检测到Smart Terrain的表面
    void surfaceFound(GameObject surface) {
        //获取被激活表面的相关引用
        SmartSurface = surface.GetComponent<SurfaceBehaviour>();
        STTrackableHandler = surface.GetComponent<SmartTerrainTrackableEventHandler>();
        //对状态改变进行通知
        if (OnChangeState != null)
            OnChangeState(GameState.SCANNING);
    }

    //场景动画播放完毕调用
    void changjingAniFin() {
        //隐藏建模体
        STEventHandler.Hide();
        //先对周围塔楼进行渲染
        STEventHandler.ShowPropClones();
        //停止地形更新
        ReconstructionBehaviour.Reconstruction.Stop();
        //进入游戏动画阶段
        currentState = GameState.ANIMATION;

        //若无周围边塔，直接对主塔进行渲染
        if (STEventHandler.num == 0)
            //播放中心塔楼动画
            towerAnim.Play();
    }

    //边塔动画播放完毕
    void AniCompleted() {
        //周围塔楼播放完毕
        aniCompleted++;
        //继周围塔楼后渲染主塔
        if (aniCompleted == (STEventHandler.num))    
            //播放中心塔楼动画
            towerAnim.Play();
    }

    //主塔动画播放完毕
    void StartGame() {
        currentState = GameState.Gaming;
        if (OnChangeState != null)
            OnChangeState(GameState.Gaming);
    }

    //玩家投掷武器
    void CreateWeapon() {
        switch (skillType) {
            case -1:
                //普通攻击
                Factory.GetWeapon(playerHand.transform.position, HitPos, 1);
                break;
            case 0:
                Factory.GetWeapon(playerHand.transform.position, HitPos, 3);
                break;
            case 1:
                Factory.GetWeapon(playerHand.transform.position, HitPos, 4);
                break;
        }
        //冷却重置技能
        skillType = -1;
        throwFin = true;
    }

    //技能冷却事件
    IEnumerator CoolSkill(Button skill) {
        int second = 10;
        //获取文本组件
        Text text = skill.gameObject.GetComponentInChildren<Text>();
        text.text = second.ToString();

        //等待10s
        while (second != 0) {
            yield return new WaitForSeconds(1);
            second--;
            text.text = second.ToString();
        }
        
        //激活按钮
        skill.interactable = true;
        //清空文本
        text.text = "";
    }

    //怪物投射武器
    void CreateArrow(GameObject remote_monster) {
        //收到远程怪物射箭的消息，从已有的弓箭位置向主角进行射击
        Factory.GetWeapon(remote_monster.GetComponent<MonsterData>().weaponPos.transform.position, player.transform.position, 2);
    }

    //游戏失败
    void GameFail() {
        //修改游戏状态
        currentState = GameState.Fail;
        //通知UI进行相应的显示
        if (OnChangeState != null)
            OnChangeState(GameState.Fail);
        GameFin();
    }

    //游戏胜利
    void GameWin() {
        //修改游戏状态
        currentState = GameState.Win;
        //通知UI进行相应的显示
        if (OnChangeState != null)
            OnChangeState(GameState.Win);
        GameFin();
    }

    //游戏结束
    void GameFin() {
        //停止刷新怪物
        foreach (MonsterSource source in monsterSource)
            source.StopFresh();
        //对已经刷新的场景中的怪物进行回收
        Factory.ClearMonster();
        //对已经刷新的场景中的武器进行回收
        Factory.ClearWeapon();
    }

    #endregion REG_FUNC

    #region LIFE_CIRCLE

    void Awake() {
        SSDirector director = SSDirector.getInstance();
        //设置场景
        director.index = 1;
        director.currentSceneController = this;
        director.currentSceneController.LoadResouces();
    }

    //初始化
    void Start() {
        Initial();
    }

    #endregion LIFE_CIRCLE
}
