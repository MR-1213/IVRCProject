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
    [SerializeField] private SpatialAnchorLocalStorageManagerBuildingBlock _storage;
    [SerializeField] private ControllerButtonsMapper _controllerButtonsMapper;

    private List<OVRSpatialAnchor> _anchors = new List<OVRSpatialAnchor>();
    //private List<GameObject> _anchorInstances = new List<GameObject>();
    [SerializeField] private GameObject _anchorInstance;

    private Vector3 currentPosition;
    private Quaternion currentRotation;
    
    private void Start()
    {
        Debug.Log("WallAnchorManager Init");
        //_storage.Reset();
        // 保存済みのアンカーをロード
        _anchorLoader.LoadAnchorsFromDefaultLocalStorage();
        
        if(_isSetAnchorOnGameStart)
        {

        }
        else
        {
            _controllerButtonsMapper.enabled = false;
            _rightControllerAnchor.transform.GetChild(2).gameObject.SetActive(false);
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
                //anchor.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
                anchor.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// ロードされたアンカーを表示する
    /// </summary>
    public void AnchorsVisibility()
    {
        if(_anchors.Count == 0)
        {
            Debug.Log("アンカーがありません");
            return;
        }

        foreach(var anchor in _anchors)
        {
            //anchor.gameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
            anchor.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// ロードされたアンカーの位置に移動用のアンカーオブジェクトを表示させる
    /// </summary>
    public void SetAnchorInstance()
    {
        foreach(var anchor in _anchors)
        {
            currentPosition = anchor.transform.position;
            currentRotation = anchor.transform.rotation;
        }
    }

    /// <summary>
    /// 右手に追従させる
    /// </summary>
    public void Active_FollowAnchorToRightHand()
    {
        _anchorInstance.transform.parent = _rightControllerAnchor.transform;
        _anchorInstance.transform.position = currentPosition;
        _anchorInstance.transform.localPosition = new Vector3(-0.03f, 0.1f, 0f);
        _anchorInstance.transform.rotation = currentRotation;
        _anchorInstance.SetActive(true);
        AnchorsInvisibility();
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

    public void AnchorInstanceInvisibility()
    {
        _anchorInstance.SetActive(false);
    }

    public void Inactive_FollowAnchorToRightHand()
    {
        _anchorInstance.transform.parent = null;
        currentPosition = _anchorInstance.transform.position;
        currentRotation = _anchorInstance.transform.rotation;
    }
}
