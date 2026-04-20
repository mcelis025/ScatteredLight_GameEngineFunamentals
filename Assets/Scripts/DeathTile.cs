using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathTile : MonoBehaviour
{
    private BoxCollider2D deathCollider;

    void Awake()
    {
        deathCollider = GetComponent<BoxCollider2D>();

        if (deathCollider == null)
        {
            deathCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        deathCollider.isTrigger = true;
        deathCollider.enabled = true;

        MatchColliderToVisuals();

        HideVisuals();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleTouch(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleTouch(collision.gameObject);
    }

    void HandleTouch(GameObject otherObject)
    {
        if (otherObject == null)
        {
            return;
        }

        PlayerController player = otherObject.GetComponent<PlayerController>();

        if (player != null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (otherObject.GetComponent<ChaseEnemy>() != null ||
            otherObject.GetComponent<FlyingEnemy>() != null ||
            otherObject.GetComponent<SpeedEnemy>() != null)
        {
            Destroy(otherObject);
        }
    }

    void MatchColliderToVisuals()
    {
        if (deathCollider == null)
        {
            return;
        }

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);

        if (renderers.Length == 0)
        {
            return;
        }

        Bounds totalBounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            totalBounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 localCenter = transform.InverseTransformPoint(totalBounds.center);
        Vector3 lossyScale = transform.lossyScale;

        float sizeX = totalBounds.size.x;
        float sizeY = totalBounds.size.y;

        if (Mathf.Abs(lossyScale.x) > 0.001f)
        {
            sizeX /= Mathf.Abs(lossyScale.x);
        }

        if (Mathf.Abs(lossyScale.y) > 0.001f)
        {
            sizeY /= Mathf.Abs(lossyScale.y);
        }

        deathCollider.offset = new Vector2(localCenter.x, localCenter.y);
        deathCollider.size = new Vector2(sizeX, sizeY);
    }

    void HideVisuals()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = false;
        }
    }
}
