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
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOMoveZ(3.0f, duration).SetEase(Ease.Linear))
                /*.Join(transform.DOScale(new Vector3(1, 1, 1), 30.0f).SetEase(Ease.Linear))*/;
    }
}
