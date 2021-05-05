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


    [SerializeField]
    float distance;

    [SerializeField]
    float moveTime;

    int M, N;

    private MoveDirection moveDirection;
    private Vector2 startTouchPosition, currentTouchPosition, endPosition;
    private Vector3 newMovePosition;


    private void Start()
    {
        M = GameController.instance.GetM();
        N = GameController.instance.GetN();

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

        if (newMovePosition == currentMovePosition) { 
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

            GameController.TileStatus tileStatus = GameController.instance.GetTileStatus(matrixIndex);
            bool isCanMove = (tileStatus != GameController.TileStatus.Wall);

            bool isPositionChanged = (newMovePosition != currentMovePosition);

            if (isCanMove)
                gameObject.transform.position = Vector3.MoveTowards(currentMovePosition, newMovePosition, moveTime * Time.deltaTime);
            else
            {
                newMovePosition = currentMovePosition;
                moveDirection = MoveDirection.None;
            }
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
