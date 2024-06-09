using UnityEngine;
using System.Collections.Generic;

public class MeshDeformer : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;
    public float deformationStrength = 0.1f;
    private MeshCollider meshCollider;
    private int[] originalTriangles;
    private Dictionary<int, List<int>> vertexToTriangles = new Dictionary<int, List<int>>();

    void Start()
    {
        // Cubeのメッシュを取得
        mesh = GetComponent<MeshFilter>().mesh;
        originalVertices = mesh.vertices;
        modifiedVertices = mesh.vertices;
        meshCollider = GetComponent<MeshCollider>();
        originalTriangles = mesh.triangles;

        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int vertexIndex = originalTriangles[i + j];
                if (!vertexToTriangles.ContainsKey(vertexIndex))
                {
                    vertexToTriangles[vertexIndex] = new List<int>();
                }
                vertexToTriangles[vertexIndex].Add(i / 3); // Add the triangle index
            }
        }
        
        // foreach(var pair in vertexToTriangles)
        // {
        //     Debug.Log(pair.Key + ": " + string.Join(", ", pair.Value));
        // }
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

    void DeformVertices(Vector3 contactPoint , Vector3 collisionPoint)
    {
        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            // 頂点座標をワールド座標に変換
            Vector3 worldVertex = transform.TransformPoint(modifiedVertices[i]);
            // 頂点と衝突点の距離を計算
            float distance = Vector3.Distance(worldVertex, contactPoint);
            // 衝突点からの距離に応じて頂点を変形
            if (distance < deformationStrength)
            {
                Vector3 deformation = new Vector3(0, 0, 1f) * deformationStrength * (1 - distance / deformationStrength);
                modifiedVertices[i] += transform.InverseTransformDirection(deformation);

                if (vertexToTriangles.ContainsKey(i) && (modifiedVertices[i].z >= 1.2f))
                {
                    foreach (int triangleIndex in vertexToTriangles[i])
                    {
                        int baseIndex = triangleIndex * 3;
                        if(modifiedVertices[originalTriangles[baseIndex]].z >= 1.0f && modifiedVertices[originalTriangles[baseIndex + 1]].z >= 1.0f && modifiedVertices[originalTriangles[baseIndex + 2]].z >= 1.0f)
                        {
                            if(Vector3.Distance(worldVertex, collisionPoint)< 0.4f)
                            {
                                originalTriangles[baseIndex] = originalTriangles[baseIndex + 1] = originalTriangles[baseIndex + 2] = 0;
                                continue;
                            }
                            
                        }
                        
                        if(modifiedVertices[originalTriangles[baseIndex]].z >= 1.0f)
                        {
                            modifiedVertices[originalTriangles[baseIndex]].z = 1.0f;
                        }
                        
                        if(modifiedVertices[originalTriangles[baseIndex + 1]].z >= 1.0f)
                        {
                            modifiedVertices[originalTriangles[baseIndex + 1]].z = 1.0f;
                        }
                        
                        if(modifiedVertices[originalTriangles[baseIndex + 2]].z >= 1.0f)
                        {
                            modifiedVertices[originalTriangles[baseIndex + 2]].z = 1.0f;
                        }

                    }
                    mesh.triangles = originalTriangles;
                } 

            }


        }
    }
}
