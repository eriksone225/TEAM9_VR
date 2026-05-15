using UnityEngine;

public class BasketballHoopTrigger : MonoBehaviour
{
    public SportsVRGameManager gameManager;
    public bool requireBallToBeThrown = true;

    void OnTriggerEnter(Collider other)
    {
        BasketballBallInfo ball = other.GetComponentInParent<BasketballBallInfo>();
        if (ball == null) return;
        if (ball.hasScored) return;
        if (requireBallToBeThrown && !ball.hasBeenThrown) return;

        ball.hasScored = true;

        if (gameManager != null)
            gameManager.AddBasketballScore(ball.points);
    }
}