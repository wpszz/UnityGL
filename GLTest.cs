using UnityEngine;

public class GLTest : MonoBehaviour
{
    public Material mat;

    public Mesh mesh;

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
        for (int i = 0; i < 10; i++)
        {
            GL.Color(Color.yellow);
            GL.Vertex3(0, i * 0.1f, 0);
            GL.Vertex3(1, i * 0.1f, 0);
            GL.Color(Color.green);
            GL.Vertex3(i * 0.1f, 0, 0);
            GL.Vertex3(i * 0.1f, 1, 0);
        }
        GL.End();
        GL.PopMatrix();

        if (mesh)
        {
            mat.SetPass(0);
            Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.Euler(-90, 0, 0));
        }
    }
}