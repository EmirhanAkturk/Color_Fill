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

    [Header("Cubes")]
    [SerializeField]
    Transform cubesParent;

    [SerializeField]
    GameObject trailCube;

    [SerializeField]
    float moveTime = 4;

    private float distance = 20;
    private MoveDirection moveDirection;

    private Vector2 startTouchPosition, currentTouchPosition;
    private Vector3 newMovePosition;
    
    private List<Vector2Int> turningPoints;
    private List<GameObject> trailCubes;


    private void Start()
    {
        turningPoints = new List<Vector2Int>();
        trailCubes = new List<GameObject>();

        newMovePosition = gameObject.transform.position;
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
                moveDirection = MoveDirection.None;
            }
        }
    }

    private void AddCubeToTrail(Vector3 cubePosition)
    {
        Vector2Int position = new Vector2Int((int)cubePosition.x, (int)cubePosition.z);
        Vector2Int matrixIndex = GameController.instance.ConvertPositionToMatrixIndex(position);

        if (GameController.instance.IsTileEmpty(matrixIndex)) 
        {
            GameObject newCube = Instantiate(trailCube, cubePosition, Quaternion.identity);
            trailCubes.Add(newCube);
            newCube.transform.parent = cubesParent;
            
            GameController.instance.SetTileFilled(matrixIndex);
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
                    if (horizontalDistance >= GetLimitDistance(false, distance))
                    {
                        // swipe right
                        if (currentTouchPosition.x > startTouchPosition.x)
                            moveDirection = MoveDirection.MoveRight;

                        //swipe left
                        else if(currentTouchPosition.x < startTouchPosition.x)
                            moveDirection = MoveDirection.MoveLeft;

                        startTouchPosition = currentTouchPosition;
                    }
                }
            }
            // verticle swipe
            else if (moveDirection != MoveDirection.MoveUp &&
                     moveDirection != MoveDirection.MoveDown)
            {
                if (verticleDistance >= GetLimitDistance(true, distance))
                {
                    if (currentTouchPosition.y > startTouchPosition.y) // swipe up
                        moveDirection = MoveDirection.MoveUp;

                    else if(currentTouchPosition.y < startTouchPosition.y) //swipe down
                        moveDirection = MoveDirection.MoveDown;

                    startTouchPosition = currentTouchPosition;
                }
            }
        }
        
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
