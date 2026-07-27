using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 15f;
    public float turnSpeed = 90f;
    public float acceleration = 10f;
    public float deceleration = 6f;
    public float waterLevel = 0.5f;
    public float shipHeight = 0.8f;

    [Header("Tilt")]
    public float maxTiltAngle = 12f;
    public float tiltSpeed = 3f;

    [Header("Boost")]
    public float boostMultiplier = 2.2f;
    public float boostDuration = 2f;
    public float boostCooldown = 5f;

    private float currentSpeed;
    private float currentTurnInput;
    private float currentYRotation;
    private float currentTilt;
    private float boostTimer;
    private float cooldownTimer;
    private bool isBoosting;
    private Vector3 startPosition;
    private Quaternion startRotation;

    public float CurrentSpeed => currentSpeed;
    public bool IsBoosting => isBoosting;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        currentYRotation = transform.eulerAngles.y;
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Update()
    {
        HandleInput();
        HandleBoost();
        ApplyMovement();
        ApplyTilt();
        KeepOnWater();
    }

    void HandleInput()
    {
        float moveInput = Input.GetAxis("Vertical");
        currentTurnInput = Input.GetAxis("Horizontal");

        float targetSpeed = moveInput * moveSpeed * (isBoosting ? boostMultiplier : 1f);
        float rate = Mathf.Abs(moveInput) > 0.1f ? acceleration : deceleration;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * rate);
    }

    void ApplyMovement()
    {
        Vector3 move = transform.forward * currentSpeed * Time.deltaTime;
        transform.position += move;

        currentYRotation += currentTurnInput * turnSpeed * Time.deltaTime;
    }

    void ApplyTilt()
    {
        float targetTilt = -currentTurnInput * maxTiltAngle;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        transform.rotation = Quaternion.Euler(0f, currentYRotation, currentTilt);
    }

    void KeepOnWater()
    {
        Vector3 pos = transform.position;
        pos.y = waterLevel + shipHeight;
        transform.position = pos;
    }

    public void ResetShip()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        currentSpeed = 0f;
        currentYRotation = startRotation.eulerAngles.y;
        currentTilt = 0f;
    }

    public float GetBoostCooldownNormalized()
    {
        if (boostCooldown <= 0f) return 0f;
        return cooldownTimer / boostCooldown;
    }

    void HandleBoost()
    {
        if (Input.GetKeyDown(KeyCode.Space) && cooldownTimer <= 0 && !isBoosting)
        {
            isBoosting = true;
            boostTimer = boostDuration;
        }

        if (isBoosting)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0)
            {
                isBoosting = false;
                cooldownTimer = boostCooldown;
            }
        }

        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
    }
}
