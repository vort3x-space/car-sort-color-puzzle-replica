using UnityEngine;
using DG.Tweening;

public class Car : MonoBehaviour
{
    public CarColor carColor;

    private int currentWaypointIndex = 0;
    private bool isMovingOnTrack = false;
    private float moveSpeed = 15f;

    private ParkingLane targetLane;
    private bool hasReservedSpot = false;
    private ParkingLane sourceLane;
    public MeshRenderer carBodyRenderer;

    public void moveToTrack(Transform connectionPoint, ParkingLane lane)
    {
        hasReservedSpot = false;
        targetLane = null;
        sourceLane = lane;

        transform.DOMove(connectionPoint.position, 0.35f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            sourceLane.carSuccessfullyLeft(this);

            int wpIndex = TrackManager.instance.waypoints.IndexOf(connectionPoint);
            if (wpIndex == -1) wpIndex = 0;

            // kendi ciktigi noktada beklememesi icin hedefini hemen bir sonraki nokta yapiyoruz
            currentWaypointIndex = (wpIndex + 1) % TrackManager.instance.waypoints.Count;

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

    [SerializeField] private float safeDistance = 2.5f;

    private void moveAlongTrackNodeByNode()
    {
        Transform targetNode = TrackManager.instance.waypoints[currentWaypointIndex];

        if (TrackManager.instance.isWaypointBlocked(targetNode) && Vector3.Distance(transform.position, targetNode.position) < safeDistance)
        {
            return;
        }

        if (isCarAheadTooClose(targetNode))
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetNode.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetNode.position) < 0.05f)
        {
            if (targetLane != null && targetNode == targetLane.trackConnectionPoint)
            {
                // rezervasyon olsa bile şerit şu an ağzına kadar doluysa içeri girme
                if (targetLane.isPhysicallyAvailable())
                {
                    isMovingOnTrack = false;
                    if (targetLane.acceptCar(this))
                    {
                        TrackManager.instance.removeCarFromTrack(this);
                        return;
                    }

                    isMovingOnTrack = true;
                }

                if (hasReservedSpot)
                {
                    targetLane.releaseReservation();
                }

                targetLane = null;
                hasReservedSpot = false;
            }

            // yeni bir hedef bul
            if (!hasReservedSpot)
            {
                findTargetParkingLane();
            }

            currentWaypointIndex = (currentWaypointIndex + 1) % TrackManager.instance.waypoints.Count;
            lookAtNextWaypoint();
        }
    }

    // onundeki araci basit bir mesafe ve yon kontrolu ile tespit etme sistemi
    private bool isCarAheadTooClose(Transform targetNode)
    {
        foreach (Car otherCar in TrackManager.instance.carsOnTrack)
        {
            if (otherCar == this) continue;

            float dist = Vector3.Distance(transform.position, otherCar.transform.position);

            if (dist < safeDistance)
            {
                Vector3 toTarget = targetNode.position - transform.position;
                if (toTarget == Vector3.zero) continue;

                Vector3 pathDirection = toTarget.normalized;
                Vector3 toOther = (otherCar.transform.position - transform.position).normalized;

                if (Vector3.Dot(pathDirection, toOther) > 0.3f)
                {
                    return true;
                }
            }
        }
        return false;
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

    // hedef bulma algoritmasini 
    private void findTargetParkingLane()
    {
        ParkingLane[] allLanes = FindObjectsOfType<ParkingLane>();

        foreach (var lane in allLanes)
        {
            if (lane != sourceLane && lane.canAcceptCar(carColor) && lane.getTotalExpectedCars() > 0)
            {
                if (reserveLane(lane)) return;
            }
        }

        foreach (var lane in allLanes)
        {
            if (lane != sourceLane && lane.canAcceptCar(carColor))
            {
                if (reserveLane(lane)) return;
            }
        }

        if (sourceLane != null && sourceLane.canAcceptCar(carColor))
        {
            reserveLane(sourceLane);
        }
    }

    // kodu temiz tutmak icin rezervasyon 
    private bool reserveLane(ParkingLane lane)
    {
        if (lane == null || !lane.reserveSpot(this)) return false;

        targetLane = lane;
        hasReservedSpot = true;
        return true;
    }

    public void setupCarColor(CarColor newColor, Material colorMaterial)
    {
        carColor = newColor;

        if (carBodyRenderer != null)
        {
            carBodyRenderer.material = colorMaterial;
        }
    }

    public void moveToParkingPosition(Transform targetTransform, ParkingLane lane)
    {
        Sequence parkSequence = DOTween.Sequence();

        parkSequence.Append(transform.DOMove(targetTransform.position, 0.4f).SetEase(Ease.InOutQuad));
        parkSequence.Join(transform.DORotateQuaternion(targetTransform.rotation, 0.4f));

        parkSequence.OnComplete(() =>
        {
            lane.updateLaneTargetColor();

            lane.checkIfCompleted();
        });
    }
}
