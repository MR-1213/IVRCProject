using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MeshDeformer : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] modifiedVertices;
    public float deformationStrength = 0.1f;
    private MeshCollider meshCollider;
    private List<int> triangles;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();
        triangles = new List<int>(mesh.triangles);

        modifiedVertices = mesh.vertices;
    }

    void UpdateMesh()
    {
        mesh.vertices = modifiedVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshCollider.sharedMesh = null; // 一旦nullに設定してから
        meshCollider.sharedMesh = mesh; // 更新されたメッシュを再設定する
    }

    void OnCollisionEnter(Collision collision)
    {
        DeformMesh(collision);
    }

    void DeformMesh(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            DeformVertices(contact.point, collision.transform.position);
        }
        UpdateMesh();
    }

    private void DeformVertices(Vector3 contactPoint , Vector3 collisionPoint)
    {
        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            if(i <= 2178 || (i >= 3268 && i <= 4356) || i >= 5446)
            {
                continue;
            }
            // 頂点を赤くする
            // Debug.DrawLine(transform.TransformPoint(modifiedVertices[i]), transform.TransformPoint(modifiedVertices[i]) + Vector3.up * 0.1f, Color.red);
            // Debug.Log(i);
            // yield return null;
            
            // 頂点座標をワールド座標に変換
            Vector3 worldVertex = transform.TransformPoint(modifiedVertices[i]);
            // 頂点と衝突点の距離を計算
            float distance = Vector3.Distance(worldVertex, contactPoint);
            float pointDistance = Vector3.Distance(worldVertex, collisionPoint);
            // 衝突点からの距離に応じて頂点を変形
            if (distance < deformationStrength && pointDistance < 0.6f)
            {
                Vector3 deformation = new Vector3(0, 0, 1f) * deformationStrength * (1 - distance / deformationStrength);
                modifiedVertices[i] += transform.InverseTransformDirection(deformation);
            }

        }
        
        // 三角形の削除操作
        for (int i = 0; i < triangles.Count; i += 3)
        {
            if(triangles[i] <= 2178 || (triangles[i] >= 3268 && triangles[i] <= 4356) || triangles[i] >= 5446)
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