using UnityEngine;

public class GLHeart : MonoBehaviour
{
    public Material mat;

    [Range(0.05f, 0.25f)]
    public float a = 0.2f;

    private void Awake()
    {
        CreateTempMat();
    }

    private void CreateTempMat()
    {
        if (mat)
            return;
        mat = new Material(Shader.Find("Particles/Alpha Blended"));
        mat.hideFlags = HideFlags.DontSave;
        mat.shader.hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnPostRender()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();
        GL.Begin(GL.LINES);

        GL.Color(Color.red);

        float sx = 0;
        float sy = 0;
        float px = 0;
        float py = 0;
        bool sxy = false;

        for (int i = 0; i < 360; i++)
        {
            float r = a * (1 - Mathf.Sin(i * Mathf.Deg2Rad));
            float x = r * Mathf.Cos(i * Mathf.Deg2Rad);
            float y = r * Mathf.Sin(i * Mathf.Deg2Rad);
            if (sxy)
            {
                GL.Vertex3(px + 0.5f, py + 0.5f, 0);
                GL.Vertex3(x + 0.5f, y + 0.5f, 0);
            }
            else
            {
                sxy = true;
                sx = x;
                sy = y;
            }
            px = x;
            py = y;
        }

        GL.Vertex3(px + 0.5f, py + 0.5f, 0);
        GL.Vertex3(sx + 0.5f, sy + 0.5f, 0);

        GL.End();
        GL.PopMatrix();
    }
}