using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MeshDeformer : MonoBehaviour
{
    public float deformationStrength = 0.1f;
    [SerializeField] private InvisibleWallManager _invisibleWallManager;

    private Mesh mesh;
    private List<int> triangles;
    private MeshCollider meshCollider;
    private Vector3[] modifiedVertices;
    private int[] verticesIndex = new int[6];
    
    private bool _isMeltAllowed = true;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();
        triangles = new List<int>(mesh.triangles);
        modifiedVertices = mesh.vertices;

        CalculateVerticesIndex();
    }

    private void CalculateVerticesIndex()
    {
        for(int i = 0; i < 6; i++)
        {
            int index = mesh.vertices.Length / 6 * (i + 1);
            verticesIndex[i] = index;
        }
    }

    void UpdateMesh()
    {
        mesh.vertices = modifiedVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshCollider.sharedMesh = null; // 一旦nullに設定してから
        meshCollider.sharedMesh = mesh; // 更新されたメッシュを再設定する
    }

    private void Update() 
    {
        if(this.transform.position.z + 12.0f < _invisibleWallManager.transform.position.z)
        {
            _isMeltAllowed = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!_isMeltAllowed)
        {
            return;
        }
        DeformMesh(collision);
    }

    void DeformMesh(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            DeformVertices(contact.point);
            //StartCoroutine(DeformVertices(contact.point));
        }
        UpdateMesh();
    }

    private void DeformVertices(Vector3 contactPoint)
    {
        for (int i = 0; i < modifiedVertices.Length; i++)
        {

            if(i <= (verticesIndex[3]))
            {
                continue;
            }
            //頂点を赤くする
            // Debug.DrawLine(transform.TransformPoint(modifiedVertices[i]), transform.TransformPoint(modifiedVertices[i]) + Vector3.up * 0.1f, Color.red);
            // Debug.Log(i);
            // yield return null;
            
            // 頂点座標をワールド座標に変換
            Vector3 worldVertex = transform.TransformPoint(modifiedVertices[i]);
            // 頂点と衝突点の距離を計算
            float distance = Vector3.Distance(worldVertex, contactPoint);
            //float pointDistance = Vector3.Distance(worldVertex, collisionPoint);
            // 衝突点からの距離に応じて頂点を変形
            if (distance < deformationStrength)
            {
                Vector3 deformation = new Vector3(0, 0, 1f) * deformationStrength * (1 - distance / deformationStrength);
                modifiedVertices[i] += transform.InverseTransformDirection(deformation);
            }

        }
        
        // 三角形の削除操作
        for (int i = 0; i < triangles.Count; i += 3)
        {

            if(i <= (verticesIndex[3]))
            {
                continue;
            }
            Vector3 v1 = modifiedVertices[triangles[i]];
            Vector3 v2 = modifiedVertices[triangles[i + 1]];
            Vector3 v3 = modifiedVertices[triangles[i + 2]];

             
            if (v1.z > 1.0f && v2.z > 1.0f && v3.z > 1.0f)
            {
                triangles.RemoveRange(i, 3);
                i -= 3;
            }
            
            else if (v1.z > 1.2f)
            {
                v1.z = 1.0f;
                modifiedVertices[triangles[i]] = v1;
            }
            else if (v2.z > 1.2f)
            {
                v2.z = 1.0f;
                modifiedVertices[triangles[i + 1]] = v2;
            }
            else if (v3.z > 1.2f)
            {
                v3.z = 1.0f;
                modifiedVertices[triangles[i + 2]] = v3;
            }
            
        }
        
        // 三角形のリストを更新
        mesh.triangles = triangles.ToArray();
    }
}