using UnityEngine;

public class BouncyTile : MonoBehaviour
{
    public float bounceForce = 15f;

    public AudioClip bounceSFX;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
            }

            if (bounceSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(bounceSFX);
            }

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayImpact(8f);
            }
        }
    }
}
