using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum TileStatus{
        Empty = 0,
        Wall = 1,
        BeingFilled = 2,
        Filled = 3,
    }

    public static GameController instance;

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
    GameObject fillingCube;

    [SerializeField]
    GameObject wallCube;

    [SerializeField] 
    Transform cubesParent;
    
    [SerializeField] 
    List<Vector2Int> turningPoints, turningPoints2;

    //List<GameObject> playerCubes;
    TileStatus[ , ] status;
    bool isGameFinished; // todo it can be with Unity event

    #region Getters - Setters

    public bool IsGameFinished { get => isGameFinished; set => isGameFinished = value; }

    public int GetM() { return M; }
    public int GetN() { return N; }

    public bool IsTileEmpty(Vector2Int matrixIndex)
    {
        return status[matrixIndex.x, matrixIndex.y] == TileStatus.Empty;
    }    
    public void SetTileEmpty(Vector2Int matrixIndex)
    {
        status[matrixIndex.x, matrixIndex.y] = TileStatus.Empty;
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
    }      
    
    public bool IsTileWall(Vector2Int matrixIndex)
    {
        return status[matrixIndex.x, matrixIndex.y] == TileStatus.Wall;
    }   
    public void SetTileWall(Vector2Int matrixIndex)
    {
        status[matrixIndex.x, matrixIndex.y] = TileStatus.Wall;
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
        status = new TileStatus[M, N];

        GridEditor();
        DrawWalls();

        //AddEndPoint(turningPoints);
        //DrawLines(turningPoints, fillingCube, false);

        //AddEndPoint(turningPoints2);
        DrawLines(turningPoints2, wallCube, false);

        //FillWithCubes(turningPoints);
        //PrintMatrix();
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

        DrawLines(cornerPoints, wallCube, false);
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

    private void DrawLines(List<Vector2Int>points, GameObject cube, bool isWall)
    {
        if (!isWall)
            AddEndPoint(points);

        for (int i = 0; i < (points.Count - 1); ++i)
        {
            DrawLine(points[i], points[i + 1], cube);
        }
    }

    public void AddEndPoint(List<Vector2Int> turningPoints)
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
            status[matrixIndex.x, matrixIndex.y] = TileStatus.Filled;
        }
        else if (cube.tag == "WallCube")
        {
            newCube = Instantiate(cube, new Vector3(point.x, 0, point.y), Quaternion.identity);
            status[matrixIndex.x, matrixIndex.y] = TileStatus.Wall;
        }
        
        newCube.transform.parent = cubesParent;

    }

    public void FillWithCubes(List<Vector2Int> turningPoints)
    {
        AddEndPoint(turningPoints);

        Vector2Int matrixIndex; // cube matrix index

        for (int i = 0; i < turningPoints.Count; ++i)
        {
            matrixIndex = ConvertPositionToMatrixIndex(turningPoints[i]);

            if (matrixIndex.x < M - 2)
                ++matrixIndex.x;

            if (matrixIndex.y < N - 2)
                ++matrixIndex.y;

            BoundaryFill(matrixIndex.x, matrixIndex.y, TileStatus.Filled, TileStatus.Wall);
        }
    }

    private void BoundaryFill(int m, int n, TileStatus fill_color, TileStatus boundary_color)
    {
        if (m >= M || m < 0 || n >= N || n < 0)
            return;

        if (status[m, n] != TileStatus.Wall &&
            status[m, n] != fill_color)
        {
            FillWithCube(m, n, fill_color);

            BoundaryFill(m + 1, n, fill_color, boundary_color);
            BoundaryFill(m, n + 1, fill_color, boundary_color);
            BoundaryFill(m - 1, n, fill_color, boundary_color);
            BoundaryFill(m, n - 1, fill_color, boundary_color);
        }
    }

    private void FillWithCube(int m, int n, TileStatus fill_color)
    {
        status[m, n] = fill_color;
        Vector2Int position = ConvertMatrixIndexToPosition(new Vector2Int(m, n));
        GameObject newCube = FillingCubePool.instance.GetFillingCube();

        newCube.transform.position = new Vector3(position.x, 0, position.y);
        newCube.transform.parent = cubesParent;
    }


    public Vector2Int ConvertPositionToMatrixIndex(Vector2Int point)
    {
        // (m, n) = (M - y - 1 , x)   // (MxN matrix) 
        Vector2Int matrixIndex = new Vector2Int(M - point.y - 1, point.x);
        return matrixIndex;
    }

    public Vector2Int ConvertMatrixIndexToPosition(Vector2Int matrixIndex)
    {
        // (x, y) = (n, M - m - 1) // (MxN matrix) 
        Vector2Int positionIndex = new Vector2Int(matrixIndex.y, M - matrixIndex.x - 1);
        return positionIndex;
    }

    public void PrintMatrix()
    {
        for (int i = 0; i < M; ++i)
        {
            if (i == 18)
            { 

            Debug.Log((i + 1) + ". satır:");

            for (int j = 0; j < N; ++j)
                Debug.Log(status[i, j]);

            Debug.Log("####################################");
            
            }
        }
    }

}
