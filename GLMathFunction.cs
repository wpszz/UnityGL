﻿using UnityEngine;
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

    [Header("RenderMode(0: line, 1: point, 2: vector, 3: integral)")]
    [Range(0, 3)]
    public int renderMode = 0;

    [Header("Gaussian")]
    [Range(0.0f, 10f)]
    public float Mean = 0f;
    [Range(0.01f, 10f)]
    public float Variance = 1f;

    [Header("Other")]
    [Range(0.0f, 10f)]
    public float Balance = 1f;

    private float lastX;
    private float lastY;

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
        GL.Vertex3(0, offsetY, 0);
        GL.Vertex3(1, offsetY, 0);
        GL.Vertex3(offsetX, 0, 0);
        GL.Vertex3(offsetX, 1, 0);
        GL.End();

        Color originColor = foreground;
        int originSelectIndexX = selectFuncIndexX;
        int originSelectIndexY = selectFuncIndexY;
        int historyCount = historys.Count;
        for (int h = 0; h <= historyCount; h++)
        {
            if (h == historyCount)
            {
                foreground = originColor;
                selectFuncIndexX = originSelectIndexX;
                selectFuncIndexY = originSelectIndexY;
            }
            else
            {
                foreground = historys[h].color;
                selectFuncIndexX = historys[h].indexX;
                selectFuncIndexY = historys[h].indexY;
            }

            GL.Begin(GL.LINES);
            GL.Color(foreground);
            float deltaScale = 1.0f / scale;
            bool prev = false;
            float prevX = 0;
            float prevY = 0;
            for (int i = -count; i < count - 1; i++)
            {
                for (int j = 0; j < scale; j++)
                {
                    float t1 = i + j * deltaScale;
                    float x1 = CaculateX(t1);
                    if (!IsValidFloat(x1))
                        continue;
                    float y1 = CaculateY(t1);
                    if (!IsValidFloat(y1))
                        continue;

                    if (prev)
                    {
                        if (renderMode == 1)
                        {
                            float pointX = PixelToRelativeX(prevX) + offsetX;
                            float pointY = PixelToRelativeX(prevY) + offsetY;
                            DrawSegmentWithCull(pointX, pointY, pointX + 0.001f, pointY + 0.001f);
                        }
                        else if (renderMode == 2)
                        {
                            float pointX = PixelToRelativeX(prevX) + offsetX;
                            float pointY = PixelToRelativeX(prevY) + offsetY;
                            DrawSegmentWithCull(pointX, pointY, 0.5f, 0.5f);
                        }
                        else if (renderMode == 3)
                        {
                            float pointX = PixelToRelativeX(prevX) + offsetX;
                            float pointY = PixelToRelativeX(prevY) + offsetY;
                            DrawSegmentWithCull(pointX, pointY, pointX, 0.5f);
                        }
                        else
                        {
                            DrawSegmentWithCull(PixelToRelativeX(prevX) + offsetX, PixelToRelativeX(prevY) + offsetY,
                                                PixelToRelativeX(x1) + offsetX, PixelToRelativeX(y1) + offsetY);
                        }
                    }
                    else
                    {
                        prev = true;
                    }
                    prevX = x1;
                    prevY = y1;
                }
            }
            GL.End();
        }

        GL.PopMatrix();
    }

    private void DrawSegmentWithCull(float x1, float y1, float x2, float y2)
    {
        // simple cull
        if (x1 < 0f && x2 < 0f || x1 > 1f && x2 > 1f || y1 < 0f && y2 < 0f || y1 > 1f && y2 > 1f)
            return;

        GL.Vertex3(x1, y1, 0);
        GL.Vertex3(x2, y2, 0);
    }

    private bool IsValidFloat(float v)
    {
        return !float.IsNaN(v) && !float.IsInfinity(v);
    }

    private float CaculateX(float t)
    {
        lastX = GetFunctionInfo(selectFuncIndexX).func(t);
        return lastX;
    }

    private float CaculateY(float t)
    {
        lastY = GetFunctionInfo(selectFuncIndexY).func(t);
        return lastY;
    }

    //==================================================================================

    public class HistoryInfo
    {
        public int indexX;
        public int indexY;
        public Color color;
    }
    private List<HistoryInfo> _historys;
    public List<HistoryInfo> historys
    {
        get
        {
            if (_historys == null)
                _historys = new List<HistoryInfo>();
            return _historys;
        }
    }
    public void AddHistory()
    {
        historys.Add(new HistoryInfo() { indexX = selectFuncIndexX, indexY = selectFuncIndexY, color = foreground });
    }
    public void RemoveHistory(int index)
    {
        if (historys.Count > index)
            historys.RemoveAt(index);
    }

    //==================================================================================

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

    private MathFunctionInfo[] _funcs;
    public MathFunctionInfo[] funcs
    {
        get
        {
            if (_funcs == null)
                _funcs = InitMathFunctions();
            return _funcs;
        }
    }
    public string[] funcNames
    {
        get
        {
            string[] names = new string[funcs.Length];
            for (int i = 0; i < funcs.Length; i++)
                names[i] = funcs[i].name;
            return names;
        }
    }
    public int selectFuncIndexY
    {
        get;
        set;
    }
    public int selectFuncIndexX
    {
        get;
        set;
    }
    public MathFunctionInfo GetFunctionInfo(int index)
    {
        if (index >= 0 && index < funcs.Length)
            return funcs[index];
        return funcs[0];
    }

    private MathFunctionInfo[] InitMathFunctions()
    {
        return new MathFunctionInfo[]
        {
            new MathFunctionInfo("self", t => { return t; }),
            new MathFunctionInfo("last/x", t => { return lastX; }),
            new MathFunctionInfo("last/y", t => { return lastY; }),
            new MathFunctionInfo("base/const(b)", x => { return Balance; }),
            new MathFunctionInfo("base/x", x => { return x; }),
            new MathFunctionInfo("base/|x|", x => { return Mathf.Abs(x); }),
            new MathFunctionInfo("base/1÷x", x => { return 1 / x; }),
            new MathFunctionInfo("base/1÷x^b", x => { return 1 / Mathf.Pow(x, Balance); }),
            new MathFunctionInfo("base/sign(x)", x => { return Mathf.Sign(x); }),
            new MathFunctionInfo("base/repeat(x, b)", x => { return Mathf.Repeat(x, Balance); }),
            new MathFunctionInfo("base/Deg2Rad", x => { return Mathf.Deg2Rad * x; }),
            new MathFunctionInfo("base/Rad2Deg", x => { return Mathf.Rad2Deg * x; }),
            new MathFunctionInfo("CG/step(b, x)", x => { return x >= Balance ? 1 : 0; }),
            new MathFunctionInfo("CG/frac(x)", x => { return x - (int)x; }),
            new MathFunctionInfo("powers/x^2", x => { return x * x; }),
            new MathFunctionInfo("powers/x^3", x => { return x * x * x; }),
            new MathFunctionInfo("powers/x^0.5(√x)", x => { return Mathf.Pow(x, 0.5f); }),
            new MathFunctionInfo("powers/x^0.1(10√x)", x => { return Mathf.Pow(x, 0.1f); }),
            new MathFunctionInfo("powers/x^b", x => { return Mathf.Pow(x, Balance); }),
            new MathFunctionInfo("exps/exp(x)", x => { return Mathf.Exp(x); }),
            new MathFunctionInfo("exps/0.5^x", x => { return Mathf.Pow(0.5f, x); }),
            new MathFunctionInfo("exps/2^x", x => { return Mathf.Pow(2, x); }),
            new MathFunctionInfo("exps/3^x", x => { return Mathf.Pow(3, x); }),
            new MathFunctionInfo("exps/x^x", x => { return Mathf.Pow(x, x); }),
            new MathFunctionInfo("logs/ln(x)", x => { return Mathf.Log(x); }),
            new MathFunctionInfo("logs/log(x, 2)", x => { return Mathf.Log(x, 2); }),
            new MathFunctionInfo("logs/log(x, 0.5)", x => { return Mathf.Log(x, 0.5f); }),
            new MathFunctionInfo("logs/xln(x)", x => { return x * Mathf.Log(x); }),
            new MathFunctionInfo("logs/xlog(x, 2)", x => { return x * Mathf.Log(x, 2); }),
            new MathFunctionInfo("logs/log(x, 10)", x => { return Mathf.Log10(x); }),
            new MathFunctionInfo("logs/xlog(x, 10)", x => { return x * Mathf.Log(x, 10); }),
            new MathFunctionInfo("misc/(x^b)÷(e^x)", x => { return Mathf.Pow(x, Balance) / Mathf.Exp(x); }),
            new MathFunctionInfo("triangles/sin(x)", x => { return Mathf.Sin(x); }),
            new MathFunctionInfo("triangles/cos(x)", x => { return Mathf.Cos(x); }),
            new MathFunctionInfo("triangles/tan(x)", x => { return Mathf.Tan(x); }),
            new MathFunctionInfo("triangles/sec(x)", x => { return 1 / Mathf.Cos(x); }),
            new MathFunctionInfo("triangles/csc(x)", x => { return 1 / Mathf.Sin(x); }),
            new MathFunctionInfo("triangles/cot(x)", x => { return 1 / Mathf.Tan(x); }),
            new MathFunctionInfo("triangles/asin(x)", x => { return Mathf.Asin(x); }),
            new MathFunctionInfo("triangles/acos(x)", x => { return Mathf.Acos(x); }),
            new MathFunctionInfo("triangles/atan(x)", x => { return Mathf.Atan(x); }),
            new MathFunctionInfo("triangles/atan2(x, 1)", x => { return Mathf.Atan2(x, 1); }),
            new MathFunctionInfo("triangles/sin(x)÷x", x => { return Mathf.Sin(x) / x; }),
            new MathFunctionInfo("triangles/sin(1÷x)", x => { return Mathf.Sin(1 / x); }),
            new MathFunctionInfo("triangles/cos(1÷x)", x => { return Mathf.Cos(1 / x); }),
            new MathFunctionInfo("triangles/x * sin(1÷x)", x => { return x * Mathf.Sin(1 / x); }),
            new MathFunctionInfo("triangles/cos(0.5π - x)", x => { return Mathf.Cos(0.5f * Mathf.PI - x); }),
            new MathFunctionInfo("triangles/sin(x) + cos(x)", x => { return Mathf.Sin(x) + Mathf.Cos(x); }),
            new MathFunctionInfo("triangles/7sin(x) + 2cos(x)", x => { return 7 * Mathf.Sin(x) + 2 * Mathf.Cos(x); }),
            new MathFunctionInfo("triangles/7sin(x)^2 + 2cos(x)", x => { return 7 * Mathf.Sin(x) * Mathf.Sin(x) + 2 * Mathf.Cos(x); }),
            new MathFunctionInfo("triangles/7sin(x^2) + 2cos(x)", x => { return 7 * Mathf.Sin(x * x) + 2 * Mathf.Cos(x); }),
            new MathFunctionInfo("triangles/7sin(x^3) + 2cos(√x) - tan(x^5)", x => { return 7 * Mathf.Sin(x * x * x) + 2 * Mathf.Cos(Mathf.Log(x, 2)) - Mathf.Tan(Mathf.Pow(x, 5)); }),
            new MathFunctionInfo("parabolics/-5x^2 + 3x + 2", x => { return -5 * x * x + 3 * x + 2; }),
            new MathFunctionInfo("parabolics/(-5x^2 + 3x + 2)'", x => { return -10 * x + 3; }),
            new MathFunctionInfo("multinomials/5x^7 + 3x^4", x => { return 5 * Mathf.Pow(x, 7) + 3 * Mathf.Pow(x, 4); }),
            new MathFunctionInfo("multinomials/(5x^7 + 3x^4)'", x => { return 35 * Mathf.Pow(x, 6) + 12 * Mathf.Pow(x, 3); }),
            new MathFunctionInfo("gaussian/base", x => { return 1 / (Variance * Mathf.Sqrt(2 * Mathf.PI)) * Mathf.Exp(-Mathf.Pow((x - Mean), 2) / (2 * Mathf.Pow(Variance, 2))); }),
            new MathFunctionInfo("gaussian/random(mu, sigma)", x => {
                return GaussianRandom(Mean, Variance);
            }),
            new MathFunctionInfo("diamond/x => pingpong(t, b)", t => { return Mathf.PingPong(t, Balance); }),
            new MathFunctionInfo("diamond/y => pingpong(t + 0.5b, b) - 0.5b", t => { return Mathf.PingPong(t + 0.5f * Balance, Balance) - 0.5f * Balance; }),
            new MathFunctionInfo("polarCoordinates/circle/x => b * cos(θ)", theta => { return Balance * Mathf.Cos(theta); }),
            new MathFunctionInfo("polarCoordinates/circle/y => b * sin(θ)", theta => { return Balance * Mathf.Sin(theta); }),
            new MathFunctionInfo("polarCoordinates/heart/x => b(1 - sin(θ)) * cos(θ)", theta => { return Balance * (1 - Mathf.Sin(theta)) * Mathf.Cos(theta); }),
            new MathFunctionInfo("polarCoordinates/heart/y => b(1 - sin(θ)) * sin(θ)", theta => { return Balance * (1 - Mathf.Sin(theta)) * Mathf.Sin(theta); }),
            new MathFunctionInfo("polarCoordinates/water/x => b(1 - sin(θ)) * cos(θ)", theta => { return Balance * (1 - Mathf.Sin(theta)) * Mathf.Cos(theta); }),
            new MathFunctionInfo("polarCoordinates/water/y => sin(θ)", theta => { return Mathf.Sin(theta); }),
            new MathFunctionInfo("polarCoordinates/spiral/x => b(θ÷π) * cos(θ)", theta => { return Balance * (theta / Mathf.PI) * Mathf.Cos(theta); }),
            new MathFunctionInfo("polarCoordinates/spiral/y => b(θ÷π) * sin(θ)", theta => { return Balance * (theta / Mathf.PI) * Mathf.Sin(theta); }),
            new MathFunctionInfo("hyperbolic/sinh(x)", x => { return (Mathf.Exp(x) - Mathf.Exp(-x)) / 2; }),
            new MathFunctionInfo("hyperbolic/cosh(x)", x => { return (Mathf.Exp(x) + Mathf.Exp(-x)) / 2; }),
            new MathFunctionInfo("hyperbolic/tanh(x)", x => { return (Mathf.Exp(x) - Mathf.Exp(-x)) / (Mathf.Exp(x) + Mathf.Exp(-x)); }),
            new MathFunctionInfo("hyperbolic/sech(x)", x => { return 2 / (Mathf.Exp(x) + Mathf.Exp(-x)); }),
            new MathFunctionInfo("hyperbolic/csch(x)", x => { return 2 / (Mathf.Exp(x) - Mathf.Exp(-x)); }),
            new MathFunctionInfo("hyperbolic/coth(x)", x => { return (Mathf.Exp(x) + Mathf.Exp(-x)) / (Mathf.Exp(x) - Mathf.Exp(-x)); }),
            new MathFunctionInfo("noise/PerlinNoise(x * b, 0)", x => { return Mathf.PerlinNoise(x * Balance, 0); }),
            new MathFunctionInfo("noise/PerlinNoise(0, x * b)", x => { return Mathf.PerlinNoise(0, x * Balance); }),
            new MathFunctionInfo("noise/PerlinNoise(x * b, x * b)", x => { return Mathf.PerlinNoise(x * Balance, x * Balance); }),
            new MathFunctionInfo("random/rnd * x", x => { return UnityEngine.Random.value * x; }),
            new MathFunctionInfo("random/rnd * x(fixed seed)", x => {
                UnityEngine.Random.InitState((int)x);
                return UnityEngine.Random.value * x;
            }),
            new MathFunctionInfo("random/uv", t => 
            {
                float u = Mathf.Repeat(t, 1);
                float v = (int)(t / 2);
                float w = Vector2.Dot(new Vector2(u, v), new Vector2(12.9898f, 78.233f)) * 43758.5453f;
                return (w - (int)w) >= Balance ? 1 : 0;
            }),
            new MathFunctionInfo("gamma/pow(x, 2.2)", x => { return Mathf.Pow(x, 2.2f); }),
            new MathFunctionInfo("gamma/pow(x, 0.45)", x => { return Mathf.Pow(x, 0.45f); }),
        };
    }

    public static float GaussianRandom(float mu, float sigma)
    {
        // use the polar form of the Box-Muller transform
        float r, x, y;
        do
        {
            x = UnityEngine.Random.Range(-1.0f, 1.0f);
            y = UnityEngine.Random.Range(-1.0f, 1.0f);
            r = x * x + y * y;
        } while (r >= 1 || r == 0);

        float v = x * Mathf.Sqrt(-2 * Mathf.Log(r) / r);
        return mu + sigma * v;

        // Remark:  y * Math.sqrt(-2 * Math.log(r) / r)
        // is an independent random gaussian
    }
}


#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(GLMathFunction), true)]
public class GLMathFunctionEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!Application.isPlaying)
            return;

        GLMathFunction comp = target as GLMathFunction;
        UnityEditor.EditorGUILayout.Separator();
        UnityEditor.EditorGUILayout.BeginHorizontal();
        UnityEditor.EditorGUILayout.LabelField("Select Function: ");
        if (GUILayout.Button("Record", GUILayout.Width(50)))
        {
            comp.AddHistory();
        }
        UnityEditor.EditorGUILayout.EndHorizontal();
        comp.selectFuncIndexX = UnityEditor.EditorGUILayout.Popup("X:", comp.selectFuncIndexX, comp.funcNames);
        comp.selectFuncIndexY = UnityEditor.EditorGUILayout.Popup("Y:", comp.selectFuncIndexY, comp.funcNames);
        UnityEditor.EditorGUILayout.Separator();
        UnityEditor.EditorGUILayout.LabelField("History: ");
        for (int i = comp.historys.Count - 1; i >= 0; i--)
        {
            UnityEditor.EditorGUILayout.BeginHorizontal();
            GLMathFunction.HistoryInfo history = comp.historys[i];
            GLMathFunction.MathFunctionInfo xInfo = comp.GetFunctionInfo(history.indexX);
            GLMathFunction.MathFunctionInfo yInfo = comp.GetFunctionInfo(history.indexY);
            UnityEditor.EditorGUILayout.LabelField(xInfo.name + "/" + yInfo.name);
            history.color = UnityEditor.EditorGUILayout.ColorField(history.color, GUILayout.Width(50));
            if (GUILayout.Button("Del", GUILayout.Width(50)))
            {
                comp.RemoveHistory(i);
                UnityEditor.EditorGUILayout.EndHorizontal();
                break;
            }
            UnityEditor.EditorGUILayout.EndHorizontal();
        }
    }
}
#endif