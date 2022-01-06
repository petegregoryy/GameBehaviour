using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    GameObject asteroid;

    [SerializeField]
    int asteroidCount;

    public float physicsCount = 0;
    public GameObject physicsController;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < asteroidCount; i++)
        {
            GameObject obj = Instantiate(asteroid,new Vector3(Random.Range(-40,40),Random.Range(-20,20),0),this.transform.rotation);
            obj.GetComponent<Shape>().shapeID = i;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //physicsController.GetComponent<SAT>().PhysicsUpdate();
        // physicsCount +=1*Time.deltaTime;
        // if(physicsCount > 0){
        //     physicsCount = 0;
            
        // }
    }
}
