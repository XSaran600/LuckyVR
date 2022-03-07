using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Optimize : MonoBehaviour
{
    GameObject[] sceneObjects;

    int interval = 3;

    void Start()
    {
        sceneObjects = GameObject.FindGameObjectsWithTag("scene");
    }

    void Update()
    {

        if (Time.frameCount % interval == 0)
        {
            foreach (GameObject i in sceneObjects)
            {
                if (Vector3.Distance(i.transform.position, Vector3.zero) > 5)
                {
                    i.transform.position = Vector3.zero;
                }
                else
                {
                    i.transform.position += Vector3.one;
                }
            }
        }
    }
   

    /*
    void Update()
    {
        GameObject[] sceneObjects = GameObject.FindGameObjectsWithTag("scene");

        foreach (GameObject i in sceneObjects)
        {
            if (Vector3.Distance(i.transform.position, new Vector3(0, 0, 0)) > 5)
            {
                i.transform.position = new Vector3(0, 0, 0);
            }
            else
            {
                i.transform.position += new Vector3(1, 1, 1);
            }
        }
    }
    */
}
