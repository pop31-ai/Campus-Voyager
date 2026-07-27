using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 18f;
    public float acceleration = 5f;
    public float turnSpeed = 80f;
    public float turnAccel = 4f;

    [Header("Water")]
    public float waterLevel = 0.5f;
    public float shipHeight = 0.8f;

    [Header("Tilt")]
    public float maxTiltAngle = 12f;
    public float tiltSpeed = 3f;

    [Header("Boost")]
    public float boostMultiplier = 2f;
    public float boostDuration = 2f;
    public float boostCooldown = 5f;

    private float currentSpeed;
    private float targetSpeed;
    private float currentYRotation;
    private float targetYRotation;
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
        currentYRotation = transform.eulerAngles.y;
        targetYRotation = currentYRotation;
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Update()
    {
        HandleInput();
        HandleBoost();
        SmoothMovement();
        SmoothTurn();
        ApplyTilt();
        KeepOnWater();
    }

    void HandleInput()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        float speedMult = isBoosting ? boostMultiplier : 1f;
        targetSpeed = moveInput * maxSpeed * speedMult;

        if (Mathf.Abs(turnInput) > 0.01f)
            targetYRotation += turnInput * turnSpeed * Time.deltaTime;
    }

    void SmoothMovement()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);
        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    void SmoothTurn()
    {
        currentYRotation = Mathf.LerpAngle(currentYRotation, targetYRotation, Time.deltaTime * turnAccel * 3f);
    }

    void ApplyTilt()
    {
        float turnInput = Input.GetAxis("Horizontal");
        float targetTilt = -turnInput * maxTiltAngle;
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
        targetSpeed = 0f;
        currentYRotation = startRotation.eulerAngles.y;
        targetYRotation = currentYRotation;
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
