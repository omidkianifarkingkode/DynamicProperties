using DynamicProperty.DataAnnotations;
using UnityEngine;
using static UnityEngine.Audio.GeneratorInstance;

namespace DynamicProperty.Samples.Basic
{
    [CreateAssetMenu(fileName = "Character Data", menuName = "DynamicProperty/Create Sample Character Data")]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] protected PropertySet properties;

        [ContextMenu("DynamicProperty/Print Properties")]
        private void PrintProperties()
        {
            var character = properties.For<CharacterProperties>();

            Debug.Log(properties != null ? properties.ToString() : "<null>", this);

            Debug.Log(character.Health(), this);
            Debug.Log(character.IsBoss(), this);
            Debug.Log(character.Weapon(), this);

            Debug.Log(character.SpawnPosition(), this);
            Debug.Log(character.BodyColor(), this);
        }
    }
}

