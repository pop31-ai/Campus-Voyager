using UnityEngine;

public class SceneSetup : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;

        if (Camera.main == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.5f, 0.7f, 0.9f);
            cam.farClipPlane = 500f;
        }
    }

    void Start()
    {
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }
    }
}
