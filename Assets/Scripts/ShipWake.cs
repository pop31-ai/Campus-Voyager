using UnityEngine;

public class ShipWake : MonoBehaviour
{
    [Header("Wake Settings")]
    public float wakeWidth = 1.5f;
    public float wakeLength = 8f;
    public float wakeLifetime = 3f;
    public int maxWakePoints = 50;

    private ShipController ship;
    private LineRenderer lineRenderer;
    private Vector3[] wakePoints;
    private int currentPoint;
    private float spawnTimer;

    void Start()
    {
        ship = GetComponentInParent<ShipController>();
        if (ship == null) ship = FindObjectOfType<ShipController>();

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = wakeWidth;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 0;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(1f, 1f, 1f, 0.6f);
        lineRenderer.endColor = new Color(1f, 1f, 1f, 0f);

        wakePoints = new Vector3[maxWakePoints];
        currentPoint = 0;
    }

    void Update()
    {
        if (ship == null) return;

        float speed = Mathf.Abs(ship.CurrentSpeed);

        if (speed > 1f)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer > 0.05f)
            {
                spawnTimer = 0f;
                AddWakePoint();
            }
        }

        UpdateWakeLine();
        FadeWakePoints();
    }

    void AddWakePoint()
    {
        Vector3 backOffset = -ship.transform.forward * wakeLength * 0.5f;
        Vector3 leftOffset = -ship.transform.right * wakeWidth * 0.5f;
        Vector3 rightOffset = ship.transform.right * wakeWidth * 0.5f;

        int idx = currentPoint % maxWakePoints;
        wakePoints[idx] = ship.transform.position + backOffset + leftOffset + Random.insideUnitSphere * 0.2f;

        int idx2 = (currentPoint + 1) % maxWakePoints;
        wakePoints[idx2] = ship.transform.position + backOffset + rightOffset + Random.insideUnitSphere * 0.2f;

        currentPoint += 2;
    }

    void UpdateWakeLine()
    {
        if (currentPoint < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        int count = Mathf.Min(currentPoint, maxWakePoints);
        lineRenderer.positionCount = count;

        for (int i = 0; i < count; i++)
        {
            lineRenderer.SetPosition(i, wakePoints[i % maxWakePoints]);
        }
    }

    void FadeWakePoints()
    {
        float fadeSpeed = 1f / wakeLifetime;
        for (int i = 0; i < wakePoints.Length; i++)
        {
            wakePoints[i] += Vector3.down * fadeSpeed * Time.deltaTime * 0.1f;
        }
    }
}
