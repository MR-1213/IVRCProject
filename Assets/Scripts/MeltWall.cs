using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;

public class MeltWall : MonoBehaviour
{
    [SerializeField]
    private Transform _startPoint;
    [SerializeField]
    private Transform _endPoint;
    [SerializeField]
    private AlembicStreamPlayer _alembic;
    [SerializeField]
    private MeshRenderer _renderer;

    private Material _material;
    private float _current = 0f;

    /// <summary>
    /// アニメーションの進行度
    /// </summary>
    public float Current
    {
        get => _current;
        set
        {
            _current = Mathf.Clamp01(value);
            _alembic.CurrentTime = _current * _alembic.Duration;
            _material.SetFloat("_Current", _current);
            _material.SetFloat("_Heat", 3000f * Mathf.Clamp01(_current * 15f));
        }
    }

    private void Start()
    {
        _material = _renderer.material;
        var radius = _material.GetFloat("_Heat_Source_Radius");
        var offset = Vector3.up * radius;
        _material.SetVector("_StartPosition", _renderer.transform.InverseTransformPoint(_startPoint.position) + offset);
        _material.SetVector("_EndPosition", _renderer.transform.InverseTransformPoint(_endPoint.position) + offset);
    }
}
