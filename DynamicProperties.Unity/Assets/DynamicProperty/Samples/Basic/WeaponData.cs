using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace DynamicProperty.Samples.Basic
{
    [CreateAssetMenu(fileName = "Weapon Data", menuName = "DynamicProperty/Create Sample Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [PropertySchema(typeof(WeaponProperties))]
        [SerializeField] protected PropertySet Properties;

        [ContextMenu("DynamicProperty/Print Properties")]
        private void PrintProperties()
        {
            Debug.Log(Properties != null ? Properties.ToString() : "<null>", this);
        }
    }
}

