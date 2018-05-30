using UnityEngine;
using System.Collections.Generic;

public class GLMathFunction : MonoBehaviour
{
    public Material mat;

    [Range(0.0f, 1.0f)]
    public float offsetX = 0.5f;
    [Range(0.0f, 1.0f)]
    public float offsetY = 0.5f;

    [Range(10, 100)]
    public int count = 50;

    [Range(1, 1000)]
    public int scale = 100;

    public Color background = Color.white;
    public Color grid = Color.gray;
    public Color foreground = Color.red;

    [Header("Gaussian")]
    [Range(0.1f, 2f)]
    public float Mean = 0.5f;
    [Range(0.0f, 10f)]
    public float Variance = 0f;

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

    private float PixelToRelativeX(float x)
    {
        return x / Screen.width * scale;
    }

    private float PixelToRelativeY(float y)
    {
        return y / Screen.height * scale;
    }

    private void OnPostRender()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();

        GL.Begin(GL.QUADS);
        GL.Color(background);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(0, 1, 0);
        GL.Vertex3(1, 1, 0);
        GL.Vertex3(1, 0, 0);
        GL.End();

        GL.Begin(GL.LINES);
        GL.Color(grid);
        GL.Vertex3(0, 0.5f, 0);
        GL.Vertex3(1, 0.5f, 0);
        GL.Vertex3(0.5f, 0, 0);
        GL.Vertex3(0.5f, 1, 0);
        GL.End();

        GL.Begin(GL.LINES);
        GL.Color(foreground);
        float deltaScale = 1.0f / scale;
        for (int i = -count; i < count - 1; i++)
        {
            for (int j = 0; j < scale; j++)
            {
                float x1 = CaculateX(i + j * deltaScale);
                float y1 = CaculateY(x1);
                float x2 = CaculateX(i + (j + 1) * deltaScale);
                float y2 = CaculateY(x2);
                GL.Vertex3(PixelToRelativeX(x1) + offsetX, PixelToRelativeX(y1) + offsetY, 0);
                GL.Vertex3(PixelToRelativeX(x2) + offsetX, PixelToRelativeX(y2) + offsetY, 0);
            }

            /*
            float x1 = CaculateX(i);
            float y1 = CaculateY(x1);
            float x2 = CaculateX(i + 1);
            float y2 = CaculateY(x2);
            GL.Vertex3(PixelToRelativeX(x1) + offsetX, PixelToRelativeX(y1) + offsetY, 0);
            GL.Vertex3(PixelToRelativeX(x2) + offsetX, PixelToRelativeX(y2) + offsetY, 0);
            */
        }
        GL.End();

        GL.PopMatrix();
    }

    private float CaculateX(float x)
    {
        return x;
    }

    private float CaculateY(float x)
    {
        //return x;
        //return x * x;
        //return x * x * x;
        //return Mathf.Sin(x);
        //return Mathf.Cos(x);
        //return Mathf.Sin(x) + Mathf.Cos(x);
        //return -5 * x * x + 3 * x + 2;
        //return 5 * Mathf.Pow(x, 7) + 3 * Mathf.Pow(x, 4);
        return 1 / (Mean * Mathf.Log(2 * Mathf.PI, 2)) * Mathf.Exp(-Mathf.Pow((x - Variance), 2) / (2 * Mathf.Pow(Mean, 2)));   // Gaussian
    }
}