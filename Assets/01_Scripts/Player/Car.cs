using UnityEngine;
using DG.Tweening;

public class Car : MonoBehaviour
{
    public CarColor carColor;

    private int currentWaypointIndex = 0;
    private bool isMovingOnTrack = false;
    private float moveSpeed = 6f;

    private ParkingLane targetLane;
    private bool hasReservedSpot = false;

    // park yerinden yola cikis hareketi
    public void moveToTrack(Transform connectionPoint, ParkingLane sourceLane)
    {
        hasReservedSpot = false;
        targetLane = null;

        // smooth cikis animasyonu
        transform.DOMove(connectionPoint.position, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            // araba baglanti noktasina vardi seritten tamamen cikti
            sourceLane.carSuccessfullyLeft(this);

            currentWaypointIndex = TrackManager.instance.waypoints.IndexOf(connectionPoint);
            if (currentWaypointIndex == -1) currentWaypointIndex = 0;

            isMovingOnTrack = true;
            TrackManager.instance.addCarToTrack(this);
            lookAtNextWaypoint();
        });
    }

    private void Update()
    {
        if (isMovingOnTrack)
        {
            moveAlongTrackNodeByNode();
        }
    }

    private void moveAlongTrackNodeByNode()
    {
        Transform targetNode = TrackManager.instance.waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetNode.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetNode.position) < 0.05f)
        {
            if (targetLane != null && targetNode == targetLane.trackConnectionPoint)
            {
                // araba hedef seride girecek ama icerisi fiziksel olarak musait mi
                if (targetLane.isPhysicallyAvailable())
                {
                    // yer var iceri giris yap
                    isMovingOnTrack = false;
                    TrackManager.instance.removeCarFromTrack(this);
                    targetLane.acceptCar(this);
                    return;
                }
                // yer henuz bosalmadiysa yolda turlamaya devam et rezervasyonu birakma
            }

            if (!hasReservedSpot)
            {
                findTargetParkingLane();
            }

            currentWaypointIndex = (currentWaypointIndex + 1) % TrackManager.instance.waypoints.Count;
            lookAtNextWaypoint();
        }
    }

    private void lookAtNextWaypoint()
    {
        Transform nextNode = TrackManager.instance.waypoints[currentWaypointIndex];
        Vector3 direction = nextNode.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.DORotateQuaternion(Quaternion.LookRotation(direction), 0.2f);
        }
    }

    private void findTargetParkingLane()
    {
        ParkingLane[] allLanes = FindObjectsOfType<ParkingLane>();
        foreach (var lane in allLanes)
        {
            if (lane.canAcceptCar(carColor))
            {
                targetLane = lane;
                lane.reserveSpot(this);
                hasReservedSpot = true;
                break;
            }
        }
    }

    // park icine pruzsuz suzulme hareketi
    public void moveToParkingPosition(Transform targetTransform, ParkingLane lane)
    {
        Sequence parkSequence = DOTween.Sequence();

        parkSequence.Append(transform.DOMove(targetTransform.position, 0.4f).SetEase(Ease.InOutQuad));
        parkSequence.Join(transform.DORotateQuaternion(targetTransform.rotation, 0.4f));

        parkSequence.OnComplete(() =>
        {
            lane.updateLaneTargetColor();
        });
    }
}