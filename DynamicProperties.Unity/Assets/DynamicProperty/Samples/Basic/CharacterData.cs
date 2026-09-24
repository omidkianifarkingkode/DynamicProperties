using UnityEngine;

namespace DynamicProperty.Samples.Basic
{
    [CreateAssetMenu(fileName = "Character Data", menuName = "DynamicProperty/Create Sample Character Data")]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] protected PropertySet Properties;

        [ContextMenu("DynamicProperty/Print Properties")]
        private void PrintProperties()
        {
            Debug.Log(Properties != null ? Properties.ToString() : "<null>", this);

            Debug.Log(Properties.PosX());
            Debug.Log(Properties.ShadowColor());
            Debug.Log(Properties.SpawnPosition());
        }
    }
}

