using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Dynamic : MonoBehaviour
{
    public Vector3 ObjPos;
    public Vector3 velocity;

    [Range(100f,1000f)]
    public float mass = 100f;
    Vector3 intermediateVelocity;

    // Start is called before the first frame update
    void Start()
    {
        ObjPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (velocity.x != 0 || velocity.y != 0)
        {
            this.transform.position += velocity * Time.deltaTime;
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
}



