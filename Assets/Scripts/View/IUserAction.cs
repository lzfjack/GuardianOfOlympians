using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public interface IUserAction{
    //获取游戏状态
    GameState GetState();
    //获取技能状态
    int GetSkill();
    //获取技能释放状态
    bool GetThrowFin();
    //按钮点击事件
    void ButtonDown(int eventType);
    //技能点击事件
    void SkillDown(int type, Button button);
    //点击投掷事件
    void Throw(Vector3 pos);
}
