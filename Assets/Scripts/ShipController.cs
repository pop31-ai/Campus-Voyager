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

    // Autopilot
    private bool autopilotActive = false;
    private Vector3 autopilotTarget;
    public float autopilotStopDistance = 20f;
    public float autopilotSpeed = 14f;

    public float CurrentSpeed => currentSpeed;
    public bool IsBoosting => isBoosting;
    public bool IsAutopilot => autopilotActive;

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

        if (autopilotActive)
            RunAutopilot();
        else
            HandleManualInput();

        SmoothMovement();
        SmoothTurn();
        ApplyTilt();
        KeepOnWater();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (autopilotActive)
            {
                StopAutopilot();
            }
            else
            {
                var gm = GameManager.Instance;
                if (gm != null && gm.nearestIsland.HasValue)
                {
                    StartAutopilot(gm.nearestIsland.Value.position);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && cooldownTimer <= 0 && !isBoosting)
        {
            isBoosting = true;
            boostTimer = boostDuration;
        }
    }

    void HandleManualInput()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        float speedMult = isBoosting ? boostMultiplier : 1f;
        targetSpeed = moveInput * maxSpeed * speedMult;

        if (Mathf.Abs(turnInput) > 0.01f)
            targetYRotation += turnInput * turnSpeed * Time.deltaTime;
    }

    public void StartAutopilot(Vector3 target)
    {
        autopilotActive = true;
        autopilotTarget = target;
        autopilotTarget.y = waterLevel + shipHeight;
    }

    public void StopAutopilot()
    {
        autopilotActive = false;
    }

    void RunAutopilot()
    {
        Vector3 toTarget = autopilotTarget - transform.position;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;

        if (dist < autopilotStopDistance)
        {
            StopAutopilot();
            GameManager.Instance?.VisitIsland();
            return;
        }

        float targetAngle = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
        targetYRotation = targetAngle;
        targetSpeed = autopilotSpeed * (isBoosting ? boostMultiplier : 1f);
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
        float tiltInput = autopilotActive ? Mathf.DeltaAngle(currentYRotation, targetYRotation) * 0.02f : Input.GetAxis("Horizontal");
        float targetTilt = -Mathf.Clamp(tiltInput, -1f, 1f) * maxTiltAngle;
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
        StopAutopilot();
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
