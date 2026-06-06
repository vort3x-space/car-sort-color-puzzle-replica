using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    public static TrackManager instance;

    public List<Transform> waypoints = new List<Transform>();
    public List<Car> carsOnTrack = new List<Car>();
    public int trackCapacity = 18; // gorseldeki 0/24 gibi dusunulebilir seviyeye gore degisir

    private void Awake()
    {
        // singleton tasarim kalibi ile erisimi kolaylastiriyoruz
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // yolda bos yer olup olmadigini kontrol eden fonksiyon
    public bool hasSpace(int carCount)
    {
        return carsOnTrack.Count + carCount <= trackCapacity;
    }

    // arabayi yola kaydeden fonksiyon
    public void addCarToTrack(Car car)
    {
        carsOnTrack.Add(car);
        // TODO ui guncellemesi burada tetiklenebilir 2/18 gibi
    }

    // araba park yerine dondugunde yoldan silen fonksiyon
    public void removeCarFromTrack(Car car)
    {
        if (carsOnTrack.Contains(car))
        {
            carsOnTrack.Remove(car);
        }
    }
}