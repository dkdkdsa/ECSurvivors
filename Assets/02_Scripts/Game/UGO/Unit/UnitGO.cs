using System.Collections.Generic;
using UnityEngine;

namespace Game.UGO
{
    [DisallowMultipleComponent]
    public class UnitGO : MonoBehaviour
    {
        public float maxHP;
        public float moveSpeed;

        [System.NonSerialized] public float currentHP;
        [System.NonSerialized] public List<DamageEventGO> damageQueue = new List<DamageEventGO>(4);

        private void Awake()
        {
            currentHP = maxHP;
        }

        private void OnEnable()
        {
            currentHP = maxHP;
            damageQueue.Clear();
            HPSystemGO.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (HPSystemGO.HasInstance)
                HPSystemGO.Instance.Unregister(this);
        }
    }
}
