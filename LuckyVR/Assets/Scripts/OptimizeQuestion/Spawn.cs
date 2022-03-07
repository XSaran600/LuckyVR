using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject cube;

    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < 10000; i++)
        {
            Instantiate(cube);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
