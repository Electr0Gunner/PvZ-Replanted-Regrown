using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class AtlasEntry
{
    public string name;
    public Texture2D texture;
}

[CreateAssetMenu(fileName = "ReanimAtlas", menuName = "ScriptableObjects/Create new ReanimAtlas", order = 1)]
[System.Serializable]
public class ReanimAtlas : ScriptableObject
{
    public List<AtlasEntry> entries = new List<AtlasEntry>();
}