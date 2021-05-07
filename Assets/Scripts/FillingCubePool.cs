using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillingCubePool : MonoBehaviour
{

    public static FillingCubePool instance;

    [SerializeField]
    GameObject fillingCube;
    
    [SerializeField]
    Transform cubesParent;

    List<GameObject> fillingCubes;
    int M;
    int N;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;

        // Allocate memory for th list
        MemoryAllocate();

        FillList();
    }

    private void MemoryAllocate()
    {
        M = LevelController.instance.GetM();
        N = LevelController.instance.GetN();

        //allocate memory as many as the number of max trail cube.
        fillingCubes = new List<GameObject>((M - 2) * (N - 2));
    }

    private void FillList()
    {
        int length = fillingCubes.Capacity;

        GameObject newCube;

        for (int i = 0; i < length; ++i)
        {
            newCube = Instantiate(fillingCube, Vector3.zero, Quaternion.identity);
            newCube.SetActive(false);
            newCube.transform.parent = cubesParent;

            fillingCubes.Add(newCube);
        }
    }

    public GameObject GetFillingCube()
    {
        if(fillingCubes.Count == 0)
        {
            GameObject newCube;

            newCube = Instantiate(fillingCube, Vector3.zero, Quaternion.identity);
            newCube.SetActive(false);
            newCube.transform.parent = cubesParent;

            fillingCubes.Add(newCube);
        }

        GameObject usingCube = fillingCubes[0];
        usingCube.SetActive(true);

        fillingCubes.RemoveAt(0);
        fillingCubes.Add(usingCube);

        return usingCube;
    }
    
}
