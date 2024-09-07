using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInstantiate : MonoBehaviour
{
    [SerializeField] private GameObject _npcPrefab;
    [SerializeField] private PointArray[] _npcMovePoints;

    [System.Serializable]
    class PointArray
    {
        public GameObject[] point;
    }
    
    [SerializeField] private CountArray[] _npcCounts;

    [System.Serializable]
    class CountArray
    {
        public int[] count;
    }

    private void Start()
    {
        SetNPCNextPoint(1);
    }

    public void SetNPCNextPoint(int pointNum)
    {
        if(pointNum > 1)
        {
            foreach(Transform npc in this.GetComponentInChildren<Transform>())
            {
                if(npc.gameObject.transform == this.transform) continue;
                Destroy(npc.gameObject);
            }
        }
        
        Transform _minPoint;
        Transform _maxPoint;
        for(int i = 0; i < _npcMovePoints[pointNum - 1].point.Length; i++)
        {
            var point = _npcMovePoints[pointNum - 1].point[i];
            _minPoint = point.transform.GetChild(0);
            _maxPoint = point.transform.GetChild(1);

            for(int j = 0; j < _npcCounts[pointNum - 1].count[i]; j++)
            {
                var npc = Instantiate(_npcPrefab, 
                            new Vector3(Random.Range(_minPoint.position.x, _maxPoint.position.x), 0f, Random.Range(_minPoint.position.z, _maxPoint.position.z)), 
                            Quaternion.identity, 
                            this.transform);

                npc.GetComponent<NPCManager>().minPoint = _minPoint;
                npc.GetComponent<NPCManager>().maxPoint = _maxPoint;
            }
        
            
        }
        
    }
}
