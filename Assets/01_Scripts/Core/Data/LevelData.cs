using UnityEngine;

// serit icindeki renk dizilimini inspector'da gosterebilmek icin yazdigimiz sinif
[System.Serializable]
public class LaneSetupData
{
    // seritteki arabalarin renkleri arkadan one dogru siralanir
    public CarColor[] carColors;
}

[CreateAssetMenu(fileName = "new_level", menuName = "car sort/level data")]
public class LevelData : ScriptableObject
{
    // bu levelin yol ve bos park yerlerini iceren tasarim prefab'i
    public GameObject levelLayoutPrefab;

    // bu levelde ayni anda yolda bulunabilecek maksimum arac sayisi
    public int trackCapacity = 18;

    // bu leveldeki seritlerin icindeki arac renk dagilimi
    public LaneSetupData[] laneSetups;
}
