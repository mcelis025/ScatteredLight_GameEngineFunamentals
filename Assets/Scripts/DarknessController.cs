using UnityEngine;
using UnityEngine.SceneManagement;

public class DarknessController : MonoBehaviour
{
    public float idleTimeBeforeWarning = 5f;
    public float idleTimeBeforeDeath = 10f;
    public CameraFollow camFollow;

    private Vector3 lastPosition;
    private float idleTimer;

    void Start()
    {
        lastPosition = transform.position;

        if (camFollow == null)
        {
            camFollow = FindFirstObjectByType<CameraFollow>();
        }
    }

    void Update()
    {
        float movement = Vector3.Distance(transform.position, lastPosition);

        if (movement > 0.01f)
        {
            idleTimer = 0f;
            lastPosition = transform.position;
        }
        else
        {
            idleTimer += Time.deltaTime;
        }

        if (camFollow != null)
        {
            camFollow.SetIdleTimer(idleTimer);
        }

        if (idleTimer >= idleTimeBeforeDeath)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
