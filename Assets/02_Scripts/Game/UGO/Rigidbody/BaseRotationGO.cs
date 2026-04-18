using UnityEngine;

namespace Game.UGO
{
    [DisallowMultipleComponent]
    public class BaseRotationGO : MonoBehaviour
    {
        [System.NonSerialized] public Quaternion baseValue = Quaternion.identity;
    }
}
