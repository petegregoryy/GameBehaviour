using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    [SerializeField]
    int points = 4;
    [SerializeField]
    int rad = 2;
    float maxMag = float.MinValue;
    public bool colliding = false;
    public bool broadPhase = false;
    public List<Vector2> vertices;
    public float broadPhaseRadius;

    [SerializeField]
    List<Vector2> localVertices;
    // Start is called before the first frame update
    void Start()
    {
        const float radians360 = 6.283185f;
        
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
    }

    // Update is called once per frame
    void Update()
    {
        vertices.Clear();
        Vector2 pos = new Vector2(this.transform.position.x, this.transform.position.y);
        for (int i = 0; i < localVertices.Count; i++)
        {
            vertices.Add(localVertices[i] + pos);
        }
        
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

    Vector3[] Vec2ToVec3List(List<Vector2> vecs){
        Vector3[] temp = new Vector3[vecs.Count];
        for (int i = 0; i < vecs.Count; i++)
        {
            temp[i] = new Vector3(vecs[i].x,vecs[i].y,0);
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
}
