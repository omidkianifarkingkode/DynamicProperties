using DynamicProperty;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using DynamicProperty.Editor.Extensions;
#endif

[CreateAssetMenu(fileName = "Character Data", menuName = "DynamicProperty/Create Sample Character Data")]
public class CharacterData : ScriptableObject
{
    [SerializeField] protected PropertySet Properties;

    [ContextMenu("DynamicProperty/Print Properties")]
    private void PrintProperties()
    {
#if UNITY_EDITOR
        Debug.Log(Properties != null ? Properties.ToPrettyString(this) : "<null>", this);
#else
        Debug.Log(Properties != null ? Properties.ToString() : "<null>", this);
#endif

        Debug.Log(Properties.PosX());
        Debug.Log(Properties.ShadowColor());
        Debug.Log(Properties.SpawnPosition());;
    }
}

