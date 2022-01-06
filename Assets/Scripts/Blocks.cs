using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocks : MonoBehaviour
{
    const float totX = 45 * 2;
    const float totY = 25 * 2;
    Vector3 topLeft = new Vector3(-45, 25, 0);
    Vector3 bottomRight = new Vector3(45, -25, 0);
    [Header("Path Finding")]
    public int[,] occupied = new int[45 * 2, 25 * 2];
    public int[,] pathingLength = new int[45 * 2, 25 * 2];
    public bool[,] visited = new bool[45 * 2, 25 * 2];
    public Vector2[,] parent = new Vector2[45 * 2, 25 * 2];
    public bool found = false;
    public List<Vector3> route = new List<Vector3>();
    public GameObject PhysicsController;
    public Vector3[] coords;
    public Vector2 shipCell;

    public Vector2 targetVec;

    int pathingCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        coords = new Vector3[PhysicsController.GetComponent<SAT>().totalObjects.Length];
        
    }

    // Update is called once per frame
    void Update()
    {
        coords = new Vector3[PhysicsController.GetComponent<SAT>().totalObjects.Length];
        occupied = new int[45 * 2, 25 * 2];
        // for (int i = 0; i < PhysicsController.GetComponent<SAT>().totalObjects.Length; i++)
        // {
        //     coords[i] =  PhysicsController.GetComponent<SAT>().totalObjects[i].transform.position;
        // }
        for (int x = 0; x < totX; x += 4)
        {
            for (int y = 0; y < totY; y += 4)
            {
                for (int i = 0; i < PhysicsController.GetComponent<SAT>().totalObjects.Length; i++)
                {
                    if (PhysicsController.GetComponent<SAT>().totalObjects[i].transform.position.x >= (x - 45) - 2 && PhysicsController.GetComponent<SAT>().totalObjects[i].transform.position.x <= (x - 45) + 2 && PhysicsController.GetComponent<SAT>().totalObjects[i].transform.position.y >= (y - 25) - 2 && PhysicsController.GetComponent<SAT>().totalObjects[i].transform.position.y <= (y - 25) + 2)
                    {
                        
                        if(PhysicsController.GetComponent<SAT>().totalObjects[i].GetComponent<Shape>().isEnemy){
                            shipCell = new Vector2(x,y);
                        }
                        else{
                            occupied[x, y] = 1;
                            if (PhysicsController.GetComponent<SAT>().totalObjects[i].transform.position.x + PhysicsController.GetComponent<SAT>().totalObjects[i].GetComponent<Shape>().broadPhaseRadius > (x - 45) + 2)
                            {
                                if (x < 45 * 2 - 4)
                                {
                                    occupied[x + 4, y] = 1;
                                }

                            }
                            if (PhysicsController.GetComponent<SAT>().totalObjects[i].transform.position.x - PhysicsController.GetComponent<SAT>().totalObjects[i].GetComponent<Shape>().broadPhaseRadius < (x - 45) - 2)
                            {
                                if (x > 4)
                                {
                                    occupied[x - 4, y] = 1;
                                }

                            }
                            if (PhysicsController.GetComponent<SAT>().totalObjects[i].transform.position.y + PhysicsController.GetComponent<SAT>().totalObjects[i].GetComponent<Shape>().broadPhaseRadius > (y - 25) + 2)
                            {
                                if (y < 25 * 2 - 4)
                                {
                                    occupied[x, y + 4] = 1;
                                }

                            }
                            if (PhysicsController.GetComponent<SAT>().totalObjects[i].transform.position.y - PhysicsController.GetComponent<SAT>().totalObjects[i].GetComponent<Shape>().broadPhaseRadius < (y - 25) - 2)
                            {
                                if (y > 4)
                                {
                                    occupied[x, y - 4] = 1;
                                }
                            }
                            break;
                        }
                        
                    }
                }
            }
        }

        if(!found){
            PathFind((int)shipCell.x, (int)shipCell.y, (int)targetVec.x, (int)targetVec.y);
        }
        // if(pathingCount > 60){
        //     found = false;
        //     pathingCount = 0;
        // }
        // else{
        //     pathingCount++;
        // }
    }

    private void OnDrawGizmos()
    {

        for (int x = 0; x < totX; x += 4)
        {
            for (int y = 0; y < totY; y += 4)
            {
                if (occupied[x, y] == 1)
                {
                    Gizmos.color = Color.yellow;
                }
                else if (visited[x, y] == true)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.blue;
                }
                
                Gizmos.DrawWireCube(new Vector3(x - 45, y - 25, 0), new Vector3(4f, 4f, 0));
            }
        }
        if(found){
            for (int i = 0; i < route.Count; i++)
            {
                if(i != route.Count-1){
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(route[i],route[i+1]);
                }
            }
        }

    }

    void PathFind(int startx, int starty, int tx, int ty)
    {
        // int x = 0;
        // int y = 0;
        // for (int i = 0; i < ((45*2)/4)*((25*2)/4); i++)
        // {

        //     x +=4;
        //     y +=4;
        // }
        int targetx = tx;
        int targety = ty;
        for (int x = 0; x < 45 * 2; x += 4)
        {
            for (int y = 0; y < 25 * 2; y += 4)
            {
                pathingLength[x, y] = int.MaxValue;
                visited[x,y] = false;

            }
        }
        parent = new Vector2[45 * 2, 25 * 2];

        List<Vector2> queue = new List<Vector2>();

        queue.Add(new Vector2(startx, starty));
        pathingLength[startx, starty] = 0;
        parent[startx, starty] = new Vector2(-1, -1);
        List<Vector2> altargs = new List<Vector2>();
        Debug.Log(string.Format("Target Array Location ({0},{1})",tx,ty));
        if(occupied[targetx,targety] == 1){
            Debug.Log("Getting New Target!");
            if(targetx < 45*2-4){
                if(occupied[targetx+4,targety] == 0){
                    altargs.Add(new Vector2(targetx+1,targety));
                }
            }
            if(targety < 25*2-4){
                if(occupied[targetx,targety+4] == 0){
                    altargs.Add(new Vector2(targetx,targety+4));
                }
            }
            if(targetx >= 4){
                if(occupied[targetx-4,targety] == 0){
                    altargs.Add(new Vector2(targetx-1,targety));
                }
            }
            if(targety >= 4){
                if(occupied[targetx,targety-4] == 0){
                    altargs.Add(new Vector2(targetx,targety-4));
                }
            }
            if(targetx >= 4 && targety >= 4 ){
                if(occupied[targetx-4,targety-4] == 0){
                    altargs.Add(new Vector2(targetx-4,targety-4));
                }
            }
            if(targetx >= 4 && targety < 25*2 -4 ){
                if(occupied[targetx-4,targety+4] == 0){
                    altargs.Add(new Vector2(targetx-4,targety+4));
                }
            }
            if(targetx < 45*2 -4 && targety >= 4 ){
                if(occupied[targetx+4,targety-4] == 0){
                    altargs.Add(new Vector2(targetx+4,targety-4));
                }
            }
            if(targetx < 45*2-4 && targety < 25*2-4 ){
                if(occupied[targetx+4,targety+4] == 0){
                    altargs.Add(new Vector2(targetx+4,targety+4));
                }
            }


            if(altargs.Count > 0){
                Vector2 newTarg = altargs[Random.Range(0,altargs.Count-1)];
                Debug.Log(string.Format("Old Target: ({0},{1}) New Target ({2},{3})",targetx,targety,newTarg.x,newTarg.y));
                targetx = (int)newTarg.x;
                targety = (int)newTarg.y;
            }
            else{
                Debug.LogError("Unable to find alternative target.");                
            }
        }

        for (int i = 0; i < 1000; i++)
        {
            if(occupied[targetx,targety] == 1){
                Debug.LogWarning("Target Obstructed!");
                break;
            }
            if(i>=queue.Count){
                Debug.LogWarning("Unable to path to destination!");
                break;
            }
            
            int length = pathingLength[(int)queue[i].x, (int)queue[i].y];
            if (visited[(int)queue[i].x, (int)queue[i].y])
            {
                //Debug.LogWarning("Revisiting a visited node!");
            }
            else if(occupied[(int)queue[i].x, (int)queue[i].y] == 1 && queue[i].x != startx && queue[i].y != starty){
                //Debug.LogWarning("Trying to visit an occupied node!");
            }
            else
            {
                visited[(int)queue[i].x, (int)queue[i].y] = true;

                if (queue[i].x == targetx && queue[i].y == targety)
                {
                    Debug.Log(string.Format("Target Found in {0} iterations.", i));
                    ResolveRoute((int)queue[i].x, (int)queue[i].y);
                    break;
                }
                //Debug.Log(string.Format("Queue I number: {0} Queue length: {1} - queue X {2} Queue Y {3}", i, queue.Count, queue[i].x, queue[i].y));
                if (queue[i].x <= 45 * 2 - 4)
                {
                    if (!visited[(int)queue[i].x + 4, (int)queue[i].y] && occupied[(int)queue[i].x + 4, (int)queue[i].y] == 0)
                    {

                        queue.Add(new Vector2(queue[i].x + 4, queue[i].y));
                        if (pathingLength[(int)queue[i].x + 4, (int)queue[i].y] > length + 1)
                        {
                            pathingLength[(int)queue[i].x + 4, (int)queue[i].y] = length + 1;
                            parent[(int)queue[i].x + 4, (int)queue[i].y] = new Vector2(queue[i].x, queue[i].y);
                        }
                    }
                    else
                    {
                        //Debug.LogWarning(string.Format("Visited!"));
                    }
                }
                if (queue[i].y < 25 * 2 - 4)
                {

                    if (!visited[(int)queue[i].x, (int)queue[i].y + 4]  && occupied[(int)queue[i].x, (int)queue[i].y + 4] == 0)
                    {
                        queue.Add(new Vector2(queue[i].x, queue[i].y + 4));
                        if (pathingLength[(int)queue[i].x, (int)queue[i].y + 4] > length + 1)
                        {
                            pathingLength[(int)queue[i].x, (int)queue[i].y + 4] = length + 1;
                            parent[(int)queue[i].x, (int)queue[i].y + 4] = new Vector2(queue[i].x, queue[i].y);
                        }
                    }
                }

                if (queue[i].y < 25 * 2 - 4 && queue[i].x <= 45 * 2 - 4)
                {
                    if (!visited[(int)queue[i].x + 4, (int)queue[i].y + 4] && occupied[(int)queue[i].x + 4, (int)queue[i].y + 4] == 0)
                    {
                        queue.Add(new Vector2(queue[i].x + 4, queue[i].y + 4));
                        if (pathingLength[(int)queue[i].x + 4, (int)queue[i].y + 4] > length + 1)
                        {
                            pathingLength[(int)queue[i].x + 4, (int)queue[i].y + 4] = length + 1;
                            parent[(int)queue[i].x + 4, (int)queue[i].y + 4] = new Vector2(queue[i].x, queue[i].y);
                        }
                    }
                }
                if (queue[i].x >= 4)
                {
                    if (!visited[(int)queue[i].x - 4, (int)queue[i].y] && occupied[(int)queue[i].x - 4, (int)queue[i].y] == 0)
                    {
                        queue.Add(new Vector2(queue[i].x - 4, queue[i].y));
                        if (pathingLength[(int)queue[i].x - 4, (int)queue[i].y] > length + 1)
                        {
                            pathingLength[(int)queue[i].x - 4, (int)queue[i].y] = length + 1;
                            parent[(int)queue[i].x - 4, (int)queue[i].y] = new Vector2(queue[i].x, queue[i].y);
                        }
                    }
                }

                if (queue[i].y >= 4)
                {
                    if (!visited[(int)queue[i].x, (int)queue[i].y - 4] && occupied[(int)queue[i].x, (int)queue[i].y - 4] == 0)
                    {
                        queue.Add(new Vector2(queue[i].x, queue[i].y - 4));
                        if (pathingLength[(int)queue[i].x, (int)queue[i].y - 4] > length + 1)
                        {
                            pathingLength[(int)queue[i].x, (int)queue[i].y - 4] = length + 1;
                            parent[(int)queue[i].x, (int)queue[i].y - 4] = new Vector2(queue[i].x, queue[i].y);
                        }
                    }
                }

                if (queue[i].y >= 4 && queue[i].x >= 4)
                {
                    if (!visited[(int)queue[i].x - 4, (int)queue[i].y - 4] && occupied[(int)queue[i].x - 4, (int)queue[i].y - 4] == 0)
                    {
                        queue.Add(new Vector2(queue[i].x - 4, queue[i].y - 4));
                        if (pathingLength[(int)queue[i].x - 4, (int)queue[i].y - 4] > length + 1)
                        {
                            pathingLength[(int)queue[i].x - 4, (int)queue[i].y - 4] = length + 1;
                            parent[(int)queue[i].x - 4, (int)queue[i].y - 4] = new Vector2(queue[i].x, queue[i].y);
                        }
                    }
                }
                if (queue[i].y >= 4 && queue[i].x < 45 * 2 - 4)
                {
                    if (!visited[(int)queue[i].x + 4, (int)queue[i].y - 4] && occupied[(int)queue[i].x + 4, (int)queue[i].y - 4] == 0)
                    {
                        queue.Add(new Vector2(queue[i].x + 4, queue[i].y - 4));
                        if (pathingLength[(int)queue[i].x + 4, (int)queue[i].y - 4] > length + 1)
                        {
                            pathingLength[(int)queue[i].x + 4, (int)queue[i].y - 4] = length + 1;
                            parent[(int)queue[i].x + 4, (int)queue[i].y - 4] = new Vector2(queue[i].x, queue[i].y);
                        }

                    }
                }
                if (queue[i].y < 25 * 2 - 4 && queue[i].x >= 4)
                {
                    if (!visited[(int)queue[i].x - 4, (int)queue[i].y + 4] && occupied[(int)queue[i].x - 4, (int)queue[i].y + 4] == 0)
                    {
                        queue.Add(new Vector2(queue[i].x - 4, queue[i].y + 4));
                        if (pathingLength[(int)queue[i].x - 4, (int)queue[i].y + 4] > length + 1)
                        {
                            pathingLength[(int)queue[i].x - 4, (int)queue[i].y + 4] = length + 1;
                            parent[(int)queue[i].x - 4, (int)queue[i].y + 4] = new Vector2(queue[i].x, queue[i].y);
                        }
                    }
                }

                // queue.Add(new Vector2(queue[i].x,queue[i].y+4));
                // queue.Add(new Vector2(queue[i].x+4,queue[i].y+4));
                // queue.Add(new Vector2(queue[i].x-4,queue[i].y));
                // queue.Add(new Vector2(queue[i].x,queue[i].y-4));
                // queue.Add(new Vector2(queue[i].x-4,queue[i].y-4));
                // queue.Add(new Vector2(queue[i].x+4,queue[i].y-4));
                // queue.Add(new Vector2(queue[i].x-4,queue[i].y+4));

            }
        }
    }

    void ResolveRoute(int x, int y)
    {
        route.Clear();
        Vector2 currentCoord = parent[x, y];
        route.Add(new Vector3(x-45,y-25,0));
        while (currentCoord.x != -1 && currentCoord.y != -1)
        {
            route.Add(new Vector3(currentCoord.x-45,currentCoord.y-25,0));
            currentCoord = parent[(int)currentCoord.x,(int)currentCoord.y];
        }
        Debug.Log("Route Pathed!");
        found = true;
    }
}
