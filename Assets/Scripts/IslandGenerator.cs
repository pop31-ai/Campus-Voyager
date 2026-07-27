using UnityEngine;
using System.Collections.Generic;

public class IslandGenerator : MonoBehaviour
{
    [Header("Island Settings")]
    public int islandCount = 6;
    public float worldRadius = 200f;
    public float minIslandRadius = 15f;
    public float maxIslandRadius = 35f;
    public float islandHeight = 3f;
    public int resolution = 32;

    [Header("Faculty Colors")]
    public Color[] facultyColors = new Color[]
    {
        new Color(0.2f, 0.5f, 0.8f),
        new Color(0.8f, 0.3f, 0.2f),
        new Color(0.2f, 0.7f, 0.3f),
        new Color(0.8f, 0.6f, 0.1f),
        new Color(0.6f, 0.3f, 0.7f),
        new Color(0.9f, 0.5f, 0.2f),
    };

    public string[] facultyNames = new string[]
    {
        "Факультет Информатики",
        "Медицинский Факультет",
        "Юридический Факультет",
        "Факультет Искусств",
        "Инженерный Факультет",
        "Факультет Экономики",
    };

    private List<GameObject> islands = new List<GameObject>();

    public struct IslandData
    {
        public GameObject gameObject;
        public Vector3 position;
        public float radius;
        public string facultyName;
        public int index;
    }

    private List<IslandData> islandDataList = new List<IslandData>();
    public List<IslandData> Islands => islandDataList;

    public void GenerateIslands()
    {
        ClearIslands();

        for (int i = 0; i < islandCount; i++)
        {
            Vector3 position = GetIslandPosition(i);
            float radius = Random.Range(minIslandRadius, maxIslandRadius);
            string name = i < facultyNames.Length ? facultyNames[i] : $"Остров {i + 1}";
            Color color = i < facultyColors.Length ? facultyColors[i] : Color.white;

            GameObject island = CreateIsland(position, radius, name, color, i);
            islandDataList.Add(new IslandData
            {
                gameObject = island,
                position = position,
                radius = radius,
                facultyName = name,
                index = i
            });
        }

        CreateDock(0);
    }

    Vector3 GetIslandPosition(int index)
    {
        float angle = index * Mathf.PI * 2f / islandCount;
        float radius = worldRadius * 0.5f + Random.Range(-30f, 30f);
        return new Vector3(
            Mathf.Cos(angle) * radius,
            0f,
            Mathf.Sin(angle) * radius
        );
    }

    GameObject CreateIsland(Vector3 position, float radius, string facultyName, Color color, int index)
    {
        GameObject island = new GameObject(facultyName);
        island.transform.position = position;
        island.transform.SetParent(transform);

        MeshFilter mf = island.AddComponent<MeshFilter>();
        MeshRenderer mr = island.AddComponent<MeshRenderer>();

        Mesh mesh = GenerateIslandMesh(radius);
        mf.mesh = mesh;

        Material mat = MaterialHelper.Create(color);
        mat.SetFloat("_Glossiness", 0.3f);
        mr.material = mat;

        AddTrees(island.transform, radius);
        AddPath(island.transform, radius, index);

        return island;
    }

    Mesh GenerateIslandMesh(float radius)
    {
        Mesh mesh = new Mesh();
        mesh.name = "IslandMesh";

        int vertCount = resolution * resolution;
        Vector3[] vertices = new Vector3[vertCount];
        Vector3[] normals = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = z * resolution + x;
                float fx = (float)x / (resolution - 1);
                float fz = (float)z / (resolution - 1);

                float px = (fx - 0.5f) * radius * 2f;
                float pz = (fz - 0.5f) * radius * 2f;
                float dist = Mathf.Sqrt(px * px + pz * pz);

                float height = Mathf.Clamp01(1f - dist / radius);
                height = Mathf.Pow(height, 0.7f) * islandHeight;

                if (dist < radius * 0.9f)
                {
                    height += Mathf.PerlinNoise(px * 0.1f, pz * 0.1f) * 1.5f;
                }

                if (dist > radius * 0.85f)
                {
                    height *= Mathf.Clamp01((radius - dist) / (radius * 0.15f));
                }

                vertices[i] = new Vector3(px, height, pz);
                normals[i] = Vector3.up;
                uvs[i] = new Vector2(fx, fz);
            }
        }

        int triCount = (resolution - 1) * (resolution - 1) * 6;
        int[] triangles = new int[triCount];
        int t = 0;

        for (int z = 0; z < resolution - 1; z++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int i = z * resolution + x;
                triangles[t++] = i;
                triangles[t++] = i + resolution;
                triangles[t++] = i + 1;
                triangles[t++] = i + 1;
                triangles[t++] = i + resolution;
                triangles[t++] = i + resolution + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    void AddTrees(Transform parent, float radius)
    {
        int treeCount = Random.Range(5, 15);
        for (int i = 0; i < treeCount; i++)
        {
            Vector2 pos = Random.insideUnitCircle * radius * 0.7f;
            float dist = pos.magnitude;
            if (dist > radius * 0.3f)
            {
                CreateTree(parent, new Vector3(pos.x, 0f, pos.y));
            }
        }
    }

    void CreateTree(Transform parent, Vector3 position)
    {
        GameObject tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tree.name = "Tree";
        tree.transform.SetParent(parent);
        tree.transform.localPosition = position + Vector3.up * 1.5f;
        tree.transform.localScale = new Vector3(0.3f, 1.5f, 0.3f);
        Destroy(tree.GetComponent<Collider>());
        tree.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.4f, 0.25f, 0.1f));

        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "Crown";
        crown.transform.SetParent(tree.transform);
        crown.transform.localPosition = Vector3.up * 1.2f;
        crown.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
        Destroy(crown.GetComponent<Collider>());
        crown.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.1f, 0.5f + Random.Range(0f, 0.3f), 0.1f));
    }

    void AddPath(Transform parent, float radius, int index)
    {
        float pathWidth = 1.5f;
        float pathLength = radius * 0.6f;

        GameObject path = GameObject.CreatePrimitive(PrimitiveType.Cube);
        path.name = "Path";
        path.transform.SetParent(parent);
        path.transform.localPosition = new Vector3(0f, 0.05f, pathLength * 0.5f);
        path.transform.localScale = new Vector3(pathWidth, 0.1f, pathLength);

        path.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.6f, 0.6f, 0.55f));
        Destroy(path.GetComponent<Collider>());
    }

    void CreateDock(int islandIndex)
    {
        if (islandIndex >= islandDataList.Count) return;

        IslandData data = islandDataList[islandIndex];
        Vector3 dockPos = data.position + Vector3.forward * (data.radius + 5f);

        GameObject dock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dock.name = "Dock";
        dock.transform.position = dockPos;
        dock.transform.localScale = new Vector3(4f, 0.5f, 8f);

        dock.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.5f, 0.35f, 0.15f));
        Destroy(dock.GetComponent<Collider>());

        for (int i = -1; i <= 1; i += 2)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "Post";
            post.transform.position = dockPos + new Vector3(i * 1.5f, 1f, 0f);
            post.transform.localScale = new Vector3(0.3f, 1.5f, 0.3f);
            Destroy(post.GetComponent<Collider>());
            post.GetComponent<MeshRenderer>().material = MaterialHelper.Create(new Color(0.5f, 0.35f, 0.15f));
        }
    }

    public IslandData? GetNearestIsland(Vector3 position)
    {
        IslandData nearest = default;
        float minDist = float.MaxValue;

        foreach (var data in islandDataList)
        {
            float dist = Vector3.Distance(position, data.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = data;
            }
        }

        if (minDist < 100f)
            return nearest;
        return null;
    }

    public void ClearIslands()
    {
        foreach (var island in islands)
        {
            if (island != null) Destroy(island);
        }
        islands.Clear();
        islandDataList.Clear();
    }
}
