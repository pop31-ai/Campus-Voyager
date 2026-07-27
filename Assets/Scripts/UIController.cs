using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private GameObject hudPanel;
    private GameObject pausePanel;
    private GameObject notificationPanel;

    private Text coinsText;
    private Text speedText;
    private Text boostText;
    private Text nearestIslandText;
    private Text notificationText;

    private float notificationTimer;
    private Canvas canvas;
    private bool uiReady = false;

    void Start()
    {
        CreateUI();
    }

    void CreateUI()
    {
        GameObject canvasObj = new GameObject("GameCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        CreateHUD(canvasObj.transform);
        CreatePausePanel(canvasObj.transform);
        CreateNotificationPanel(canvasObj.transform);

        uiReady = true;
    }

    void CreateHUD(Transform parent)
    {
        hudPanel = CreatePanel(parent, "HUD", new Vector2(10, -10), new Vector2(320, 100), new Color(0, 0, 0, 0.6f));
        coinsText = CreateText(hudPanel.transform, "Coins", "Coins: 50", 16, Color.yellow, new Vector2(10, -8));
        speedText = CreateText(hudPanel.transform, "Speed", "Speed: 0", 16, Color.white, new Vector2(10, -32));
        boostText = CreateText(hudPanel.transform, "Boost", "Boost: Ready", 14, Color.cyan, new Vector2(10, -54));
        nearestIslandText = CreateText(hudPanel.transform, "Near", "", 14, Color.green, new Vector2(10, -76));
    }

    void CreatePausePanel(Transform parent)
    {
        pausePanel = CreatePanel(parent, "Pause", new Vector2(0, 0), new Vector2(400, 250), new Color(0, 0, 0, 0.85f));
        pausePanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        CreateText(pausePanel.transform, "Title", "PAUSED", 32, Color.white, new Vector2(0, 60));
        CreateText(pausePanel.transform, "Hint", "WASD - Move\nSpace - Boost\nF - Visit island\nR - Reset\nESC - Resume", 18, Color.gray, new Vector2(0, -20));
        pausePanel.SetActive(false);
    }

    void CreateNotificationPanel(Transform parent)
    {
        notificationPanel = CreatePanel(parent, "Notif", new Vector2(0, 120), new Vector2(500, 50), new Color(0.1f, 0.4f, 0.1f, 0.9f));
        notificationPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 120);
        notificationText = CreateText(notificationPanel.transform, "Text", "", 18, Color.white, Vector2.zero);
        notificationPanel.SetActive(false);
    }

    GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Color bgColor)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        Image img = panel.AddComponent<Image>();
        img.color = bgColor;
        return panel;
    }

    Text CreateText(Transform parent, string name, string content, int fontSize, Color color, Vector2 anchoredPos)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(0, fontSize + 8);
        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleLeft;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return text;
    }

    void Update()
    {
        if (!uiReady) return;
        UpdateHUD();
        HandleNotifications();
        HandleInput();
    }

    void UpdateHUD()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;
        if (coinsText != null) coinsText.text = $"Coins: {gm.coins}";
        if (speedText != null) speedText.text = $"Speed: {Mathf.Abs(gm.ship != null ? gm.ship.CurrentSpeed : 0f):F1}";
        if (boostText != null)
        {
            if (gm.ship != null && gm.ship.IsBoosting)
                boostText.text = "BOOSTING!";
            else if (gm.ship != null)
                boostText.text = $"Boost: {((1f - gm.ship.GetBoostCooldownNormalized()) * 100f):F0}%";
        }
    }

    void HandleNotifications()
    {
        if (notificationPanel != null && notificationPanel.activeSelf)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0)
                notificationPanel.SetActive(false);
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.Instance?.ship?.ResetShip();
            ShowNotification("Ship reset!");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            GameManager.Instance?.VisitIsland();
        }
    }

    public void ShowNotification(string message)
    {
        if (notificationPanel == null || notificationText == null) return;
        notificationPanel.SetActive(true);
        notificationText.text = message;
        notificationTimer = 3f;
    }

    public void ShowIslandPrompt(string islandName)
    {
        if (nearestIslandText != null)
            nearestIslandText.text = $"> {islandName} [F]";
    }

    public void HideIslandPrompt()
    {
        if (nearestIslandText != null)
            nearestIslandText.text = "";
    }

    public void UpdateStats(int coins, int reputation, int knowledge)
    {
        if (coinsText != null) coinsText.text = $"Coins: {coins}";
    }

    public void TogglePause(bool paused)
    {
        if (pausePanel != null)
            pausePanel.SetActive(paused);
    }
}
