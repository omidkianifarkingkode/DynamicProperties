using DynamicProperty.DataAnnotations;
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
            var weapon = properties.For<WeaponProperties>();

            Debug.Log(properties != null ? properties.ToString() : "<null>", this);
            Debug.Log(weapon.Demage(), this);
            Debug.Log(weapon.Meele(), this);
            Debug.Log(weapon.Health(), this);
        }
    }
}

