using System.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SAT : MonoBehaviour
{
    public GameObject gameController;
    public Dynamic playerDynamic;
    public GameObject[] totalObjects;
    public GameObject[] objects;
    public GameObject[] prevObjects;
    public bool prevObjectsMatch = false;
    public List<int> pairs;
    float[] lastMins;
    float min;
    Vector3 minAxis = Vector3.zero;
    float lastMin;
    Vector3[] aNormal;
    Vector3[] bNormal;
    Vector3[] aAxis;
    Vector3[] bAxis;
    Vector3[] axis;
    List<Vector3> penAxes;
    List<float> penAxesDistances;
    Vector3[] aVerts;
    Vector3[] bVerts;
    int playerId = -1;
    int enemyId = -1;
    public float[,] minPairs;

    [Header("Gizmo Settings")]
    GizmoController gz;

    // Start is called before the first frame update
    void Start()
    {
        //Get the gizmo controller (Doesn't work, yet code is correct).
        gz = GameObject.FindWithTag("GameController").GetComponent<GizmoController>();
    }

    void Awake()
    {
        // List of collision objects (asteroids) in the scene.
        GameObject[] grabObjs = GameObject.FindGameObjectsWithTag("Collision Object");

        // Set length of total objects to the number of asteroids plus the enemy and player ships.
        totalObjects = new GameObject[grabObjs.Length + 2];

        // Adds asteroids to the total object list
        for (int i = 0; i < grabObjs.Length; i++)
        {
            totalObjects[i] = grabObjs[i];
        }

        // Gets the Player and Enemy objects and sets their shape ID to be the highest in the scene;
        totalObjects[grabObjs.Length] = GameObject.FindGameObjectWithTag("Player");
        totalObjects[grabObjs.Length + 1] = GameObject.FindGameObjectWithTag("Enemy");
        playerId = totalObjects[grabObjs.Length].GetComponent<Shape>().shapeID = grabObjs.Length;
        enemyId = totalObjects[grabObjs.Length + 1].GetComponent<Shape>().shapeID = grabObjs.Length + 1;

        // Sets size of pairs list 
        minPairs = new float[grabObjs.Length + 2, grabObjs.Length + 2];



    }

    // Update is called once per frame
    void Update()
    {
        // Will run if there are no asteroids detected. The player and enemy and two constrained asteroids are already in the list.
        if (totalObjects.Length <= 4)
        {
            // Same as previous, gets asteroids in scene and sets player and enemy shape IDs.
            GameObject[] grabObjs = GameObject.FindGameObjectsWithTag("Collision Object");
            totalObjects = new GameObject[grabObjs.Length + 2];
            for (int i = 0; i < grabObjs.Length; i++)
            {
                totalObjects[i] = grabObjs[i];
            }
            totalObjects[grabObjs.Length] = GameObject.FindGameObjectWithTag("Player");
            totalObjects[grabObjs.Length + 1] = GameObject.FindGameObjectWithTag("Enemy");
            playerId = totalObjects[grabObjs.Length].GetComponent<Shape>().shapeID = grabObjs.Length;
            enemyId = totalObjects[grabObjs.Length + 1].GetComponent<Shape>().shapeID = grabObjs.Length + 1;
            minPairs = new float[grabObjs.Length + 4, grabObjs.Length + 4];

            // Generates pairs of objects. Pairs are used to check every object with each other for collision.
            for (int i = 0; i < objects.Length; i++)
            {
                for (int k = 0; k < objects.Length; k++)
                {
                    if (k > i)
                    {
                        pairs.Add(i);
                        pairs.Add(k);
                    }
                }
            }
            Debug.Log(pairs.Count);
        }

        // List of objects for broad phase collision detection.
        List<GameObject> objs = new List<GameObject>();
        objs.Add(GameObject.FindGameObjectWithTag("Player"));
        objs.Add(GameObject.FindGameObjectWithTag("Enemy"));
        // Set all objects in the scene to not broad phase colliding.
        for (int i = 0; i < totalObjects.Length; i++)
        {
            totalObjects[i].GetComponent<Shape>().broadPhase = false;
        }

        // Check every shape for broad phase collision, if colliding mark the shape and add it to the objects list used for collision.
        for (int i = 0; i < totalObjects.Length; i++)
        {
            for (int k = 0; k < totalObjects.Length; k++)
            {
                if (i < k)
                {
                    float aRad = totalObjects[i].GetComponent<Shape>().broadPhaseRadius;
                    float bRad = totalObjects[k].GetComponent<Shape>().broadPhaseRadius;
                    Vector3 vec = totalObjects[k].transform.position - totalObjects[i].transform.position;
                    float mag = (float)Math.Sqrt((vec.x * vec.x) + (vec.y * vec.y) + (vec.z * vec.z));

                    if (mag < aRad + bRad)
                    {
                        objs.Add(totalObjects[i]);
                        objs.Add(totalObjects[k]);
                        totalObjects[i].GetComponent<Shape>().broadPhase = totalObjects[k].GetComponent<Shape>().broadPhase = true;
                    }
                }
            }
        }
        // Set SAT collison objects to detected broadphase objects.
        objects = new GameObject[objs.Count];
        for (int i = 0; i < objs.Count; i++)
        {
            objects[i] = objs[i];
        }
        // If first frame set previous objects list
        if (prevObjects.Length == 0)
        {
            prevObjects = objects;
        }
        // Runs if the detected objects are different from previous. This stops the pairs from being generated every frame, only when a change in the broadphase objects occur.
        else if (objects != prevObjects)
        {
            pairs.Clear();
            for (int i = 0; i < objects.Length; i++)
            {
                for (int k = 0; k < objects.Length; k++)
                {
                    if (k > i)
                    {
                        pairs.Add(i);
                        pairs.Add(k);
                    }
                }
            }

            prevObjects = objects;
        }
        SATCollision();
    }

    void SATCollision()
    {
        // Makes sure it has at least two objects to check with
        if (pairs.Count >= 2)
        {
            // Steps through the pairs two at a time
            for (int i = 0; i < pairs.Count; i += 2)
            {
                // Checks if the two objects are colliding, then resolves the collision.
                if (Colliding(objects[pairs[i]], objects[pairs[i + 1]]))
                {
                    //Gets the shape ID of the two objects, sets them to colliding and then checks whether the shapes are the player and the enemy. 
                    // If they are, set the player to dead as touching the enemy ship kills the player
                    int aId = objects[pairs[i]].GetComponent<Shape>().shapeID;
                    int bId = objects[pairs[i + 1]].GetComponent<Shape>().shapeID;
                    objects[pairs[i]].GetComponent<Shape>().colliding = objects[pairs[i + 1]].GetComponent<Shape>().colliding = true;
                    ResolvePhysics(objects[pairs[i]], objects[pairs[i + 1]], minAxis, i);
                    if (objects[pairs[i]].GetComponent<Shape>().shapeID == playerId && objects[pairs[i + 1]].GetComponent<Shape>().shapeID == enemyId || objects[pairs[i + 1]].GetComponent<Shape>().shapeID == playerId && objects[pairs[i]].GetComponent<Shape>().shapeID == enemyId)
                    {
                        GameObject.FindGameObjectWithTag("Player").GetComponent<Dynamic>().isDead = true;
                    }

                    // Using the shape IDs, store the min distance for the shapes. This is used to stop double application of velocity, as when the shapes are moving apart they would collide again and double the velocity.
                    // This caused them to ping away at immense speed.
                    minPairs[aId, bId] = min;
                    minPairs[bId, aId] = min;
                }
                else
                {
                    // Resets the min values and sets the shapes to not colliding.
                    int aId = objects[pairs[i]].GetComponent<Shape>().shapeID;
                    int bId = objects[pairs[i + 1]].GetComponent<Shape>().shapeID;

                    objects[pairs[i]].GetComponent<Shape>().colliding = objects[pairs[i + 1]].GetComponent<Shape>().colliding = false;

                    minPairs[aId, bId] = float.MinValue;
                    minPairs[bId, aId] = float.MinValue;
                }


            }
        }
    }

    bool Colliding(GameObject objA, GameObject objB)
    {


        min = 0;
        minAxis = Vector3.zero;

        // This gets the axes for the shapes and the normals. These are then used to project the shapes for the collision detection.
        aNormal = objA.GetComponent<Shape>().GetNormals();
        bNormal = objB.GetComponent<Shape>().GetNormals();

        aAxis = objA.GetComponent<Shape>().GetAxes();
        bAxis = objB.GetComponent<Shape>().GetAxes();

        axis = new Vector3[aNormal.Length + bNormal.Length];

        // Adds the normals and the axes to the list of total axes. The shape side vectors are used as well as the normals to give more accuracy.
        for (int i = 0; i < aNormal.Length; i++)
        {
            axis[i] = aNormal[i];
        }
        for (int i = 0; i < bNormal.Length; i++)
        {
            axis[i + aNormal.Length] = bNormal[i];
        }

        // Get shape verts
        aVerts = objA.GetComponent<Shape>().GetVertices();
        bVerts = objB.GetComponent<Shape>().GetVertices();
        
        // List of each axis and penetration distance.
        penAxes = new List<Vector3>();
        penAxesDistances = new List<float>();

        bool colliding = false;

        // Project the shapes onto the axes and return whether colliding.
        // Both shapes are projected to ensure collision accuracy.
        if (axis.Length != 0)
        {
            if (Project(objA, objB, axis))
            {
                colliding = true;
            }
            else if (Project(objB, objA, axis))
            {
                colliding = true;
            }
        }
        return colliding;
    }

    bool Project(GameObject objA, GameObject objB, Vector3[] axis)
    {
        // Set min to max value possible
        min = float.PositiveInfinity;


        // Do SAT for each axis.
        for (int i = 0; i < axis.Length; i++)
        {
            float aMin = float.MaxValue;
            float aMax = float.MinValue;
            float bMin = float.MaxValue;
            float bMax = float.MinValue;

            Vector3 ax = axis[i];

            for (int k = 0; k < aVerts.Length; k++)
            {
                // Get projection value for each vertex in shape, apply to max and min values.
                float diff = Vector3.Dot(aVerts[k], ax);

                if (diff < aMin)
                {
                    aMin = diff;
                }
                if (diff > aMax)
                {
                    aMax = diff;
                }
            }

            for (int k = 0; k < bVerts.Length; k++)
            {
                // Get projection value for each vertex in shape, apply to max and min values.
                float diff = Vector3.Dot(bVerts[k], ax);
                if (diff < bMin)
                {
                    bMin = diff;
                }
                if (diff > bMax)
                {
                    bMax = diff;
                }
            }

            // Gets the overlap distance of the two shapes on the specific axis
            float o = ResolveOverlap(aMin, aMax, bMin, bMax);

            if (o < min)
            {
                min = o;
                minAxis = ax;

                penAxes.Add(ax);
                penAxesDistances.Add(o);
            }

            if (o <= 0)
            {
                //Debug.Log("Quitting");
                return false;
            }
        }
        return true;
    }

    float ResolveOverlap(float aMin, float aMax, float bMin, float bMax)
    {
        // Checks whether the min and max values overlap. If not, return 0;
        if (aMin < bMin)
        {
            if (aMax < bMin)
            {
                return 0f;
            }

            return aMax - bMin;
        }

        if (bMax < aMin)
        {
            return 0f;
        }

        return bMax - aMin;
    }

    // Resolves physics between the two objects
    int ResolvePhysics(GameObject objA, GameObject objB, Vector3 collisionNormal, int index)
    {
        // Get the IDs of the two objects for last collision information.
        int aId = objA.GetComponent<Shape>().shapeID;
        int bId = objB.GetComponent<Shape>().shapeID;

        // Get the relative velocity and the relative velocity along the collision normal.
        Vector3 relativeVelocity = objB.GetComponent<Dynamic>().velocity - objA.GetComponent<Dynamic>().velocity;
        float velAlongNormal = Vector3.Dot(relativeVelocity, collisionNormal);

        // Get the distance between the two objects center points before any positions are updated.
        Vector2 vec = objA.transform.position - objB.transform.position;
        float mag = (float)Mathf.Sqrt((vec.x * vec.x) + (vec.y * vec.y));

        // Update the positions of the objects by applying using the minimum axis and the minimum distance.
        objA.transform.position += minAxis * (min / 2);
        objB.transform.position -= minAxis * (min / 2);

        // Get the distance between the two objects again after positions have been updated.
        Vector2 vec2 = objA.transform.position - objB.transform.position;
        float mag2 = (float)Mathf.Sqrt((vec2.x * vec2.x) + (vec2.y * vec2.y));

        // If the two objects are now closer, apply the seperation again to the opposite objects. 
        // This should really apply the full min amount, however when doing this the objects seemed to bounce quite violently. Applying half seems more natural.
        if (mag > mag2)
        {
            objA.transform.position -= minAxis * min / 2;
            objB.transform.position += minAxis * min / 2;
        }

        // If the two objects are moving apart and aren't touching, stop resolving.
        if (velAlongNormal > 0 && min < 0)
        {
            return -1;
        }
        // If in the previous frame the two objects were colliding stop resolving.
        if (minPairs[aId, bId] > 0)
        {
            return -1;
        }
        // If in the previous frame the two objects were colliding stop resolving.
        if (minPairs[bId, aId] > 0)
        {
            return -1;
        }
        // If this is the first collision frame, set the value to the current minimum. This is then used in the next frame to check whether two objects have been colliding for more than one frame.
        // This was used to avoid extreme accelleration when colliding, as objects would intersect for multiple frames, applying the same impulse each frame until they no longer intersected.
        if (minPairs[aId, bId] == float.MinValue)
        {
            minPairs[aId, bId] = min;
            minPairs[bId, aId] = min;
        }
        if (minPairs[bId, aId] == float.MinValue)
        {
            minPairs[aId, bId] = min;
            minPairs[bId, aId] = min;
        }


        //Score updating - Done here to stop updates happening when objects are not actually touching
        if (aId == playerId || bId == playerId)
        {
            if (gameController.GetComponent<GameController>().score > 0 && !playerDynamic.isDead)
            {
                gameController.GetComponent<GameController>().score -= 1;
            }
        }

        // Every object has the same restitution in this simulation, therefore selecting the lowest is not needed.
        float restitution = 0.8f;

        // When using formula provided in slides the objects would accelerate with every collision. Removing the addition to 1 fixed this.
        double impulseScale = -(restitution) * velAlongNormal;
        impulseScale = impulseScale / (1 / objA.GetComponent<Dynamic>().mass) + (1 / objB.GetComponent<Dynamic>().mass);
        Vector3 impulseTotal = (float)impulseScale * collisionNormal;

        objA.GetComponent<Dynamic>().velocity -= 1 / objA.GetComponent<Dynamic>().mass * impulseTotal;
        objB.GetComponent<Dynamic>().velocity += 1 / objB.GetComponent<Dynamic>().mass * impulseTotal;
        return 0;
    }
}
