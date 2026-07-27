using UnityEngine;

public static class MaterialHelper
{
    private static Shader cachedShader;

    public static Material Create(Color color)
    {
        Material mat = new Material(GetShader());
        mat.color = color;
        return mat;
    }

    static Shader GetShader()
    {
        if (cachedShader != null) return cachedShader;

        string[] shaderNames = new string[]
        {
            "Standard",
            "Universal Render Pipeline/Lit",
            "HDRP/Lit",
            "Unlit/Color"
        };

        foreach (string name in shaderNames)
        {
            Shader s = Shader.Find(name);
            if (s != null)
            {
                cachedShader = s;
                return s;
            }
        }

        cachedShader = Shader.Find("Sprites/Default");
        return cachedShader;
    }
}
