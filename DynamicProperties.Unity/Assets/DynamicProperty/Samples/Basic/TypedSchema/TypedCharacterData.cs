using UnityEngine;

namespace DynamicProperty.Samples.TypedSchema
{
    [CreateAssetMenu(fileName = "Typed Character Data", menuName = "DynamicProperty/Create Typed Schema Character Data")]
    public class TypedCharacterData : ScriptableObject
    {
        [SerializeField] private PropertySet properties = new();

        [ContextMenu("DynamicProperty/Write And Print Typed Schema Properties")]
        private void WriteAndPrintProperties()
        {
            var character =
                properties.For<CharacterSchema>();

            character.SetHealth(250);

            character.SetSpawnPosition(
                new Vector3(1, 2, 3));

            Debug.Log(character.Health(), this);
            Debug.Log(character.IsBoss(), this);
            Debug.Log(character.Weapon(), this);

            Debug.Log(character.SpawnPosition(), this);
            Debug.Log(character.BodyColor(), this);
        }
    }
}
