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

        InitMathFunctions();
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
        //return 1 / (Mean * Mathf.Log(2 * Mathf.PI, 2)) * Mathf.Exp(-Mathf.Pow((x - Variance), 2) / (2 * Mathf.Pow(Mean, 2)));   // Gaussian
        return selectFunc(x);
    }

    public delegate float MathFunction(float x);
    public class MathFunctionInfo
    {
        public string name;
        public MathFunction func;
        public MathFunctionInfo(string name, MathFunction func)
        {
            this.name = name;
            this.func = func;
        }
    }

    public MathFunctionInfo[] funcs { get;set;}
    public int selectFuncIndex { get; set; }
    public MathFunction selectFunc
    {
        get
        {
            if (funcs != null && selectFuncIndex >= 0 && selectFuncIndex < funcs.Length)
                return funcs[selectFuncIndex].func;
            return CaculateX;
        }
    }
    public string[] funcNames
    {
        get
        {
            if (funcs == null)
                InitMathFunctions();
            string[] names = new string[funcs.Length];
            for (int i = 0; i < funcs.Length; i++)
                names[i] = funcs[i].name;
            return names;
        }
    }

    private void InitMathFunctions()
    {
        funcs = new MathFunctionInfo[]
        {
            new MathFunctionInfo("const", x => { return 0; }),
            new MathFunctionInfo("x", x => { return x; }),
            new MathFunctionInfo("powers/x^2", x => { return x * x; }),
            new MathFunctionInfo("powers/x^3", x => { return x * x * x; }),
            new MathFunctionInfo("logs/√x", x => { return Mathf.Log(x, 2); }),
            new MathFunctionInfo("logs/x√x", x => { return x * Mathf.Log(x, 2); }),
            new MathFunctionInfo("logs/log(x, 10)", x => { return Mathf.Log(x, 10); }),
            new MathFunctionInfo("logs/xlog(x, 10)", x => { return x * Mathf.Log(x, 10); }),
            new MathFunctionInfo("triangles/sin(x)", x => { return Mathf.Sin(x); }),
            new MathFunctionInfo("triangles/cos(x)", x => { return Mathf.Cos(x); }),
            new MathFunctionInfo("triangles/tan(x)", x => { return Mathf.Tan(x); }),
            new MathFunctionInfo("triangles/asin(x)", x => { return Mathf.Asin(x); }),
            new MathFunctionInfo("triangles/acos(x)", x => { return Mathf.Acos(x); }),
            new MathFunctionInfo("triangles/atan(x)", x => { return Mathf.Atan(x); }),
            new MathFunctionInfo("triangles/sin(x) + cos(x)", x => { return Mathf.Sin(x) + Mathf.Cos(x); }),
            new MathFunctionInfo("triangles/7sin(x) + 2cos(x)", x => { return 7 * Mathf.Sin(x) + 2 * Mathf.Cos(x); }),
            new MathFunctionInfo("triangles/7sin(x)^2 + 2cos(x)", x => { return 7 * Mathf.Sin(x) * Mathf.Sin(x) + 2 * Mathf.Cos(x); }),
            new MathFunctionInfo("triangles/7sin(x^2) + 2cos(x)", x => { return 7 * Mathf.Sin(x * x) + 2 * Mathf.Cos(x); }),
            new MathFunctionInfo("parabolics/-5x^2 + 3x + 2", x => { return -5 * x * x + 3 * x + 2; }),
            new MathFunctionInfo("multinomials/5x^7 + 3x^4", x => { return 5 * Mathf.Pow(x, 7) + 3 * Mathf.Pow(x, 4); }),
            new MathFunctionInfo("Gaussian", x => { return 1 / (Mean * Mathf.Log(2 * Mathf.PI, 2)) * Mathf.Exp(-Mathf.Pow((x - Variance), 2) / (2 * Mathf.Pow(Mean, 2))); }),
        };
    }
}


#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(GLMathFunction), true)]
public class GLMathFunctionEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GLMathFunction comp = target as GLMathFunction;
        UnityEditor.EditorGUILayout.Separator();
        UnityEditor.EditorGUILayout.LabelField("Select Function");
        comp.selectFuncIndex = UnityEditor.EditorGUILayout.Popup(comp.selectFuncIndex, comp.funcNames);
    }
}
#endif