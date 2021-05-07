using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    bool isVerticle;

    [SerializeField]
    float moveDistance;

    [SerializeField]
    float moveSpeed;


    float newAxisPosition;
    Vector3 enemyOffset;

    private void Start()
    {
        enemyOffset = transform.position;
    }

    private void Update()
    {
        if(GameManager.instance.IsPlaying)
            EnemyLoop();
    }

    private void EnemyLoop()
    {
        newAxisPosition = Mathf.PingPong(Time.time * moveSpeed, moveDistance);

        Vector3 currPosition = transform.position;

        if (isVerticle)
            transform.position = new Vector3(currPosition.x, currPosition.y, newAxisPosition + enemyOffset.z);
        else
            transform.position = new Vector3(newAxisPosition + enemyOffset.x, currPosition.y, currPosition.z);

    }
}
