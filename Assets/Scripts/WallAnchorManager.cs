using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using UnityEngine;

public class WallAnchorManager : MonoBehaviour
{
    public bool _isSetAnchorOnGameStart = false;
    [SerializeField] private GameObject _rightControllerAnchor;

    [SerializeField] private SpatialAnchorLoaderBuildingBlock _anchorLoader;
    [SerializeField] private ControllerButtonsMapper _controllerButtonsMapper;

    private List<OVRSpatialAnchor> _anchors = new List<OVRSpatialAnchor>();
    private List<GameObject> _anchorInstances = new List<GameObject>();
    
    private void Start()
    {
        Debug.Log("WallAnchorManager Init");
        // 保存済みのアンカーをロード
        _anchorLoader.LoadAnchorsFromDefaultLocalStorage();
        
        if(_isSetAnchorOnGameStart)
        {

        }
        else
        {
            _controllerButtonsMapper.enabled = false;
            _rightControllerAnchor.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 保存済みアンカーを取得し、非表示にする
    ///　Start()の初期アンカーロード後にOnAnchorsLoadCompletedイベントから一度呼び出される 
    /// </summary>
    /// <param name="anchors"></param>
    public void AnchorsLoadingAndInvisibility(List<OVRSpatialAnchor> anchors)
    {
        if(anchors.Count == 0)
        {
            Debug.Log("アンカーがありません");
            return;
        }

        _anchors = anchors;
        if(_isSetAnchorOnGameStart)
        {

        }
        else
        {
            foreach(var anchor in anchors)
            {
                anchor.gameObject.SetActive(false);
            }
        }
    }

    public void AnchorsVisibility()
    {
        if(_anchors.Count == 0)
        {
            Debug.Log("アンカーがありません");
            return;
        }

        foreach(var anchor in _anchors)
        {
            anchor.gameObject.SetActive(true);
        }
    }

    public void SetAnchorInstance()
    {
        foreach(var anchor in _anchors)
        {
            var anchorInstance = Instantiate(anchor, anchor.transform.position, anchor.transform.rotation);
            _anchorInstances.Add(anchorInstance.gameObject);
        }

    }

    public void Active_FollowAnchorToRightHand()
    {
        foreach(var anchorInstance in _anchorInstances)
        {
            anchorInstance.transform.parent = _rightControllerAnchor.transform;
        }
    }

    public void AnchorsInvisibility()
    {
        if(_anchors.Count == 0)
        {
            Debug.Log("アンカーがありません");
            return;
        }

        foreach(var anchor in _anchors)
        {
            anchor.gameObject.SetActive(false);
        }
    }

    public void Inactive_FollowAnchorToRightHand()
    {
        foreach(var anchorInstance in _anchorInstances)
        {
            anchorInstance.transform.parent = null;
        }
    }
}
