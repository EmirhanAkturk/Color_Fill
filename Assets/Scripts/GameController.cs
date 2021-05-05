﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum TileStatus{
        Empty = 0,
        Wall = 1,
        Painted = 2,
    }

    [Header("Plane values")]

    //x = 0, y = 0 bottom left
    //For matrix A of MxN dimensions
    // A[m][n]
    // m = M - y - 1
    // n = x
    [Tooltip("Number of rows and columns of the matrix")]
    [SerializeField] 
    int M, N;

    [SerializeField]
    MeshRenderer planeMeshRender;

    [Header("Cubes")]
    [SerializeField] 
    GameObject playerCube;
    [SerializeField]
    GameObject wallCube;

    [SerializeField] 
    Transform parentCubes;
    
    [SerializeField] 
    List<Vector2Int> turningPoints, turningPoints2;

    List<GameObject> playerCubes;
    TileStatus[ , ] status;

    // Start is called before the first frame update
    private void Start()
    {
        playerCubes = new List<GameObject>();
        status = new TileStatus[M, N];

        GridEditor();
        DrawWalls();

        AddEndPoint(turningPoints);
        DrawLines(turningPoints, playerCube);

        AddEndPoint(turningPoints2);
        DrawLines(turningPoints2, wallCube);

        FillWithCubes();
        PrintMatrix();
    }

    //it may be unnecessary
    private void FillWithCubes()
    {
        Vector2Int matrixIndex; // cube matrix index
        Vector2Int position; //cube position

        for (int i = 0; i< turningPoints.Count; ++i)
        {
            matrixIndex = ConvertPositionToMatrixIndex(turningPoints[i]);
            if (matrixIndex.x < M-1)
                ++matrixIndex.x;

            if (matrixIndex.y < N-1)
                ++matrixIndex.y;
            boundaryFill4(matrixIndex.x, matrixIndex.y, TileStatus.Painted, TileStatus.Wall);
        }
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
    }

    private void AddEndPoint(List<Vector2Int> turningPoints)
    {
        int lenght = turningPoints.Count;

        //the list must have at least two items
        if (lenght >= 2)
        {
            Vector2Int otherPoint = turningPoints[lenght - 2];
            Vector2Int lastPoint = turningPoints[lenght - 1];

            if (lastPoint.x == otherPoint.x) //vertical line
            {
                if (lastPoint.y > otherPoint.y)
                    ++lastPoint.y;
                else
                    --lastPoint.y;
            }
            else if (lastPoint.y == otherPoint.y) //vertical line
            {
                if (lastPoint.x > otherPoint.x)
                    ++lastPoint.x;
                else
                    --lastPoint.x;
            }

            turningPoints[lenght - 1] = lastPoint;
        }
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
                {
                    playerCubes.Add(newCube);
                    status[matrixIndex.x, matrixIndex.y] = TileStatus.Painted;
                }
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
                {
                    playerCubes.Add(newCube);
                    status[matrixIndex.x, matrixIndex.y] = TileStatus.Painted;
                }
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

    private void boundaryFill4(int m, int n, TileStatus fill_color, TileStatus boundary_color)
    {
        if (status[m, n] != TileStatus.Wall &&
            status[m, n] != fill_color)
        {
            FillWithCube(m, n, fill_color);

            boundaryFill4(m + 1, n, fill_color, boundary_color);
            boundaryFill4(m, n + 1, fill_color, boundary_color);
            boundaryFill4(m - 1, n, fill_color, boundary_color);
            boundaryFill4(m, n - 1, fill_color, boundary_color);
        }
    }

    private void FillWithCube(int m, int n, TileStatus fill_color)
    {
        status[m, n] = fill_color;
        Vector2Int position = ConvertMatrixIndexToPosition(new Vector2Int(m, n));
        Instantiate(playerCube, new Vector3(position.x, 0, position.y), Quaternion.identity);
    }
}
