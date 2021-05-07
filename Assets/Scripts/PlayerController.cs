using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileStatus = LevelController.TileStatus;

public class PlayerController : MonoBehaviour
{
    public enum MoveDirection
    {
        None = 0,
        MoveUp = 1,
        MoveDown = 2,
        MoveLeft = 3,
        MoveRight = 4,
    }

    public enum FillDirection
    {
        FillUp = 1,
        FillDown = 2,
        FillLeft = 3,
        FillRight = 4,
    }


    [Header("Player Movement")]
    [SerializeField]
    float moveTime = 4;

    [Header("Cubes")]
    [SerializeField]
    Transform cubesParent;

    [SerializeField]
    GameObject fillingCube;

    [SerializeField]
    Vector3 tailCubeScale; // cubes in the tail behind the player when moving

    [SerializeField]
    Vector3 fillingCubeScale;

    //For matrix A of MxN dimensions
    int M, N;

    private float swipeLimitDistance = 20;
    private MoveDirection moveDirection = MoveDirection.None;

    private Vector2 startTouchPosition, currentTouchPosition;
    private Vector3 newMovePosition;

    //private WaitForSeconds fillDelay;

    private List<Vector2Int> turningPoints;
    private List<GameObject> tailCubes;

    private void Start()
    {
        M = LevelController.instance.GetM();
        N = LevelController.instance.GetN();

        //fillDelay = new WaitForSeconds(0.1f);

        turningPoints = new List<Vector2Int>();
        tailCubes = new List<GameObject>();
    }

    private void Update()
    {
        if (GameManager.instance.IsPlaying)
        {
            TouchControl();
            MovePlayer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.CompareTag("TailCube"))
        //GameManager.instance.LevelFail();
    }

    private void TouchControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            currentTouchPosition = Input.mousePosition;

            float horizontalDistance = Mathf.Abs(currentTouchPosition.x - startTouchPosition.x);
            float verticleDistance = Mathf.Abs(currentTouchPosition.y - startTouchPosition.y);

            // horizontal swipe
            if (horizontalDistance > verticleDistance)
            {
                if (moveDirection != MoveDirection.MoveLeft &&
                     moveDirection != MoveDirection.MoveRight)
                {
                    //If the swipe distance is more than the limit value, the swipe is applied.
                    if (horizontalDistance >= GetLimitDistance(false, swipeLimitDistance))
                    {
                        // swipe right
                        if (currentTouchPosition.x > startTouchPosition.x)
                            moveDirection = MoveDirection.MoveRight;

                        //swipe left
                        else if (currentTouchPosition.x < startTouchPosition.x)
                            moveDirection = MoveDirection.MoveLeft;

                        startTouchPosition = currentTouchPosition;

                        AddTurningPoint();
                    }
                }
            }
            // verticle swipe
            else if (moveDirection != MoveDirection.MoveUp &&
                     moveDirection != MoveDirection.MoveDown)
            {
                if (verticleDistance >= GetLimitDistance(true, swipeLimitDistance))
                {
                    if (currentTouchPosition.y > startTouchPosition.y) // swipe up
                        moveDirection = MoveDirection.MoveUp;

                    else if (currentTouchPosition.y < startTouchPosition.y) //swipe down
                        moveDirection = MoveDirection.MoveDown;

                    startTouchPosition = currentTouchPosition;
                    AddTurningPoint();
                }
            }
        }

    }

    private void AddTurningPoint()
    {
        Vector3 currentPosition = newMovePosition;
        Vector2Int newTurningPosition = new Vector2Int((int)currentPosition.x, (int)currentPosition.z);

        if (turningPoints.Count == 0 || !turningPoints.Contains(newTurningPosition))
        {
            turningPoints.Add(newTurningPosition);
        }
    }

    private void MovePlayer()
    {
        Vector3 currentMovePosition = gameObject.transform.position;

        if (newMovePosition == currentMovePosition)
        {
            //check for hitting the tail
            if (!DidHitTail(currentMovePosition))
            {
                CheckIndoorArea(newMovePosition);

                AddCubeToTrail(newMovePosition);

                switch (moveDirection)
                {
                    case MoveDirection.MoveUp:
                        newMovePosition = currentMovePosition + Vector3.forward;
                        break;

                    case MoveDirection.MoveDown:
                        newMovePosition = currentMovePosition + Vector3.back;
                        break;

                    case MoveDirection.MoveLeft:
                        newMovePosition = currentMovePosition + Vector3.left;
                        break;

                    case MoveDirection.MoveRight:
                        newMovePosition = currentMovePosition + Vector3.right;
                        break;

                    default:
                        newMovePosition = currentMovePosition;
                        break;
                }
            }
            else
            {
                moveDirection = MoveDirection.None;
                GameManager.instance.LevelFail();
            }
        }
        else
        {
            Vector2Int position = new Vector2Int((int)newMovePosition.x, (int)newMovePosition.z);
            Vector2Int matrixIndex = ConvertPositionToMatrixIndex(position);

            bool isCanMove = !LevelController.instance.IsTileWall(matrixIndex);

            if (isCanMove)
            {
                gameObject.transform.position = Vector3.MoveTowards(currentMovePosition, newMovePosition, moveTime * Time.deltaTime);
            }
            else
            {
                newMovePosition = currentMovePosition;
                moveDirection = MoveDirection.None;
            }
        }
    }

    //check for hitting the tail
    private bool DidHitTail(Vector3 currentPosition)
    {
        for (int i = 0; i < tailCubes.Count - 1; ++i)
        {
            ////The player's cube crashed into its tail.
            if (tailCubes[i].transform.position == currentPosition)
            {
                HittedTail();
                return true;
            }
        }

        return false;
    }

    private void HittedTail()
    {
        foreach (GameObject cube in tailCubes)
            cube.SetActive(false);

        tailCubes.Clear();
    }

    private void CheckIndoorArea(Vector3 movePosition)
    {
        Vector2Int currentPosition = new Vector2Int((int)movePosition.x, (int)movePosition.z);
        Vector2Int nextPosition = GetNextPosition(currentPosition);

        Vector2Int matrixIndex = ConvertPositionToMatrixIndex(currentPosition);
        Vector2Int nextMatrixIndex = ConvertPositionToMatrixIndex(nextPosition);

        bool isTileFilled = LevelController.instance.IsTileFilled(matrixIndex);
        bool isNextTileFilled = LevelController.instance.IsTileFilled(nextMatrixIndex);
        bool isNextTileWall = LevelController.instance.IsTileWall(nextMatrixIndex);

        if (isTileFilled)
            turningPoints.Clear();

        if (isNextTileWall || isNextTileFilled)
        {
            //if(tailCubes.Count > 1) //todo delete this
            //    Debug.Log("Before Last item in tailCubes: " + tailCubes[tailCubes.Count - 1].transform.position +"," + tailCubes.Count);

            AddCubeToTrail(movePosition);

            //if (tailCubes.Count > 1) //todo delete this
            //    Debug.Log("After Last item in tailCubes: " + tailCubes[tailCubes.Count - 1].transform.position + "," + tailCubes.Count);

            UpdateTailStatus();

            if (!turningPoints.Contains(currentPosition))
                AddTurningPoint();

            //if (turningPoints.Count > 1) // todo delete this
            //{
            //    print("###################");
            //    foreach (var point in turningPoints)
            //        print(point);
            //    print("###################");
            //}

            //GameController.instance.AddEndPoint(turningPoints);

            FillWithCubes(turningPoints);

            turningPoints.Clear();
        }
    }

    private void AddCubeToTrail(Vector3 cubePosition)
    {
        Vector2Int position = new Vector2Int((int)cubePosition.x, (int)cubePosition.z);
        Vector2Int matrixIndex = ConvertPositionToMatrixIndex(position);

        if (LevelController.instance.IsTileEmpty(matrixIndex))
        {
            if (tailCubes.Count == 0 && !turningPoints.Contains(position))
                turningPoints.Add(position);

            GameObject newCube = FillingCubePool.instance.GetFillingCube();
            newCube.transform.position = cubePosition;
            newCube.transform.localScale = tailCubeScale;
            newCube.tag = "TailCube";

            tailCubes.Add(newCube);
            newCube.transform.parent = cubesParent;

            LevelController.instance.SetTileBeingFilled(matrixIndex);

        }
    }

    private void UpdateTailStatus()
    {
        Vector2Int cubePosition;
        Vector2Int matrixIndex;

        foreach (GameObject cube in tailCubes)
        {
            cube.tag = "FillingCube";
            cubePosition = new Vector2Int((int)cube.transform.position.x, (int)cube.transform.position.z);
            matrixIndex = ConvertPositionToMatrixIndex(cubePosition);
            LevelController.instance.SetTileFilled(matrixIndex);

            cube.transform.localScale = fillingCubeScale;
        }

        tailCubes.Clear();
    }

    public void FillWithCubes(List<Vector2Int> turningPoints)
    {
        Vector2Int matrixIndex; // cube matrix index
        Vector2Int position; // cube matrix index

        Vector2Int point1, point2;

        // filling distances  distances
        int upDistance = 0, downDistance = 0, leftDistance = 0, rightDistance = 0;
        int fillingDistance, distance1, distance2;

        if(turningPoints.Count > 1)
            Debug.Log(turningPoints.Count);

        if (turningPoints.Count % 2 == 0)
        {
            if(turningPoints.Count == 2) 
            { 
                point1 = turningPoints[0];
                point2 = turningPoints[1];
            }
            else
            {
                point1 = turningPoints[0]; // first point
                point2 = turningPoints[turningPoints.Count - 2]; // second to last point
            }

            position = (point1 + point2) / 2;

            #region temp

            //todo delete lines in comment below
            Debug.Log(position);

            distance1 = GetFillDistance(point1, FillDirection.FillUp);
            distance2 = GetFillDistance(point2, FillDirection.FillUp);
            upDistance = distance1 < distance2 ? distance1 : distance2;

            distance1 = GetFillDistance(point1, FillDirection.FillDown);
            distance2 = GetFillDistance(point2, FillDirection.FillDown);
            downDistance = distance1 < distance2 ? distance1 : distance2;

            distance1 = GetFillDistance(point1, FillDirection.FillRight);
            distance2 = GetFillDistance(point2, FillDirection.FillRight);
            rightDistance = distance1 < distance2 ? distance1 : distance2;

            distance1 = GetFillDistance(point1, FillDirection.FillLeft);
            distance2 = GetFillDistance(point2, FillDirection.FillLeft);
            leftDistance = distance1 < distance2 ? distance1 : distance2;

            Debug.Log($"up:{upDistance}, down:{downDistance}, right: {rightDistance}, left: {leftDistance}");

            #endregion

            if (point1.y == point2.y) // horizontal line
            {
                //Debug.Log($"Point1:{point1}, Point2: {point2}");

                distance1 = GetFillDistance(point1, FillDirection.FillUp);
                distance2 = GetFillDistance(point2, FillDirection.FillUp);
                upDistance = distance1 > distance2 ? distance1 : distance2;

                distance1 = GetFillDistance(point1, FillDirection.FillDown);
                distance2 = GetFillDistance(point2, FillDirection.FillDown);
                downDistance = distance1 > distance2 ? distance1 : distance2;

                if ( upDistance > downDistance)
                {
                    --position.y;
                    fillingDistance = downDistance;
                }

                else
                {
                    ++position.y;
                    fillingDistance = upDistance;
                }

                point1.y = position.y;
                point2.y = position.y;
            }
            else // verticle line
            {
                distance1 = GetFillDistance(point1, FillDirection.FillRight);
                distance2 = GetFillDistance(point2, FillDirection.FillRight);
                rightDistance = distance1 > distance2 ? distance1 : distance2;

                distance1 = GetFillDistance(point1, FillDirection.FillLeft);
                distance2 = GetFillDistance(point2, FillDirection.FillLeft);
                leftDistance = distance1 > distance2 ? distance1 : distance2;

                if (rightDistance > leftDistance)
                {
                    --position.x;
                    fillingDistance = leftDistance;
                }

                else
                {
                    ++position.x;
                    fillingDistance = rightDistance;
                }

                point1.x = position.x;
                point2.x = position.x;

            }

            Vector2Int matrixIndex1 = ConvertPositionToMatrixIndex(point1);
            Vector2Int matrixIndex2 = ConvertPositionToMatrixIndex(point2);

            //Debug.Log($"up:{maxUpDistance}, down:{maxDownDistance}, right: {maxRightDistance}, left: {maxLeftDistance}");

            if (fillingDistance > 0) { 
                BoundaryFill(matrixIndex1.x, matrixIndex1.y, TileStatus.Filled, TileStatus.Wall);
                BoundaryFill(matrixIndex2.x, matrixIndex2.y, TileStatus.Filled, TileStatus.Wall);
            }
        }

        else if (turningPoints.Count > 1)
        {
            position = turningPoints[turningPoints.Count / 2];

            FillDirection verticle;
            FillDirection horizontal;

            int verticleLength, horizontalLength;

            upDistance = GetFillDistance(position, FillDirection.FillUp);
            downDistance = GetFillDistance(position, FillDirection.FillDown);
            leftDistance = GetFillDistance(position, FillDirection.FillLeft);
            rightDistance = GetFillDistance(position, FillDirection.FillRight);

            if(downDistance == 0) 
            { 
                verticle = FillDirection.FillDown;
                verticleLength = M - 2 - upDistance;
            }
            else
            {
                verticle = FillDirection.FillUp;
                verticleLength = M - 2 - downDistance;
            }
            
            if(rightDistance == 0) 
            { 
                horizontal = FillDirection.FillRight;
                horizontalLength = N - 2 - leftDistance;
            }
            else
            {
                horizontal = FillDirection.FillLeft;
                horizontalLength = N - 2 - rightDistance;
            }

            int innerSpace = horizontalLength * verticleLength;
            int outerSpace = LevelController.instance.GetEmptyTileCount() - innerSpace;

            Debug.Log("Inner space: " + innerSpace + ", outerSpace: " + outerSpace);
            //change fill direction
            if (innerSpace > outerSpace)
            {
                if (horizontal == FillDirection.FillLeft)
                    horizontal = FillDirection.FillRight;
                else
                    horizontal = FillDirection.FillLeft;

                if (verticle == FillDirection.FillUp)
                    verticle = FillDirection.FillDown;
                else
                    verticle = FillDirection.FillUp;
            }

            matrixIndex = GetFillMatrixIndex(position, verticle, horizontal);
            Debug.Log($"up:{upDistance}, down:{downDistance}, right: {rightDistance}, left: {leftDistance}");

            BoundaryFill(matrixIndex.x, matrixIndex.y, TileStatus.Filled, TileStatus.Wall);
        }
    }

    private Vector2Int GetFillMatrixIndex(Vector2Int position, FillDirection verticleDirection, FillDirection horizontalDirection)
    {
        Vector2Int matrixIndex = ConvertPositionToMatrixIndex(position);
        bool isTileEmpty = LevelController.instance.IsTileEmpty(matrixIndex);
                                                                            
        // for matrixIndex change
        int matrixVerticleChange = (verticleDirection == FillDirection.FillUp) ? -1 : 1; 
        int matrixHorizontalChange = (horizontalDirection == FillDirection.FillRight) ? 1 : -1;

        //to prevent the infinite loop
        int maxLoopCount = 10;

        while (!isTileEmpty && maxLoopCount > 0)
        {
            //Check for no out of range
            if (matrixIndex.x == 1 && matrixVerticleChange == -1)
                matrixVerticleChange = 0;
            
            else if(matrixIndex.x == M-2 && matrixVerticleChange == 1)
                matrixVerticleChange = 0;

            if (matrixIndex.y == 1 && matrixHorizontalChange == -1)
                matrixHorizontalChange = 0;
            
            else if(matrixIndex.y == N - 2 && matrixHorizontalChange == 1)
                matrixHorizontalChange = 0;
 

            matrixIndex.x += matrixVerticleChange;
            matrixIndex.y += matrixHorizontalChange;

            isTileEmpty = LevelController.instance.IsTileEmpty(matrixIndex);
            
            --maxLoopCount;
        }

        return matrixIndex;
    }

    private void BoundaryFill(int m, int n, TileStatus fill_color, TileStatus boundary_color)
    {
        if (m >= M || m < 0 || n >= N || n < 0)
            return;

        if (LevelController.instance.IsTileEmpty(new Vector2Int(m, n)))
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

        LevelController.instance.SetTileFilled(new Vector2Int(m, n));
        Vector2Int position = ConvertMatrixIndexToPosition(new Vector2Int(m, n));
        GameObject newCube = FillingCubePool.instance.GetFillingCube();

        newCube.transform.position = new Vector3(position.x, 0.45f, position.y);
        newCube.transform.parent = cubesParent;
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

    private Vector2Int GetNextPosition(Vector2Int currentPosition)
    {
        Vector2Int newPosition;

        switch (moveDirection)
        {
            case MoveDirection.MoveUp:
                newPosition = currentPosition + Vector2Int.up;
                break;

            case MoveDirection.MoveDown:
                newPosition = currentPosition + Vector2Int.down;
                break;

            case MoveDirection.MoveLeft:
                newPosition = currentPosition + Vector2Int.left;
                break;

            case MoveDirection.MoveRight:
                newPosition = currentPosition + Vector2Int.right;
                break;

            default:
                newPosition = currentPosition;
                break;
        }

        return newPosition;
    }

    private int GetFillDistance(Vector2Int currentPosition, FillDirection moveDirection)
    {
        Vector2Int matrixIndex = ConvertPositionToMatrixIndex(currentPosition);
        bool isTileWall, isTileFilled;
        int fillDistance = 0;

        switch (moveDirection)
        {
            case FillDirection.FillUp:
                {
                    for (int i = matrixIndex.x - 1; i >= 0; --i)
                    {
                        isTileWall = LevelController.instance.IsTileWall(new Vector2Int(i, matrixIndex.y));
                        isTileFilled = LevelController.instance.IsTileFilled(new Vector2Int(i, matrixIndex.y));

                        if (isTileWall || isTileFilled)
                            break;
                        else
                            ++fillDistance;
                    }

                    return fillDistance;
                }

            case FillDirection.FillDown:
                {
                    for (int i = matrixIndex.x + 1; i < M; ++i)
                    {
                        isTileWall = LevelController.instance.IsTileWall(new Vector2Int(i, matrixIndex.y));
                        isTileFilled = LevelController.instance.IsTileFilled(new Vector2Int(i, matrixIndex.y));

                        if (isTileWall || isTileFilled)
                            break;
                        else
                            ++fillDistance;
                    }

                    return fillDistance;
                }

            case FillDirection.FillRight:
                {
                    for (int i = matrixIndex.y + 1; i < N; ++i)
                    {
                        isTileWall = LevelController.instance.IsTileWall(new Vector2Int(matrixIndex.x, i));
                        isTileFilled = LevelController.instance.IsTileFilled(new Vector2Int(matrixIndex.x, i));

                        if (isTileWall || isTileFilled)
                            break;
                        else
                            ++fillDistance;
                    }

                    return fillDistance;
                }

            case FillDirection.FillLeft:
                {
                    for (int i = matrixIndex.y - 1; i >= 0; --i)
                    {
                        isTileWall = LevelController.instance.IsTileWall(new Vector2Int(matrixIndex.x, i));
                        isTileFilled = LevelController.instance.IsTileFilled(new Vector2Int(matrixIndex.x, i));

                        if (isTileWall || isTileFilled)
                            break;
                        else
                            ++fillDistance;
                    }

                    return fillDistance;
                }
        }

        return 0;
    }

    private float GetLimitDistance(bool isVerticle, float distance)
    {
        float referanceWidth = 1080;
        float referanceHeight = 1920;

        float thisScreenWidth = Screen.width;
        float thisScreenHeight = Screen.height;

        float limitDistance;

        if (isVerticle)
            limitDistance = distance * referanceHeight / thisScreenHeight;

        else
            limitDistance = distance * referanceWidth / thisScreenWidth;

        return limitDistance;
    }
}
