using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class DT_ImageTargetEventHandler : MonoBehaviour, ITrackableEventHandler {

    #region PUBLIC_MEMBERS

    //观察者模式
    public delegate void ImageTrackableFoundFirstTime();
    public static event ImageTrackableFoundFirstTime OnFoundImage;

    //与检测状态相关的mark图标
    public GameObject Checkmark;

    #endregion //PUBLIC_METHODS


    #region PRIVATE_MEMBER_VARIABLES

    private TrackableBehaviour DT_TrackableBehaviour;
    private bool DT_TrackableDetectedForFirstTime;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region UNTIY_MONOBEHAVIOUR_METHODS

    void Start() {
        DT_TrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (DT_TrackableBehaviour)
            DT_TrackableBehaviour.RegisterTrackableEventHandler(this);
    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS



    #region PUBLIC_METHODS

    /// <summary>
    /// Implementation of the ITrackableEventHandler function called when the
    /// tracking state changes.
    /// </summary>
    public void OnTrackableStateChanged(
                                    TrackableBehaviour.Status previousStatus,
                                    TrackableBehaviour.Status newStatus)
    {
        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED)
        {
            OnTrackingFound();
        }
        else
        {
            OnTrackingLost();
        }
    }

    #endregion // PUBLIC_METHODS

    #region PRIVATE_METHODS

    private void OnTrackingFound() {
        Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
        Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

        // Enable rendering:
        foreach (Renderer component in rendererComponents)
            component.enabled = true;

        // Enable colliders:
        foreach (Collider component in colliderComponents)
            component.enabled = true;

        //对场记进行通知
        if (!DT_TrackableDetectedForFirstTime) {
            if (OnFoundImage != null) {
                OnFoundImage();
                Debug.Log("First Time Trackable Found at [" + Time.time + "]");
            }
            DT_TrackableDetectedForFirstTime = true;
        }

        if (Checkmark.activeInHierarchy == true)
            //将Checkmark置为绿色
            Checkmark.GetComponent<UnityEngine.UI.Image>().color = Color.green;
    }

    private void OnTrackingLost() {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        if (Checkmark != null && Checkmark.activeInHierarchy == true)
        //将Checkmark置为白色
            Checkmark.GetComponent<UnityEngine.UI.Image>().color = Color.white;
    }

    #endregion // PRIVATE_METHODS
}
