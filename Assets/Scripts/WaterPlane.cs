using UnityEngine;

public class WaterPlane : MonoBehaviour
{
    [Header("Water Settings")]
    public float size = 500f;
    public int resolution = 100;
    public float waveHeight = 0.5f;
    public float waveSpeed = 1f;
    public float waveFrequency = 0.3f;

    [Header("Color")]
    public Color shallowColor = new Color(0.1f, 0.5f, 0.7f, 0.8f);
    public Color deepColor = new Color(0.02f, 0.1f, 0.25f, 0.95f);

    private MeshFilter mf;
    private MeshRenderer mr;
    private Vector3[] baseVertices;
    private Vector3[] animatedVertices;
    private Mesh mesh;

    void Start()
    {
        mf = GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();

        mr = GetComponent<MeshRenderer>();
        if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();

        CreateWaterMesh();
        CreateWaterMaterial();
    }

    void Update()
    {
        AnimateWaves();
    }

    void CreateWaterMesh()
    {
        mesh = new Mesh();
        mesh.name = "WaterMesh";

        int vertCount = resolution * resolution;
        baseVertices = new Vector3[vertCount];
        animatedVertices = new Vector3[vertCount];
        Vector3[] normals = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = z * resolution + x;
                float fx = (float)x / (resolution - 1);
                float fz = (float)z / (resolution - 1);

                float px = (fx - 0.5f) * size;
                float pz = (fz - 0.5f) * size;

                baseVertices[i] = new Vector3(px, 0f, pz);
                animatedVertices[i] = baseVertices[i];
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

        mesh.vertices = baseVertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mf.mesh = mesh;
    }

    void AnimateWaves()
    {
        if (baseVertices == null || mesh == null) return;

        float time = Time.time * waveSpeed;

        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 v = baseVertices[i];
            float wave1 = Mathf.Sin(v.x * waveFrequency + time) * waveHeight;
            float wave2 = Mathf.Sin(v.z * waveFrequency * 0.8f + time * 1.2f) * waveHeight * 0.6f;
            float wave3 = Mathf.Sin((v.x + v.z) * waveFrequency * 0.5f + time * 0.7f) * waveHeight * 0.3f;

            animatedVertices[i] = new Vector3(v.x, wave1 + wave2 + wave3, v.z);
        }

        mesh.vertices = animatedVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    void CreateWaterMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        Color c = Color.Lerp(deepColor, shallowColor, 0.5f);
        mat.color = c;
        mat.SetFloat("_Glossiness", 0.9f);
        mat.SetFloat("_Metallic", 0.3f);

        mr.material = mat;
    }

    public float GetWaterHeightAt(Vector3 position)
    {
        if (baseVertices == null) return 0f;
        float time = Time.time * waveSpeed;
        float wave1 = Mathf.Sin(position.x * waveFrequency + time) * waveHeight;
        float wave2 = Mathf.Sin(position.z * waveFrequency * 0.8f + time * 1.2f) * waveHeight * 0.6f;
        float wave3 = Mathf.Sin((position.x + position.z) * waveFrequency * 0.5f + time * 0.7f) * waveHeight * 0.3f;
        return wave1 + wave2 + wave3;
    }
}
