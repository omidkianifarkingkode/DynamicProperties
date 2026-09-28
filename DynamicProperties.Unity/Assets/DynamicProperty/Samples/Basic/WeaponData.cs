using DynamicProperty.Samples.TypedSchema;
using UnityEngine;

namespace DynamicProperty.Samples.Basic
{
    [CreateAssetMenu(fileName = "Weapon Data", menuName = "DynamicProperty/Create Sample Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [SerializeField] protected PropertySet properties;

        [ContextMenu("DynamicProperty/Print Properties")]
        private void PrintProperties()
        {
            var weapon = properties.For<WeaponSchema>();

            Debug.Log(properties != null ? properties.ToString() : "<null>", this);
            Debug.Log(weapon.Damage(), this);
            Debug.Log(weapon.Melee(), this);
            Debug.Log(weapon.Health(), this);
        }
    }
}
