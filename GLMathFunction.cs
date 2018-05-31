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
        GL.Vertex3(0, offsetY, 0);
        GL.Vertex3(1, offsetY, 0);
        GL.Vertex3(offsetX, 0, 0);
        GL.Vertex3(offsetX, 1, 0);
        GL.End();

        Color originColor = foreground;
        int originSelectIndex = selectFuncIndex;
        int historyCount = historys.Count;
        for (int h = 0; h <= historyCount; h++)
        {
            if (h == historyCount)
            {
                foreground = originColor;
                selectFuncIndex = originSelectIndex;
            }
            else
            {
                foreground = historys[h].color;
                selectFuncIndex = historys[h].index;
            }

            GL.Begin(GL.LINES);
            GL.Color(foreground);
            float deltaScale = 1.0f / scale;
            for (int i = -count; i < count - 1; i++)
            {
                for (int j = 0; j < scale; j++)
                {
                    float x1 = CaculateX(i + j * deltaScale);
                    if (!IsValidFloat(x1))
                        continue;
                    float y1 = CaculateY(x1);
                    if (!IsValidFloat(y1))
                        continue;
                    float x2 = CaculateX(i + (j + 1) * deltaScale);
                    if (!IsValidFloat(x2))
                        continue;
                    float y2 = CaculateY(x2);
                    if (!IsValidFloat(y2))
                        continue;
                    GL.Vertex3(PixelToRelativeX(x1) + offsetX, PixelToRelativeX(y1) + offsetY, 0);
                    GL.Vertex3(PixelToRelativeX(x2) + offsetX, PixelToRelativeX(y2) + offsetY, 0);
                }
            }
            GL.End();
        }

        GL.PopMatrix();
    }

    private bool IsValidFloat(float v)
    {
        return !float.IsNaN(v) && !float.IsInfinity(v);
    }

    private float CaculateX(float x)
    {
        return x;
    }

    private float CaculateY(float x)
    {
        return GetFunctionInfo(selectFuncIndex).func(x);
    }

    //==================================================================================

    public class HistoryInfo
    {
        public int index;
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
        historys.Add(new HistoryInfo() { index = selectFuncIndex, color = foreground });
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
    public int selectFuncIndex
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
            new MathFunctionInfo("const", x => { return 0; }),
            new MathFunctionInfo("x", x => { return x; }),
            new MathFunctionInfo("powers/x^2", x => { return x * x; }),
            new MathFunctionInfo("powers/x^3", x => { return x * x * x; }),
            new MathFunctionInfo("powers/x^0.5(√x)", x => { return Mathf.Pow(x, 0.5f); }),
            new MathFunctionInfo("powers/x^0.1(10√x)", x => { return Mathf.Pow(x, 0.1f); }),
            new MathFunctionInfo("exps/exp(x)", x => { return Mathf.Exp(x); }),
            new MathFunctionInfo("exps/0.5^x", x => { return Mathf.Pow(0.5f, x); }),
            new MathFunctionInfo("exps/2^x", x => { return Mathf.Pow(2, x); }),
            new MathFunctionInfo("exps/3^x", x => { return Mathf.Pow(3, x); }),
            new MathFunctionInfo("exps/x^x", x => { return Mathf.Pow(x, x); }),
            new MathFunctionInfo("logs/log(x, 2)", x => { return Mathf.Log(x); }),
            new MathFunctionInfo("logs/log(x, 0.5)", x => { return Mathf.Log(x, 0.5f); }),
            new MathFunctionInfo("logs/xlog(x, 2)", x => { return x * Mathf.Log(x, 2); }),
            new MathFunctionInfo("logs/log(x, 10)", x => { return Mathf.Log10(x); }),
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
            new MathFunctionInfo("triangles/7sin(x^3) + 2cos(√x) - tan(x^5)", x => { return 7 * Mathf.Sin(x * x * x) + 2 * Mathf.Cos(Mathf.Log(x, 2)) - Mathf.Tan(Mathf.Pow(x, 5)); }),
            new MathFunctionInfo("parabolics/-5x^2 + 3x + 2", x => { return -5 * x * x + 3 * x + 2; }),
            new MathFunctionInfo("parabolics/(-5x^2 + 3x + 2)'", x => { return -10 * x + 3; }),
            new MathFunctionInfo("multinomials/5x^7 + 3x^4", x => { return 5 * Mathf.Pow(x, 7) + 3 * Mathf.Pow(x, 4); }),
            new MathFunctionInfo("multinomials/(5x^7 + 3x^4)'", x => { return 35 * Mathf.Pow(x, 6) + 12 * Mathf.Pow(x, 3); }),
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
        comp.selectFuncIndex = UnityEditor.EditorGUILayout.Popup(comp.selectFuncIndex, comp.funcNames);
        UnityEditor.EditorGUILayout.Separator();
        UnityEditor.EditorGUILayout.LabelField("History: ");
        for (int i = comp.historys.Count - 1; i >= 0; i--)
        {
            UnityEditor.EditorGUILayout.BeginHorizontal();
            GLMathFunction.HistoryInfo history = comp.historys[i];
            GLMathFunction.MathFunctionInfo info = comp.GetFunctionInfo(history.index);
            UnityEditor.EditorGUILayout.LabelField(info.name);
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