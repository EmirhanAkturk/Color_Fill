using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailCubePool : MonoBehaviour
{

    public static TrailCubePool instance;

    [SerializeField]
    GameObject trailCube;
    
    [SerializeField]
    Transform cubesParent;

    List<GameObject> trailCubes;
    int M;
    int N;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        M = GameController.instance.GetM();
        N = GameController.instance.GetN();

        //allocate memory as many as the number of max trail cube.
        trailCubes = new List<GameObject>((M-1) * (N-1));

        TrailCubeGenerator();
    }

    private void TrailCubeGenerator()
    {
        int length = trailCubes.Capacity;

        GameObject newCube;

        for (int i = 0; i < length; ++i)
        {
            newCube = Instantiate(trailCube, Vector3.zero, Quaternion.identity);
            newCube.SetActive(false);
            newCube.transform.parent = cubesParent;

            trailCubes.Add(newCube);
        }
    }

    public GameObject GetTrailCube()
    {
        if(trailCubes.Count == 0)// trailCubes[0] == null)
        {
            GameObject newCube;

            newCube = Instantiate(trailCube, Vector3.zero, Quaternion.identity);
            newCube.SetActive(false);
            newCube.transform.parent = cubesParent;

            trailCubes.Add(newCube);
        }

        GameObject usingCube = trailCubes[0];
        usingCube.SetActive(true);

        trailCubes.RemoveAt(0);
        trailCubes.Add(usingCube);

        return usingCube;
    }
    
}
