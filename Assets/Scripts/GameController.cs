using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum TileStatus{
        Empty = 0,
        Painted = 1
    }

    //x = 0, y = 0 sol alt
    //MxN matris için
    // arr[m][n]
    // m = M - y - 1
    // n = x
    [SerializeField] 
    int M, N;

    [SerializeField] 
    GameObject cube;

    [SerializeField] 
    Transform parentCubes;

    [SerializeField] 
    List<Vector2Int> points;

    [SerializeField]
    MeshRenderer planeMeshRender;

    TileStatus[ , ] status;

    // Start is called before the first frame update
    private void Start()
    {
        status = new TileStatus[M, N];

        GridEditor();
        DrawLines();
    }

    private void GridEditor()
    {
        Material planeGridMaterial = planeMeshRender.material;

        planeGridMaterial.mainTextureScale = new Vector2(M, N);
    }

    private void DrawLines()
    {
        for (int i = 0; i < (points.Count - 1); ++i)
        {
            DrawLine(points[i], points[i + 1]);
        }
    }

    private void DrawLine(Vector2Int p1, Vector2Int p2)
    {
        Vector2Int point = Vector2Int.zero;
        GameObject newCube;

        if ( p1.x == p2.x)
        {
            int min = p1.y < p2.y ? p1.y : p2.y;
            int max = p1.y > p2.y ? p1.y : p2.y;

            // todo fix repetition of vertex values.
            for (int i = min + 1; i <= max; ++i) 
            {
                newCube = Instantiate(cube, new Vector3(p1.x, 0, i), Quaternion.identity);
                newCube.transform.parent = parentCubes;

                point.x = p1.x;
                point.y = i;

                Vector2Int matrixIndex = ConvertPositionToMatris(point);
                status[matrixIndex.x, matrixIndex.y] = TileStatus.Painted;
            }
        }
        else if(p1.y == p2.y)
        {
            int min = p1.x < p2.x ? p1.x : p2.x;
            int max = p1.x > p2.x ? p1.x : p2.x;

            // todo fix repetition of vertex values.
            for (int i = min + 1; i <= max; ++i) 
            {
                newCube = Instantiate(cube, new Vector3(i, 0, p1.y), Quaternion.identity);
                newCube.transform.parent = parentCubes;

                point.x = i;
                point.y = p1.y;

                Vector2Int matrixIndex = ConvertPositionToMatris(point);
                status[matrixIndex.x, matrixIndex.y] = TileStatus.Painted;
            }
        }
    }

    private Vector2Int ConvertPositionToMatris(Vector2Int point)
    {
        Vector2Int matrixIndex = new Vector2Int(M - point.y - 1 , point.x);

        return matrixIndex;
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
