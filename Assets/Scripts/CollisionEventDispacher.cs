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

    private void OnTriggerEnter(Collider other)
    {
        _OnColliderEvent.Invoke(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        _OnColliderEvent.Invoke(collision.collider);
    }

    private void OnCollisionExit(Collision collision) 
    { 
        _OnColliderExitEvent.Invoke(collision.collider);
    }

    private void OnTriggerExit(Collider other)
    {
        _OnColliderExitEvent.Invoke(other);
    }

}