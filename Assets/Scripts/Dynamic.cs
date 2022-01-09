using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Dynamic : MonoBehaviour
{
    [SerializeField]
    GameController gc;
    public Vector3 ObjPos;
    public Vector3 velocity;

    [Range(100f, 1000f)]
    public float mass = 100f;
    Vector3 intermediateVelocity;

    [SerializeField]
    bool isShip = false;

    [SerializeField]
    public float angle = 0f;

    public bool isDead = false;
    public GameObject deadText;
    public bool hold = false;
    Vector2 addVector = new Vector2(0, 0);

    public Vector3[] futurePos = new Vector3[3];

    [Header("Gizmo Settings")]

    public GizmoController gz;
    public GameObject debugObj;
    bool debugSpawned = false;
    GameObject debugSpawn;
    LineRenderer dblr;
    Vector3[] localDebug;

    // public List<List<float>> this.GetComponent<Shape().rotmat;


    // Start is called before the first frame update
    void Start()
    {
        // Set to start position of object
        ObjPos = this.transform.position;
        
        // If the object isn't the ship apply a random velocity and set its mass.
        if (!isShip)
        {
            velocity = new Vector3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5), 0);
            mass = UnityEngine.Random.Range(100f, 200f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Set the future positions for the object based on the current frame time and the current velocity.
        futurePos[0] = this.transform.position + (velocity * Time.deltaTime) * 60;
        futurePos[1] = this.transform.position + (velocity * Time.deltaTime) * 120;
        futurePos[2] = this.transform.position + (velocity * Time.deltaTime) * 180;
        
        // Only apply if there is a velocity to apply and the velocity isn't 0;
        if (velocity.x != float.NaN && velocity.y != float.NaN)
        {
            if (velocity.x != 0 || velocity.y != 0)
            {
                this.transform.position += velocity * Time.deltaTime;
            }
        }


        if (isShip)
        {
            // Set the shape rotation matrix
            this.GetComponent<Shape>().rotMat[0, 0] = Mathf.Cos(angle);
            this.GetComponent<Shape>().rotMat[0, 1] = -Mathf.Sin(angle);
            this.GetComponent<Shape>().rotMat[1, 0] = Mathf.Sin(angle);
            this.GetComponent<Shape>().rotMat[1, 1] = Mathf.Cos(angle);

            // If the player is dead, lock out all controls and show the dead text.
            if (isDead)
            {
                deadText.active = true;
                this.GetComponent<Shape>().thrusting = false;
                
                // If player is reset, set position and velocity, reset score and hide dead text
                if (Input.GetKey("r"))
                {
                    isDead = false;
                    this.transform.position = new Vector3(0, 0, 0);
                    velocity = new Vector3(0, 0, 0);
                    deadText.active = false;
                    gc.score = 0;
                }
            }
            else
            {
                // Thrust key
                if (Input.GetKey("space"))
                {
                    //Lock velocity to 25 in each axis
                    if (velocity.x > 25)
                    {
                        velocity = new Vector2(25.1f, velocity.y);
                    }
                    else if (velocity.x < -25)
                    {
                        velocity = new Vector2(-25.1f, velocity.y);
                    }

                    if (velocity.y > 25)
                    {
                        velocity = new Vector2(velocity.x, 25.1f);
                    }
                    else if (velocity.y < -25)
                    {
                        velocity = new Vector2(velocity.x, -25.1f);
                    }

                    // Check that applying more velocity in the current direction won't bring the total velocity over the maximum of 25
                    float prevX = velocity.x;
                    float prevY = velocity.y;
                    addVector = new Vector2(0, 0);
                    
                    // If in the last frame the velocity was over 25
                    if (prevX > 25 || prevX < -25)
                    {
                        // Check that applying the forward vector X to the velocity X won't make it bigger than 25 or smaller than -25
                        if (prevX + this.GetComponent<Shape>().forwardVector.x > 25 || prevX + this.GetComponent<Shape>().forwardVector.x < -25)
                        {
                            // If applying X will make the ship too fast
                            //Check Y
                            if (prevY > 25 || prevY < -25)
                            {
                                //If Y is out of bounds check if adding the vel will make it further out of bounds
                                if (prevY + this.GetComponent<Shape>().forwardVector.y > 25 || prevY + this.GetComponent<Shape>().forwardVector.y < -25)
                                {
                                    // If applying Y too will make the ship too fast then neither can be applied
                                }
                                else
                                {
                                    // Y will not be out of bounds, apply Y;
                                    addVector = new Vector2(0, this.GetComponent<Shape>().forwardVector.y);
                                }
                            }
                            else
                            {
                                // if Y is less than 25 apply Y
                                addVector = new Vector2(0, this.GetComponent<Shape>().forwardVector.y);
                            }
                        }
                        else
                        {
                            // If applying X will not make the ship too fast
                            //Check Y
                            if (prevY > 25 || prevY < -25)
                            {
                                //If Y is out of bounds check if adding the vel will make it further out of bounds
                                if (prevY + this.GetComponent<Shape>().forwardVector.y > 25 || prevY + this.GetComponent<Shape>().forwardVector.y < -25)
                                {
                                    // if Y will be out of bounds apply X
                                    addVector = new Vector2(this.GetComponent<Shape>().forwardVector.x, 0);
                                }
                                else
                                {
                                    // Y will not be out of bounds, apply Y and X;
                                    addVector = new Vector2(this.GetComponent<Shape>().forwardVector.x, this.GetComponent<Shape>().forwardVector.y);
                                }
                            }
                            else
                            {
                                // if Y is less than 25 apply X and Y
                                addVector = new Vector2(this.GetComponent<Shape>().forwardVector.x, this.GetComponent<Shape>().forwardVector.y);
                            }                            
                        }
                    }
                    else
                    {
                        // If X isnt out of bounds

                        //Check Y
                        if (prevY > 25 || prevY < -25)
                        {
                            //If Y is out of bounds check if adding the vel will make it further out of bounds
                            if (prevY + this.GetComponent<Shape>().forwardVector.y >= 25 || prevY + this.GetComponent<Shape>().forwardVector.y <= -25)
                            {
                                // if Y will be out of bounds apply X
                                addVector = new Vector2(this.GetComponent<Shape>().forwardVector.x, 0);
                            }
                            else
                            {
                                // Y will not be out of bounds, apply Y and X;
                                addVector = new Vector2(this.GetComponent<Shape>().forwardVector.x, this.GetComponent<Shape>().forwardVector.y);
                            }
                        }
                        else
                        {
                            // if Y is less than 25 apply X and Y
                            addVector = new Vector2(this.GetComponent<Shape>().forwardVector.x, this.GetComponent<Shape>().forwardVector.y);
                        }

                    }
                    
                    // Actually apply the thrust vector. If no vector can be applied the vector will be 0
                    velocity += new Vector3(addVector.x * 50, addVector.y * 50, 0) * Time.deltaTime;
                    this.GetComponent<Shape>().thrusting = true;
                }
                else
                {
                    this.GetComponent<Shape>().thrusting = false;
                }

                // Turn right
                if (Input.GetKey("right"))
                {
                    angle += 5f * Time.deltaTime;
                }

                // Turn left
                if (Input.GetKey("left"))
                {
                    angle -= 5f * Time.deltaTime;
                }
            }
        }



        // Hold mode stops the shape from looping around at the edges of the screen. This is used before the game starts to hold the player outside the play area.
        if (!hold)
        {
            if (this.transform.position.x > 47)
            {
                this.transform.position = new Vector3((this.transform.position.x - 1) * -1, this.transform.position.y, 0);
            }
            if (this.transform.position.y > 27)
            {
                this.transform.position = new Vector3(this.transform.position.x, (this.transform.position.y - 1) * -1, 0);
            }
            if (this.transform.position.x < -47)
            {
                this.transform.position = new Vector3((this.transform.position.x + 1) * -1, this.transform.position.y, 0);
            }
            if (this.transform.position.y < -27)
            {
                this.transform.position = new Vector3(this.transform.position.x, (this.transform.position.y + 1) * -1, 0);
            }
        }


        GizmosDraw();
    }

    UnityEngine.Vector3 Vec2ToVec3(UnityEngine.Vector2 vec)
    {
        return new UnityEngine.Vector3(vec.x, vec.y, 0);
    }


    Vector2 InvertVector(Vector2 vec)
    {
        return new Vector2(-vec.x, -vec.y);
    }

    Vector2 ApplyMatrix(float[,] mat, Vector2 vec)
    {
        float x = vec.x * mat[0, 0] + vec.y * mat[1, 0];
        float y = vec.x * mat[0, 1] + vec.y * mat[1, 1];
        return new Vector2(x, y);
    }

    private void OnDrawGizmos()
    {
        if (gz.drawFutureVector)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.transform.position, this.transform.position + (velocity * Time.deltaTime) * 180);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(this.transform.position, this.transform.position + (velocity * Time.deltaTime) * 120);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(this.transform.position, this.transform.position + (velocity * Time.deltaTime) * 60);
        }
    }

    // Sets up gizmo objects to draw the same information as gizmos, however as it uses a line renderer it works in the build
    void GizmosDraw(){
        if(gz.drawFutureVector){
            if(!debugSpawned){
                
                debugSpawn = Instantiate(debugObj,this.transform.position,this.transform.rotation);
                debugSpawn.transform.SetParent(this.transform);
                dblr = debugSpawn.GetComponent<LineRenderer>();
                List<Vector3> debugVerts = new List<Vector3>();
                debugVerts.Add(this.transform.position);
                debugVerts.Add(this.transform.position + (velocity * Time.deltaTime) * 180);
                dblr.positionCount = 2;
                dblr.SetPositions(ListToArrayVec3(debugVerts));
                localDebug = ListToArrayVec3(debugVerts);
                dblr.loop = false;
                debugSpawned = true;

                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f),new GradientColorKey(Color.yellow, 0.5f),new GradientColorKey(Color.red, 1.0f)},
                    new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f),new GradientAlphaKey(1f, 0.5f),new GradientAlphaKey(1f, 1.0f)}
                );
                dblr.colorGradient = gradient;
            }
            else{
                debugSpawn.transform.position = this.transform.position;
                
                Vector3[] vecL = new Vector3[dblr.positionCount];
                dblr.GetPositions(vecL);
                dblr.SetPosition(0,this.transform.position);
                dblr.SetPosition(1,this.transform.position + (velocity * Time.deltaTime) * 180);
            }
        }
        

        if(!gz.drawFutureVector && debugSpawned){
            Destroy(debugSpawn);
            debugSpawned = false;
        }
    }

    private UnityEngine.Vector3[] ListToArrayVec3(List<UnityEngine.Vector3> l)
    {
        UnityEngine.Vector3[] temp = new UnityEngine.Vector3[l.Count];
        for (int i = 0; i < l.Count; i++)
        {
            temp[i] = l[i];
        }
        return temp;
    }
}




