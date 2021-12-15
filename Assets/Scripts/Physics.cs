using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class Physics : MonoBehaviour
{
    public GameObject[] objects;

    public List<Vector2> faces1 = new List<Vector2>();
    public List<Vector2> faces2 = new List<Vector2>();
    public List<Vector2> normal1 = new List<Vector2>();
    public List<Vector2> normal2 = new List<Vector2>();

    public List<int> indexPairs;
    public Vector2 SeperationAxis;
    public float minOverlap = 999999;

    // Start is called before the first frame update
    void Start()
    {
        objects = GameObject.FindGameObjectsWithTag("Collision Object");
        indexPairs.Add(0);
        indexPairs.Add(1);

        indexPairs.Add(0);
        indexPairs.Add(2);

        indexPairs.Add(1);
        indexPairs.Add(2);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.LogWarning(indexPairs.Count);
        for (int i = 0; i < indexPairs.Count; i++)
        {
            if(i%2 == 0){
                Debug.LogWarning("even: " +i);
                if (CheckCollision(indexPairs[i],indexPairs[i+1]))
                {
                    // objects[indexPairs[i]].transform.position +=  Vec2ToVec3(SeperationAxis * minOverlap);
                    // Vector2 obj1Vel = objects[indexPairs[i]].gameObject.GetComponent<Dynamic>().vectorDirection * objects[indexPairs[i]].gameObject.GetComponent<Dynamic>().velocity;
                    // Vector2 obj2Vel = objects[indexPairs[i+1]].gameObject.GetComponent<Dynamic>().vectorDirection * objects[indexPairs[i+1]].gameObject.GetComponent<Dynamic>().velocity;
                    // Vector2 relativeVelocity = obj2Vel - obj1Vel;

                    // float velOnNormal = DotProductVec2(relativeVelocity,SeperationAxis);
                    // Debug.LogWarning(velOnNormal);
                    // if(velOnNormal < 0){
                    //     float impulse = -1 * velOnNormal;
                    //     Vector2 impulseVec = impulse * SeperationAxis;

                    //     objects[indexPairs[i]].gameObject.GetComponent<Dynamic>().vectorDirection -= SeperationAxis;
                    //     objects[indexPairs[i+1]].gameObject.GetComponent<Dynamic>().vectorDirection += SeperationAxis;

                    //     objects[indexPairs[i]].gameObject.GetComponent<Dynamic>().velocity = impulse;
                    //     objects[indexPairs[i+1]].gameObject.GetComponent<Dynamic>().velocity = impulse;

                    // }

                    // if(objects[indexPairs[i]].gameObject.GetComponent<Dynamic>().vectorDirection.x == 0 && objects[indexPairs[i]].gameObject.GetComponent<Dynamic>().vectorDirection.y == 0){
                    //     objects[indexPairs[i]].gameObject.GetComponent<Dynamic>().vectorDirection = objects[indexPairs[i+1]].gameObject.GetComponent<Dynamic>().vectorDirection;
                    //     objects[indexPairs[i+1]].gameObject.GetComponent<Dynamic>().Bounce();
                    //     objects[indexPairs[i+1]].gameObject.GetComponent<Dynamic>().velocity = objects[indexPairs[i+1]].gameObject.GetComponent<Dynamic>().velocity / 2;
                    //     objects[indexPairs[i]].gameObject.GetComponent<Dynamic>().velocity = objects[indexPairs[i+1]].gameObject.GetComponent<Dynamic>().velocity;
                    // }
                    // else{
                    //     objects[indexPairs[i+1]].gameObject.GetComponent<Dynamic>().vectorDirection = objects[indexPairs[i]].gameObject.GetComponent<Dynamic>().vectorDirection;
                    //     objects[indexPairs[i]].gameObject.GetComponent<Dynamic>().Bounce();
                    //     objects[indexPairs[i]].gameObject.GetComponent<Dynamic>().velocity = objects[indexPairs[i]].gameObject.GetComponent<Dynamic>().velocity / 2;
                    //     objects[indexPairs[i+1]].gameObject.GetComponent<Dynamic>().velocity = objects[indexPairs[i]].gameObject.GetComponent<Dynamic>().velocity;
                    // }
                }
            }
            else{
                Debug.LogWarning("odd: " +i);
            }
            
        }
        
    }

    bool CheckCollision(int index1, int index2)
    {
        faces1.Clear();
        faces2.Clear();
        normal1.Clear();
        normal2.Clear();
        List<Vector2> verts1 = objects[index1].GetComponent<Shape>().vertices;
        List<Vector2> verts2 = objects[index2].GetComponent<Shape>().vertices;


        for (int i = 0; i < verts1.Count; i++)
        {
            if (i == verts1.Count - 1)
            {
                faces1.Add(verts1[0] - verts1[i]);
            }
            else
            {
                faces1.Add(verts1[i + 1] - verts1[i]);
            }
        }

        for (int i = 0; i < verts2.Count; i++)
        {
            if (i == verts2.Count - 1)
            {
                faces2.Add(verts2[0] - verts2[i]);
            }
            else
            {
                faces2.Add(verts2[i + 1] - verts2[i]);
            }
        }

        Debug.Log(faces1.Count + " - " + faces2.Count);

        for (int i = 0; i < faces1.Count; i++)
        {
            normal1.Add(new Vector2(faces1[i].y * -1, faces1[i].x));
        }

        for (int i = 0; i < faces2.Count; i++)
        {
            normal2.Add(new Vector2(faces2[i].y * -1, faces2[i].x));
        }

        for (int i = 0; i < normal1.Count; i++)
        {
            float len = Mathf.Sqrt((normal1[i].x * normal1[i].x) + (normal1[i].y * normal1[i].y));
            normal1[i] = new Vector2(normal1[i].x / len, normal1[i].y / len);
        }

        for (int i = 0; i < normal2.Count; i++)
        {
            float len = Mathf.Sqrt((normal2[i].x * normal2[i].x) + (normal2[i].y * normal2[i].y));
            normal2[i] = new Vector2(normal2[i].x / len, normal2[i].y / len);
        }


        bool colliding1 = true;
        bool colliding2 = true;
        int count = 0;
        foreach (Vector2 p in normal1)
        {
            float min = 0;
            float max = 0;
            float min2 = 0;
            float max2 = 0;
            for (int i = 0; i < verts1.Count; i++)
            {
                float dotprod = DotProduct(p, verts1[i]);
                if (i == 0)
                {
                    min = dotprod;
                    max = dotprod;
                }
                if (dotprod <= min)
                {
                    min = dotprod;
                }
                if (dotprod >= max)
                {
                    max = dotprod;
                }

            }
            for (int i = 0; i < verts2.Count; i++)
            {
                float dotprod = DotProduct(p, verts2[i]);
                if (i == 0)
                {
                    min2 = dotprod;
                    max2 = dotprod;
                }
                if (dotprod <= min2)
                {
                    min2 = dotprod;
                }
                if (dotprod >= max2)
                {
                    max2 = dotprod;
                }

            }

            //Debug.Log(" ONE! = " + (min2 - max));
            //Debug.Log(" TWO! = " + (min - max2));

            if ((min2 - max <= 0) && (min - max2 <= 0))
            {
                colliding1 = true;
                float penetration = Math.Min(max, max2) - Math.Max(min,min2);

                if(penetration < minOverlap){
                    minOverlap = penetration;
                    SeperationAxis = p;
                }
                count++;
            }
            else
            {
                colliding1 = false;
                break;
            }
        }
        foreach (Vector2 p in normal2)
        {
            float min = 0;
            float max = 0;
            float min2 = 0;
            float max2 = 0;
            for (int i = 0; i < verts2.Count; i++)
            {
                float dotprod = DotProduct(p, verts2[i]);
                if (i == 0)
                {
                    min = dotprod;
                    max = dotprod;
                }
                if (dotprod <= min)
                {
                    min = dotprod;
                }
                if (dotprod >= max)
                {
                    max = dotprod;
                }

            }
            for (int i = 0; i < verts1.Count; i++)
            {
                float dotprod = DotProduct(p, verts1[i]);
                if (i == 0)
                {
                    min2 = dotprod;
                    max2 = dotprod;
                }
                if (dotprod <= min2)
                {
                    min2 = dotprod;
                }
                if (dotprod >= max2)
                {
                    max2 = dotprod;
                }

            }

            Debug.Log(" ONE! = " + (min2 - max));
            Debug.Log(" TWO! = " + (min - max2));

            if ((min2 - max <= 0) && (min - max2 <= 0))
            {
                colliding2 = true;
                float penetration = Math.Min(max, max2) - Math.Max(min,min2);

                if(penetration < minOverlap){
                    minOverlap = penetration;
                    SeperationAxis = p;
                }
                count++;
            }
            else
            {
                colliding2 = false;
                break;
            }
        }
        Debug.Log("Collision: " + colliding1 + " Count: " + count);
        if (colliding1 && colliding2)
        {
            objects[index1].GetComponent<Shape>().colliding = true;
            objects[index2].GetComponent<Shape>().colliding = true;

            
            
            return true;
        }
        else
        {
            objects[index1].GetComponent<Shape>().colliding = false;
            objects[index2].GetComponent<Shape>().colliding = false;
            return false;
        }

    }

    public float DotProduct(Vector2 a, Vector2 b)
    {
        return (a.x * b.x) + (a.y * b.y);
    }

    Vector3 Vec2ToVec3(UnityEngine.Vector2 vec){
        return new UnityEngine.Vector3(vec.x,vec.y,0);
    }

    float DotProductVec2(Vector2 vec1,Vector2 vec2){
        return (vec1.x * vec2.x) + (vec1.y * vec2.y);
    }
}
