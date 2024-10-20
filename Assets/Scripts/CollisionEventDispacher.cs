using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ColliderEvent : UnityEvent<Collider> { }

public class CollisionEventDispacher : MonoBehaviour
{
    [Header("ColliderEnter側のオプション")]
    public bool IsDispatchThisGameObject = false;
    public ColliderEvent _OnColliderEvent;
    public ColliderEvent _OnColliderExitEvent;

    public bool IsDetectOnlyOnce = false;
    private bool _isFirst = true;

    private void OnTriggerEnter(Collider other)
    {
        if(!_isFirst && IsDetectOnlyOnce) return;

        if(IsDispatchThisGameObject)
        {
            _OnColliderEvent.Invoke(this.GetComponent<Collider>());
        }
        else
        {
            _OnColliderEvent.Invoke(other);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!_isFirst && IsDetectOnlyOnce) return;

        if(IsDispatchThisGameObject)
        {
            _OnColliderEvent.Invoke(this.GetComponent<Collider>());
        }
        else
        {
            _OnColliderEvent.Invoke(collision.collider);
        }
    }

    private void OnCollisionExit(Collision collision) 
    { 
        _isFirst = false;
        if(IsDispatchThisGameObject)
        {
            _OnColliderExitEvent.Invoke(this.GetComponent<Collider>());
        }
        else
        {
            _OnColliderExitEvent.Invoke(collision.collider);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _isFirst = false;
        if(IsDispatchThisGameObject)
        {
            _OnColliderExitEvent.Invoke(this.GetComponent<Collider>());
        }
        else
        {
            _OnColliderExitEvent.Invoke(other);
        }
    }

}