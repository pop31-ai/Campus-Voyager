using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 12f;
    public float turnSpeed = 45f;
    public float reverseSpeed = 6f;
    public float acceleration = 8f;
    public float deceleration = 5f;

    [Header("Tilt & Bob")]
    public float maxTiltAngle = 8f;
    public float tiltSpeed = 2f;
    public float bobAmplitude = 0.3f;
    public float bobFrequency = 1.5f;

    [Header("Boost")]
    public float boostMultiplier = 2f;
    public float boostDuration = 2f;
    public float boostCooldown = 5f;

    private float currentSpeed;
    private float currentTurnInput;
    private float targetTilt;
    private float bobTimer;
    private float boostTimer;
    private float cooldownTimer;
    private bool isBoosting;
    private Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;

    public float CurrentSpeed => currentSpeed;
    public bool IsBoosting => isBoosting;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.drag = 2f;
        rb.angularDrag = 4f;

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Update()
    {
        HandleInput();
        HandleBoost();
        ApplyTilt();
        ApplyBob();
        ClampPosition();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    void HandleInput()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        currentSpeed = Mathf.Lerp(currentSpeed,
            moveInput * moveSpeed * (isBoosting ? boostMultiplier : 1f),
            Time.deltaTime * (moveInput != 0 ? acceleration : deceleration));

        currentTurnInput = turnInput;
    }

    void ApplyMovement()
    {
        Vector3 forwardMove = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);

        float turnAmount = currentTurnInput * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0, turnAmount, 0);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    void ApplyTilt()
    {
        targetTilt = -currentTurnInput * maxTiltAngle;
        float currentTilt = transform.localEulerAngles.z;
        if (currentTilt > 180f) currentTilt -= 360f;
        float newTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            newTilt);
    }

    void ApplyBob()
    {
        bobTimer += Time.deltaTime * bobFrequency;
        float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude * Mathf.Abs(currentSpeed) / moveSpeed;
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, pos.y + bobOffset, Time.deltaTime * 3f);
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

    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, 0.5f, 3f);
        transform.position = pos;
    }

    public void ResetShip()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        currentSpeed = 0f;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public float GetBoostCooldownNormalized()
    {
        return cooldownTimer / boostCooldown;
    }
}
