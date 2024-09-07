using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ColliderEvent : UnityEvent<Collider> { }

public class CollisionEventDispacher : MonoBehaviour
{

    public ColliderEvent _OnColliderEvent;
    public ColliderEvent _OnColliderExitEvent;

    public bool IsDetectOnlyOnce = false;
    private bool _isFirst = true;

    private void OnTriggerEnter(Collider other)
    {
        if(!_isFirst && IsDetectOnlyOnce) return;

        _OnColliderEvent.Invoke(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!_isFirst && IsDetectOnlyOnce) return;

        _OnColliderEvent.Invoke(collision.collider);
    }

    private void OnCollisionExit(Collision collision) 
    { 
        _isFirst = false;
        _OnColliderExitEvent.Invoke(collision.collider);
    }

    private void OnTriggerExit(Collider other)
    {
        _isFirst = false;
        _OnColliderExitEvent.Invoke(other);
    }

}