using System.Security.Cryptography;
using System.Collections.Specialized;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    [Header("Shape Details")]
    [SerializeField]
    bool isDemo = false;

    [SerializeField]
    int mode = 0;
    [SerializeField]
    public List<Vector2> localVertices;
    public List<Vector2> vertices;

    [SerializeField]
    public LineRenderer lr;

    [SerializeField]
    int points = 4;
    public int shapeID = -1;

    [Header("Physics")]
    public float[,] rotMat;
    public float rotAngle = 0f;
    public bool colliding = false;
    public bool broadPhase = false;
    [SerializeField]
    float rad = 2;
    float maxMag = float.MinValue;
    public float broadPhaseRadius;

    [Header("Asteroid Settings")]
    public float rotationValue = 0;

    [Header("Ship Settings")]
    [SerializeField]
    public bool isShip = false;
    public bool isEnemy = false;
    public Vector3 forwardVector;
    public bool thrusting = false;
    public GameObject thruster;
    Vector3[] localthrustVerts = { new Vector3(0.4f, -0.8f, 0), new Vector3(0.0f, -1.5f, 0), new Vector3(-0.4f, -0.8f, 0) };
    Vector3[] thrustVerts = new Vector3[3];
    Dynamic d;

    [Header("GizmoSettings")]
    public GizmoController gz;
    public GameObject debugObj;
    bool debugSpawned = false;
    GameObject debugSpawn;
    LineRenderer dblr;
    Vector3[] localDebug;

    // Start is called before the first frame update
    void Start()
    {
        // If the shape is not a demo (menu) shape, setup lists and setup the rotation matrix.
        if (!isDemo)
        {
            d = this.GetComponent<Dynamic>();
            rotMat = new float[2, 2];
            rotMat[0, 0] = Mathf.Cos(rotAngle);
            rotMat[0, 1] = -Mathf.Sin(rotAngle);
            rotMat[1, 0] = Mathf.Sin(rotAngle);
            rotMat[1, 1] = Mathf.Cos(rotAngle);
        }

        const float radians360 = 6.283185f;

        lr = this.GetComponent<LineRenderer>();

        // Generate random asteroid shape using a random radius and the number of points.
        rad = UnityEngine.Random.Range(0.5f, 2.0f);
        float perStep = radians360 / points;
        float angle = 0;
        for (int i = 0; i < points; i++)
        {
            Vector2 pointVector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Make point distance from center random
            float mag = UnityEngine.Random.Range(rad * 0.8f, rad * 1.2f);
            if (mag > maxMag)
            {
                maxMag = mag;
            }
            localVertices.Add((pointVector * mag));
            angle += perStep;
        }

        // Broadphase radius used for broad phase collision is slightly larger than the maximum vertex
        broadPhaseRadius = maxMag * 1.1f;

        // If the shape is the ship, replace the vertices with the ship vertices.
        // Does not do random generation
        if (isShip)
        {
            broadPhaseRadius = 1.2f;
            if (!isDemo)
            {
                this.transform.position = new Vector3(0, 0, 0);
            }

            localVertices.Clear();
            localVertices.Add(new Vector2(0, 1.0f));
            localVertices.Add(new Vector2(0.8f, -1.0f));
            localVertices.Add(new Vector2(0, -0.6f));
            localVertices.Add(new Vector2(-0.8f, -1.0f));
            forwardVector = Vec2ToVec3(localVertices[0]) - this.transform.position;
            float forwardMag = (float)Math.Sqrt((forwardVector.x * forwardVector.x) + (forwardVector.y * forwardVector.y) + (forwardVector.z * forwardVector.z));
            forwardVector = new Vector3(forwardVector.x / forwardMag, forwardVector.y / forwardMag, 0);
            points = 4;
        }
        // Same as the player, if enemy change points to enemy ship shape.
        else if (isEnemy)
        {
            broadPhaseRadius = 1.2f;
            if (!isDemo)
            {
                // If not demo ship, move to start position
                this.transform.position = new Vector3(-44, 24, 0);
            }
            else
            {
                // Depending on the mode change the line colour of the enemy ship.
                if (mode == 0)
                {
                    Gradient gradient = new Gradient();
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.cyan, 0.5f), new GradientColorKey(Color.white, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f), new GradientAlphaKey(1f, 1.0f) }
                    );
                    lr.colorGradient = gradient;
                }
                else if (mode == 1)
                {
                    Gradient gradient = new Gradient();
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.yellow, 0.5f), new GradientColorKey(Color.white, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f), new GradientAlphaKey(1f, 1.0f) }
                    );
                    lr.colorGradient = gradient;
                }
                else
                {
                    Gradient gradient = new Gradient();
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.red, 0.5f), new GradientColorKey(Color.white, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f), new GradientAlphaKey(1f, 1.0f) }
                    );
                    lr.colorGradient = gradient;
                }
            }

            localVertices.Clear();
            localVertices.Add(new Vector2(0, 1.0f));
            localVertices.Add(new Vector2(1, 0f));
            localVertices.Add(new Vector2(0, -1f));
            localVertices.Add(new Vector2(-1f, -0f));
            forwardVector = Vec2ToVec3(localVertices[0]) - this.transform.position;
            float forwardMag = (float)Math.Sqrt((forwardVector.x * forwardVector.x) + (forwardVector.y * forwardVector.y) + (forwardVector.z * forwardVector.z));
            forwardVector = new Vector3(forwardVector.x / forwardMag, forwardVector.y / forwardMag, 0);
            points = 4;
        }
        // If not enemy or player, set random rotation speed
        else
        {
            rotationValue = UnityEngine.Random.Range(-1f, 1f);
        }

        // If the object is a demo object, set draw vertices and setup line renderer.
        if (isDemo)
        {
            for (int i = 0; i < localVertices.Count; i++)
            {
                vertices.Add(Vec2ToVec3List(localVertices)[i] + this.transform.position);
            }
            lr.positionCount = vertices.Count + 1;
            lr.SetPositions(Vec2ToVec3List(vertices));
            lr.SetPosition(vertices.Count, vertices[0]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isShip && !isDemo)
        {
            if (thrusting)
            {
                // Draw thruster when thrusting.
                thruster.GetComponent<LineRenderer>().SetPositions(thrustVerts);
            }
            else
            {
                // When not thrusting, draw off screen.
                thruster.GetComponent<LineRenderer>().SetPosition(0, new Vector3(100, 100, 0));
                thruster.GetComponent<LineRenderer>().SetPosition(1, new Vector3(100, 100, 0));
                thruster.GetComponent<LineRenderer>().SetPosition(2, new Vector3(100, 100, 0));
            }
        }

        // Only update physics if not demo.
        if (!isDemo)
        {
            PhysicsUpdate();
            GizmoBuild();
        }
    }

    void PhysicsUpdate()
    {
        // If the shape is not a demo (menu) shape, update lists and rotation matrix.
        if (!isShip)
        {
            rotAngle += rotationValue * Time.deltaTime;
            rotMat[0, 0] = Mathf.Cos(rotAngle);
            rotMat[0, 1] = -Mathf.Sin(rotAngle);
            rotMat[1, 0] = Mathf.Sin(rotAngle);
            rotMat[1, 1] = Mathf.Cos(rotAngle);
        }

        vertices.Clear();
        Vector2 pos = new Vector2(this.transform.position.x, this.transform.position.y);

        // Apply the rotation matrix to the shape vertices and the thruster vertices.
        for (int i = 0; i < localVertices.Count; i++)
        {
            Vector2 tempVert = localVertices[i];

            float x = localVertices[i].x * rotMat[0, 0] + localVertices[i].y * rotMat[1, 0];
            float y = localVertices[i].x * rotMat[0, 1] + localVertices[i].y * rotMat[1, 1];
            tempVert = new Vector2(x, y);

            vertices.Add(tempVert + pos);
        }
        for (int i = 0; i < localthrustVerts.Length; i++)
        {
            Vector2 tempVert = localthrustVerts[i];

            float x = localthrustVerts[i].x * rotMat[0, 0] + localthrustVerts[i].y * rotMat[1, 0];
            float y = localthrustVerts[i].x * rotMat[0, 1] + localthrustVerts[i].y * rotMat[1, 1];
            tempVert = new Vector2(x, y);

            thrustVerts[i] = (tempVert + pos);
        }

        // Get forward vector for ship and normalise it.
        forwardVector = Vec2ToVec3(vertices[0]) - this.transform.position;
        float forwardMag = (float)Math.Sqrt((forwardVector.x * forwardVector.x) + (forwardVector.y * forwardVector.y) + (forwardVector.z * forwardVector.z));
        forwardVector = new Vector3(forwardVector.x / forwardMag, forwardVector.y / forwardMag, 0);

        // Setup line renderer
        lr.positionCount = vertices.Count + 1;
        lr.SetPositions(Vec2ToVec3List(vertices));
        lr.SetPosition(vertices.Count, vertices[0]);
    }

    public Vector3[] GetNormals()
    {
        Vector3[] faces = new Vector3[vertices.Count];
        Vector3[] normals = new Vector3[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            if (i == vertices.Count - 1)
            {
                faces[i] = vertices[0] - vertices[i];
            }
            else
            {
                faces[i] = vertices[i + 1] - vertices[i];
            }
        }

        for (int i = 0; i < faces.Length; i++)
        {
            Vector3 normal = new Vector3(faces[i].y * -1, faces[i].x, 0);
            float mag = (float)Math.Sqrt((normal.x * normal.x) + (normal.y * normal.y) + (normal.z * normal.z));
            normals[i] = new Vector3(normal.x / mag, normal.y / mag, 0);
        }

        return normals;
    }

    public Vector3[] GetAxes()
    {
        Vector3[] faces = new Vector3[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            if (i == vertices.Count - 1)
            {
                faces[i] = vertices[0] - vertices[i];
            }
            else
            {
                faces[i] = vertices[i + 1] - vertices[i];
            }
        }

        for (int i = 0; i < faces.Length; i++)
        {
            float mag = (float)Math.Sqrt((faces[i].x * faces[i].x) + (faces[i].y * faces[i].y) + (faces[i].z * faces[i].z));
            faces[i] = new Vector3(faces[i].x / mag, faces[i].y / mag, 0);
        }
        return faces;
    }

    public Vector3[] GetVertices()
    {
        return Vec2ToVec3List(vertices);
    }

    // Convert Vector2 list to Vector3 array.
    UnityEngine.Vector3[] Vec2ToVec3List(List<UnityEngine.Vector2> vecs)
    {
        UnityEngine.Vector3[] temp = new UnityEngine.Vector3[vecs.Count];
        for (int i = 0; i < vecs.Count; i++)
        {
            temp[i] = new UnityEngine.Vector3(vecs[i].x, vecs[i].y, 0);
        }
        return temp;
    }

    // Convert Collection to Array
    private UnityEngine.Vector3[] ListToArrayVec3(List<UnityEngine.Vector3> l)
    {
        UnityEngine.Vector3[] temp = { };
        for (int i = 0; i < l.Count; i++)
        {
            temp[i] = l[i];
        }
        return temp;
    }

    // Convert single Vector2 to Vector3
    private Vector3 Vec2ToVec3(Vector2 vec)
    {
        return new Vector3(vec.x, vec.y, 0);
    }

    // Apply Matrix to shape vertices
    public void ApplyMatToVerts(float[,] mat)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            float x = vertices[i].x * mat[0, 0] + vertices[i].y * mat[1, 0];
            float y = vertices[i].x * mat[0, 1] + vertices[i].y * mat[1, 1];
            vertices[i] = new Vector2(x, y);
        }
    }

    private void OnDrawGizmos()
    {
        if (!isDemo)
        {
            if (colliding)
            {
                Gizmos.color = Color.red;
            }
            else if (broadPhase)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.white;
            }
            if (gz.drawCollisionBounds)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    if (i == vertices.Count - 1)
                    {
                        Gizmos.DrawLine(vertices[i], vertices[0]);
                    }
                    else
                    {
                        Gizmos.DrawLine(vertices[i], vertices[i + 1]);
                    }
                }
            }
            if (gz.drawBroadPhaseBounds)
            {
                Gizmos.DrawWireSphere(this.transform.position, broadPhaseRadius);
            }
        }
    }

    // Sets up gizmo objects to draw the same information as gizmos, however as it uses a line renderer it works in the build

    void GizmoBuild()
    {
        if (gz.drawBroadPhaseBounds)
        {
            if (!debugSpawned)
            {
                debugSpawn = Instantiate(debugObj, this.transform.position, this.transform.rotation);
                debugSpawn.transform.SetParent(this.transform);
                dblr = debugSpawn.GetComponent<LineRenderer>();
                List<Vector2> debugVerts = new List<Vector2>();
                const float radians360 = 6.283185f;

                float perStep = radians360 / 16;
                float angle = 0;
                for (int i = 0; i < 16; i++)
                {
                    Vector2 pointVector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                    debugVerts.Add((pointVector * broadPhaseRadius));
                    angle += perStep;
                }
                dblr.positionCount = 16;
                dblr.SetPositions(Vec2ToVec3List(debugVerts));
                localDebug = Vec2ToVec3List(debugVerts);
                dblr.loop = true;
                debugSpawned = true;
            }
            else
            {
                debugSpawn.transform.position = this.transform.position;

                Vector3[] vecL = new Vector3[dblr.positionCount];
                dblr.GetPositions(vecL);

                for (int i = 0; i < vecL.Length; i++)
                {
                    dblr.SetPosition(i, localDebug[i] + this.transform.position);
                }

                Gradient gradient = new Gradient();
                if (colliding)
                {
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f) }
                    );
                    dblr.colorGradient = gradient;
                }
                else if (broadPhase)
                {
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f), new GradientColorKey(Color.green, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f) }
                    );
                    dblr.colorGradient = gradient;
                }
                else
                {
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f) }
                    );
                    dblr.colorGradient = gradient;
                }
            }
        }

        if (!gz.drawBroadPhaseBounds && debugSpawned)
        {
            Destroy(debugSpawn);
            debugSpawned = false;
        }
    }

}
