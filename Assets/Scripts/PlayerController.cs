using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //[Header("Colors")]
    //[SerializeField]
    //Color darkBlue;

    //[SerializeField]
    //Color lightBlue;

    private float swipeLimitDistance = 20;
    private MoveDirection moveDirection = MoveDirection.None;

    private Vector2 startTouchPosition, currentTouchPosition;
    private Vector3 newMovePosition;

    private List<Vector2Int> turningPoints;
    private List<GameObject> tailCubes;

    private void Start()
    {
        turningPoints = new List<Vector2Int>();
        tailCubes = new List<GameObject>();
    }

    private void Update()
    {
        TouchControl();
        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector3 currentMovePosition = gameObject.transform.position;

        if (newMovePosition == currentMovePosition)
        {
            //check for hitting the tail
            if (!DidHitTail(currentMovePosition))
            {
                CheckIndoorArea(currentMovePosition);

                AddCubeToTrail(currentMovePosition);

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
                moveDirection = MoveDirection.None;
        }
        else {

            Vector2Int position = new Vector2Int((int)newMovePosition.x, (int)newMovePosition.z);
            Vector2Int matrixIndex = GameController.instance.ConvertPositionToMatrixIndex(position);

            bool isCanMove = !GameController.instance.IsTileWall(matrixIndex);

            if (isCanMove) 
            {
                gameObject.transform.position = Vector3.MoveTowards(currentMovePosition, newMovePosition, moveTime * Time.deltaTime);
            }
            else
            {
                newMovePosition = currentMovePosition;
            }
        }
    }

    private void CheckIndoorArea(Vector3 currentMovePosition)
    {
        Vector2Int currentPosition = new Vector2Int((int)currentMovePosition.x, (int)currentMovePosition.z);
        Vector2Int nextPosition = GetNextPosition(currentPosition);
        //Debug.Log(currentMovePosition + " " + currentPosition);

        Vector2Int matrixIndex = GameController.instance.ConvertPositionToMatrixIndex(currentPosition);
        Vector2Int nextMatrixIndex = GameController.instance.ConvertPositionToMatrixIndex(nextPosition);

        bool isTileFilled = GameController.instance.IsTileFilled(matrixIndex);
        bool isTileWall = GameController.instance.IsTileWall(nextMatrixIndex);

        if (isTileWall || isTileFilled) 
        {
            UpdateTail();
            
            if (!turningPoints.Contains(currentPosition))
                turningPoints.Add(currentPosition);

            //GameController.instance.FillWithCubes(turningPoints);
        }
    }

    private void UpdateTail()
    {
        Vector2Int cubePosition;
        Vector2Int matrixIndex;

        foreach (GameObject cube in tailCubes)
        {
            cubePosition = new Vector2Int((int)cube.transform.position.x, (int)cube.transform.position.z);
            matrixIndex = GameController.instance.ConvertPositionToMatrixIndex(cubePosition);
            GameController.instance.SetTileFilled(matrixIndex);

            cube.transform.localScale = fillingCubeScale;
        }

        tailCubes.Clear();
    }

    //check for hitting the tail
    private bool DidHitTail(Vector3 currentPosition)
    {
        for(int i = 0; i< tailCubes.Count - 1; ++i)
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
        GameController.instance.IsGameFinished = true;
    }

    private void AddCubeToTrail(Vector3 cubePosition)
    {
        Vector2Int position = new Vector2Int((int)cubePosition.x, (int)cubePosition.z);
        Vector2Int matrixIndex = GameController.instance.ConvertPositionToMatrixIndex(position);

        if (GameController.instance.IsTileEmpty(matrixIndex)) 
        {
            GameObject newCube = FillingCubePool.instance.GetFillingCube();
            newCube.transform.position = cubePosition;
            newCube.transform.localScale = tailCubeScale;
           
            tailCubes.Add(newCube);
            newCube.transform.parent = cubesParent;

            GameController.instance.SetTileBeingFilled(matrixIndex);
        }
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
                        else if(currentTouchPosition.x < startTouchPosition.x)
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

                    else if(currentTouchPosition.y < startTouchPosition.y) //swipe down
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
