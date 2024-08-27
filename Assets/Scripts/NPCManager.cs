using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCManager : MonoBehaviour
{
    [SerializeField] private Transform _minPoint;
    [SerializeField] private Transform _maxPoint;
    public float moveSpeed = 3.0f;

    private NavMeshAgent _navMeshAgent;
    private Animator _animator;

    private void Start() 
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _navMeshAgent.speed = moveSpeed;

        _animator.SetInteger("legs", 5);
        StartCoroutine(MoveNPC());
    }

    private IEnumerator MoveNPC()
    {
        while (true)
        {
            Vector3 randomPosition = new Vector3(Random.Range(_minPoint.position.x, _maxPoint.position.x), 0, Random.Range(_minPoint.position.z, _maxPoint.position.z));

            _navMeshAgent.SetDestination(randomPosition);
            _animator.SetInteger("legs", 1);
            _animator.SetInteger("arms", 1);
            //Debug.Log("Moving to " + randomPosition);
            yield return new WaitUntil(() => (_navMeshAgent.remainingDistance < 0.1f && !_navMeshAgent.pathPending));

            _animator.SetInteger("legs", 5);
            _animator.SetInteger("arms", 5);
            //Debug.Log("Reached destination");
            yield return new WaitForSeconds(5.0f);
            
        }
    }
    
}
