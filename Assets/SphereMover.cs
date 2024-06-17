using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SphereMover : MonoBehaviour
{
    // 到着にかかる時間
    public float duration = 10.0f;

    private void Start() 
    {
        
        transform.DOMoveZ(3.0f, duration).SetEase(Ease.Linear);//.SetLoops(-1, LoopType.Restart);
    }
}
