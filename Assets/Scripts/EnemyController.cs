using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("FillingCube"))
        {
            if(GameManager.instance.IsPlaying)
                Destroy(gameObject);
        }

        else if (other.gameObject.CompareTag("PlayerCube"))
            GameManager.instance.LevelFail();

        else if (other.gameObject.CompareTag("TailCube"))
            GameManager.instance.LevelFail();
    }
}
