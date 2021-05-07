using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileStatus = LevelController.TileStatus;
using FillDirection = FillController.FillDirection;

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
    private int M, N;

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
            AddCubeToTrail(movePosition);

            UpdateTailStatus();

            if (!turningPoints.Contains(currentPosition))
                AddTurningPoint();

            FillController.instance.FillWithCubes(turningPoints);

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
