using UnityEngine;

public class BasketballBallInfo : MonoBehaviour
{
    [Header("Current Shot Info")]
    public int points = 1;
    public bool hasScored;
    public bool hasBeenThrown;

    [Header("Shot Spots")]
    public Transform shotSpotDekat;
    public Transform shotSpotTengah;
    public Transform shotSpotJauh;

    [Header("Point Values")]
    public int dekatPoints = 1;
    public int tengahPoints = 2;
    public int jauhPoints = 3;

    [Header("Throw Detection")]
    public SportsVRGameManager gameManager;
    public float throwVelocityThreshold = 1.5f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (hasBeenThrown) return;
        if (rb == null) return;

        if (rb.linearVelocity.magnitude >= throwVelocityThreshold)
        {
            hasBeenThrown = true;
            points = GetPointsFromNearestShotSpot();

            if (gameManager != null)
            {
                gameManager.RegisterBasketballShot();
                gameManager.SetInfo("Tembakan dari area " + points + " poin!");
            }
        }
    }

    int GetPointsFromNearestShotSpot()
    {
        Transform nearestSpot = null;
        float nearestDistance = Mathf.Infinity;
        int nearestPoints = dekatPoints;

        CheckSpot(shotSpotDekat, dekatPoints, ref nearestSpot, ref nearestDistance, ref nearestPoints);
        CheckSpot(shotSpotTengah, tengahPoints, ref nearestSpot, ref nearestDistance, ref nearestPoints);
        CheckSpot(shotSpotJauh, jauhPoints, ref nearestSpot, ref nearestDistance, ref nearestPoints);

        if (nearestSpot == null)
            return points;

        return nearestPoints;
    }

    void CheckSpot(
        Transform spot,
        int spotPoints,
        ref Transform nearestSpot,
        ref float nearestDistance,
        ref int nearestPoints)
    {
        if (spot == null) return;

        float distance = Vector3.Distance(transform.position, spot.position);

        if (distance < nearestDistance)
        {
            nearestSpot = spot;
            nearestDistance = distance;
            nearestPoints = spotPoints;
        }
    }

    public void ResetBallState()
    {
        hasScored = false;
        hasBeenThrown = false;
        points = 1;
    }
}