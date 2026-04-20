using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class ChaseEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;
    public float stopDistance = 0.1f;

    [Header("Hearing")]
    public float hearingRange = 10f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip growl;
    public float minTime = 2f;
    public float maxTime = 5f;

    private Rigidbody2D rb;
    private float targetX;
    private bool heardSomething;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.freezeRotation = true;

        targetX = transform.position.x;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        if (growl == null)
        {
            growl = Resources.Load<AudioClip>("Audio/SFX/growl");
        }

        if (audioSource != null && growl != null)
        {
            StartCoroutine(GrowlLoop());
        }
    }

    void FixedUpdate()
    {
        if (!heardSomething)
        {
            StopMoving();
            return;
        }

        float distanceToTarget = targetX - rb.position.x;

        if (Mathf.Abs(distanceToTarget) <= stopDistance)
        {
            heardSomething = false;
            StopMoving();
            return;
        }

        float direction = Mathf.Sign(distanceToTarget);
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    public void HearPulse(Vector2 soundPosition)
    {
        targetX = soundPosition.x;
        heardSomething = true;
    }

    public void SetTarget(Vector2 soundPosition, float soundRange)
    {
        HearPulse(soundPosition);
    }

    IEnumerator GrowlLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minTime, maxTime);
            yield return new WaitForSeconds(waitTime);

            audioSource.pitch = Random.Range(0.7f, 1.2f);
            audioSource.volume = Random.Range(0.4f, 0.8f);
            audioSource.PlayOneShot(growl);
        }
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.TakeDamage(1);
            }
        }
    }
}
