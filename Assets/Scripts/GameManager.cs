using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public ShipController ship;
    public CameraFollow cameraFollow;
    public IslandGenerator islandGenerator;
    public BuildingGenerator buildingGenerator;
    public WaterPlane waterPlane;
    public QuestSystem questSystem;
    public UIController uiController;

    [Header("Player Stats")]
    public int coins = 0;
    public int reputation = 0;
    public int knowledge = 0;
    public int currentIslandIndex = 0;

    [Header("Game State")]
    public bool isGameStarted = false;
    public bool isPaused = false;
    public float gameTime = 0f;
    public int dayNumber = 1;

    private IslandGenerator.IslandData? nearestIsland;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        SetupScene();
        StartGame();
    }

    void SetupScene()
    {
        if (ship == null)
        {
            GameObject shipObj = CreateShip();
            ship = shipObj.GetComponent<ShipController>();
        }

        if (cameraFollow == null)
        {
            Camera.main.gameObject.AddComponent<CameraFollow>();
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
            cameraFollow.target = ship.transform;
        }

        if (islandGenerator == null)
            islandGenerator = gameObject.AddComponent<IslandGenerator>();

        if (buildingGenerator == null)
            buildingGenerator = gameObject.AddComponent<BuildingGenerator>();

        if (waterPlane == null)
        {
            GameObject water = new GameObject("Water");
            waterPlane = water.AddComponent<WaterPlane>();
        }

        if (questSystem == null)
            questSystem = gameObject.AddComponent<QuestSystem>();

        islandGenerator.GenerateIslands();
        buildingGenerator.GenerateBuildings(islandGenerator);

        SetupLighting();
    }

    GameObject CreateShip()
    {
        GameObject shipObj = new GameObject("Ship");
        shipObj.transform.position = new Vector3(0f, 1f, 0f);

        GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hull.name = "Hull";
        hull.transform.SetParent(shipObj.transform);
        hull.transform.localPosition = Vector3.zero;
        hull.transform.localScale = new Vector3(2.5f, 1.2f, 6f);

        Material hullMat = new Material(Shader.Find("Standard"));
        hullMat.color = new Color(0.55f, 0.3f, 0.1f);
        hull.GetComponent<MeshRenderer>().material = hullMat;

        GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
        deck.name = "Deck";
        deck.transform.SetParent(shipObj.transform);
        deck.transform.localPosition = new Vector3(0f, 0.7f, -0.5f);
        deck.transform.localScale = new Vector3(2.2f, 0.15f, 4.5f);

        Material deckMat = new Material(Shader.Find("Standard"));
        deckMat.color = new Color(0.65f, 0.45f, 0.2f);
        deck.GetComponent<MeshRenderer>().material = deckMat;

        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "Cabin";
        cabin.transform.SetParent(shipObj.transform);
        cabin.transform.localPosition = new Vector3(0f, 1.4f, -1.5f);
        cabin.transform.localScale = new Vector3(1.8f, 1.2f, 2f);

        Material cabinMat = new Material(Shader.Find("Standard"));
        cabinMat.color = new Color(0.7f, 0.55f, 0.3f);
        cabin.GetComponent<MeshRenderer>().material = cabinMat;

        GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mast.name = "Mast";
        mast.transform.SetParent(shipObj.transform);
        mast.transform.localPosition = new Vector3(0f, 2.5f, 0.5f);
        mast.transform.localScale = new Vector3(0.15f, 2.5f, 0.15f);

        Material mastMat = new Material(Shader.Find("Standard"));
        mastMat.color = new Color(0.4f, 0.25f, 0.1f);
        mast.GetComponent<MeshRenderer>().material = mastMat;

        GameObject sail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sail.name = "Sail";
        sail.transform.SetParent(mast.transform);
        sail.transform.localPosition = new Vector3(0.6f, 0.3f, 0f);
        sail.transform.localScale = new Vector3(0.02f, 2f, 1.5f);

        Material sailMat = new Material(Shader.Find("Standard"));
        sailMat.color = new Color(0.95f, 0.9f, 0.8f);
        sail.GetComponent<MeshRenderer>().material = sailMat;

        GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flag.name = "Flag";
        flag.transform.SetParent(mast.transform);
        flag.transform.localPosition = new Vector3(0.4f, 1.2f, 0f);
        flag.transform.localScale = new Vector3(0.02f, 0.5f, 0.8f);

        Material flagMat = new Material(Shader.Find("Standard"));
        flagMat.color = new Color(0.8f, 0.2f, 0.15f);
        flag.GetComponent<MeshRenderer>().material = flagMat;

        shipObj.AddComponent<ShipController>();

        Rigidbody rb = shipObj.GetComponent<Rigidbody>();
        rb.useGravity = true;

        return shipObj;
    }

    void SetupLighting()
    {
        Light[] lights = FindObjectsOfType<Light>();
        if (lights.Length == 0)
        {
            GameObject lightObj = new GameObject("Sun");
            Light sun = lightObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.95f, 0.85f);
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        RenderSettings.ambientLight = new Color(0.4f, 0.5f, 0.6f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.6f, 0.75f, 0.9f);
        RenderSettings.fogStartDistance = 100f;
        RenderSettings.fogEndDistance = 400f;
    }

    void StartGame()
    {
        isGameStarted = true;
        gameTime = 0f;
        coins = 50;
        reputation = 0;
        knowledge = 0;

        questSystem.InitializeQuests();
    }

    void Update()
    {
        if (!isGameStarted || isPaused) return;

        gameTime += Time.deltaTime;

        if (gameTime >= 120f)
        {
            gameTime = 0f;
            dayNumber++;
        }

        CheckIslandProximity();
        HandlePause();
    }

    void CheckIslandProximity()
    {
        nearestIsland = islandGenerator.GetNearestIsland(ship.transform.position);

        if (nearestIsland.HasValue)
        {
            float dist = Vector3.Distance(ship.transform.position, nearestIsland.Value.position);
            if (dist < nearestIsland.Value.radius + 5f)
            {
                uiController?.ShowIslandPrompt(nearestIsland.Value.facultyName);
            }
            else
            {
                uiController?.HideIslandPrompt();
            }
        }
    }

    public void VisitIsland()
    {
        if (!nearestIsland.HasValue) return;

        currentIslandIndex = nearestIsland.Value.index;
        uiController?.ShowNotification($"Добро пожаловать на {nearestIsland.Value.facultyName}!");

        questSystem.CheckQuestProgress(currentIslandIndex);
    }

    void HandlePause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            uiController?.TogglePause(isPaused);
        }
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        uiController?.UpdateStats(coins, reputation, knowledge);
    }

    public void AddReputation(int amount)
    {
        reputation += amount;
        uiController?.UpdateStats(coins, reputation, knowledge);
    }

    public void AddKnowledge(int amount)
    {
        knowledge += amount;
        uiController?.UpdateStats(coins, reputation, knowledge);
    }
}
