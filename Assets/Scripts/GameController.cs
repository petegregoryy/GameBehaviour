using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField]
    GameObject asteroid;

    [SerializeField]
    int asteroidCount;

    public float physicsCount = 0;
    public GameObject physicsController;

    [SerializeField]
    GameObject player;

    [SerializeField]
    GameObject pickupPrefab;

    GameObject activePickup;

    bool spawned = false;

    public int score = 0;
    public bool started = false;

    [SerializeField]
    Text scoreText;

    [SerializeField]
    GameObject startPanel;

    [SerializeField]
    GameObject scorePanel;

    [SerializeField]
    GameObject instructionPanel;

    [SerializeField]
    GameObject instructionDemos;

    [SerializeField]
    Dynamic playerDynamic;

    [SerializeField]
    Shape constraintParent;

    [SerializeField]
    Shape constraintChild;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < asteroidCount; i++)
        {
            GameObject obj = Instantiate(asteroid,new Vector3(Random.Range(-40,40),Random.Range(-20,20),0),this.transform.rotation);
            obj.GetComponent<Shape>().shapeID = i;
            obj.GetComponent<Shape>().gz = this.GetComponent<GizmoController>();
            obj.GetComponent<Dynamic>().gz = this.GetComponent<GizmoController>();
        }
        constraintParent.shapeID = asteroidCount;
        constraintChild.shapeID = asteroidCount +1;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey("q")){
            Application.Quit();
        }
        
        // If game isnt started, hold player off screen
        if(!started){
            player.transform.position = new Vector3(100,100,0);
            player.GetComponent<Dynamic>().hold = true;
            
            // If start button pressed, set UI elements and move player onto screen, set velocity and angle to 0 as player can still rotate and set velocity when in hold.
            if(Input.GetKey("r")){
                started = true;
                startPanel.SetActive(false);
                scorePanel.SetActive(true);
                instructionPanel.SetActive(false);
                instructionDemos.SetActive(false);
                player.transform.position = new Vector3(0,0,0);
                player.GetComponent<Dynamic>().hold = false;
                player.GetComponent<Dynamic>().velocity = new Vector3(0,0,0);
                player.GetComponent<Dynamic>().angle = 0;
            }
        }
        // If game is running
        else{
            scoreText.text = score.ToString();
            // If no pickup spawned, spawn pickup and set up gizmo Controller
            if(!spawned){
                activePickup = Instantiate(pickupPrefab,new Vector3(Random.Range(-40,40),Random.Range(-18,18),0),this.transform.rotation);
                activePickup.GetComponent<Pickup>().gz = this.GetComponent<GizmoController>();
                spawned = true;
            }
            // Check for pickup collision between player and current pickup. Only check when player is alive, otherwise score can increase after player has died.
            else if(!playerDynamic.isDead){
                Vector3 a = player.transform.position;
                Vector3 b = activePickup.transform.position;

                Vector2 vec = a - b;
                float mag = (float)Mathf.Sqrt((vec.x * vec.x) + (vec.y * vec.y));

                // If the player collides with the pickup, increase the score, delete the pickup and set spawned to false to respawn the new pickup.
                if(player.GetComponent<Shape>().broadPhaseRadius + activePickup.GetComponent<Pickup>().rad > mag){
                    score++;
                    
                    Destroy(activePickup);
                    spawned = false;
                }
            }   
        }
             
    }
}
