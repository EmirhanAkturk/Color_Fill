using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum TileStatus{
        Empty = 0,
        Wall = 1,
        Painted = 2,
    }

    [SerializeField]
    MeshRenderer planeMeshRender;

    //x = 0, y = 0 sol alt
    //MxN matris için
    // arr[m][n]
    // m = M - y - 1
    // n = x
    [Tooltip("Number of rows and columns of the matrix")]
    [SerializeField] 
    int M, N;

    [Header("Cubes")]
    [SerializeField] 
    GameObject playerCube;
    [SerializeField]
    GameObject wallCube;

    [SerializeField] 
    Transform parentCubes;

    [SerializeField] 
    List<Vector2Int> turningPoints;

    TileStatus[ , ] status;

    // Start is called before the first frame update
    private void Start()
    {
        status = new TileStatus[M, N];

        GridEditor();
        DrawWalls();
        //DrawLines(turningPoints, playerCube);
    }

    private void GridEditor()
    {
        Material planeGridMaterial = planeMeshRender.material;

        planeGridMaterial.mainTextureScale = new Vector2(M, N);
    }

    private void DrawWalls()
    {
        List<Vector2Int> cornerPoints = new List<Vector2Int>();
        List<Vector2Int> cornerMatrixIndexs = new List<Vector2Int>();

        AddCornerPoints(cornerPoints, cornerMatrixIndexs);

        DrawLines(cornerPoints, wallCube);

        PrintMatrix();
    }

    private void AddCornerPoints(List<Vector2Int> cornerPoints, List<Vector2Int> cornerMatrixIndexs)
    {
        Vector2Int point = ConvertMatrixIndexToPosition(new Vector2Int(0, 0));
        cornerPoints.Add(point);
        cornerMatrixIndexs.Add(new Vector2Int(0, 0));

        point = ConvertMatrixIndexToPosition(new Vector2Int(0, N - 1));
        cornerPoints.Add(point);
        cornerMatrixIndexs.Add(new Vector2Int(0, N - 1));

        point = ConvertMatrixIndexToPosition(new Vector2Int(M - 1, N - 1));
        cornerPoints.Add(point);
        cornerMatrixIndexs.Add(new Vector2Int(M - 1, N - 1));

        point = ConvertMatrixIndexToPosition(new Vector2Int(M - 1, 0));
        cornerPoints.Add(point);
        cornerMatrixIndexs.Add(new Vector2Int(M - 1, 0));

        point = ConvertMatrixIndexToPosition(new Vector2Int(0, 0));
        cornerPoints.Add(point); // for the last wall
        cornerMatrixIndexs.Add(new Vector2Int(0, 0));
    }

    private void DrawLines(List<Vector2Int>points, GameObject cube)
    {
        for (int i = 0; i < (points.Count - 1); ++i)
        {
            DrawLine(points[i], points[i + 1], cube);
        }
    }

    private void DrawLine(Vector2Int p1, Vector2Int p2, GameObject cube)
    {
        Vector2Int point = Vector2Int.zero;
        GameObject newCube;

        if ( p1.x == p2.x)
        {
            int upperBound;
            int lowerBound;
            if (p2.y < p1.y)
            {
                ++p2.y;
                upperBound = p1.y;
                lowerBound = p2.y;
            }
            else
            {
                --p2.y;
                upperBound = p2.y;
                lowerBound = p1.y;
            }

            for (int i = lowerBound ; i <= upperBound; ++i) 
            {
                newCube = Instantiate(cube, new Vector3(p1.x, 0, i), Quaternion.identity);
                newCube.transform.parent = parentCubes;

                point.x = p1.x;
                point.y = i;

                Vector2Int matrixIndex = ConvertPositionToMatrixIndex(point);

                if (cube.tag == "PlayerCube")
                    status[matrixIndex.x, matrixIndex.y] = TileStatus.Painted;

                else if (cube.tag == "WallCube")
                    status[matrixIndex.x, matrixIndex.y] = TileStatus.Wall;
            }
        }
        else if(p1.y == p2.y)
        {
            int upperBound;
            int lowerBound;
            if (p2.x < p1.x)
            {
                ++p2.x;
                upperBound = p1.x;
                lowerBound = p2.x;
            }
            else
            {
                --p2.x;
                upperBound = p2.x;
                lowerBound = p1.x;
            }

            for (int i = lowerBound; i <= upperBound; ++i) 
            {
                newCube = Instantiate(cube, new Vector3(i, 0, p1.y), Quaternion.identity);
                newCube.transform.parent = parentCubes;

                point.x = i;
                point.y = p1.y;

                Vector2Int matrixIndex = ConvertPositionToMatrixIndex(point);

                if (cube.tag == "PlayerCube")
                    status[matrixIndex.x, matrixIndex.y] = TileStatus.Painted;

                else if (cube.tag == "WallCube")
                    status[matrixIndex.x, matrixIndex.y] = TileStatus.Wall;
            }
        }
    }

    private Vector2Int ConvertPositionToMatrixIndex(Vector2Int point)
    {
        // (m, n) = (M - y - 1 , x)   // (MxN matrix) 
        Vector2Int matrixIndex = new Vector2Int(M - point.y - 1, point.x);
        return matrixIndex;
    }
    
    private Vector2Int ConvertMatrixIndexToPosition(Vector2Int matrixIndex)
    {
        // (x, y) = (n, M - m - 1) // (MxN matrix) 
        Vector2Int positionIndex = new Vector2Int(matrixIndex.y, M - matrixIndex.x - 1 );
        return positionIndex;
    }

    private void PrintMatrix()
    {
        for (int i = 0; i < M; ++i)
        {
            Debug.Log((i + 1) + ". satır:");

            for (int j = 0; j < N; ++j)
                Debug.Log(status[i, j]);

            Debug.Log("####################################");
        }
    }
}
