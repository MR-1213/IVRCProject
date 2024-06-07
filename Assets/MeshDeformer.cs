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

    void OnTriggerEnter(Collider other)
    {
        DeformMesh(other);
    }

    void DeformMesh(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            DeformVertices(contact.point, collision.relativeVelocity);
        }
        UpdateMesh();
    }

    void DeformMesh(Collider other)
    {
        // Assuming the player object has a Rigidbody to calculate relative velocity
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 relativeVelocity = rb.velocity;
            DeformVertices(other.ClosestPoint(transform.position), relativeVelocity);
            UpdateMesh();
        }
    }

    void DeformVertices(Vector3 contactPoint, Vector3 force)
    {
        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            Vector3 worldVertex = transform.TransformPoint(modifiedVertices[i]);
            float distance = Vector3.Distance(worldVertex, contactPoint);
            if (distance < deformationStrength)
            {
                Vector3 deformation = force.normalized * deformationStrength * (1 - distance / deformationStrength);
                modifiedVertices[i] += transform.InverseTransformDirection(deformation);
            }
        }
    }
}
