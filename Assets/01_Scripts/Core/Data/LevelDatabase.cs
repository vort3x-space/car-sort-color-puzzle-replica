using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "level_database", menuName = "car sort/level database")]
public class LevelDatabase : ScriptableObject
{
    public List<LevelData> levels = new List<LevelData>();
}