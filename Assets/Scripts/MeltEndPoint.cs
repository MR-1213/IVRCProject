// using UnityEngine;
// using UnityEngine.Formats.Alembic.Importer;

// namespace V2
// {
//     class GameManager : MonoBehaviour
//     {
//         private MeltManager meltManager;

//         public void StartMelt(MeltWall wall)
//         {
//         }
//     }

//     class MeltManager : MonoBehaviour
//     {
//         public void Melt(MeltWall wall)
//         {
//         }
//     }

//     class MeltWall : MonoBehaviour
//     {
//         [SerializeField]
//         private MeltPoint _startPoint;
//         [SerializeField]
//         private Transform _endPoint;
//         [SerializeField]
//         private AlembicStreamPlayer _alembic;
//         [SerializeField]
//         private MeshRenderer _renderer;
//         [SerializeField]
//         [Range(0f, 1f)]
//         private float _meltability;
//         [SerializeField]
//         private AnimationCurve _heatCurve;

//         private float _current = 0f;

//         public float Current
//         {
//             get => _current;
//             set
//             {
//                 _current = Mathf.Clamp01(value);
//                 _alembic.CurrentTime = _current * _alembic.Duration;
//                 _material.SetFloat("_Current", _current);
//                 _material.SetFloat("_Heat", 3000f * Mathf.Clamp01(_current * 15f));
//             }
//         }
//     }

//     class MeltPoint : MonoBehaviour
//     {
//     }
// }