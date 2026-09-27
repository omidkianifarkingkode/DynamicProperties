using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace DynamicProperty.Samples.Basic
{
    [CreateAssetMenu(fileName = "Character Data", menuName = "DynamicProperty/Create Sample Character Data")]
    public class CharacterData : ScriptableObject
    {
        [PropertySchema(typeof(CharacterProperties))]
        [SerializeField] protected PropertySet Properties;

        [ContextMenu("DynamicProperty/Print Properties")]
        private void PrintProperties()
        {
            Debug.Log(Properties != null ? Properties.ToString() : "<null>", this);

            Debug.Log(Properties.Health(), this);
            Debug.Log(Properties.IsBoss(), this);
            Debug.Log(Properties.Weapon(), this);

            Debug.Log(Properties.PosX(), this);
            Debug.Log(Properties.ColorR(), this);
        }
    }
}

