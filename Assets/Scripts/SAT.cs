using System.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SAT : MonoBehaviour
{
    public GameObject gameController;
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

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] grabObjs = GameObject.FindGameObjectsWithTag("Collision Object");
        totalObjects = new GameObject[grabObjs.Length + 2];
        for (int i = 0; i < grabObjs.Length; i++)
        {
            totalObjects[i] = grabObjs[i];
        }
        totalObjects[grabObjs.Length] = GameObject.FindGameObjectWithTag("Player");
        totalObjects[grabObjs.Length+1] = GameObject.FindGameObjectWithTag("Enemy");
        playerId = totalObjects[grabObjs.Length].GetComponent<Shape>().shapeID = grabObjs.Length;
        enemyId = totalObjects[grabObjs.Length+1].GetComponent<Shape>().shapeID = grabObjs.Length+1;
        minPairs = new float[grabObjs.Length+2,grabObjs.Length+2];
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
        // for (int i = 0; i < pairs.Count; i++)
        // {
        //     lastMins = new float[pairs.Count];
        // }
    }

    // Update is called once per frame
    void Update()
    {
        List<GameObject> objs = new List<GameObject>();
        objs.Add(GameObject.FindGameObjectWithTag("Player"));
        objs.Add(GameObject.FindGameObjectWithTag("Enemy"));
        for (int i = 0; i < totalObjects.Length; i++)
        {
            totalObjects[i].GetComponent<Shape>().broadPhase = false;
        }

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
                        //Debug.Log(string.Format("Magnitude: {0} Rad1: {1} Rad2 {2} Collision: TRUE", mag, aRad, bRad));
                        objs.Add(totalObjects[i]);
                        objs.Add(totalObjects[k]);
                        totalObjects[i].GetComponent<Shape>().broadPhase = totalObjects[k].GetComponent<Shape>().broadPhase = true;
                    }
                    else
                    {
                        //Debug.Log(string.Format("Magnitude: {0} Rad1: {1} Rad2 {2} Collision: FALSE", mag, aRad, bRad));
                    }
                }
            }
        }
        objects = new GameObject[objs.Count];
        for (int i = 0; i < objs.Count; i++)
        {
            objects[i] = objs[i];
        }
        if (prevObjects.Length == 0)
        {
            prevObjects = objects;
        }
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
            //Debug.Log(pairs.Count);
            
            prevObjects = objects;
        }
        if (prevObjects == objects)
        {
            prevObjectsMatch = true;
        }
        else
        {
            prevObjectsMatch = false;
        }
        SATCollision();
        // for (int i = 0; i < totalObjects.Length; i++)
        // {
        //     totalObjects[i].GetComponent<Dynamic>().PhysicsUpdate();
        // }
    }

    void SATCollision()
    {
        if (pairs.Count >= 2)
        {
            for (int i = 0; i < pairs.Count; i++)
            {
                if (i % 2 == 0)
                {
                    //Debug.Log("even: " + i);
                    if (Colliding(objects[pairs[i]], objects[pairs[i + 1]]))
                    {
                        int aId = objects[pairs[i]].GetComponent<Shape>().shapeID;
                        int bId = objects[pairs[i+1]].GetComponent<Shape>().shapeID;
                        //Debug.Log("colliding!");
                        //Debug.LogWarning(string.Format("A ID: {0} B ID: {1}",aId,bId));
                        objects[pairs[i]].GetComponent<Shape>().colliding = objects[pairs[i + 1]].GetComponent<Shape>().colliding = true;
                        ResolvePhysics(objects[pairs[i]], objects[pairs[i + 1]], minAxis, i);
                        if(objects[pairs[i]].GetComponent<Shape>().shapeID == playerId && objects[pairs[i+1]].GetComponent<Shape>().shapeID == enemyId || objects[pairs[i+1]].GetComponent<Shape>().shapeID == playerId && objects[pairs[i]].GetComponent<Shape>().shapeID == enemyId ){
                            GameObject.FindGameObjectWithTag("Player").GetComponent<Dynamic>().isDead = true;
                        }
                        //lastMins[i] = min;
                        minPairs[aId,bId] = min;
                        minPairs[bId,aId] = min;
                        //objects[0].transform.position += (minAxis*min);                        
                    }
                    else
                    {
                        int aId = objects[pairs[i]].GetComponent<Shape>().shapeID;
                        int bId = objects[pairs[i+1]].GetComponent<Shape>().shapeID;
                        //Debug.LogWarning(string.Format("A ID: {0} B ID: {1}",aId,bId));
                        objects[pairs[i]].GetComponent<Shape>().colliding = objects[pairs[i + 1]].GetComponent<Shape>().colliding = false;
                        //lastMins[i] = float.MinValue;
                        minPairs[aId,bId] = float.MinValue;
                        minPairs[bId,aId] = float.MinValue;
                    }
                    //Debug.Log(minPairs);
                }
                else
                {
                    //Debug.Log("odd: " + i);
                }
            }
        }
    }

    bool Colliding(GameObject objA, GameObject objB)
    {
        min = 0;
        minAxis = Vector3.zero;

        aNormal = objA.GetComponent<Shape>().GetNormals();
        bNormal = objB.GetComponent<Shape>().GetNormals();

        aAxis = objA.GetComponent<Shape>().GetAxes();
        bAxis = objB.GetComponent<Shape>().GetAxes();


        axis = new Vector3[aNormal.Length + bNormal.Length];

        for (int i = 0; i < aNormal.Length; i++)
        {
            axis[i] = aNormal[i];
        }
        for (int i = 0; i < bNormal.Length; i++)
        {
            axis[i + aNormal.Length] = bNormal[i];
        }

        // for (int i = 0; i < aNormal.Length; i++)
        // {
        //     axis[i] = aNormal[i];
        // }
        // for (int i = 0; i < aAxis.Length; i++)
        // {
        //     axis[i + aNormal.Length] = aAxis[i];
        // }
        // for (int i = 0; i < bNormal.Length; i++)
        // {
        //     axis[i + aNormal.Length + aAxis.Length] = bNormal[i];
        // }
        // for (int i = 0; i < bAxis.Length; i++)
        // {
        //     axis[i + aNormal.Length + bNormal.Length + bNormal.Length] = bAxis[i];
        // }

        aVerts = objA.GetComponent<Shape>().GetVertices();
        bVerts = objB.GetComponent<Shape>().GetVertices();

        penAxes = new List<Vector3>();
        penAxesDistances = new List<float>();

        bool colliding = false;

        if (axis.Length != 0)
        {
            if (Project(objA, objB, axis))
            {
                //Debug.Log(string.Format("Min: {0}, MinAxis: ({1},{2}), Axis Length: {3}, aVerts: {4}, bVerts: {5}", min, minAxis.x, minAxis.y, axis.Length, aVerts.Length, bVerts.Length));
                colliding = true;
            }
            else if (Project(objB, objA, axis))
            {
                //Debug.Log(string.Format("Min: {0}, MinAxis: ({1},{2}), Axis Length: {3}, aVerts: {4}, bVerts: {5}", min, minAxis.x, minAxis.y, axis.Length, aVerts.Length, bVerts.Length));
                colliding = true;
            }
            //Debug.Log(string.Format("Min: {0}, MinAxis: ({1},{2}), Axis Length: {3}, aVerts: {4}, bVerts: {5}", min, minAxis.x, minAxis.y, axis.Length, aVerts.Length, bVerts.Length));
        }


        if (colliding)
        {
            //Debug.Log("Colliding!!!");

        }
        return colliding;
    }

    bool Project(GameObject objA, GameObject objB, Vector3[] axis)
    {

        min = float.PositiveInfinity;

        for (int i = 0; i < axis.Length; i++)
        {
            float aMin = float.MaxValue;
            float aMax = float.MinValue;
            float bMin = float.MaxValue;
            float bMax = float.MinValue;

            Vector3 ax = axis[i];

            for (int k = 0; k < aVerts.Length; k++)
            {
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

            float o = ResolveOverlap(aMin, aMax, bMin, bMax);

            if (o < min)
            {
                min = o;
                minAxis = ax;

                penAxes.Add(ax);
                penAxesDistances.Add(o);
            }
            //Debug.Log(string.Format("i: {5}, Axis Length: {6}, o: {4}, min:{7}, aMin: {0}, aMax: {1}, bMin: {2}, bMax: {3}", aMin, aMax, bMin, bMax, o,i,axis.Length,min));
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

    int ResolvePhysics(GameObject objA, GameObject objB, Vector3 collisionNormal, int index)
    {
        int aId = objA.GetComponent<Shape>().shapeID;
        int bId = objB.GetComponent<Shape>().shapeID;
        //Debug.Log(min + " : " + minAxis);
        Vector3 relativeVelocity = objB.GetComponent<Dynamic>().velocity - objA.GetComponent<Dynamic>().velocity;
        float velAlongNormal = Vector3.Dot(relativeVelocity, collisionNormal);
        // objA.transform.position += minAxis;
        // objB.transform.position -= minAxis;
        //Debug.Log(string.Format("RelativeVel: {0}, VelAlongNormal: {1}, MassA: {2}, MassB: {3}", relativeVelocity, velAlongNormal, objA.GetComponent<Dynamic>().mass, objB.GetComponent<Dynamic>().mass));
        if (velAlongNormal > 0 && min < 0)
        {
            return -1;
        }
        if (minPairs[aId,bId] > 0)
        {
            //Debug.LogWarning(string.Format("Double Collision, Dumping! aId: {0} bId: {1}",aId,bId));
            return -1;
        }
        if (minPairs[bId,aId] > 0)
        {
            //Debug.LogWarning(string.Format("Double Collision, Dumping! aId: {0} bId: {1}",aId,bId));
            return -1;
        }
        if (minPairs[aId,bId] == float.MinValue)
        {
            minPairs[aId,bId] = min;
            minPairs[bId,aId] = min;
        }
        if (minPairs[bId,aId] == float.MinValue)
        {
            minPairs[aId,bId] = min;
            minPairs[bId,aId] = min;
        }
        float restitution = 0.8f;
        double impulseScale = -(restitution) * velAlongNormal;
        impulseScale = impulseScale / (1 / objA.GetComponent<Dynamic>().mass) + (1 / objB.GetComponent<Dynamic>().mass);
        Vector3 impulseTotal = (float)impulseScale * collisionNormal;
        //Debug.Log(string.Format("RelativeVel: {0}, VelAlongNormal: {1}, ImpulseScale: {2}, impulseTotal:{3}", relativeVelocity, velAlongNormal, impulseScale, impulseTotal));
        objA.GetComponent<Dynamic>().velocity -= 1 / objA.GetComponent<Dynamic>().mass * impulseTotal;
        objB.GetComponent<Dynamic>().velocity += 1 / objB.GetComponent<Dynamic>().mass * impulseTotal;
        return 0;
    }
}
