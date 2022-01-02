using System.Security.Cryptography;
using System.Collections.Specialized;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    [SerializeField]
    bool rectangle = false;
    [SerializeField]
    int points = 4;
    [SerializeField]
    float rad = 2;
    float maxMag = float.MinValue;

    public Vector3 forwardVector;

    [SerializeField]
    bool isShip = false;
    public bool colliding = false;
    public bool broadPhase = false;
    public List<Vector2> vertices;
    public float broadPhaseRadius;

[SerializeField]
    public LineRenderer lr;

    [SerializeField]
    public List<Vector2> localVertices;

    public float[,] rotMat;
    // Start is called before the first frame update
    void Start()
    {
        rotMat = new float[2,2];
        const float radians360 = 6.283185f;
        lr = this.GetComponent<LineRenderer>();
        rad = UnityEngine.Random.Range(2.0f,5.0f);
        float perStep = radians360 / points;
        float angle = 0;
        for (int i = 0; i < points; i++)
        {
            Vector2 pointVector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Debug.Log(pointVector);
            float mag = UnityEngine.Random.Range(rad * 0.8f, rad * 1.2f);
            if(mag > maxMag){
                maxMag = mag;
            }
            localVertices.Add((pointVector * mag));
            angle += perStep;
        }
        broadPhaseRadius = maxMag * 1.5f;

        if(isShip){
            this.transform.position = new Vector3(0,0,0);
            localVertices.Clear();
            localVertices.Add(new Vector2(0,2.5f));
            localVertices.Add(new Vector2(1.5f,-2.5f));
            localVertices.Add(new Vector2(-1.5f,-2.5f));
            forwardVector = Vec2ToVec3(localVertices[0]) - this.transform.position ;
            float forwardMag = (float)Math.Sqrt((forwardVector.x * forwardVector.x)+(forwardVector.y * forwardVector.y)+(forwardVector.z * forwardVector.z));
            forwardVector = new Vector3(forwardVector.x / forwardMag,forwardVector.y / forwardMag,0);
            points = 3;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        vertices.Clear();
        Vector2 pos = new Vector2(this.transform.position.x, this.transform.position.y);
        for (int i = 0; i < localVertices.Count; i++)
        {   Vector2 tempVert = localVertices[i];
            if(isShip){
                float x = localVertices[i].x * rotMat[0,0] + localVertices[i].y * rotMat[1,0];
                float y = localVertices[i].x * rotMat[0,1] + localVertices[i].y * rotMat[1,1];
                tempVert =  new Vector2(x,y);
            }
            vertices.Add(tempVert + pos);
        }
        forwardVector = Vec2ToVec3(vertices[0]) - this.transform.position ;
        float forwardMag = (float)Math.Sqrt((forwardVector.x * forwardVector.x)+(forwardVector.y * forwardVector.y)+(forwardVector.z * forwardVector.z));
        forwardVector = new Vector3(forwardVector.x / forwardMag,forwardVector.y / forwardMag,0);
        
        lr.positionCount = vertices.Count + 1;
        lr.SetPositions(Vec2ToVec3List(vertices));
        lr.SetPosition(vertices.Count,vertices[0]);
    }

    public Vector3[] GetNormals(){
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
            Vector3 normal = new Vector3(faces[i].y * -1, faces[i].x,0);
            float mag = (float)Math.Sqrt((normal.x * normal.x)+(normal.y * normal.y)+(normal.z * normal.z));
            normals[i] = new Vector3(normal.x/mag ,normal.y/mag, 0);
        }

        return normals;
    }

    public Vector3[] GetAxes(){
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
            float mag = (float)Math.Sqrt((faces[i].x * faces[i].x)+(faces[i].y * faces[i].y)+(faces[i].z * faces[i].z));
            faces[i] = new Vector3(faces[i].x/mag ,faces[i].y/mag, 0);
        }
        return faces;
    }

    public Vector3[] GetVertices(){
        return Vec2ToVec3List(vertices);
    }

    UnityEngine.Vector3[] Vec2ToVec3List(List<UnityEngine.Vector2> vecs){
        UnityEngine.Vector3[] temp = new UnityEngine.Vector3[vecs.Count];
        for (int i = 0; i < vecs.Count; i++)
        {
            temp[i] = new UnityEngine.Vector3(vecs[i].x,vecs[i].y,0);
        }
        return temp;
    }


    private void OnDrawGizmos()
    {
        if (colliding)
        {
            Gizmos.color = Color.red;
        }
        else if(broadPhase){
            Gizmos.color = Color.green;
        }        
        else
        {
            Gizmos.color = Color.white;
        }
        
        for (int i = 0; i < vertices.Count; i++)
        {
            if(i == vertices.Count - 1)
            {
                Gizmos.DrawLine(vertices[i], vertices[0]);
            }
            else
            {
                Gizmos.DrawLine(vertices[i], vertices[i + 1]);
            }            
        }
        Gizmos.DrawWireSphere(this.transform.position,broadPhaseRadius);
    }

    private UnityEngine.Vector3[] ListToArrayVec3(List<UnityEngine.Vector3> l){
        UnityEngine.Vector3[] temp = {};
        for (int i = 0; i < l.Count; i++)
        {
            temp[i] = l[i];
        }
        return temp;
    }

    private Vector3 Vec2ToVec3(Vector2 vec){
        return new Vector3(vec.x,vec.y,0);
    }

    public void ApplyMatToVerts(float[,] mat){
        for (int i = 0; i < vertices.Count; i++)
        {
            float x = vertices[i].x * mat[0,0] + vertices[i].y * mat[1,0];
            float y = vertices[i].x * mat[0,1] + vertices[i].y * mat[1,1];
            vertices[i] =  new Vector2(x,y);
        }
    }
}
