using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField]
    GameController gc;
    [SerializeField]
    Vector2 target;
    [SerializeField]
    Vector2 currentCell;
    [SerializeField]
    Vector2 movementTarget;
    public Dynamic d;
    public Shape s;

    public Blocks blocks;

    public List<Vector3> route = new List<Vector3>();

    public bool atTarget = false;
    public int count = 0;
    public int agroCount = 0;

    enum Mode
    {
        Patrol,
        Hunt,
        Escaped,
        Agro
    }

    [SerializeField]
    LineRenderer lr;

    [Header("Enemy Settings")]
    Mode mode = Mode.Patrol;

    [SerializeField]
    float viewRadius = 12f;

    public float radiusIncrease = 0.01f;
    public float speedIncrease = 0.01f;
    
    [SerializeField]
    float speed = 20f;

    [SerializeField]
    GameObject player;

    [Header("Gizmo Settings")]
    [SerializeField]
    GizmoController gz;

    public Vector2[] patrolTargets = { new Vector2(4 * 3, 4 * 3), new Vector2(4 * 21, 4 * 3), new Vector2(4 * 21, 4 * 11), new Vector2(4 * 3, 4 * 11), new Vector2(4 * 12, 4 * 6),new Vector2(4 * 12, 4 * 6) };
    // Start is called before the first frame update
    void Start()
    {
        target = patrolTargets[UnityEngine.Random.Range(0, 6)];
        blocks.targetVec = target;
    }

    // Update is called once per frame
    void Update()
    {
        CheckView();
        if(gc.started && !player.GetComponent<Dynamic>().isDead){
            viewRadius += radiusIncrease*Time.deltaTime;
            speed += speedIncrease*Time.deltaTime;
        }
        else if(player.GetComponent<Dynamic>().isDead){
            viewRadius = 12f;
            speed = 20f;
        }
        
        switch (mode)
        {
            case Mode.Patrol:
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.cyan, 0.5f), new GradientColorKey(Color.white, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f), new GradientAlphaKey(1f, 1.0f) }
                );
                lr.colorGradient = gradient;
                agroCount = 0;
                //Debug.Log("Patrolling");
                Patrol(4.0f);
                break;
            case Mode.Hunt:
                //Debug.Log("Hunting");
                Hunt();
                break;
            case Mode.Escaped:
                //Debug.Log("Escaped");
                Escape();
                break;
            case Mode.Agro:
                //Debug.Log("Agro");
                Agro();

                break;
        }
    }

    void Patrol(float speed)
    {
        
        currentCell = blocks.shipCell;
        if (currentCell == target)
        {
            atTarget = true;
        }

        if (atTarget)
        {
            Vector2 tempTarget = patrolTargets[UnityEngine.Random.Range(0, 6)];
            while (tempTarget == target)
            {
                float x = UnityEngine.Random.Range(0,45*2);
                float y = UnityEngine.Random.Range(0,25*2);
                while(x%4!=0){
                    x = UnityEngine.Random.Range(0,45*2);
                }
                while(y%4!=0){
                    y = UnityEngine.Random.Range(0,25*2);
                }
                patrolTargets[5] = new Vector3(x,y,0);
                tempTarget = patrolTargets[UnityEngine.Random.Range(0, 6)];
            }
            target = tempTarget;
            blocks.targetVec = target;
            atTarget = false;
        }
        if (blocks.found)
        {
            route = blocks.route;
            if (route.Count < 2)
            {
                blocks.found = false;
            }
            else
            {
                movementTarget = new Vector2(route[route.Count - 2].x, route[route.Count - 2].y);
                if (new Vector3(currentCell.x, currentCell.y, 0) == new Vector3(route[route.Count - 2].x + 45, route[route.Count - 2].y + 25, 0))
                {
                    blocks.found = false;
                }
                Vector2 p1 = new Vector2(movementTarget.x, movementTarget.y);
                Vector2 p2 = new Vector2(this.transform.position.x, this.transform.position.y);
                Vector2 vec = p1 - p2;
                float mag = (float)Math.Sqrt((vec.x * vec.x) + (vec.y * vec.y));

                vec = new Vector2(vec.x / mag, vec.y / mag);
                if (count == 15)
                {
                    blocks.found = false;
                }
                if (count > 30)
                {

                    d.velocity = vec * speed;
                    count = 0;
                }
                else
                {
                    count++;
                }
            }
        }
    }

    void Hunt()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.red, 0.5f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f), new GradientAlphaKey(1f, 1.0f) }
        );
        lr.colorGradient = gradient;

        Vector2 p1 = new Vector2(player.transform.position.x, player.transform.position.y);
        Vector2 p2 = new Vector2(this.transform.position.x, this.transform.position.y);
        Vector2 vec = p1 - p2;
        float mag = (float)Math.Sqrt((vec.x * vec.x) + (vec.y * vec.y));

        vec = new Vector2(vec.x / mag, vec.y / mag);

        if (count > 25)
        {
            d.velocity = vec * speed;
            count = 0;
        }
        else
        {
            count++;
        }

    }

    void Escape()
    {


        Vector2 p1 = new Vector2(player.transform.position.x, player.transform.position.y);
        Vector2 p2 = new Vector2(this.transform.position.x, this.transform.position.y);
        Vector2 vec = p1 - p2;
        float mag = (float)Math.Sqrt((vec.x * vec.x) + (vec.y * vec.y));

        vec = new Vector2(vec.x / mag, vec.y / mag);

        if (count > 120)
        {
            count = 0;
            mode = Mode.Patrol;
        }
        else
        {
            count++;
        }
    }

    void Agro()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.yellow, 0.5f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f), new GradientAlphaKey(1f, 1.0f) }
        );
        lr.colorGradient = gradient;
        if (agroCount > 120)
        {
            agroCount = 0;
            mode = Mode.Hunt;
        }
        else
        {
            agroCount++;
            Patrol(6f);
        }
    }

    void CheckView()
    {
        Vector3 a = this.transform.position;
        Vector3 b = player.transform.position;
        Vector3 vec = b - a;
        float mag = (float)Math.Sqrt((vec.x * vec.x) + (vec.y * vec.y) + (vec.z * vec.z));

        if(!player.GetComponent<Dynamic>().isDead){
            if (viewRadius + player.GetComponent<Shape>().broadPhaseRadius > mag)
            {
                if (mode != Mode.Hunt)
                {
                    mode = Mode.Agro;
                }
            }
            else
            {
                if (mode == Mode.Hunt)
                {
                    mode = Mode.Escaped;
                }
                if (mode == Mode.Agro)
                {
                    mode = Mode.Patrol;
                }
            }
        }
        else{
            mode = Mode.Patrol;
        }
    }

    private void OnDrawGizmos()
    {
        if(gz.drawEnemyView){
            Gizmos.color = Color.white;

            Gizmos.DrawWireSphere(this.transform.position, viewRadius);
        }
        
    }
}
