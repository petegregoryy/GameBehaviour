using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    GameObject asteroid;

    [SerializeField]
    int asteroidCount;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < asteroidCount; i++)
        {
            Instantiate(asteroid,new Vector3(Random.Range(-40,40),Random.Range(-25,25),0),this.transform.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
