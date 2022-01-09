using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("Shape")]
    [SerializeField]
    public float rad = 1f;

    [SerializeField]
    int points = 12;

    [SerializeField]
    Vector3[] verts;

    [Header("References")]
    [SerializeField]
    GameObject player;



    [SerializeField]
    LineRenderer lr;

    [Header("Gizmo Settings")]
    public GizmoController gz;


    // Start is called before the first frame update
    void Start()
    {


        verts = new Vector3[points];
        player = GameObject.FindGameObjectWithTag("Player");
        lr = this.GetComponent<LineRenderer>();
        const float radians360 = 6.283185f;


        float perStep = radians360 / points;
        float angle = 0;
        for (int i = 0; i < points; i++)
        {
            Vector2 pointVector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            //Debug.Log(pointVector);

            verts[i] = (new Vector3(pointVector.x, pointVector.y, 0) + this.transform.position) * rad;
            angle += perStep;
        }
        lr.SetVertexCount(points);
        lr.SetPositions(verts);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {

        if (gz.drawPickups)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(this.transform.position, rad);
        }
    }
}
