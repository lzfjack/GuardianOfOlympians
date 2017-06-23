using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//游戏工厂
//简单工厂模式-词典实现版
public class GameFactory : MonoBehaviour {
    //场记
    SceneController sceneController;

    //投掷武器预制体引用
    //玩家武器预制体
    public GameObject weapon;
    //玩家技能
    //飞鸟
    public GameObject weapon_CrowStorm;
    //雷切
    public GameObject weapon_LightStrike;

    //远程怪物武器预制体
    public GameObject monsterWeapon;

    //需要对数据结构进行升级，因为存在多种不同的武器需要进行回收管理
    //链表用于管理正在使用的投掷物与被回收的投掷物
    //采用链表词典进行管理
    Dictionary<int, List<WeaponData>> usingWeapon_;
    Dictionary<int, List<WeaponData>> unusedWeapon_;

    //怪物预制体引用
    //近战怪物引用
    public GameObject monster;
    //远程怪物引用
    public GameObject remote_monster;

    //采用链表词典进行管理
    Dictionary<int, List<MonsterData>> usingMonster_;
    Dictionary<int, List<MonsterData>> unusedMonster_;

    //初始化
    void Start() {
        //对场记进行初始化
        sceneController = SSDirector.getInstance().currentSceneController as SceneController;
        
        //对工厂赋予引用
        sceneController.Factory = this;

        //对武器链表词典进行初始化
        usingWeapon_ = new Dictionary<int, List<WeaponData>>();
        unusedWeapon_ = new Dictionary<int, List<WeaponData>>();

        //对怪物链表词典进行初始化
        usingMonster_ = new Dictionary<int, List<MonsterData>>();
        unusedMonster_ = new Dictionary<int, List<MonsterData>>();
    }

    //获取投掷武器
    //获取投掷武器无非是三个参数：
    //1. 从哪里发射
    //2. 发射到哪里去
    //3. 发射哪种武器
    public void GetWeapon(Vector3 throwPoint, Vector3 hitPoint, int type) {
        //创建为临时变量，避免临界区问题
        List<WeaponData> usingWeapon;
        List<WeaponData> unusedWeapon;

        //检查词典链表是否为空，并进行初始化
        if (usingWeapon_.ContainsKey(type) == false)
            usingWeapon_.Add(type, new List<WeaponData>());
        if (unusedWeapon_.ContainsKey(type) == false)
            unusedWeapon_.Add(type, new List<WeaponData>());

        //获取对相应武器类型的链表引用
        usingWeapon = usingWeapon_[type];
        unusedWeapon = unusedWeapon_[type];

        //生成投掷物
        WeaponData weaponData = null;
        //存在可回收投掷武器
        if (unusedWeapon.Count != 0) {
            //取出回收投掷武器进行复用
            weaponData = unusedWeapon[0];
            //重新启用游戏对象
            weaponData.gameObject.SetActive(true);
            //对复用投掷武器在回收链表中进行删除
            unusedWeapon.RemoveAt(0);
        }
        //不存在可回收投掷武器
        else {
            //根据类型创建新投掷物
            GameObject newWeapon = null;
            switch(type) {
                case 1:
                    newWeapon = Instantiate(weapon);
                    //对投掷武器的名字进行更改
                    newWeapon.gameObject.name = "PWeapon" + type + "_" + usingWeapon.Count;
                    break;
                case 2:
                    newWeapon = Instantiate(monsterWeapon);
                    //对投掷武器的名字进行更改
                    newWeapon.gameObject.name = "MWeapon" + type + "_" + usingWeapon.Count;
                    break;
                case 3:
                    newWeapon = Instantiate(weapon_CrowStorm);
                    //对投掷武器的名字进行更改
                    newWeapon.gameObject.name = "PWeapon_CrowStorm" + type + "_" + usingWeapon.Count;
                    break;
                case 4:
                    newWeapon = Instantiate(weapon_LightStrike);
                    //对投掷武器的名字进行更改
                    newWeapon.gameObject.name = "PWeapon_LightStrike" + type + "_" + usingWeapon.Count;
                    break;
            }
            if (newWeapon != null) {
                //获取数据脚本
                weaponData = newWeapon.GetComponent<WeaponData>();
            }
        }

        //创建不为空
        if (weaponData != null) {
            //将投掷武器设置为smart terrain surface的子对象
            weaponData.transform.SetParent(sceneController.SmartSurface.gameObject.transform);

            //非大招技能
            if (type <= 2) {
                //在投掷点的位置生成投掷武器
                weaponData.gameObject.transform.position = throwPoint;

                //投掷物按鼠标位置进行发射
                //改变投掷物朝向
                weaponData.gameObject.transform.LookAt(hitPoint);
                //将武器加入使用列表
                usingWeapon.Add(weaponData);
                //对投掷武器添加动作
                //根据射线碰撞点的信息对投掷物进行发射
                Vector3 direction = hitPoint - weaponData.gameObject.transform.position;

                //为了更好的表现追尾效果，所有投掷武器不使用重力，采用恒定速度

                switch (type) {
                    case 1:
                        //玩家投掷武器为了更好的表现追尾效果采用较慢速度
                        weaponData.gameObject.GetComponent<Rigidbody>().velocity = direction;
                        break;
                    case 2:
                        //弓箭采用较快速度
                        weaponData.gameObject.GetComponent<Rigidbody>().velocity = direction * 3;
                        break;
                }
            }
            //大招技能
            else {
                //在撞击点的位置生成大招武器
                weaponData.gameObject.transform.position = hitPoint;
                //选择大招类型
                //直接对大招技能开启协程等待动画播放完毕后回收
                switch (type) {
                    case 3:
                        StartCoroutine(weaponData.ShowCrowAround());
                        break;
                    case 4:
                        StartCoroutine(weaponData.ShowLightEffect());
                        break;
                }
            }
        }
    }

    //回收投掷武器
    public void FreeWeapon(WeaponData weapon) {

        List<WeaponData> usingWeapon;
        List<WeaponData> unusedWeapon;

        //获取类型
        int type = weapon.type;
        //检查词典链表是否为空，并进行初始化
        if (usingWeapon_.ContainsKey(type) == false)
            usingWeapon_.Add(type, new List<WeaponData>());
        if (unusedWeapon_.ContainsKey(type) == false)
            unusedWeapon_.Add(type, new List<WeaponData>());

        //获取对相应武器类型的链表引用
        usingWeapon = usingWeapon_[type];
        unusedWeapon = unusedWeapon_[type];

        //将投掷武器设置为smart terrain surface的子对象
        weapon.transform.SetParent(sceneController.gameObject.transform);

        //将相应飞镖置为不可用
        weapon.gameObject.SetActive(false);
        //恢复飞镖出场设置
        switch (type) {
            case 1:
                weapon.gameObject.transform.position = weapon.transform.position;
                weapon.gameObject.transform.eulerAngles = weapon.transform.eulerAngles;
                break;
            case 2:
                weapon.gameObject.transform.position = monsterWeapon.transform.position;
                weapon.gameObject.transform.eulerAngles = monsterWeapon.transform.eulerAngles;
                break;
        }
        //初速度置为0
        weapon.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        weapon.GetComponent<Rigidbody>().velocity = Vector3.zero;
        //移出使用链表
        usingWeapon.Remove(weapon);
        //加入回收链表
        unusedWeapon.Add(weapon);
    }

    //生产怪物
    //生产怪物需要两个参数：
    //1. 怪物的生产点
    //2. 怪物的类型
    public void GetMonster(Vector3 monsterSource, MonsterSource callback, int type) {

        List<MonsterData> usingMonster;
        List<MonsterData> unusedMonster;

        //检查词典链表是否为空，并进行初始化
        if (usingMonster_.ContainsKey(type) == false)
            usingMonster_.Add(type, new List<MonsterData>());
        if (unusedMonster_.ContainsKey(type) == false)
            unusedMonster_.Add(type, new List<MonsterData>());

        //获取对相应武器类型的链表引用
        usingMonster = usingMonster_[type];
        unusedMonster = unusedMonster_[type];

        MonsterData monsterData = null;
        if (unusedMonster.Count != 0) {
            //取出怪物复用
            monsterData = unusedMonster[0];
            monsterData.gameObject.SetActive(true);
            unusedMonster.RemoveAt(0);
        }
        else {
            GameObject newMonster = null;
            //根据怪物类型进行实例化
            switch (type) {
                case 1:
                    newMonster = Instantiate(monster);
                    break;
                case 2:
                    newMonster = Instantiate(remote_monster);
                    break;
            }
            if (newMonster != null) {
                monsterData = newMonster.GetComponent<MonsterData>();
                //对怪物名字进行修改
                newMonster.gameObject.name = "Monster" + type + "_" + usingMonster.Count;
            }
        }

        //创建不为空
        if (monsterData != null) {
            //对怪物信息进行初始化
            ResetMonster(monsterData, monsterSource, callback);
            //Vector3.forward * 0.3f;
            usingMonster.Add(monsterData);

            //近战怪物需要添加动作
            //远战怪物通过动画帧事件来进行发射攻击
            switch (type) {
                case 1:
                    //获取终点
                    Vector3 destination = sceneController.mainTower.transform.position;
                    destination.x += Random.Range(-0.5f, 0.5f);
                    destination.z += Random.Range(-0.5f, 0.5f);

                    //记录运动终点
                    monsterData.destination = destination;

                    //对怪物添加动作
                    sceneController.actionManager.Play(monsterData.gameObject, new Vector3(destination.x, 0, destination.z));
                    break;
                case 2:
                    //令怪物朝向主角方向
                    monsterData.gameObject.transform.LookAt(new Vector3(sceneController.player.transform.position.x, 
                                                            monsterData.gameObject.transform.position.y, 
                                                            sceneController.player.transform.position.z));
                    //修改怪物的动画状态
                    monsterData.gameObject.GetComponent<Animator>().SetBool("Attack", true);
                    break;
            }

        }
    }

    //怪物出厂设置
    void ResetMonster(MonsterData monsterData, Vector3 monsterSource, MonsterSource callback) {
        //所有的Monster应该在刷怪处
        //修改位置属性
        //在塔防为中心的矩形范围内随机生成终点
        monsterData.gameObject.transform.SetParent(sceneController.SmartSurface.gameObject.transform);
        monsterData.gameObject.transform.position = monsterSource;
        monsterData.isDie = false;
        monsterData.isInTower = false;
        monsterData.hp = 3;
        monsterData.sourceCallback = callback;
        //对动画机进行设置
        monsterData.gameObject.GetComponent<Animator>().SetBool("Attack", false);
    }

    //回收怪物
    public void FreeMonster(MonsterData _monster) {

        List<MonsterData> usingMonster;
        List<MonsterData> unusedMonster;

        //指定类型
        int type = _monster.type;

        //检查词典链表是否为空，并进行初始化
        if (usingMonster_.ContainsKey(type) == false)
            usingMonster_.Add(type, new List<MonsterData>());
        if (unusedMonster_.ContainsKey(type) == false)
            unusedMonster_.Add(type, new List<MonsterData>());

        //获取对相应武器类型的链表引用
        usingMonster = usingMonster_[type];
        unusedMonster = unusedMonster_[type];

        //便于对怪物残留的动作进行清理
        _monster.isDie = true;
        _monster.gameObject.SetActive(false);
        //将其移出smart terrain的追踪范围
        _monster.gameObject.transform.SetParent(sceneController.gameObject.transform);
        //恢复怪物出场设置
        _monster.gameObject.transform.eulerAngles = Vector3.zero;
        //移出使用链表
        usingMonster.Remove(_monster);
        //加入回收链表
        unusedMonster.Add(_monster);
    }

    //对现存的所有怪物进行回收
    public void ClearMonster() {
        //对字典中的每一个键值对
        foreach (KeyValuePair<int, List<MonsterData>> pair in usingMonster_) {
            //取出怪物链表
            List<MonsterData> usingMonster = pair.Value;
            //对链表中的每一个怪物进行回收
            while (usingMonster.Count != 0) {
                MonsterData monster = usingMonster[0]; 
                FreeMonster(monster);
            }
        }
    }

    //对现存的所有武器进行回收
    public void ClearWeapon() {
        //对字典中的每一个键值对
        foreach (KeyValuePair<int, List<WeaponData>> pair in usingWeapon_) {
            //取出怪物链表
            List<WeaponData> usingWeapon = pair.Value;
            //对链表中的每一武器进行回收
            while (usingWeapon.Count != 0) {
                WeaponData weapon = usingWeapon[0];
                FreeWeapon(weapon);
            }
        }
    }
}
