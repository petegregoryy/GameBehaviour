using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constraint : MonoBehaviour
{
    public GameObject child;

    public Dynamic childDynamic;
    public Dynamic parentDynamic;

    Vector3 prevParentVelocity;
    Vector3 prevChildVelocity;

    // Start is called before the first frame update
    void Start()
    {
        parentDynamic = this.GetComponent<Dynamic>();
        childDynamic = child.GetComponent<Dynamic>();
        prevParentVelocity = parentDynamic.velocity;
        prevChildVelocity = childDynamic.velocity;
        childDynamic.velocity = parentDynamic.velocity;
    }

    // Update is called once per framex
    void Update()
    {
        // If parent gets hit, move child too
        if(parentDynamic.velocity != prevParentVelocity){
            childDynamic.velocity = parentDynamic.velocity;
            prevChildVelocity = childDynamic.velocity;
            prevParentVelocity = parentDynamic.velocity;
        }
        // If child gets hit, move parent too
        else if(childDynamic.velocity != prevChildVelocity){
            parentDynamic.velocity = childDynamic.velocity;
            prevChildVelocity = childDynamic.velocity;
            prevParentVelocity = parentDynamic.velocity;
        }
        // Move child with parent if no collision
        else{
            childDynamic.velocity = parentDynamic.velocity;
        }        
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.magenta;

        Gizmos.DrawLine(this.transform.position,child.transform.position);
    }
}
