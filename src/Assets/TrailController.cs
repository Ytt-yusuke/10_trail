using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TrailController : MonoBehaviour
{
    [SerializeField]
    private int FRAME_MAX = 10;
    [SerializeField]
    private int DIVIDE_MAX = 4;

    private List<Vector3> Points0 = new List<Vector3>();
    private List<Vector3> Points1 = new List<Vector3>();

    private Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;

        mesh = GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update()
    {
        if(FRAME_MAX <= Points0.Count)
        {
            Points0.RemoveAt(0);
            Points1.RemoveAt(0);
        }

        Points0.Add(transform.position);
        Points1.Add(transform.TransformPoint(new Vector3(0.0f, 1.0f, 0.0f)));
        
        if(2 <= Points0.Count)
        {
            Render();
        }
    }

    private void Render()
    {
        mesh.Clear();

        int n = Points0.Count;
        Vector3[] vertexArray = new Vector3[2 * (n - 1) * DIVIDE_MAX + 2];
        Vector2[] uvArray = new Vector2[2 * (n - 1) * DIVIDE_MAX + 2];
        int[] indexArray = new int[6 * (n - 1) * DIVIDE_MAX];

        int idv = 0, idi = 0;
        float duv = 1.0f / ((float)DIVIDE_MAX * ((float)n - 1.0f));

        int id0 = 0, id1 = 0, id2 = 1, id3 = 2;
        if(n - 1 < id3) id3 = n - 1;

        for(int i = 0; i < n - 1; i++)
        {
            for(int j = 0; j < DIVIDE_MAX; j++)
            {
                float t = (float) j / (float)DIVIDE_MAX;
                Vector3 p0 = CatmullRom(Points0[id0], Points0[id1], Points0[id2], Points0[id3], t);
                Vector3 p1 = CatmullRom(Points1[id0], Points1[id1], Points1[id2], Points1[id3], t);

                vertexArray[idv + 0] = transform.InverseTransformPoint(p0);
                vertexArray[idv + 1] = transform.InverseTransformPoint(p1);

                uvArray[idv + 0].x = uvArray[idv + 1].x = 
                    1.0f - duv * (float)(i * DIVIDE_MAX + j);
                uvArray[idv + 0].y = 0.0f;
                uvArray[idv + 1].y = 1.0f;

                indexArray[idi + 0] = idv + 0;
                indexArray[idi + 1] = idv + 1;
                indexArray[idi + 2] = idv + 2;
                indexArray[idi + 3] = idv + 2;
                indexArray[idi + 4] = idv + 1;
                indexArray[idi + 5] = idv + 3;

                idv += 2;
                idi += 6;
            }
            if(i != 0) id0++;
            if(n <= ++id1) id1 = n - 1;
            if(n <= ++id2) id2 = n - 1;
            if(n <= ++id3) id3 = n - 1;
        }

        vertexArray[idv + 0] = transform.InverseTransformPoint(Points0[n - 1]);
        vertexArray[idv + 1] = transform.InverseTransformPoint(Points1[n - 1]);
        uvArray[idv + 0].x = uvArray[idv + 1].x = 0.0f;
        uvArray[idv + 0].y = 0.0f;
        uvArray[idv + 1].y = 1.0f;


        mesh.vertices = vertexArray;
        mesh.uv = uvArray;
        mesh.triangles = indexArray;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t * t2;
        float c0 = -0.5f * t3 +        t2 - 0.5f * t;
        float c1 = +1.5f * t3 - 2.5f * t2            + 1.0f;
        float c2 = -1.5f * t3 + 2.0f * t2 + 0.5f * t;
        float c3 = +0.5f * t3 - 0.5f * t2;

        return p0 * c0 + p1 * c1 + p2 * c2 + p3 * c3;
    }
}
