using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Dynamic : MonoBehaviour
{
    public Vector3 ObjPos;
    public Vector3 velocity;

    [Range(100f, 1000f)]
    public float mass = 100f;
    Vector3 intermediateVelocity;

    [SerializeField]
    bool isShip = false;

    [SerializeField]
    float angle = 0f;

    // public List<List<float>> this.GetComponent<Shape().rotmat;


    // Start is called before the first frame update
    void Start()
    {



        ObjPos = this.transform.position;
        if (!isShip)
        {
            velocity = new Vector3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5), 0);
            mass = UnityEngine.Random.Range(100f, 150f);
        }
    }

    // Update is called once per frame
    void Update(){
        if (velocity.x != 0 || velocity.y != 0)
        {
            this.transform.position += velocity * Time.deltaTime;
        }

        if (isShip)
        {
            this.GetComponent<Shape>().rotMat[0, 0] = Mathf.Cos(angle);
            this.GetComponent<Shape>().rotMat[0, 1] = -Mathf.Sin(angle);
            this.GetComponent<Shape>().rotMat[1, 0] = Mathf.Sin(angle);
            this.GetComponent<Shape>().rotMat[1, 1] = Mathf.Cos(angle);


            if (Input.GetKey("space"))
            {
                if(velocity.x > 25){
                    velocity = new Vector2(25.1f,velocity.y);
                }
                else if(velocity.x < -25){
                    velocity = new Vector2(-25.1f,velocity.y);
                }

                if(velocity.y > 25){
                    velocity = new Vector2(velocity.x,25.1f);
                }
                else if(velocity.y < -25){
                    velocity = new Vector2(velocity.x,-25.1f);
                }


                float prevX = velocity.x;
                float prevY = velocity.y;
                Vector2 addVector = new Vector2(0, 0);
                if (prevX > 25 || prevX < -25)
                {
                    if (prevX + this.GetComponent<Shape>().forwardVector.x > 25 || prevX + this.GetComponent<Shape>().forwardVector.x < -25)
                    {
                        // If X is too fast

                        //Check Y
                        if (prevY > 25 || prevY < -25)
                        {
                            //If Y is out of bounds check if adding the vel will make it further out of bounds
                            if (prevY + this.GetComponent<Shape>().forwardVector.y > 25 || prevY + this.GetComponent<Shape>().forwardVector.y < -25)
                            {
                                // if Y will be out of bounds neither can be applied
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
                        // If X wont increase too fast

                        //Check Y
                        if (prevY > 25 || prevY < -25)
                        {
                            //If Y is out of bounds check if adding the vel will make it further out of bounds
                            if (prevY + this.GetComponent<Shape>().forwardVector.y > 25 || prevY + this.GetComponent<Shape>().forwardVector.y < -25)
                            {
                                // if Y will be out of bounds apply X
                                addVector = new Vector2(this.GetComponent<Shape>().forwardVector.x,0);
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

                        // If Y will not get faster then apply both
                        // Else only apply X
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
                                addVector = new Vector2(this.GetComponent<Shape>().forwardVector.x,0);
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
                velocity += new Vector3(addVector.x*50, addVector.y*50, 0) * Time.deltaTime;


            }
            if (Input.GetKey("right"))
            {
                angle += 5f * Time.deltaTime;
            }
            if (Input.GetKey("left"))
            {
                angle -= 5f  * Time.deltaTime;
            }
        }

        


        if (this.transform.position.x > 47)
        {
            this.transform.position = new Vector3((this.transform.position.x -1) * -1, this.transform.position.y, 0);
        }
        if (this.transform.position.y > 27)
        {
            this.transform.position = new Vector3(this.transform.position.x, (this.transform.position.y -1) * -1, 0);
        }
        if (this.transform.position.x < -47)
        {
            this.transform.position = new Vector3((this.transform.position.x +1) * -1, this.transform.position.y, 0);
        }
        if (this.transform.position.y < -27)
        {
            this.transform.position = new Vector3(this.transform.position.x, (this.transform.position.y +1) * -1, 0);
        }


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
}



