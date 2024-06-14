using UnityEngine;
using System.Collections.Generic;

public class MeshDeformer : MonoBehaviour
{
    private GameObject _cube;
    [SerializeField] private GameObject sphere;
    private Mesh mesh;
    private Vector3[] modifiedVertices;
    public float deformationStrength = 0.1f;
    private MeshCollider meshCollider;
    private int[] modifiedTriangles;
    private Dictionary<int, List<int>> vertexToTriangles = new Dictionary<int, List<int>>();
    private float elapsedTime = 0f;

    void Start()
    {
        // Cubeのメッシュを取得
        mesh = GetComponent<MeshFilter>().mesh;
        modifiedVertices = mesh.vertices;
        meshCollider = GetComponent<MeshCollider>();
        modifiedTriangles = mesh.triangles;

        _cube = this.gameObject;

        for (int i = 0; i < modifiedTriangles.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int vertexIndex = modifiedTriangles[i + j];
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

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime > 10f)
        {
            elapsedTime = 0f;
            Destroy(_cube);
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
                        if(modifiedVertices[modifiedTriangles[baseIndex]].z >= 1.0f && modifiedVertices[modifiedTriangles[baseIndex + 1]].z >= 1.0f && modifiedVertices[modifiedTriangles[baseIndex + 2]].z >= 1.0f)
                        {
                            if(Vector3.Distance(worldVertex, collisionPoint)< 0.4f)
                            {
                                modifiedTriangles[baseIndex] = modifiedTriangles[baseIndex + 1] = modifiedTriangles[baseIndex + 2] = 0;
                                continue;
                            }
                            
                        }
                        
                        if(modifiedVertices[modifiedTriangles[baseIndex]].z >= 1.0f)
                        {
                            modifiedVertices[modifiedTriangles[baseIndex]].z = 1.0f;
                        }
                        
                        if(modifiedVertices[modifiedTriangles[baseIndex + 1]].z >= 1.0f)
                        {
                            modifiedVertices[modifiedTriangles[baseIndex + 1]].z = 1.0f;
                        }
                        
                        if(modifiedVertices[modifiedTriangles[baseIndex + 2]].z >= 1.0f)
                        {
                            modifiedVertices[modifiedTriangles[baseIndex + 2]].z = 1.0f;
                        }

                    }
                    mesh.triangles = modifiedTriangles;
                } 

            }


        }
    }
}
