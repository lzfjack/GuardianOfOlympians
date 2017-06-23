using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SSDirector : System.Object {

    private static SSDirector _instance;

    //当前场景
    public ISceneController currentSceneController { get; set; }

    //对经过装备的人物进行存储
    public GameObject player;

    //对所有的场景进行存储
    string[] scenes = new string[2] { "Start_Scene", "Game_Scene"};
   
    //场景次序
    public int index = 0;

    //导演单实例
    public static SSDirector getInstance() {
        if (_instance == null)
            _instance = new SSDirector();
        return _instance;
    }

    //重新开始游戏
    public void Restart() {
        currentSceneController.Initial();
        currentSceneController.LoadResouces();
    }

    //切换场景
    public void NextScene() {
        //如果后续还有场景,切换到下一场景
        if (index < scenes.Length - 1) {
            index++;
            SceneManager.LoadScene(scenes[index]);
        }
        //否则退出游戏
        else
            Application.Quit();
    }
}
