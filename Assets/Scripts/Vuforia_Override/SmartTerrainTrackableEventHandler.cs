/*==============================================================================
Copyright (c) 2013-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
==============================================================================*/

using UnityEngine;
using Vuforia;

/// <summary>
/// Slightly different implementation than the DefaultTrackableEventHandler class:
/// In addition to its children, we turn on/off components of CylinderTrackable and its children here. 
/// </summary>
public class SmartTerrainTrackableEventHandler : MonoBehaviour,
                                            ITrackableEventHandler
{
    #region PUBLIC_MEMBERS

    //a way for the StateManager to know if the SmartTerrainTrackable was lost or found most recently
    //Accordingly, show/hide the surface based on what state the app is in.
    public bool m_trackablesFound = false;
    #endregion //PUBLIC_MEMBERS

    #region PRIVATE_MEMBER_VARIABLES

    private ImageTargetAbstractBehaviour DT_ImageTarget;
    private TrackableBehaviour mTrackableBehaviour;
    
    //only required to hide the surface mesh the first time it's detected
    private bool m_TrackableDetectedForFirstTime = true;

    #endregion // PRIVATE_MEMBER_VARIABLES

    #region UNTIY_MONOBEHAVIOUR_METHODS
    
    void Start()
    {
        DT_ImageTarget = FindObjectOfType(typeof(ImageTargetAbstractBehaviour)) as ImageTargetAbstractBehaviour;

        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
        {
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
        }
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


    private void OnTrackingFound()
    {
        Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
        Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);
		WireframeBehaviour[] wireframeComponents = GetComponentsInChildren<WireframeBehaviour>(true);

        // Enable rendering:
        foreach (Renderer component in rendererComponents) {
            component.enabled = true;
            if (m_TrackableDetectedForFirstTime)
                m_TrackableDetectedForFirstTime = false;
                
        }

        // Enable colliders:
        foreach (Collider component in colliderComponents)
        {
            component.enabled = true;
        }
		
		// Enable wireframe rendering:
        foreach (WireframeBehaviour component in wireframeComponents)
        {
            component.enabled = true;
        }

        Debug.Log("Trackable " + mTrackableBehaviour.gameObject.name + " found");

        if (DT_ImageTarget != null)
        {
            Renderer[] rendererComponentsOfCylinder = DT_ImageTarget.gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer component in rendererComponentsOfCylinder)
            {
                component.enabled = true;
            }
        }
        
        m_trackablesFound = true;
    }


    private void OnTrackingLost()
    {
        Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
        Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);
		WireframeBehaviour[] wireframeComponents = GetComponentsInChildren<WireframeBehaviour>(true);

        // Disable rendering:
        foreach (Renderer component in rendererComponents)
        {
            component.enabled = false;
        }

        // Disable colliders:
        foreach (Collider component in colliderComponents)
        {
            component.enabled = false;
        }
		
		// Disable wireframe rendering:
        foreach (WireframeBehaviour component in wireframeComponents)
        {
            component.enabled = false;
        }

        Debug.Log("Trackable " + mTrackableBehaviour.gameObject.name + " lost");

        //当smart terrain消失时同时将image下的物体进行disable

        if (DT_ImageTarget != null)
        {
            Renderer[] rendererComponentsOfCylinder = DT_ImageTarget.gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer component in rendererComponentsOfCylinder)
            {
                component.enabled = false;
            }
        }
        
        m_trackablesFound = false;
    }

    #endregion // PRIVATE_METHODS
}
