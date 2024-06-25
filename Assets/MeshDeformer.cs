using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MeshDeformer : MonoBehaviour
{
    public Transform _sphere; // Cubeを融かす球体のTransform
    public float deformationStrength = 0.1f; // 変形の強さ

    private MeshCollider meshCollider;
    private Mesh mesh;
    private Vector3[] modifiedVertices; // 変形する頂点の配列
    private List<int> triangles; // 三角形のリスト
    private int[] _lastVertexIndex = new int[6]; //+Z方向を基準として、上、下、奥、左、手前、右の面の最後尾の頂点インデックス
    private int[] _meltVertices_1 = new int[2]; // 頂点の変形操作を行う頂点インデックスの範囲1(プレイヤーがいる方向の面)
    private int[] _meltVertices_2 = new int[2] {-1, -1}; // 頂点の変形操作を行う頂点インデックスの範囲2(ななめ方向であった場合のとなりの面)
    private int[] _penetrationVertices_1 = new int[2];
    private Vector3 _meltDirection; // 融ける方向のベクトル(前後左右ななめのいずれか)
    private bool _isFirst = true; // 衝突時、初回のみ処理を行うためのフラグ

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();

        // 三角形の配列をコピー
        triangles = new List<int>(mesh.triangles);
        // 頂点の配列をコピー
        modifiedVertices = mesh.vertices;
        // 頂点の数を確認し、各面の最後尾の頂点インデックスを確認する
        CheckVerticesCount();
        _penetrationVertices_1[0] = _lastVertexIndex[1] + 1;
        _penetrationVertices_1[1] = _lastVertexIndex[2];
    }

    private void CheckVerticesCount()
    {
        for(int i = 0; i < 6; i++)
        {
            _lastVertexIndex[i] = modifiedVertices.Length / 6 * (i + 1);     
        }
    }

    private void CheckMeltFace()
    {
        // Cubeの正面に対して、どの角度にプレイヤーがいるかを判定する
        float angle = Vector3.Angle(transform.forward, _sphere.position - transform.position);
        // 外積を求める
        Vector3 cross = Vector3.Cross(transform.forward, _sphere.position - transform.position);
        // 右回り360度表現に変換
        float playerAngle = cross.y > 0 ? angle : (360 - angle);
        //Debug.Log(playerAngle);

        // どの面に対して操作を走らせるかを判定する
        switch(playerAngle)
        {
            case float n when ((n >= 0f && n < 45f) || (n >= 315f && n <= 360f)):
                //Debug.Log("奥の面");
                _meltVertices_1[0] = _lastVertexIndex[1] + 1; // 対象の面の最初の頂点インデックス
                _meltVertices_1[1] = _lastVertexIndex[2]; // 対象の面の最後の頂点インデックス
                _meltDirection = new Vector3(0, 0, -1); // 融ける方向のベクトル
                break;
            case float n when (n >= 45f && n < 135f):
                //Debug.Log("右の面");
                _meltVertices_1[0] = _lastVertexIndex[4] + 1;
                _meltVertices_1[1] = _lastVertexIndex[5];
                _meltDirection = new Vector3(-1, 0, 0);
                break;
            case float n when (n >= 135f && n < 225f):
                //Debug.Log("手前の面");
                _meltVertices_1[0] = _lastVertexIndex[3] + 1;
                _meltVertices_1[1] = _lastVertexIndex[4];
                _meltDirection = new Vector3(0, 0, 1);
                break;
            case float n when (n >= 225f && n < 315f):
                //Debug.Log("左の面");
                _meltVertices_1[0] = _lastVertexIndex[2] + 1;
                _meltVertices_1[1] = _lastVertexIndex[3];
                _meltDirection = new Vector3(1, 0, 0);
                break;
        }
        
        // Cubeのななめ方向からの進入の場合、2面に対して操作を走らせるため、その面を判定する

        // Cube中心からの斜め方向のRayを飛ばし、衝突した場合、そのRayの両隣の面が操作対象となる
        Vector3[] rayDirection = new Vector3[4] {new Vector3(1, 0, 1), new Vector3(-1, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1)};
        for(int i = 0; i < 4; i++)
        {
            if(Physics.Raycast(transform.position, rayDirection[i], out RaycastHit hit, 5.0f))
            {
                switch(i)
                {
                    case 0:
                        //Debug.Log("奥右");
                        _meltVertices_2[0] = _meltDirection == new Vector3(0, 0, -1) ? _lastVertexIndex[4] + 1 : _lastVertexIndex[1] + 1; // 対象の面の最初の頂点インデックス
                        _meltVertices_2[1] = _meltDirection == new Vector3(0, 0, -1) ? _lastVertexIndex[5] : _lastVertexIndex[2]; // 対象の面の最後の頂点インデックス
                        _meltDirection = new Vector3(-1, 0, -1); // 融ける方向のベクトル
                        break;
                    case 1:
                        //Debug.Log("奥左");
                        _meltVertices_2[0] = _meltDirection == new Vector3(0, 0, -1) ? _lastVertexIndex[2] + 1 : _lastVertexIndex[1] + 1;
                        _meltVertices_2[1] = _meltDirection == new Vector3(0, 0, -1) ? _lastVertexIndex[3] : _lastVertexIndex[2];
                        _meltDirection = new Vector3(1, 0, -1);
                        break;
                    case 2:
                        //Debug.Log("手前右");
                        _meltVertices_2[0] = _meltDirection == new Vector3(0, 0, 1) ? _lastVertexIndex[4] + 1 : _lastVertexIndex[3] + 1;
                        _meltVertices_2[1] = _meltDirection == new Vector3(0, 0, 1) ? _lastVertexIndex[5] : _lastVertexIndex[4];
                        _meltDirection = new Vector3(-1, 0, 1);
                        break;
                    case 3:
                        //Debug.Log("手前左");
                        _meltVertices_2[0] = _meltDirection == new Vector3(0, 0, 1) ? _lastVertexIndex[2] + 1 : _lastVertexIndex[3] + 1;
                        _meltVertices_2[1] = _meltDirection == new Vector3(0, 0, 1) ? _lastVertexIndex[3] : _lastVertexIndex[4];
                        _meltDirection = new Vector3(1, 0, 1);
                        break;
                }
            }
            else
            {
                Debug.Log("Nothing");
            }
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        DeformMesh(collision);
    }

    private void DeformMesh(Collision collision)
    {
        if(_isFirst)
        {
            // 初回のみ処理を行う
            _isFirst = false;
            CheckMeltFace();
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            // 衝突点ごとに頂点を変形
            DeformVertices(contact.point, collision.transform.position);

            //StartCoroutine(DeformVertices(contact.point, collision.transform.position));
        }
        // メッシュコライダー更新のため、メッシュを更新
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        mesh.vertices = modifiedVertices;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshCollider.sharedMesh = null; // 一旦nullに設定してから
        meshCollider.sharedMesh = mesh; // 更新されたメッシュを再設定する
    }

    private void DeformVertices(Vector3 contactPoint , Vector3 collisionPoint)
    {
        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            // 処理する頂点の範囲外であれば、スキップ
            if(!(_meltVertices_1[0] <= i && i <= _meltVertices_1[1]))
            {
                if(_meltVertices_2[0] != -1)
                {
                    if(!(_meltVertices_2[0] <= i && i <= _meltVertices_2[1]))
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
            }
            
            // 頂点を赤くする
            // Debug.DrawLine(transform.TransformPoint(modifiedVertices[i]), transform.TransformPoint(modifiedVertices[i]) + Vector3.up * 0.1f, Color.red);
            // yield return null;
            
            // 頂点座標をワールド座標に変換
            Vector3 worldVertex = transform.TransformPoint(modifiedVertices[i]);
            // 頂点と衝突点の距離を計算
            float distance = Vector3.Distance(worldVertex, contactPoint);
            float pointDistance = Vector3.Distance(worldVertex, collisionPoint);
            // 衝突点からの距離に応じて頂点を変形
            if (distance < deformationStrength && pointDistance < 0.6f)
            {
                Vector3 deformation = _meltDirection* deformationStrength * (1 - distance / deformationStrength);
                modifiedVertices[i] += transform.InverseTransformDirection(deformation);
            }

        }
        
        // 三角形の削除操作
        // これより下は未修正

        for (int i = 0; i < triangles.Count; i += 3)
        {
            // 処理する頂点の範囲外であれば、スキップ
            if(!(_meltVertices_1[0] <= triangles[i] && triangles[i] <= _meltVertices_1[1]))
            {
                if(!(_penetrationVertices_1[0] <= triangles[i] && triangles[i] <= _penetrationVertices_1[1]))
                {
                    continue;
                }
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
