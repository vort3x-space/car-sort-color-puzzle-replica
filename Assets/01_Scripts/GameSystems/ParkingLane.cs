using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ParkingLane : MonoBehaviour, IPointerClickHandler
{
    public List<Car> parkedCars = new List<Car>();

    // seritten cikis animasyonu devam eden araclari tuttugumuz liste
    public List<Car> exitingCars = new List<Car>();

    public int laneCapacity = 4;
    public CarColor laneTargetColor;
    public Transform[] parkPositions;
    public Transform trackConnectionPoint;

    // yoldan bu seride gelmek uzere rezerve edilmis alan sayisi
    private int reservedSpots = 0;

    private void Start()
    {
        updateLaneTargetColor();

        // oyun basladiginda listedeki arabalari fiziksel olarak dogru noktalara isinla
        // boylece inspector listesi ile sahne dizilimi arasindaki kaymalar onlenir
        for (int i = 0; i < parkedCars.Count; i++)
        {
            if (parkedCars[i] != null && parkPositions.Length > i)
            {
                parkedCars[i].transform.position = parkPositions[i].position;
                parkedCars[i].transform.rotation = parkPositions[i].rotation;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        sendTopCarsToTrack();
    }

    // seridin toplamda ne kadar dolu olacagini hesaplar
    public int getTotalExpectedCars()
    {
        return parkedCars.Count + reservedSpots + exitingCars.Count;
    }

    // seridin anlik olarak fiziksel kapasitesini kontrol eder
    public bool isPhysicallyAvailable()
    {
        return (parkedCars.Count + exitingCars.Count) < laneCapacity;
    }

    public bool canAcceptCar(CarColor color)
    {
        // eger serit kapasitesine ulasilacaksa kabul etme
        if (getTotalExpectedCars() >= laneCapacity) return false;

        // eger serit tamamen bosalacaksa ve kimse gelmiyorsa kabul et
        if (getTotalExpectedCars() == 0) return true;

        // eger seritte arac varsa veya gelecek varsa renk uyumuna bak
        return laneTargetColor == color;
    }

    public void reserveSpot(Car car)
    {
        reservedSpots++;
        if (getTotalExpectedCars() == 1)
        {
            laneTargetColor = car.carColor;
        }
    }

    public void acceptCar(Car car)
    {
        // arac gercekten geldiginde rezervasyonu kaldir ve listeye ekle
        if (reservedSpots > 0) reservedSpots--;
        parkedCars.Add(car);
        updateLaneTargetColor();

        // aracin gidecegi hedef noktayi bul
        Transform targetPos = parkPositions[parkedCars.Count - 1];
        car.moveToParkingPosition(targetPos, this);
    }

    // araba yola tamamen ulastiginda cagrilan fonksiyon
    public void carSuccessfullyLeft(Car car)
    {
        exitingCars.Remove(car);
    }

    public void updateLaneTargetColor()
    {
        if (parkedCars.Count > 0)
        {
            laneTargetColor = parkedCars[0].carColor;
        }
    }

    private void sendTopCarsToTrack()
    {
        if (parkedCars.Count == 0) return;

        List<Car> carsToMove = new List<Car>();
        CarColor topColor = parkedCars[parkedCars.Count - 1].carColor;

        for (int i = parkedCars.Count - 1; i >= 0; i--)
        {
            if (parkedCars[i].carColor == topColor)
            {
                carsToMove.Add(parkedCars[i]);
            }
            else break;
        }

        if (!TrackManager.instance.hasSpace(carsToMove.Count)) return;

        float delay = 0f;
        for (int i = carsToMove.Count - 1; i >= 0; i--)
        {
            Car car = carsToMove[i];

            // araci park edilmislerden cikarip cikanlar listesine aliyoruz
            parkedCars.Remove(car);
            exitingCars.Add(car);

            updateLaneTargetColor();

            // araclari yola gonderirken smooth bir cikis icin delay veriyoruz
            DOVirtual.DelayedCall(delay, () =>
            {
                car.moveToTrack(trackConnectionPoint, this);
            });
            delay += 0.2f;
        }
    }
}