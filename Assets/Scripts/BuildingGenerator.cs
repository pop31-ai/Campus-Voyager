using UnityEngine;
using System.Collections.Generic;

public class BuildingGenerator : MonoBehaviour
{
    [Header("Buildings per Island")]
    public int minBuildings = 3;
    public int maxBuildings = 7;

    [Header("Building Sizes")]
    public float minWidth = 3f;
    public float maxWidth = 8f;
    public float minHeight = 4f;
    public float maxHeight = 15f;
    public float minDepth = 3f;
    public float maxDepth = 8f;

    private List<GameObject> allBuildings = new List<GameObject>();

    public void GenerateBuildings(IslandGenerator islandGen)
    {
        ClearBuildings();

        foreach (var island in islandGen.Islands)
        {
            GenerateForIsland(island);
        }
    }

    void GenerateForIsland(IslandGenerator.IslandData island)
    {
        int buildingCount = Random.Range(minBuildings, maxBuildings + 1);
        float safeRadius = island.radius * 0.6f;

        for (int i = 0; i < buildingCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * safeRadius;
            if (offset.magnitude < 3f) offset = offset.normalized * 3f;

            Vector3 pos = island.position + new Vector3(offset.x, 0f, offset.y);
            float height = Random.Range(minHeight, maxHeight);
            float width = Random.Range(minWidth, maxWidth);
            float depth = Random.Range(minDepth, maxDepth);

            int style = Random.Range(0, 4);
            GameObject building = null;

            switch (style)
            {
                case 0: building = CreateMainBuilding(pos, width, height, depth, island.index); break;
                case 1: building = CreateTower(pos, width, height, depth, island.index); break;
                case 2: building = CreateLectureHall(pos, width, height, depth, island.index); break;
                case 3: building = CreateLab(pos, width, height, depth, island.index); break;
            }

            if (building != null)
                allBuildings.Add(building);
        }
    }

    GameObject CreateMainBuilding(Vector3 pos, float w, float h, float d, int facultyIndex)
    {
        GameObject building = new GameObject("MainBuilding");
        building.transform.position = pos + Vector3.up * (h * 0.5f);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(building.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(w, h, d);

        Material mat = GetFacultyMaterial(facultyIndex);
        body.GetComponent<MeshRenderer>().material = mat;

        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.SetParent(building.transform);
        roof.transform.localPosition = new Vector3(0f, h * 0.5f + 0.3f, 0f);
        roof.transform.localScale = new Vector3(w + 0.5f, 0.6f, d + 0.5f);

        roof.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.3f, 0.3f, 0.35f));

        AddDoor(building.transform, w, d);
        AddWindows(building.transform, w, h, d);

        building.transform.position = pos;
        building.transform.position += Vector3.up * (h * 0.5f + 0.5f);

        return building;
    }

    GameObject CreateTower(Vector3 pos, float w, float h, float d, int facultyIndex)
    {
        float towerHeight = h * 1.5f;
        GameObject building = new GameObject("Tower");
        building.transform.position = pos + Vector3.up * (towerHeight * 0.5f + 0.5f);

        float tw = w * 0.6f;
        float td = d * 0.6f;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(building.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(tw, towerHeight, td);

        Material mat = GetFacultyMaterial(facultyIndex);
        body.GetComponent<MeshRenderer>().material = mat;

        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cap.name = "Cap";
        cap.transform.SetParent(building.transform);
        cap.transform.localPosition = new Vector3(0f, towerHeight * 0.5f + 0.5f, 0f);
        cap.transform.localScale = new Vector3(tw * 0.8f, 1f, td * 0.8f);

        cap.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.5f, 0.2f, 0.2f));

        AddWindows(building.transform, tw, towerHeight, td);

        return building;
    }

    GameObject CreateLectureHall(Vector3 pos, float w, float h, float d, int facultyIndex)
    {
        float hallHeight = h * 0.6f;
        GameObject building = new GameObject("LectureHall");
        building.transform.position = pos + Vector3.up * (hallHeight * 0.5f + 0.5f);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(building.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(w * 1.3f, hallHeight, d * 1.2f);

        Material mat = GetFacultyMaterial(facultyIndex);
        body.GetComponent<MeshRenderer>().material = mat;

        GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dome.name = "Dome";
        dome.transform.SetParent(building.transform);
        dome.transform.localPosition = new Vector3(0f, hallHeight * 0.5f, 0f);
        dome.transform.localScale = new Vector3(w * 1.2f, 1.5f, d * 1.2f);

        dome.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.4f, 0.4f, 0.45f));

        AddDoor(building.transform, w * 1.3f, d * 1.2f);

        return building;
    }

    GameObject CreateLab(Vector3 pos, float w, float h, float d, int facultyIndex)
    {
        float labHeight = h * 0.8f;
        GameObject building = new GameObject("Lab");
        building.transform.position = pos + Vector3.up * (labHeight * 0.5f + 0.5f);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(building.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(w, labHeight, d);

        Material mat = GetFacultyMaterial(facultyIndex);
        Color c = mat.color;
        c.r *= 0.8f;
        c.g *= 0.8f;
        c.b *= 0.8f;
        mat.color = c;
        body.GetComponent<MeshRenderer>().material = mat;

        for (int i = 0; i < 3; i++)
        {
            GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            antenna.name = "Antenna";
            antenna.transform.SetParent(building.transform);
            antenna.transform.localPosition = new Vector3(
                (i - 1) * w * 0.3f,
                labHeight * 0.5f + 0.5f + i * 0.3f,
                0f);
            antenna.transform.localScale = new Vector3(0.1f, 0.5f + i * 0.2f, 0.1f);

            antenna.GetComponent<MeshRenderer>().material = MaterialHelper.Create(Color.red);
        }

        AddWindows(building.transform, w, labHeight, d);

        return building;
    }

    void AddDoor(Transform parent, float width, float depth)
    {
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        door.transform.SetParent(parent);
        door.transform.localPosition = new Vector3(0f, -parent.localScale.y * 0.5f + 1.2f, depth * 0.5f + 0.01f);
        door.transform.localScale = new Vector3(1.5f, 2.4f, 0.1f);

        door.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.35f, 0.2f, 0.1f));
    }

    void AddWindows(Transform parent, float w, float h, float d)
    {
        int floors = Mathf.Max(1, (int)(h / 3f));
        int windowsPerFloor = Mathf.Max(1, (int)(w / 2f));

        for (int floor = 0; floor < floors; floor++)
        {
            for (int win = 0; win < windowsPerFloor; win++)
            {
                float fx = (float)win / (windowsPerFloor - 1) - 0.5f;
                float fy = (float)floor / floors - 0.3f;

                GameObject windowObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                windowObj.name = "Window";
                windowObj.transform.SetParent(parent);
                windowObj.transform.localPosition = new Vector3(
                    fx * w * 0.8f,
                    fy * h,
                    d * 0.5f + 0.01f);
                windowObj.transform.localScale = new Vector3(0.8f, 1f, 0.05f);

                windowObj.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.6f, 0.8f, 1f));
            }
        }
    }

    Material GetFacultyMaterial(int index)
    {
        Color c;
        switch (index % 6)
        {
            case 0: c = new Color(0.6f, 0.7f, 0.85f); break;
            case 1: c = new Color(0.85f, 0.65f, 0.6f); break;
            case 2: c = new Color(0.6f, 0.8f, 0.65f); break;
            case 3: c = new Color(0.85f, 0.75f, 0.5f); break;
            case 4: c = new Color(0.75f, 0.6f, 0.8f); break;
            default: c = new Color(0.9f, 0.7f, 0.55f); break;
        }
        return MaterialHelper.Create(c);
    }

    public void ClearBuildings()
    {
        foreach (var b in allBuildings)
        {
            if (b != null) Destroy(b);
        }
        allBuildings.Clear();
    }
}
