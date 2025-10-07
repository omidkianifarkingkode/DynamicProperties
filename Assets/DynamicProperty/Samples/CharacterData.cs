using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    public List<Property32> properties;
    public List<Property64> properties1;

    [ContextMenu("Debug/Print All Properties")]
    private void PrintAllProperties()
    {
        PropertyPrinter.LogReport(
            this,
            properties,
            properties1,
            $"CharacterData \"{name}\""
        );
    }
}
