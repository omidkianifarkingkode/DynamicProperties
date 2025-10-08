using DynamicProperty;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Data", menuName = "DynamicProperty/Create Sample Character Data")]
public class CharacterData : ScriptableObject
{
    public List<DynamicProperty32> Properties1;
    public List<DynamicProperty64> Properties2;
}
