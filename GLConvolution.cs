using UnityEngine;
using System.Collections.Generic;

public class GLConvolution : MonoBehaviour
{
    public Material mat;

    [Range(0.1f, 0.5f)]
    public float r = 0.2f;

    [Range(1, 100)]
    public int count = 10;

    public bool convolution = false;

    [Range(0, 100)]
    public int noiseSeed = 0;

    [Range(1, 100)]
    public int noiseCount = 10;

    public bool gray = true;

    static List<int> listNoiseSeedR = new List<int>();

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

        float delta = r / count;

        Color color = Color.white;

        listNoiseSeedR.Clear();
        if (noiseSeed > 0)
        {
            Random.InitState(noiseSeed);
            for (int i = 0; i < noiseCount; i++)
            {
                listNoiseSeedR.Add(Random.Range(0, count * count));
            }
        }

        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                float x = -r * 0.5f + i * delta;
                float y = -r * 0.5f + j * delta;

                x += 0.5f;
                y += 0.5f;

                if (convolution)
                    // convolution image
                    color.r = ConvolutionR(i, j, count);
                else
                    // original image
                    color.r = CaculateR(i, j, count);

                if (gray)
                {
                    color.g = color.r;
                    color.b = color.r;
                }
                else
                {
                    color.g = 1f - color.r;
                    color.b = Mathf.Lerp(color.r, color.g, 0.5f);
                }

                GL.Color(color);

                GL.Vertex3(x, y, 0);
                GL.Vertex3(x, y - delta, 0);

                GL.Vertex3(x, y, 0);
                GL.Vertex3(x + delta, y, 0);
            }
        }

        GL.End();
        GL.PopMatrix();
    }

    static float CaculateR(int i, int j, int count)
    {
        // nosie
        int tmp = i * count + j;
        if (listNoiseSeedR.Contains(tmp))
            return 1;

        return (i * count + j) / (count * count * 1.0f);
    }

    static float[,] Matrix3x3 = new float[3, 3];
    static float[,] Kernel3x3 = new float[3, 3]
    {
        //{ -1, -2, -1 },
        //{  0,  0,  0 },
        //{  1,  2,  1 },

        // Gaussian&Normal distribution
        { 0.075113f, 0.123841f, 0.075113f },
        { 0.123841f, 0.204179f, 0.123841f },
        { 0.075113f, 0.123841f, 0.075113f },
    };
    static float ConvolutionR(int i, int j, int count)
    {
        // border skip(1 pixel, could instead zero padding)
        if (i > 0 && i < count - 1 && j > 0 && j < count -1)
        {
            for (int i2 = 0; i2 < 3; i2++)
                for (int j2 = 0; j2 < 3; j2++)
                {
                    Matrix3x3[i2, j2] = CaculateR(i - 1 + i2, j - 1 + j2, count);
                }
            return Convolution(Matrix3x3, Kernel3x3);
        }
        return CaculateR(i, j, count);
    }

    static float Convolution(float[,] mat, float[,] kernel)
    {
        float ret = 0;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int i2 = 1 - i;
                int j2 = 1 - j;

                i2++;
                j2++;

                float fij = mat[i, j];
                float gij = kernel[i2, j2];

                ret += fij * gij;
            }
        }

        return ret;
    }
}