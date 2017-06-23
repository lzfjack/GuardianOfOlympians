using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//UGUI
public class UserGUI : MonoBehaviour {

    #region PUBLIC_MEMBERS

    //获取按钮
    public GameObject[] Buttons = new GameObject[4];
    //技能
    public GameObject[] Skill;
    //确认框
    public GameObject Checkmark;
    //刷新
    public GameObject Refresh;
    //游戏场景
    public GameObject Game;
    //游戏失败画布
    public GameObject FailState;

    #endregion PUBLIC_MEMBERS

    #region PRIVATE_MEMBERS

    //门面模式与场景控制器进行交互
    private IUserAction action;
    //Gameing状态
    bool GameStart = false;
    
    #endregion PRIVATE_MEMBERS

    // 初始化
    void Start () {
        //获取引用
        action = SSDirector.getInstance().currentSceneController as IUserAction;
        //对场记的状态变更事件进行订阅
        SceneController.OnChangeState += DoChangeState;
    }
	
    //按钮点击
    public void OnButtonClick(int type) {
        //灭活被点击按钮
        Buttons[type].SetActive(false);
        action.ButtonDown(type);

        //额外处理工作
        switch(type) {
            case 0:
                Checkmark.SetActive(true);
                Refresh.SetActive(true);
                break;
            case 1:
                Checkmark.SetActive(false);
                Refresh.SetActive(false);
                break;
            case 2:
                //开始游戏
                GameStart = true;
                //激活退出按钮与技能槽
                Game.SetActive(true);
                Buttons[3].SetActive(true);
                break;
        }
    }

    //技能点击
    public void OnSkillClick(int type) {
        Button button = Skill[type].GetComponentInChildren<Button>();
        int currentType = action.GetSkill();
        Image currentBox = null;
        Image preBoX = null;
        //技能可用
        //且不重复点选
        if (GameStart == true            &&
            button.interactable == true && 
            currentType != type         &&
            action.GetThrowFin() == true) {
            //存在已经点选的技能
            if (currentType != -1) {
                preBoX = Skill[currentType].GetComponent<Image>();
                preBoX.color = Color.white;
            }
            //获取技能框的image组件
            //记录点选技能的下标
            currentBox = Skill[type].GetComponent<Image>();
            currentBox.color = Color.green;

            //通知场记进行处理
            action.SkillDown(type, button);
        }
    }

    //对状态改变事件进行订阅
    void DoChangeState(GameState state) {
        //根据相应状态激活按钮
        switch (state) {
            case GameState.SCANNING:
                Buttons[1].SetActive(true);
                break;
            case GameState.Gaming:
                Buttons[2].SetActive(true);
                break;
            case GameState.Fail:
                //游戏结束
                GameStart = false;
                //启动失败界面遮罩
                FailState.SetActive(true);
                break;
        }
    }

    //将点击事件交给场记进行逻辑处理
    void Update() {
        //点击事件
        if (GameStart == true && Input.GetMouseButtonDown(0)) {
            action.Throw(Input.mousePosition);
        }
    }
}
