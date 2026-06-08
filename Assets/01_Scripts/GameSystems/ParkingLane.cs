using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ParkingLane : MonoBehaviour, IPointerClickHandler
{
    public List<Car> parkedCars = new List<Car>();

    public List<Car> exitingCars = new List<Car>();

    public int laneCapacity = 4;
    public CarColor laneTargetColor;
    public Transform[] parkPositions;
    public Transform trackConnectionPoint;

    private int reservedSpots = 0;

    public Transform barrierObj;
    public float barrierClosedY = -1.7f;
    public float barrierOpenY = -0.1f;
    private bool isCompleted = false;
    public bool IsCompleted => isCompleted;

    public bool isLaneColorConsistent(CarColor color)
    {
        foreach (Car car in parkedCars)
        {
            // eğer şerit içinde en öndeki renkten farklı bir renk varsa, 
            // ve biz o rengi şeride sokmaya çalışıyorsak engelle
            if (car.carColor != color) return false;
        }
        return true;
    }

    private void Start()
    {
        updateLaneTargetColor();
        setBarrierLocalY(isCompleted ? barrierOpenY : barrierClosedY);

        // oyun basladiginda listedeki arabalari fiziksel olarak dogru noktalara isinla
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
        if (isCompleted) return;
        sendTopCarsToTrack();
    }

    // araba park yerine tam oturdugunda cagrilacak kontrol fonksiyonu
    public void checkIfCompleted()
    {
        int maxParkedCars = getMaxParkedCars();
        if (maxParkedCars <= 0 || parkedCars.Count < maxParkedCars) return;

        CarColor targetC = parkedCars[0].carColor;
        foreach (Car car in parkedCars)
        {
            if (car.carColor != targetC) return;
        }

        isCompleted = true;

        // bariyeri yukari kaldirma animasyonu
        if (barrierObj != null)
        {
            barrierObj.DOLocalMoveY(barrierOpenY, 0.5f).SetEase(Ease.OutBack);
        }

        if (LevelManager.instance != null)
        {
            LevelManager.instance.checkLevelCompleted();
        }
    }

    private void setBarrierLocalY(float y)
    {
        if (barrierObj == null) return;

        Vector3 localPosition = barrierObj.localPosition;
        localPosition.y = y;
        barrierObj.localPosition = localPosition;
    }

    // seridin toplamda ne kadar dolu olacagini hesaplar
    public int getTotalExpectedCars()
    {
        int parkedCount = parkedCars != null ? parkedCars.Count : 0;
        int exitingCount = exitingCars != null ? exitingCars.Count : 0;

        return parkedCount + reservedSpots + exitingCount;
    }

    private int getMaxParkedCars()
    {
        int positionCapacity = parkPositions != null ? parkPositions.Length : 0;
        return Mathf.Min(laneCapacity, positionCapacity);
    }

    // seridin anlik olarak fiziksel kapasitesini kontrol eder
    public bool isPhysicallyAvailable()
    {
        if (isCompleted) return false;

        int parkedCount = parkedCars != null ? parkedCars.Count : 0;

        return parkedCount < getMaxParkedCars();
    }

    public bool canAcceptCar(CarColor color)
    {
        if (isCompleted) return false;
        if (getMaxParkedCars() <= 0) return false;
        if (getTotalExpectedCars() >= getMaxParkedCars()) return false;

        if (parkedCars == null || parkedCars.Count == 0)
        {
            return reservedSpots == 0 || laneTargetColor == color;
        }

        CarColor topColor = parkedCars[parkedCars.Count - 1].carColor;

        if (topColor != color) return false;

        return isLaneColorConsistent(topColor);
    }

    public bool reserveSpot(Car car)
    {
        if (car == null || !canAcceptCar(car.carColor)) return false;

        if ((parkedCars == null || parkedCars.Count == 0) && reservedSpots == 0)
        {
            laneTargetColor = car.carColor;
        }

        reservedSpots++;
        if (getTotalExpectedCars() == 1)
        {
            laneTargetColor = car.carColor;
        }

        return true;
    }

    public void releaseReservation()
    {
        if (reservedSpots > 0) reservedSpots--;
    }

    public bool acceptCar(Car car)
    {
        if (car == null || !isPhysicallyAvailable()) return false;
        if (parkedCars == null) parkedCars = new List<Car>();

        int targetIndex = parkedCars.Count;
        if (parkPositions == null || targetIndex < 0 || targetIndex >= getMaxParkedCars()) return false;

        Transform targetPos = parkPositions[targetIndex];
        if (targetPos == null) return false;

        if (reservedSpots > 0) reservedSpots--;
        parkedCars.Add(car);
        updateLaneTargetColor();

        car.moveToParkingPosition(targetPos, this);
        return true;
    }

    // araba yola tamamen ulastiginda cagrilan fonksiyon
    public void carSuccessfullyLeft(Car car)
    {
        exitingCars.Remove(car);

        if (exitingCars.Count == 0)
        {
            TrackManager.instance.unblockWaypoint(trackConnectionPoint);
        }
    }

    public void updateLaneTargetColor()
    {
        if (parkedCars == null || parkedCars.Count == 0)
        {
            return;
        }

        laneTargetColor = parkedCars[parkedCars.Count - 1].carColor;
    }

    private void sendTopCarsToTrack()
    {
        if (parkedCars == null || parkedCars.Count == 0) return;

        int lastIndex = parkedCars.Count - 1;
        Car lastCar = parkedCars[lastIndex];

        if (lastCar == null) return;

        CarColor topColor = lastCar.carColor;
        List<Car> carsToMove = new List<Car>();

        for (int i = lastIndex; i >= 0; i--)
        {
            if (parkedCars[i] != null && parkedCars[i].carColor == topColor)
            {
                carsToMove.Add(parkedCars[i]);
            }
            else break;
        }

        if (carsToMove.Count == 0 || !TrackManager.instance.hasSpace(carsToMove.Count)) return;

        TrackManager.instance.blockWaypoint(trackConnectionPoint);

        float delay = 0f;
        for (int i = carsToMove.Count - 1; i >= 0; i--)
        {
            Car car = carsToMove[i];

            parkedCars.Remove(car);
            exitingCars.Add(car);

            updateLaneTargetColor();

            DOVirtual.DelayedCall(delay, () =>
            {
                if (car != null) car.moveToTrack(trackConnectionPoint, this);
            });
            delay += 0.2f;
        }
    }
}
