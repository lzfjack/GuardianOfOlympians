/*==============================================================================
Copyright (c) 2013-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Vuforia;

/// <summary>
///  A custom handler that implements the ITrackerEventHandler interface.
/// </summary>
public class SmartTerrainEventHandler : MonoBehaviour
{

    #region PRIVATE_MEMBERS

    private bool m_propsCloned;
    private bool DT_Display = true;
    private ReconstructionBehaviour mReconstructionBehaviour;
    SmartTerrainTracker DT_Tracker;

    //表面是否被发现
    private bool surfaceFound;

    //prop引用
    private PropAbstractBehaviour[] props;
    //surface引用
    private SurfaceAbstractBehaviour[] surfaces;

    #endregion //PRIVATE MEMBERS

    #region PUBLIC_MEMBERS

    //观察者模式
    public delegate void SurfaceFoundForFirstTime(GameObject surface);
    public static event SurfaceFoundForFirstTime OnFoundSurface;

    public PropBehaviour PropTemplate;
    public SurfaceBehaviour SurfaceTemplate;

    //塔楼模型
    public GameObject tower;

    public bool propsCloned
    {
        get
        {
            return m_propsCloned;
        }
    }

    //对扫描的塔楼数量进行控制
    const int maxProp = 3;
    public int num = 0;
    //增加少许边界便宜
    const float offset = 0.5f;

    #endregion

    #region UNITY_MONOBEHAVIOUR

    //对回调函数进行注册
    void Start() {
        //获取Smart Terrain Tracker 与 Reconstruction
        mReconstructionBehaviour = GetComponent<ReconstructionBehaviour>();
        DT_Tracker = TrackerManager.Instance.GetTracker<SmartTerrainTracker>();

        if (mReconstructionBehaviour) {
            mReconstructionBehaviour.RegisterInitializedCallback(OnInitialized);
            mReconstructionBehaviour.RegisterPropCreatedCallback(OnPropCreated);
            mReconstructionBehaviour.RegisterSurfaceCreatedCallback(OnSurfaceCreated);
        }
    }

    //对回调函数进行注销
    void OnDestroy() {
        if (mReconstructionBehaviour) {
            mReconstructionBehaviour.UnregisterInitializedCallback(OnInitialized);
            mReconstructionBehaviour.UnregisterPropCreatedCallback(OnPropCreated);
            mReconstructionBehaviour.UnregisterSurfaceCreatedCallback(OnSurfaceCreated);
        }
    }

    #endregion //UNITY_MONOBEHAVIOUR

    #region ISmartTerrainEventHandler_Implementations

    //smart terrain利用Target进行初始化
    public void OnInitialized(SmartTerrainInitializationInfo initializationInfo) {
        Debug.Log("Finished initializing at [" + Time.time + "]");
    }

    //对边塔的生成进行监听回调
    public void OnPropCreated(Prop prop) {
        //此处可以根据prop相关信息为其挂载不同的游戏逻辑
        //同时对于prop数量进行监控
        if (mReconstructionBehaviour && num < maxProp) {
            mReconstructionBehaviour.AssociateProp(PropTemplate, prop);
            PropAbstractBehaviour behaviour;
            if (mReconstructionBehaviour.TryGetPropBehaviour(prop, out behaviour))
                behaviour.gameObject.name = "Prop " + prop.ID;
            //增加边塔数量
            num++;
        }
    }

    //对smart terrain的生成进行监听回调
    public void OnSurfaceCreated(Surface surface) {
        //shows an example of how you could get a handle on the surface game objects to perform different game logic
        if (mReconstructionBehaviour) {
            mReconstructionBehaviour.AssociateSurface(SurfaceTemplate, surface);
            SurfaceAbstractBehaviour behaviour;
            if (mReconstructionBehaviour.TryGetSurfaceBehaviour(surface, out behaviour)) {
                behaviour.gameObject.name = "Primary " + surface.ID;
                //第一次发现通知场记
                if (!surfaceFound) {
                    OnFoundSurface(behaviour.gameObject);
                    surfaceFound = true;
                }
            }
        }
    }

    #endregion // ISmartTerrainEventHandler_Implementations

    #region PUBLIC_METHODS

    //将实物建模渲染为游戏边塔
    public void ShowPropClones() {
        if (!m_propsCloned) {
            //对prop进行置换
            PropAbstractBehaviour[] props = gameObject.GetComponentsInChildren<PropAbstractBehaviour>();
         
            foreach (PropAbstractBehaviour prop in props) {
                // 塔楼实例化
                GameObject Tower = Instantiate(tower);
                //置于prop下
                Tower.gameObject.transform.SetParent(prop.gameObject.transform);
                //初始化
                Vector3 scale = prop.Prop.BoundingBox.HalfExtents;
                float length = Mathf.Max(scale.x, scale.z);
                //设置边缘，使得完全被覆盖
                Tower.transform.localScale = new Vector3(length * 10 + offset, scale.y * 6, length * 10 + offset);
                Tower.transform.eulerAngles = new Vector3(0, prop.Prop.BoundingBox.RotationY, 0);
                
                //对粒子特效范围进行设置
                ParticleSystem[] particle = Tower.transform.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem p in particle) {
                    ParticleSystem.ShapeModule shapeModule = p.shape;
                    shapeModule.radius = shapeModule.radius * length * 10;
                }

                //设置位置属性
                Vector3 pos = prop.Prop.BoundingBox.Center;
                Tower.transform.localPosition = new Vector3(pos.x, 0, pos.z);

                Debug.Log(Tower.transform.position + "   " + length * 2);

                //自我计算地图占用情况
                StartCoroutine(Singleton<MapController>.Instance.CoverMap(Tower.transform.position.x, Tower.transform.position.z, length*2));

                //对动画进行播放
                Tower.GetComponent<grow>().Play();
            }
          
            m_propsCloned = true;
        }
    }

    //停止prop与surface的更新
    public void StopUpdate() {
        props = gameObject.GetComponentsInChildren<PropAbstractBehaviour>();
        foreach (PropAbstractBehaviour prop in props)
            //停止更新但继续追踪
            prop.SetAutomaticUpdatesDisabled(true);
       
        surfaces = gameObject.GetComponentsInChildren<SurfaceAbstractBehaviour>();
        foreach (SurfaceAbstractBehaviour surface in surfaces)
            //关闭surface的更新
            surface.SetAutomaticUpdatesDisabled(true);
    }

    //在生成边塔前对实物建模的prop进行去除
    public void Hide() {
        if (DT_Display) {
            //对prop进行隐藏
            props = gameObject.GetComponentsInChildren<PropAbstractBehaviour>();
            foreach (PropAbstractBehaviour prop in props) {
                //将外层的建模体去除
                Renderer propRenderer = prop.GetComponent<MeshRenderer>();
                if (propRenderer != null) {
                    //去除mesh建模
                    Destroy(propRenderer);
                }
            }

            //对surface进行置换
            SurfaceAbstractBehaviour[] surfaces = gameObject.GetComponentsInChildren<SurfaceAbstractBehaviour>();

            foreach (SurfaceAbstractBehaviour surface in surfaces) {
                Renderer surfaceRenderer = surface.GetComponent<MeshRenderer>();   
                //去掉smart terrain的Render
                if (surfaceRenderer != null) {
                    Destroy(surfaceRenderer);
                }
            }

            DT_Display = false;
        }
    }

    //对实物建模进行刷新
    public void Refresh() {
              
        if ((mReconstructionBehaviour != null) && (mReconstructionBehaviour.Reconstruction != null)) {
            bool trackerWasActive = DT_Tracker.IsActive;
            // 停止Smart Terrain Tracker
            if (trackerWasActive)
                DT_Tracker.Stop();
            // 重置Reconstruction
            mReconstructionBehaviour.Reconstruction.Reset();
            // 重新进行扫描检测
            if (trackerWasActive) {
                DT_Tracker.Start();
                mReconstructionBehaviour.Reconstruction.Start();
            }
        }

        //重置表面为未发现
        surfaceFound = false;
        //重新置为0
        num = 0;
    }

    #endregion //PUBLIC_METHODS
}



