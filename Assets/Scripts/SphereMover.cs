using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SphereMover : MonoBehaviour
{
    public Transform target; // 目的地
    // 到着にかかる時間
    public float duration = 8.0f;

    private void Start() 
    {
        
        transform.DOMove(target.position, duration).SetEase(Ease.Linear);//.SetLoops(-1, LoopType.Restart);
    }
}
