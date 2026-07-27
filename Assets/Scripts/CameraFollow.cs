using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Position")]
    public Vector3 offset = new Vector3(0f, 12f, -18f);
    public float followSpeed = 5f;

    [Header("Look")]
    public Vector3 lookOffset = new Vector3(0f, 2f, 0f);
    public float lookSpeed = 8f;

    [Header("Shake")]
    public float shakeIntensity = 0.1f;
    public float shakeSpeed = 20f;

    private float shakeTimer;

    void Start()
    {
        if (target == null)
        {
            ShipController ship = FindObjectOfType<ShipController>();
            if (ship != null)
                target = ship.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleShake();

        Vector3 desiredPosition = target.position + target.rotation * offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        Vector3 lookTarget = target.position + lookOffset;
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, lookSpeed * Time.deltaTime);
    }

    void HandleShake()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            transform.position += Random.insideUnitSphere * shakeIntensity;
        }
    }

    public void Shake(float duration = 0.3f, float intensity = 0.15f)
    {
        shakeTimer = duration;
        shakeIntensity = intensity;
    }
}
