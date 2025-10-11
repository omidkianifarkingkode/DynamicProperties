using DynamicProperty;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Data", menuName = "DynamicProperty/Create Sample Character Data")]
public class CharacterData : ScriptableObject
{

    public PropertySet Properties;

    [ContextMenu("DynamicProperty/Print Properties")]
    private void PrintProperties()
    {
        Debug.Log(PropertySetPrinter.Format(Properties, this), this);
    }
}

