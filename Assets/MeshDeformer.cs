using UnityEngine;

public class MeshDeformer : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;
    public float deformationStrength = 0.1f;
    private MeshCollider meshCollider;

    void Start()
    {
        // Cubeのメッシュを取得
        mesh = GetComponent<MeshFilter>().mesh;
        originalVertices = mesh.vertices;
        modifiedVertices = mesh.vertices;
        meshCollider = GetComponent<MeshCollider>();
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
            Vector3 moveDirection = transform.position - collision.transform.position;
            DeformVertices(contact.point, moveDirection);
        }
        UpdateMesh();
    }

    void DeformVertices(Vector3 contactPoint, Vector3 moveDirection)
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
                
                int[] triangles = mesh.triangles;
                for (int j = 0; j < triangles.Length; j += 3)
                {
                    // 三角形のすべての頂点がz軸の値が1.0fを超えている場合、その三角形を削除する
                    if (modifiedVertices[triangles[j]].z >= 1.0f && modifiedVertices[triangles[j + 1]].z >= 1.0f && modifiedVertices[triangles[j + 2]].z >= 1.0f && originalVertices[triangles[j]].z < 1.0f)
                    {
                        triangles[j] = triangles[j + 1] = triangles[j + 2] = 0;
                    }
                    else if ((triangles[j] == i || triangles[j + 1] == i || triangles[j + 2] == i) && originalVertices[triangles[j]].z >= 1.0f)
                    {
                        triangles[j] = triangles[j + 1] = triangles[j + 2] = 0;
                    }
                    // 一部のみ頂点のz軸の値が1.0fを超えている場合、頂点のZ座標を1にする
                    else 
                    {
                        if(modifiedVertices[triangles[j]].z > 1.0f)
                        {
                            modifiedVertices[triangles[j]].z = 1.0f;
                        }
                        
                        if(modifiedVertices[triangles[j + 1]].z > 1.0f)
                        {
                            modifiedVertices[triangles[j + 1]].z = 1.0f;
                        }
                        
                        if(modifiedVertices[triangles[j + 2]].z > 1.0f)
                        {
                            modifiedVertices[triangles[j + 2]].z = 1.0f;
                        }
                    }
                }
                mesh.triangles = triangles; 

            }


        }
    }
}
