using IVRC2024.Utils;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public sealed class Meltee : MonoBehaviour
{
    /// <summary>
    /// 内外判定メッシュ
    /// </summary>
    [SerializeField]
    private Mesh pointInPolygonMesh;
    [SerializeField]
    private float radius = 0.5f;
    [SerializeField]
    private float strength = 0.1f;

    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    private GameObject _pointInPolygonObject;
    private MeshCollider _pointInPolygonCollider;
    private Mesh _mesh;
    private NativeArray<Vector3> _vertices;
    private NativeList<ushort> _indices;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
    }

    private void Start()
    {
        // メッシュを読み込む
        _mesh = _meshFilter.mesh;
        _mesh.MarkDynamic();
        using (var dataArray = Mesh.AcquireReadOnlyMeshData(_mesh))
        {
            var data = dataArray[0];

            var vertices = new NativeArray<Vector3>(_mesh.vertexCount, Allocator.Persistent);
            data.GetVertices(vertices);
            _vertices = vertices;

            var indexData = data.GetIndexData<ushort>();
            var indices = new NativeList<ushort>(indexData.Length, Allocator.Persistent);
            indices.CopyFrom(indexData);
            _indices = indices;
        }

        // 内外判定用のオブジェクトを生成する
        _pointInPolygonObject = new GameObject($"{name}_col");
        _pointInPolygonObject.transform.SetParent(transform, false);
        _pointInPolygonCollider = _pointInPolygonObject.AddComponent<MeshCollider>();
        _pointInPolygonCollider.sharedMesh = pointInPolygonMesh != null ? pointInPolygonMesh : _mesh;
        _pointInPolygonCollider.convex = true;
        _pointInPolygonCollider.isTrigger = true;
    }

    private void OnDestroy()
    {
        if (_pointInPolygonObject != null)
        {
            Destroy(_pointInPolygonObject);
        }
        _pointInPolygonObject = null;
        _pointInPolygonCollider = null;
        _vertices.Dispose();
        _indices.Dispose();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 熱源の位置を取得
        using var contactBuffer = GetContacts(collision);
        var contacts = contactBuffer.Span;
        var available = contacts.Length;
        const int MaxStackLimit = 1024;
        using SpanOwner<Vector3> heatSourceBuffer = available * Unsafe.SizeOf<Vector3>() <= MaxStackLimit ? new(stackalloc Vector3[available]) : new(available);
        var heatSources = heatSourceBuffer.Span;
        for (int i = 0; i < available; i++)
        {
            heatSources[i] = contacts[i].point;
        }

        var direction = (collision.relativeVelocity != Vector3.zero ? collision.relativeVelocity : transform.position - collision.transform.position).normalized;
        DeformMesh(heatSources, direction, radius, strength);

        // メッシュに反映
        _mesh.SetVertices(_vertices);
        _mesh.SetIndices(_indices.AsArray(), MeshTopology.Triangles, 0);
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        _mesh.RecalculateTangents();
        // コライダーを設定
        _meshCollider.sharedMesh = _mesh;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static RentedArray<ContactPoint> GetContacts(Collision collision)
    {
        var pool = ArrayPool<ContactPoint>.Shared;
        var array = pool.Rent(collision.contactCount);
        var available = collision.GetContacts(array);
        return new(array, available, pool);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DeformMesh(ReadOnlySpan<Vector3> heatSources, Vector3 direction, float radius, float strength)
    {
        // 方向ベクトルをローカル座標に変換する
        var localDirection = transform.InverseTransformDirection(direction).normalized;
        // 頂点座標をワールド座標に変換する
        using var positions = new NativeArray<Vector3>(_vertices.Length, Allocator.Temp);
        transform.TransformPoints(_vertices, positions);
        // 飛び出した頂点を記録するHashSet
        using var flags = new NativeHashSet<ushort>(_vertices.Length, Allocator.Temp);

        // 頂点を移動させる
        for (int i = 0; i < _vertices.Length; i++)
        {
            // 熱源の影響度を計算
            var factor = 0f;
            foreach (var heatSource in heatSources)
            {
                factor += (1 - math.saturate(math.distance(positions[i], heatSource) / radius)) * strength;
            }

            if (factor > 0)
            {
                _vertices[i] += factor * localDirection;
                if (!IsWithIn(factor * direction + positions[i]))
                {
                    flags.Add((ushort)i);
                }
            }
        }

        if (flags.IsEmpty)
        {
            return;
        }

        using var checkedFaces = new NativeList<int>(_indices.Length / 3, Allocator.Temp);
        var indices = _indices.AsArray().AsReadOnlySpan();
        var faces = MemoryMarshal.Cast<ushort, UShort3>(indices);
        Span<ushort> buffer = stackalloc ushort[3];

        // 一部のみ外側にある面の頂点を移動させる
        foreach (var face in faces)
        {
            var writed = 0;
            foreach (var i in face.AsSpan())
            {
                if (flags.Contains(i))
                {
                    buffer[writed] = i;
                    writed += 1;
                }
            }

            if (writed == 3)
            {
                checkedFaces.Add(face.X);
                continue;
            }

            foreach (var i in face.AsSpan()[..writed])
            {
                if (flags.Remove(i) && _pointInPolygonCollider.Raycast(new(positions[i], -direction), out var hitInfo, strength * 4))
                {
                    // _vertices[i] = transform.InverseTransformPoint(Raymarch(positions[i], -direction));
                    Debug.DrawRay(positions[i], hitInfo.point - positions[i], Color.red, 1);
                    _vertices[i] = transform.InverseTransformPoint(hitInfo.point);
                }
            }
        }

        // 全て外側に出た面を消去する
        foreach (var i in checkedFaces)
        {
            var face = indices.Slice(i, 3);
            var isSubsetOf = true;
            foreach (var item in face)
            {
                if (!flags.Contains(item))
                {
                    isSubsetOf = false;
                    break;
                }
            }

            if (isSubsetOf)
            {
                _indices.RemoveRange(i, 3);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Pow2(float x) => x * x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWithIn(Vector3 position, float epsilon = math.EPSILON)
    {
        var closestPoint = _pointInPolygonCollider.ClosestPoint(position);
        return math.distancesq(position, closestPoint) < epsilon * epsilon;
    }
}

#nullable enable

[StructLayout(LayoutKind.Sequential)]
struct UShort3 : IEquatable<UShort3>
{
    public ushort X;
    public ushort Y;
    public ushort Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UShort3(ushort x, ushort y, ushort z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public ref ushort this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref AsSpan()[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<ushort> AsSpan()
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.As<UShort3, ushort>(ref this), 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return (obj is UShort3 other) && Equals(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(UShort3 other)
    {
        return X.Equals(other.X) &&
               Y.Equals(other.Y) &&
               Z.Equals(other.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(UShort3 left, UShort3 right)
    {
        return (left.X == right.X) &&
               (left.Y == right.Y) &&
               (left.Z == right.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(UShort3 left, UShort3 right)
    {
        return !(left == right);
    }
}
