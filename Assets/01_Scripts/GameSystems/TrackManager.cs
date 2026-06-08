using System.Collections.Generic;
using UnityEngine;
using TMPro; // textmeshpro kütüphanesini kullanmak icin ekliyoruz

public class TrackManager : MonoBehaviour
{
    public static TrackManager instance;

    public List<Transform> waypoints = new List<Transform>();
    public List<Car> carsOnTrack = new List<Car>();
    public int trackCapacity = 18;
    public List<Transform> blockedWaypoints = new List<Transform>();

    // ui bilesenleri ve renk ayarlari
    public TextMeshProUGUI capacityText;
    public Color normalTextColor = Color.white;
    public Color fullTextColor = Color.red;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // oyun basladiginda yaziyi ilk degerine gore guncelle
        updateCapacityUI();
    }

    public bool hasSpace(int carCount)
    {
        return carsOnTrack.Count + carCount <= trackCapacity;
    }

    public void setTrackCapacity(int capacity)
    {
        trackCapacity = Mathf.Max(0, capacity);
        updateCapacityUI();
    }

    public void addCarToTrack(Car car)
    {
        carsOnTrack.Add(car);
        updateCapacityUI();
    }

    public void removeCarFromTrack(Car car)
    {
        if (carsOnTrack.Contains(car))
        {
            carsOnTrack.Remove(car);
            updateCapacityUI();
        }
    }

    public void blockWaypoint(Transform wp)
    {
        if (!blockedWaypoints.Contains(wp)) blockedWaypoints.Add(wp);
    }

    public void unblockWaypoint(Transform wp)
    {
        if (blockedWaypoints.Contains(wp)) blockedWaypoints.Remove(wp);
    }

    public bool isWaypointBlocked(Transform wp)
    {
        return blockedWaypoints.Contains(wp);
    }

    // arayuzdeki metni ve rengini guncelleyen ozel fonksiyon
    private void updateCapacityUI()
    {
        // eger text atanmamissa hata vermemesi icin guvenlik kontrolu
        if (capacityText == null) return;

        // ekrandaki metni ornegin 5/18 seklinde yazdir
        capacityText.text = carsOnTrack.Count + "/" + trackCapacity;

        // eger yoldaki arac sayisi kapasiteye ulastiysa rengi kirmizi yap
        if (carsOnTrack.Count >= trackCapacity)
        {
            capacityText.color = fullTextColor;
        }
        else
        {
            capacityText.color = normalTextColor;
        }
    }
}
