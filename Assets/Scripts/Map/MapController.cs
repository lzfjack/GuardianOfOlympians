using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//地图管理器
//希望对现有的场景进行网格化管理
//可用于寻路功能的实现
//怪物源点的选取
//等
public class MapController : MonoBehaviour {

    private SceneController sceneController;

    float zMax = 1f;
    float zMIn = -1f;
    float xMIn = -1.4f;
    float xMax = 1.4f;
    float unit = 0.1f;

    //白色瓷砖
    public GameObject Wtrick;
    //黑色瓷砖
    public GameObject Btrick;

    //地图管理
    int[,] map = new int[28, 20];

    //对map进行初始化
    void Start () {
        //获取场记引用
        sceneController = SSDirector.getInstance().currentSceneController as SceneController;

        //所有空格都未被占用
        for (int i = 0; i < 28; i++) {
            for (int j = 0; j < 20; j++)
                map[i, j] = 0;
        }
	}
	
    //由建筑物对自己所占空格进行填充
    public IEnumerator CoverMap(float centerX, float centerZ, float length) {
        int x = (int)((centerX + 1.4f) / unit) ;
        int z = (int)((centerZ + 1)    / unit);
        int l = (int)((length  + 0.05f)    / unit);
        for (int i = x - l/2; i <= x + l/2; i++) {
            for (int j = z - l/2; j <= z + l/2; j++) {
                //合法空格被占用
                if (i >= 0 && i < 28 && j >= 0 && j < 20) {
                    map[i, j] = 1;
                }
            }
            //利用协程分布计算，提高效率
            yield return 0;
        }
    }

    //对地图进行可视化
    public void show() {
        float x, z;
        GameObject trick;
        //所有空格都未被占用
        for (int i = 0; i < 28; i++) {
            for (int j = 0; j < 20; j++) {
                //中心位置
                x = i * unit - 1.4f + 0.05f;
                z = j * unit - 1f   + 0.05f;
                //未被占用
                if (map[i, j] == 0)
                    trick = Instantiate(Wtrick);
                //被占用
                else
                    trick = Instantiate(Btrick);
                //设置为smart terrain surface的子对象
                trick.transform.SetParent(sceneController.SmartSurface.gameObject.transform);
                trick.transform.localScale = new Vector3(0.1f, 0.01f, 0.1f);
                trick.transform.position = new Vector3(x, 0.05f, z);
            }
        }
    }

    public Vector3 GetNextPos(Vector3 currentPos, Vector3 destination) {
        //明确当前位置
        int x_c = (int)((currentPos.x + 1.4f) / unit);
        int z_c = (int)((currentPos.z + 1) / unit);

        int x_d = (int)((destination.x + 1.4f) / unit);
        int z_d = (int)((destination.z + 1) / unit);
        
        //目标位置
        int result = Mathf.Abs(x_c - x_d) + Mathf.Abs(z_c - z_d); 

        for (int i = x_c-1; i <= x_c+1; i++) {
            for (int j = z_c-1; j <= z_c+1; j++) {
                if (i >= 0 && i < 28 && j >= 0 && j < 20) {
                    if (map[i, j] == 0) {
                        //曼哈顿距离来进行启发式搜索
                        int temp = Mathf.Abs(i - x_d) + Mathf.Abs(j - z_d);
                        //只要找到一个比当前更近的，立马前往
                        if (temp < result)
                            return new Vector3(i * unit - 1.4f + 0.05f, 0, j * unit - 1f + 0.05f);
                    }
                }
            }
        }

        //实在找不到，返回主塔位置
        return sceneController.mainTower.transform.position;
    }
}
