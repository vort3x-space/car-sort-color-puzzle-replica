using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "level_database", menuName = "car sort/level database")]
public class LevelDatabase : ScriptableObject
{
    // oyundaki tum levellerin sirali listesi
    public List<LevelData> levels = new List<LevelData>();
}