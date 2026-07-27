using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject islandPanel;
    public GameObject questPanel;
    public GameObject notificationPanel;

    [Header("HUD Elements")]
    public Text coinsText;
    public Text reputationText;
    public Text knowledgeText;
    public Text dayText;
    public Text speedText;
    public Text boostText;
    public Text nearestIslandText;

    [Header("Island Panel")]
    public Text islandNameText;
    public Text islandDescriptionText;
    public Button visitButton;
    public Button closeButton;

    [Header("Quest Panel")]
    public Text questTitleText;
    public Text questDescriptionText;
    public Text questProgressText;

    [Header("Notification")]
    public Text notificationText;
    public float notificationDuration = 3f;

    private float notificationTimer;
    private Canvas canvas;

    void Start()
    {
        CreateUI();
    }

    void CreateUI()
    {
        GameObject canvasObj = new GameObject("GameCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CreateHUD(canvasObj.transform);
        CreatePausePanel(canvasObj.transform);
        CreateIslandPanel(canvasObj.transform);
        CreateNotificationPanel(canvasObj.transform);
        CreateBoostBar(canvasObj.transform);
        CreateMiniMap(canvasObj.transform);
    }

    void CreateHUD(Transform parent)
    {
        GameObject hud = CreatePanel(parent, "HUDPanel", new Vector2(10, -10), new Vector2(350, 120), new Color(0, 0, 0, 0.6f));

        coinsText = CreateText(hud.transform, "Coins", "Coins: 0", new Vector2(10, -10), 18, Color.yellow);
        reputationText = CreateText(hud.transform, "Rep", "Rep: 0", new Vector2(10, -35), 18, Color.cyan);
        knowledgeText = CreateText(hud.transform, "Know", "Knowledge: 0", new Vector2(10, -60), 18, Color.green);
        dayText = CreateText(hud.transform, "Day", "Day 1", new Vector2(10, -85), 16, Color.white);
        speedText = CreateText(hud.transform, "Speed", "Speed: 0", new Vector2(200, -10), 16, Color.white);
        nearestIslandText = CreateText(hud.transform, "NearIsland", "", new Vector2(200, -35), 14, Color.white);

        hudPanel = hud;
    }

    void CreatePausePanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "PausePanel", new Vector2(0, 0), new Vector2(400, 300), new Color(0, 0, 0, 0.85f));
        panel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        CreateText(panel.transform, "PauseTitle", "PAUSED", new Vector2(0, 80), 36, Color.white);
        CreateText(panel.transform, "PauseHint", "ESC - Resume\nR - Reset Ship\nQ - Quests", new Vector2(0, -20), 18, Color.gray);

        pausePanel = panel;
        panel.SetActive(false);
    }

    void CreateIslandPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "IslandPanel", new Vector2(0, 50), new Vector2(450, 350), new Color(0.05f, 0.1f, 0.2f, 0.9f));
        panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);

        islandNameText = CreateText(panel.transform, "IslandName", "Island", new Vector2(0, 130), 24, Color.white);
        islandDescriptionText = CreateText(panel.transform, "IslandDesc", "Description", new Vector2(0, 60), 16, Color.gray);

        visitButton = CreateButton(panel.transform, "VisitBtn", "Dock Ship", new Vector2(0, -40), new Color(0.2f, 0.6f, 0.3f));
        closeButton = CreateButton(panel.transform, "CloseBtn", "Close", new Vector2(0, -90), new Color(0.5f, 0.2f, 0.2f));

        islandPanel = panel;
        panel.SetActive(false);
    }

    void CreateNotificationPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "NotificationPanel", new Vector2(0, 150), new Vector2(500, 60), new Color(0.1f, 0.3f, 0.1f, 0.9f));
        panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 150);

        notificationText = CreateText(panel.transform, "NotifText", "", new Vector2(0, 0), 20, Color.white);

        notificationPanel = panel;
        panel.SetActive(false);
    }

    void CreateBoostBar(Transform parent)
    {
        GameObject barBg = CreatePanel(parent, "BoostBg", new Vector2(10, -140), new Vector2(200, 20), new Color(0.2f, 0.2f, 0.2f, 0.7f));

        GameObject barFill = CreatePanel(barBg.transform, "BoostFill", new Vector2(2, -2), new Vector2(196, 16), new Color(0.2f, 0.6f, 1f, 0.9f));
        barFill.GetComponent<RectTransform>().anchoredPosition = new Vector2(2, -2);

        boostText = CreateText(barBg.transform, "BoostLabel", "BOOST", new Vector2(0, 0), 12, Color.white);
    }

    void CreateMiniMap(Transform parent)
    {
        GameObject miniMapBg = CreatePanel(parent, "MiniMapBg", new Vector2(-10, -10), new Vector2(180, 180), new Color(0, 0, 0, 0.7f));
        miniMapBg.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
        miniMapBg.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        miniMapBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, -100);

        CreateText(miniMapBg.transform, "MapLabel", "MAP", new Vector2(0, 75), 12, Color.gray);
    }

    GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = panel.AddComponent<Image>();
        img.color = bgColor;

        return panel;
    }

    Text CreateText(Transform parent, string name, string content, Vector2 anchoredPos, int fontSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(0, fontSize + 10);

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleLeft;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return text;
    }

    Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(200, 40);

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = bgColor * 1.2f;
        btn.colors = cb;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return btn;
    }

    void Update()
    {
        UpdateHUD();
        HandleNotifications();
        HandleInput();
    }

    void UpdateHUD()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        if (coinsText != null) coinsText.text = $"Coins: {gm.coins}";
        if (reputationText != null) reputationText.text = $"Rep: {gm.reputation}";
        if (knowledgeText != null) knowledgeText.text = $"Knowledge: {gm.knowledge}";
        if (dayText != null) dayText.text = $"Day {gm.dayNumber}";
        if (speedText != null && gm.ship != null)
            speedText.text = $"Speed: {Mathf.Abs(gm.ship.CurrentSpeed):F1}";

        if (boostText != null && gm.ship != null)
        {
            if (gm.ship.IsBoosting)
                boostText.text = "BOOSTING!";
            else
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

        if (Input.GetKeyDown(KeyCode.Q))
        {
            questPanel?.SetActive(!questPanel.activeSelf);
        }
    }

    public void ShowNotification(string message)
    {
        if (notificationPanel == null || notificationText == null) return;
        notificationPanel.SetActive(true);
        notificationText.text = message;
        notificationTimer = notificationDuration;
    }

    public void ShowIslandPrompt(string islandName)
    {
        if (nearestIslandText != null)
            nearestIslandText.text = $"Near: {islandName} [F to visit]";
    }

    public void HideIslandPrompt()
    {
        if (nearestIslandText != null)
            nearestIslandText.text = "";
    }

    public void ShowIslandInfo(string name, string description)
    {
        if (islandPanel == null) return;
        islandPanel.SetActive(true);
        if (islandNameText != null) islandNameText.text = name;
        if (islandDescriptionText != null) islandDescriptionText.text = description;
    }

    public void UpdateStats(int coins, int reputation, int knowledge)
    {
        if (coinsText != null) coinsText.text = $"Coins: {coins}";
        if (reputationText != null) reputationText.text = $"Rep: {reputation}";
        if (knowledgeText != null) knowledgeText.text = $"Knowledge: {knowledge}";
    }

    public void TogglePause(bool paused)
    {
        if (pausePanel != null)
            pausePanel.SetActive(paused);
    }

    public void UpdateQuestInfo(string title, string description, string progress)
    {
        if (questTitleText != null) questTitleText.text = title;
        if (questDescriptionText != null) questDescriptionText.text = description;
        if (questProgressText != null) questProgressText.text = progress;
    }
}
