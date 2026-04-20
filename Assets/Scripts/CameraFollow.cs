using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Follow")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    
    

    [Header("Shake")]
    public float shakeAmount = 0.1f;
    public float shakeSpeed = 20f;

    private float idleTimer = 0f;

    public void SetIdleTimer(float time)
    {
        idleTimer = time;
    }

    void Awake()
    {
        FindTargetIfNeeded();
    }

    void LateUpdate()
    {
        FindTargetIfNeeded();

        if (target == null) return;

        
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        
        if (idleTimer > 5f)
        {
            float shakeX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * 2f;
            float shakeY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * 2f;

            smoothPosition += new Vector3(shakeX, shakeY, 0f) * shakeAmount;
        }

        
        
        
        

        transform.position = smoothPosition;
    }

    void FindTargetIfNeeded()
    {
        if (target != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }
}
