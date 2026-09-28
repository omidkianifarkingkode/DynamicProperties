using DynamicProperty.Samples.TypedSchema;
using UnityEngine;

namespace DynamicProperty.Samples.Basic
{
    [CreateAssetMenu(fileName = "Character Data", menuName = "DynamicProperty/Create Sample Character Data")]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] protected PropertySet properties;

        [ContextMenu("DynamicProperty/Print Properties")]
        private void PrintProperties()
        {
            var character = properties.For<CharacterSchema>();

            Debug.Log(properties != null ? properties.ToString() : "<null>", this);

            Debug.Log(character.Health(), this);
            Debug.Log(character.IsBoss(), this);
            Debug.Log(character.Weapon(), this);

            Debug.Log(character.SpawnPosition(), this);
            Debug.Log(character.BodyColor(), this);
        }
    }
}
