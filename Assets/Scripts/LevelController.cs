using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public enum TileStatus{
        Empty = 0,
        Wall = 1,
        BeingFilled = 2,
        Filled = 3,
    }

    [SerializeField]
    int currentLevel;

    public static LevelController instance;

    [Header("Plane values")]
    [SerializeField]
    MeshRenderer planeMeshRender;
    
    //x = 0, y = 0 bottom left
    //For matrix A of MxN dimensions
    // A[m][n]
    // m = M - y - 1
    // n = x
    [Tooltip("Number of rows and columns of the matrix")]
    [SerializeField] 
    int M, N;

    [Header("Cubes")]
    [SerializeField] 
    GameObject fillingCube;

    [SerializeField]
    GameObject wallCube;

    [SerializeField] 
    Transform cubesParent;

    [Header("Wall Points")]
    [SerializeField]
    List<Vector2Int> level3WallPoints;

    TileStatus[ , ] status;
    int emptyTileCount;

    #region Getters - Setters

    public int GetM() { return M; }
    public int GetN() { return N; }

    public int GetEmptyTileCount() { return emptyTileCount; }

    public bool IsTileEmpty(Vector2Int matrixIndex)
    {
        return status[matrixIndex.x, matrixIndex.y] == TileStatus.Empty;
    }    
    public void SetTileEmpty(Vector2Int matrixIndex)
    {
        status[matrixIndex.x, matrixIndex.y] = TileStatus.Empty;
        IncreaseEmptyTileCount();
    }

    public bool IsTileBeingFilled(Vector2Int matrixIndex)
    {
        return status[matrixIndex.x, matrixIndex.y] == TileStatus.BeingFilled;
    }    
    public void SetTileBeingFilled(Vector2Int matrixIndex)
    {
        status[matrixIndex.x, matrixIndex.y] = TileStatus.BeingFilled;
    }

    public bool IsTileFilled(Vector2Int matrixIndex)
    {
        return status[matrixIndex.x, matrixIndex.y] == TileStatus.Filled;
    }    
    public void SetTileFilled(Vector2Int matrixIndex)
    {
        status[matrixIndex.x, matrixIndex.y] = TileStatus.Filled;
        DecreaseEmptyTileCount();
    }

    public bool IsTileWall(Vector2Int matrixIndex)
    {
        return status[matrixIndex.x, matrixIndex.y] == TileStatus.Wall;
    }   
    public void SetTileWall(Vector2Int matrixIndex)
    {
        status[matrixIndex.x, matrixIndex.y] = TileStatus.Wall;
        DecreaseEmptyTileCount();
    }
    #endregion

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        emptyTileCount = M * N;
        status = new TileStatus[M, N];
        GridEditor();

        //draw the outermost wall
        DrawWalls(Vector2Int.zero, new Vector2Int(M - 1, N - 1));

        //Draw existing level specific walls
        DrawLevelWalls();
    }

    private void GridEditor()
    {
        Material planeGridMaterial = planeMeshRender.material;
        planeGridMaterial.mainTextureScale = new Vector2(M, N);
    }

    private void DrawLevelWalls()
    {
        if(currentLevel == 3)
        {
            for (int i = 0; i < level3WallPoints.Count - 1; i = i + 2)
                DrawWalls(level3WallPoints[i], level3WallPoints[i + 1]);
        }
    }

    private void DrawWalls(Vector2Int corner1, Vector2Int corner2)
    {
        List<Vector2Int> cornerPoints;

        cornerPoints = GetCornerPoints(corner1, corner2);

        DrawLines(cornerPoints, wallCube);
    }

    private List<Vector2Int> GetCornerPoints(Vector2Int corner1, Vector2Int corner2)
    {
        List<Vector2Int> cornerPoints = new List<Vector2Int>();

        Vector2Int point = corner1;

        cornerPoints.Add(point);

        point = new Vector2Int(corner1.x, corner2.y);
        cornerPoints.Add(point);

        point = corner2;

        cornerPoints.Add(point);

        point = new Vector2Int(corner2.x, corner1.y);

        cornerPoints.Add(point);

        point = corner1;

        cornerPoints.Add(point); // for the last wall

        return cornerPoints;
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
        GameObject newCube = null;

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
                point.x = p1.x;
                point.y = i;

                Vector2Int matrixIndex = ConvertPositionToMatrixIndex(point);

                CreateCube(cube, ref point, ref newCube, matrixIndex);
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
                point.x = i;
                point.y = p1.y;

                Vector2Int matrixIndex = ConvertPositionToMatrixIndex(point);
                
                CreateCube(cube, ref point, ref newCube, matrixIndex);
            }
        }
    }

    private void CreateCube(GameObject cube, ref Vector2Int point, ref GameObject newCube, Vector2Int matrixIndex)
    {
        if (cube.tag == "FillingCube")
        {
            newCube = Instantiate(fillingCube, new Vector3(point.x, 0, point.y), Quaternion.identity);
            //newCube = FillingCubePool.instance.GetFillingCube();
            //newCube.transform.position = new Vector3(point.x, 0, point.y);
            SetTileFilled(matrixIndex);
        }
        else if (cube.tag == "WallCube")
        {
            newCube = Instantiate(cube, new Vector3(point.x, 0, point.y), Quaternion.identity);
            SetTileWall(matrixIndex);
        }
        
        newCube.transform.parent = cubesParent;
    }

    private void IncreaseEmptyTileCount()
    {
        ++emptyTileCount;
    }

    private void DecreaseEmptyTileCount()
    {
        --emptyTileCount;

        if (emptyTileCount == 0)
            GameManager.instance.LevelComplate();
    }

    public Vector2Int ConvertPositionToMatrixIndex(Vector2Int point)
    {
        // (m, n) = (M - y - 1 , x)   // (MxN matrix) 
        Vector2Int matrixIndex = new Vector2Int(M - point.y - 1, point.x);
        return matrixIndex;
    }

    public void PrintMatrix()
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
