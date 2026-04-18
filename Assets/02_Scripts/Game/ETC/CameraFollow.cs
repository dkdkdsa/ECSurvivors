using UnityEngine;

namespace Game.ETC
{
    public class CameraFollow : MonoBehaviour
    {
        public void OnPlayerPosition(Vector3 pos)
        {
            pos.z = -10;
            transform.position = pos;
        }
    }
}