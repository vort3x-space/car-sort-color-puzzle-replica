using UnityEngine;

[System.Serializable]
public class LaneSetupData
{
    public CarColor[] carColors;
}

[CreateAssetMenu(fileName = "new_level", menuName = "car sort/level data")]
public class LevelData : ScriptableObject
{
    public GameObject levelLayoutPrefab;

    public int trackCapacity = 18;

    public LaneSetupData[] laneSetups;
}
