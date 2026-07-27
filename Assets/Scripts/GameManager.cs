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

    public IslandGenerator.IslandData? nearestIsland;

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
        SetupLighting();

        if (waterPlane == null)
        {
            GameObject water = new GameObject("Water");
            water.transform.position = Vector3.zero;
            waterPlane = water.AddComponent<WaterPlane>();
        }

        if (islandGenerator == null)
            islandGenerator = gameObject.AddComponent<IslandGenerator>();

        islandGenerator.GenerateIslands();

        if (buildingGenerator == null)
            buildingGenerator = gameObject.AddComponent<BuildingGenerator>();

        buildingGenerator.GenerateBuildings(islandGenerator);

        if (ship == null)
        {
            GameObject shipObj = CreateShip();
            ship = shipObj.GetComponent<ShipController>();
        }

        if (cameraFollow == null)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("MainCamera");
                camObj.tag = "MainCamera";
                cam = camObj.AddComponent<Camera>();
            }
            cam.gameObject.AddComponent<CameraFollow>();
            cameraFollow = cam.GetComponent<CameraFollow>();
            cameraFollow.target = ship.transform;
            cam.transform.position = ship.transform.position + new Vector3(0, 12, -18);
        }

        if (questSystem == null)
            questSystem = gameObject.AddComponent<QuestSystem>();

        if (uiController == null)
            uiController = gameObject.AddComponent<UIController>();
    }

    GameObject CreateShip()
    {
        GameObject shipObj = new GameObject("Ship");
        shipObj.transform.position = new Vector3(0f, 2f, 0f);

        // Основной корпус — плоская лодка
        GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hull.name = "Hull";
        hull.transform.SetParent(shipObj.transform);
        hull.transform.localPosition = new Vector3(0f, 0f, 0f);
        hull.transform.localScale = new Vector3(2.5f, 0.5f, 6f);
        hull.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.55f, 0.3f, 0.1f));
        Destroy(hull.GetComponent<Collider>());

        // Палуба сверху корпуса
        GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
        deck.name = "Deck";
        deck.transform.SetParent(shipObj.transform);
        deck.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        deck.transform.localScale = new Vector3(2.3f, 0.05f, 5.8f);
        deck.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.7f, 0.5f, 0.25f));
        Destroy(deck.GetComponent<Collider>());

        // Каюта на палубе
        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "Cabin";
        cabin.transform.SetParent(shipObj.transform);
        cabin.transform.localPosition = new Vector3(0f, 0.7f, -1.2f);
        cabin.transform.localScale = new Vector3(1.6f, 0.7f, 1.8f);
        cabin.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.7f, 0.55f, 0.3f));
        Destroy(cabin.GetComponent<Collider>());

        // Мачта — из каюты вверх
        GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mast.name = "Mast";
        mast.transform.SetParent(shipObj.transform);
        mast.transform.localPosition = new Vector3(0f, 2.2f, 0.5f);
        mast.transform.localScale = new Vector3(0.1f, 2f, 0.1f);
        mast.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.4f, 0.25f, 0.1f));
        Destroy(mast.GetComponent<Collider>());

        // Парус — рядом с мачтой
        GameObject sail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sail.name = "Sail";
        sail.transform.SetParent(shipObj.transform);
        sail.transform.localPosition = new Vector3(0.5f, 2f, 0.5f);
        sail.transform.localScale = new Vector3(0.02f, 1.8f, 1.3f);
        sail.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.95f, 0.9f, 0.8f));
        Destroy(sail.GetComponent<Collider>());

        // Флаг на мачте
        GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flag.name = "Flag";
        flag.transform.SetParent(shipObj.transform);
        flag.transform.localPosition = new Vector3(0.4f, 3.2f, 0.5f);
        flag.transform.localScale = new Vector3(0.02f, 0.4f, 0.7f);
        flag.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.8f, 0.2f, 0.15f));
        Destroy(flag.GetComponent<Collider>());

        shipObj.AddComponent<ShipController>();

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
            sun.intensity = 1.5f;
            sun.shadows = LightShadows.Soft;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        RenderSettings.ambientIntensity = 1.2f;
        RenderSettings.fog = false;
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
        HandleGlobalInput();

        if (!isGameStarted || isPaused) return;

        gameTime += Time.deltaTime;
        if (gameTime >= 120f)
        {
            gameTime = 0f;
            dayNumber++;
        }

        CheckIslandProximity();
    }

    void HandleGlobalInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            VisitIsland();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ship?.ResetShip();
            uiController?.ShowNotification("Ship reset!");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (ship == null) ship = FindObjectOfType<ShipController>();
            if (islandGenerator == null) islandGenerator = FindObjectOfType<IslandGenerator>();
            if (ship == null || islandGenerator == null) return;

            if (ship.IsAutopilot)
            {
                ship.StopAutopilot();
                uiController?.ShowNotification("Autopilot OFF");
            }
            else
            {
                var nearest = islandGenerator.GetNearestIsland(ship.transform.position);
                if (nearest.HasValue)
                {
                    ship.StartAutopilot(nearest.Value.position);
                    uiController?.ShowNotification($"Autopilot -> {nearest.Value.facultyName}");
                }
                else
                {
                    uiController?.ShowNotification("No island found!");
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            uiController?.TogglePause(isPaused);
        }
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
                if (Input.GetKeyDown(KeyCode.F))
                    VisitIsland();
            }
            else
            {
                uiController?.HideIslandPrompt();
            }
        }
    }

    public void VisitIsland()
    {
        var nearest = islandGenerator.GetNearestIsland(ship.transform.position);
        if (!nearest.HasValue) return;
        currentIslandIndex = nearest.Value.index;
        uiController?.ShowNotification($"Добро пожаловать на {nearest.Value.facultyName}!");
        questSystem.CheckQuestProgress(currentIslandIndex);
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
