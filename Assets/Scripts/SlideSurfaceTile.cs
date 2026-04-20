using UnityEngine;

public class SlideSurfaceTile : MonoBehaviour
{
    public float slideDirection = 1f;
    public float slideSpeed = 4f;

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        Rigidbody2D playerRb = collision.rigidbody;

        if (playerRb == null)
        {
            return;
        }

        float slideX = slideDirection * slideSpeed;
        float slideY = -slideSpeed;

        if (slideDirection < 0f)
        {
            slideX = Mathf.Min(playerRb.linearVelocity.x, slideX);
        }
        else
        {
            slideX = Mathf.Max(playerRb.linearVelocity.x, slideX);
        }

        slideY = Mathf.Min(playerRb.linearVelocity.y, slideY);
        playerRb.linearVelocity = new Vector2(slideX, slideY);
    }
}
