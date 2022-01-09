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

    public List<bool[,]> futureOccupied = new List<bool[,]>();
    public bool found = false;
    public List<Vector3> route = new List<Vector3>();
    public GameObject PhysicsController;
    public Vector3[] coords;
    public Vector2 shipCell;

    public Vector2 targetVec;

    int pathingCount = 0;

    [Header("Gizmo Settings")]
    [SerializeField]
    GizmoController gz;
    public GameObject debugObj;
    bool debugSpawned = false;
    GameObject[,] debugSpawn;
    LineRenderer dblr;
    Vector3[] localDebug;
    bool pathDebugSpawned = false;
    GameObject pathDebugSpawn;


    // Start is called before the first frame update
    void Start()
    {
        gz = this.GetComponent<GizmoController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Set lengths and clear lists
        coords = new Vector3[PhysicsController.GetComponent<SAT>().totalObjects.Length];
        occupied = new int[45 * 2, 25 * 2];
        futureOccupied.Clear();
        futureOccupied.Add(new bool[45 * 2, 25 * 2]);
        futureOccupied.Add(new bool[45 * 2, 25 * 2]);
        futureOccupied.Add(new bool[45 * 2, 25 * 2]);

        // Loop X and Y of pathfinding blocks
        for (int x = 0; x < totX; x += 4)
        {
            for (int y = 0; y < totY; y += 4)
            {
                // Run through every object in the scene and check which block each occupied. Mark each occupied block.
                for (int i = 0; i < PhysicsController.GetComponent<SAT>().totalObjects.Length; i++)
                {
                    if (PhysicsController.GetComponent<SAT>().totalObjects[i].transform.position.x >= (x - 45) - 2 && PhysicsController.GetComponent<SAT>().totalObjects[i].transform.position.x <= (x - 45) + 2 && PhysicsController.GetComponent<SAT>().totalObjects[i].transform.position.y >= (y - 25) - 2 && PhysicsController.GetComponent<SAT>().totalObjects[i].transform.position.y <= (y - 25) + 2)
                    {
                        // Instead of marking the enemy ships block as occupied (causing pathfinding problems), it is instead set as the ship cell.
                        if (PhysicsController.GetComponent<SAT>().totalObjects[i].GetComponent<Shape>().isEnemy)
                        {
                            shipCell = new Vector2(x, y);
                        }
                        else
                        {
                            // Set current cell as occupied
                            occupied[x, y] = 1;

                            // Check the if the extent of the object overlaps the above, below, left and right blocks to the current location.
                            // If the block is, mark it so.
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

                    // If the object isn't the enemy ship, mark blocks for each stages of future location (60 frames, 120 frames, 180 frames)
                    if (!PhysicsController.GetComponent<SAT>().totalObjects[i].GetComponent<Shape>().isEnemy)
                    {
                        SAT s = PhysicsController.GetComponent<SAT>();
                        Dynamic sd = s.totalObjects[i].GetComponent<Dynamic>();
                        if (sd.futurePos[0].x >= (x - 45) - 2 && sd.futurePos[0].x <= (x - 45) + 2 && sd.futurePos[0].y >= (y - 25) - 2 && sd.futurePos[0].y <= (y - 25) + 2)
                        {
                            // Block occupation 60 frames ahead
                            futureOccupied[0][x, y] = true;
                        }
                        if (sd.futurePos[1].x >= (x - 45) - 2 && sd.futurePos[1].x <= (x - 45) + 2 && sd.futurePos[1].y >= (y - 25) - 2 && sd.futurePos[1].y <= (y - 25) + 2)
                        {
                            // Block occupation 120 frames ahead
                            futureOccupied[1][x, y] = true;
                        }
                        if (sd.futurePos[2].x >= (x - 45) - 2 && sd.futurePos[2].x <= (x - 45) + 2 && sd.futurePos[2].y >= (y - 25) - 2 && sd.futurePos[2].y <= (y - 25) + 2)
                        {
                            // Block occupation 180 frames ahead
                            futureOccupied[2][x, y] = true;
                        }
                    }
                }
            }
        }

        // If path not found, find path.
        if (!found)
        {
            PathFind((int)shipCell.x, (int)shipCell.y, (int)targetVec.x, (int)targetVec.y);
        }
        GizmoDraw();
    }



    // Pathfind using Dijkstra Algorithm
    void PathFind(int startx, int starty, int tx, int ty)
    {
        int targetx = tx;
        int targety = ty;

        // Set all blocks to max distance and not visited
        for (int x = 0; x < 45 * 2; x += 4)
        {
            for (int y = 0; y < 25 * 2; y += 4)
            {
                pathingLength[x, y] = int.MaxValue;
                visited[x, y] = false;

            }
        }

        parent = new Vector2[45 * 2, 25 * 2];

        List<Vector2> queue = new List<Vector2>();

        // Add start point to queue and set its own distance to 0.
        queue.Add(new Vector2(startx, starty));
        pathingLength[startx, starty] = 0;

        // As the starting block will have no parent, set its parent to -1. 
        // As this calculation is done in list space coordinates (0 to 90 in X, 0 to 50 in Y) the parent coordinate will never be negative
        parent[startx, starty] = new Vector2(-1, -1);

        // List of alternative targets to use when target location is blocked by an object.
        List<Vector2> altargs = new List<Vector2>();

        // Check the 8 blocks surrounding the target block and add them to the potential alternative target list
        if (occupied[targetx, targety] == 1)
        {
            #region New Target Search
            if (targetx < 45 * 2 - 4)
            {
                if (occupied[targetx + 4, targety] == 0)
                {
                    altargs.Add(new Vector2(targetx + 1, targety));
                }
            }
            if (targety < 25 * 2 - 4)
            {
                if (occupied[targetx, targety + 4] == 0)
                {
                    altargs.Add(new Vector2(targetx, targety + 4));
                }
            }
            if (targetx >= 4)
            {
                if (occupied[targetx - 4, targety] == 0)
                {
                    altargs.Add(new Vector2(targetx - 1, targety));
                }
            }
            if (targety >= 4)
            {
                if (occupied[targetx, targety - 4] == 0)
                {
                    altargs.Add(new Vector2(targetx, targety - 4));
                }
            }
            if (targetx >= 4 && targety >= 4)
            {
                if (occupied[targetx - 4, targety - 4] == 0)
                {
                    altargs.Add(new Vector2(targetx - 4, targety - 4));
                }
            }
            if (targetx >= 4 && targety < 25 * 2 - 4)
            {
                if (occupied[targetx - 4, targety + 4] == 0)
                {
                    altargs.Add(new Vector2(targetx - 4, targety + 4));
                }
            }
            if (targetx < 45 * 2 - 4 && targety >= 4)
            {
                if (occupied[targetx + 4, targety - 4] == 0)
                {
                    altargs.Add(new Vector2(targetx + 4, targety - 4));
                }
            }
            if (targetx < 45 * 2 - 4 && targety < 25 * 2 - 4)
            {
                if (occupied[targetx + 4, targety + 4] == 0)
                {
                    altargs.Add(new Vector2(targetx + 4, targety + 4));
                }
            }

            // If there are alternative targets found, pick one from random and set it as the current target.
            if (altargs.Count > 0)
            {
                Vector2 newTarg = altargs[Random.Range(0, altargs.Count - 1)];

                targetx = (int)newTarg.x;
                targety = (int)newTarg.y;
            }
            else
            {
                Debug.LogError("Unable to find alternative target.");
            }
            #endregion
        }

        // Pathfinding Loop. This runs for 1000 iterations. Generally the path will be found between 300 and 800 iterations.
        // If it cannot be found in less than 1000 for that frame the pathfinding is rejected and the enemy ship doesnt move.
        for (int i = 0; i < 1000; i++)
        {
            // While under 128 iterations, use the current occupation and the 60 frames future occupation data in pathfinding.
            if (i < 128)
            {
                if (occupied[targetx, targety] == 1)
                {
                    Debug.LogWarning("Target Obstructed!");
                    break;
                }

                // If the iteration number gets higher than the queue count number then it is unable to path. This will only happen when every cell checked.
                if (i >= queue.Count)
                {
                    Debug.LogWarning("Unable to path to destination!");
                    break;
                }

                // Length is the current cells distance from the starting cell.
                int length = pathingLength[(int)queue[i].x, (int)queue[i].y];

                // If the current cell is already visited (Shouldn't happen but still does)
                if (visited[(int)queue[i].x, (int)queue[i].y])
                {
                    // Cell is visited, do not path.
                }

                // If block is occupied and its not the start pathfinding location, do not path.
                else if (occupied[(int)queue[i].x, (int)queue[i].y] == 1 && queue[i].x != startx && queue[i].y != starty)
                {
                    // Do not path
                }

                // If current cell is valid, pathfind from there.
                else
                {
                    visited[(int)queue[i].x, (int)queue[i].y] = true;

                    // If current cell is the target, resolve the route and quit.
                    if (queue[i].x == targetx && queue[i].y == targety)
                    {
                        ResolveRoute((int)queue[i].x, (int)queue[i].y);
                        break;
                    }

                    // Check whether current cell is on the edge of the scene. If it isn't, check the cells around it.
                    // If the cell isn't visited, occupied or occupied in the future, add it to the queue.
                    // If the distance from the current cell to the cell being checked is less than that cells current distance, set the distance and change the cells parent (previous cell with shortest path) to the current cell.

                    #region Cell Checking - Current and 60 Frames Future
                    if (queue[i].x <= 45 * 2 - 4)
                    {
                        if (!visited[(int)queue[i].x + 4, (int)queue[i].y] && occupied[(int)queue[i].x + 4, (int)queue[i].y] == 0 && futureOccupied[0][(int)queue[i].x + 4, (int)queue[i].y] == false)
                        {

                            queue.Add(new Vector2(queue[i].x + 4, queue[i].y));
                            if (pathingLength[(int)queue[i].x + 4, (int)queue[i].y] > length + 1)
                            {
                                pathingLength[(int)queue[i].x + 4, (int)queue[i].y] = length + 1;
                                parent[(int)queue[i].x + 4, (int)queue[i].y] = new Vector2(queue[i].x, queue[i].y);
                            }
                        }
                    }
                    if (queue[i].y < 25 * 2 - 4)
                    {

                        if (!visited[(int)queue[i].x, (int)queue[i].y + 4] && occupied[(int)queue[i].x, (int)queue[i].y + 4] == 0 && futureOccupied[0][(int)queue[i].x, (int)queue[i].y + 4] == false)
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
                        if (!visited[(int)queue[i].x + 4, (int)queue[i].y + 4] && occupied[(int)queue[i].x + 4, (int)queue[i].y + 4] == 0 && futureOccupied[0][(int)queue[i].x + 4, (int)queue[i].y + 4] == false)
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
                        if (!visited[(int)queue[i].x - 4, (int)queue[i].y] && occupied[(int)queue[i].x - 4, (int)queue[i].y] == 0 && futureOccupied[0][(int)queue[i].x - 4, (int)queue[i].y] == false)
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
                        if (!visited[(int)queue[i].x, (int)queue[i].y - 4] && occupied[(int)queue[i].x, (int)queue[i].y - 4] == 0 && futureOccupied[0][(int)queue[i].x, (int)queue[i].y - 4] == false)
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
                        if (!visited[(int)queue[i].x - 4, (int)queue[i].y - 4] && occupied[(int)queue[i].x - 4, (int)queue[i].y - 4] == 0 && futureOccupied[0][(int)queue[i].x - 4, (int)queue[i].y - 4] == false)
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
                        if (!visited[(int)queue[i].x + 4, (int)queue[i].y - 4] && occupied[(int)queue[i].x + 4, (int)queue[i].y - 4] == 0 && futureOccupied[0][(int)queue[i].x + 4, (int)queue[i].y - 4] == false)
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
                        if (!visited[(int)queue[i].x - 4, (int)queue[i].y + 4] && occupied[(int)queue[i].x - 4, (int)queue[i].y + 4] == 0 && futureOccupied[0][(int)queue[i].x - 4, (int)queue[i].y + 4] == false)
                        {
                            queue.Add(new Vector2(queue[i].x - 4, queue[i].y + 4));
                            if (pathingLength[(int)queue[i].x - 4, (int)queue[i].y + 4] > length + 1)
                            {
                                pathingLength[(int)queue[i].x - 4, (int)queue[i].y + 4] = length + 1;
                                parent[(int)queue[i].x - 4, (int)queue[i].y + 4] = new Vector2(queue[i].x, queue[i].y);
                            }
                        }
                    }
                    #endregion
                }
            }
            else if (i < 256)
            {
                // This is identical to the above, however it uses the 60 frames future positions only.

                if (futureOccupied[0][targetx, targety] == true)
                {
                    Debug.LogWarning("Target Obstructed!");
                    break;
                }
                if (i >= queue.Count)
                {
                    Debug.LogWarning("Unable to path to destination!");
                    break;
                }

                int length = pathingLength[(int)queue[i].x, (int)queue[i].y];
                if (visited[(int)queue[i].x, (int)queue[i].y])
                {
                    // Is already visited, do not path
                }
                else if (futureOccupied[0][(int)queue[i].x, (int)queue[i].y] == true && queue[i].x != startx && queue[i].y != starty)
                {
                    // Do not path
                }
                else
                {
                    visited[(int)queue[i].x, (int)queue[i].y] = true;

                    if (queue[i].x == targetx && queue[i].y == targety)
                    {
                        ResolveRoute((int)queue[i].x, (int)queue[i].y);
                        break;
                    }

                    #region Cell Checking - 60 Frames Future
                    if (queue[i].x <= 45 * 2 - 4)
                    {
                        if (!visited[(int)queue[i].x + 4, (int)queue[i].y] && futureOccupied[0][(int)queue[i].x + 4, (int)queue[i].y] == false)
                        {

                            queue.Add(new Vector2(queue[i].x + 4, queue[i].y));
                            if (pathingLength[(int)queue[i].x + 4, (int)queue[i].y] > length + 1)
                            {
                                pathingLength[(int)queue[i].x + 4, (int)queue[i].y] = length + 1;
                                parent[(int)queue[i].x + 4, (int)queue[i].y] = new Vector2(queue[i].x, queue[i].y);
                            }
                        }
                    }
                    if (queue[i].y < 25 * 2 - 4)
                    {

                        if (!visited[(int)queue[i].x, (int)queue[i].y + 4] && futureOccupied[0][(int)queue[i].x, (int)queue[i].y + 4] == false)
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
                        if (!visited[(int)queue[i].x + 4, (int)queue[i].y + 4] && futureOccupied[0][(int)queue[i].x + 4, (int)queue[i].y + 4] == false)
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
                        if (!visited[(int)queue[i].x - 4, (int)queue[i].y] && futureOccupied[0][(int)queue[i].x - 4, (int)queue[i].y] == false)
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
                        if (!visited[(int)queue[i].x, (int)queue[i].y - 4] && futureOccupied[0][(int)queue[i].x, (int)queue[i].y - 4] == false)
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
                        if (!visited[(int)queue[i].x - 4, (int)queue[i].y - 4] && futureOccupied[0][(int)queue[i].x - 4, (int)queue[i].y - 4] == false)
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
                        if (!visited[(int)queue[i].x + 4, (int)queue[i].y - 4] && futureOccupied[0][(int)queue[i].x + 4, (int)queue[i].y - 4] == false)
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
                        if (!visited[(int)queue[i].x - 4, (int)queue[i].y + 4] && futureOccupied[0][(int)queue[i].x - 4, (int)queue[i].y + 4] == false)
                        {
                            queue.Add(new Vector2(queue[i].x - 4, queue[i].y + 4));
                            if (pathingLength[(int)queue[i].x - 4, (int)queue[i].y + 4] > length + 1)
                            {
                                pathingLength[(int)queue[i].x - 4, (int)queue[i].y + 4] = length + 1;
                                parent[(int)queue[i].x - 4, (int)queue[i].y + 4] = new Vector2(queue[i].x, queue[i].y);
                            }
                        }
                    }
                    #endregion
                }
            }
            else if (i > 512)
            {
                // This is identical to the above, however it uses the 120 frames future positions only.

                if (futureOccupied[1][targetx, targety] == true)
                {
                    Debug.LogWarning("Target Obstructed!");
                    break;
                }
                if (i >= queue.Count)
                {
                    Debug.LogWarning("Unable to path to destination!");
                    break;
                }

                int length = pathingLength[(int)queue[i].x, (int)queue[i].y];
                if (visited[(int)queue[i].x, (int)queue[i].y])
                {
                    // Do not path
                }
                else if (futureOccupied[1][(int)queue[i].x, (int)queue[i].y] == true && queue[i].x != startx && queue[i].y != starty)
                {
                    // Do not path
                }
                else
                {
                    visited[(int)queue[i].x, (int)queue[i].y] = true;

                    if (queue[i].x == targetx && queue[i].y == targety)
                    {
                        ResolveRoute((int)queue[i].x, (int)queue[i].y);
                        break;
                    }

                    #region Cell Checking - 120 Frames Future
                    if (queue[i].x <= 45 * 2 - 4)
                    {
                        if (!visited[(int)queue[i].x + 4, (int)queue[i].y] && futureOccupied[1][(int)queue[i].x + 4, (int)queue[i].y] == false)
                        {

                            queue.Add(new Vector2(queue[i].x + 4, queue[i].y));
                            if (pathingLength[(int)queue[i].x + 4, (int)queue[i].y] > length + 1)
                            {
                                pathingLength[(int)queue[i].x + 4, (int)queue[i].y] = length + 1;
                                parent[(int)queue[i].x + 4, (int)queue[i].y] = new Vector2(queue[i].x, queue[i].y);
                            }
                        }
                    }
                    if (queue[i].y < 25 * 2 - 4)
                    {

                        if (!visited[(int)queue[i].x, (int)queue[i].y + 4] && futureOccupied[1][(int)queue[i].x, (int)queue[i].y + 4] == false)
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
                        if (!visited[(int)queue[i].x + 4, (int)queue[i].y + 4] && futureOccupied[1][(int)queue[i].x + 4, (int)queue[i].y + 4] == false)
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
                        if (!visited[(int)queue[i].x - 4, (int)queue[i].y] && futureOccupied[1][(int)queue[i].x - 4, (int)queue[i].y] == false)
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
                        if (!visited[(int)queue[i].x, (int)queue[i].y - 4] && futureOccupied[1][(int)queue[i].x, (int)queue[i].y - 4] == false)
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
                        if (!visited[(int)queue[i].x - 4, (int)queue[i].y - 4] && futureOccupied[1][(int)queue[i].x - 4, (int)queue[i].y - 4] == false)
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
                        if (!visited[(int)queue[i].x + 4, (int)queue[i].y - 4] && futureOccupied[1][(int)queue[i].x + 4, (int)queue[i].y - 4] == false)
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
                        if (!visited[(int)queue[i].x - 4, (int)queue[i].y + 4] && futureOccupied[1][(int)queue[i].x - 4, (int)queue[i].y + 4] == false)
                        {
                            queue.Add(new Vector2(queue[i].x - 4, queue[i].y + 4));
                            if (pathingLength[(int)queue[i].x - 4, (int)queue[i].y + 4] > length + 1)
                            {
                                pathingLength[(int)queue[i].x - 4, (int)queue[i].y + 4] = length + 1;
                                parent[(int)queue[i].x - 4, (int)queue[i].y + 4] = new Vector2(queue[i].x, queue[i].y);
                            }
                        }
                    }
                    #endregion
                }
            }
            else
            {
                // This is identical to the above, however it uses the 180 frames future positions only.

                if (futureOccupied[2][targetx, targety] == true)
                {
                    Debug.LogWarning("Target Obstructed!");
                    break;
                }
                if (i >= queue.Count)
                {
                    Debug.LogWarning("Unable to path to destination!");
                    break;
                }

                int length = pathingLength[(int)queue[i].x, (int)queue[i].y];
                if (visited[(int)queue[i].x, (int)queue[i].y])
                {
                    // Do not path
                }
                else if (futureOccupied[2][(int)queue[i].x, (int)queue[i].y] == true && queue[i].x != startx && queue[i].y != starty)
                {
                    // Do not path
                }
                else
                {
                    visited[(int)queue[i].x, (int)queue[i].y] = true;

                    if (queue[i].x == targetx && queue[i].y == targety)
                    {
                        ResolveRoute((int)queue[i].x, (int)queue[i].y);
                        break;
                    }
                    #region Cell Checking - 180 Frames Future
                    if (queue[i].x <= 45 * 2 - 4)
                    {
                        if (!visited[(int)queue[i].x + 4, (int)queue[i].y] && futureOccupied[2][(int)queue[i].x + 4, (int)queue[i].y] == false)
                        {

                            queue.Add(new Vector2(queue[i].x + 4, queue[i].y));
                            if (pathingLength[(int)queue[i].x + 4, (int)queue[i].y] > length + 1)
                            {
                                pathingLength[(int)queue[i].x + 4, (int)queue[i].y] = length + 1;
                                parent[(int)queue[i].x + 4, (int)queue[i].y] = new Vector2(queue[i].x, queue[i].y);
                            }
                        }
                    }
                    if (queue[i].y < 25 * 2 - 4)
                    {

                        if (!visited[(int)queue[i].x, (int)queue[i].y + 4] && futureOccupied[2][(int)queue[i].x, (int)queue[i].y + 4] == false)
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
                        if (!visited[(int)queue[i].x + 4, (int)queue[i].y + 4] && futureOccupied[2][(int)queue[i].x + 4, (int)queue[i].y + 4] == false)
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
                        if (!visited[(int)queue[i].x - 4, (int)queue[i].y] && futureOccupied[2][(int)queue[i].x - 4, (int)queue[i].y] == false)
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
                        if (!visited[(int)queue[i].x, (int)queue[i].y - 4] && futureOccupied[2][(int)queue[i].x, (int)queue[i].y - 4] == false)
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
                        if (!visited[(int)queue[i].x - 4, (int)queue[i].y - 4] && futureOccupied[2][(int)queue[i].x - 4, (int)queue[i].y - 4] == false)
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
                        if (!visited[(int)queue[i].x + 4, (int)queue[i].y - 4] && futureOccupied[2][(int)queue[i].x + 4, (int)queue[i].y - 4] == false)
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
                        if (!visited[(int)queue[i].x - 4, (int)queue[i].y + 4] && futureOccupied[2][(int)queue[i].x - 4, (int)queue[i].y + 4] == false)
                        {
                            queue.Add(new Vector2(queue[i].x - 4, queue[i].y + 4));
                            if (pathingLength[(int)queue[i].x - 4, (int)queue[i].y + 4] > length + 1)
                            {
                                pathingLength[(int)queue[i].x - 4, (int)queue[i].y + 4] = length + 1;
                                parent[(int)queue[i].x - 4, (int)queue[i].y + 4] = new Vector2(queue[i].x, queue[i].y);
                            }
                        }
                    }
                    #endregion
                }
            }
        }
    }

    // Resolve the route found.
    void ResolveRoute(int x, int y)
    {
        route.Clear();
        Vector2 currentCoord = parent[x, y];
        route.Add(new Vector3(x - 45, y - 25, 0));
        // Set back through each cell from the target cell, through each cells "parent" (shortest previous cell) until the starting cell is found.
        // This produces the shortest path between the two positions
        while (currentCoord.x != -1 && currentCoord.y != -1)
        {
            // Add the cell to the route, converting the coordinate from list space to game space (Game space is -45 to 45 X, -25 to 25 Y)
            route.Add(new Vector3(currentCoord.x - 45, currentCoord.y - 25, 0));
            currentCoord = parent[(int)currentCoord.x, (int)currentCoord.y];
        }
        Debug.Log("Route Pathed!");
        found = true;
    }
    
    #region Debug Drawing
    private void OnDrawGizmos()
    {
        if (gz.drawGrid)
        {
            for (int x = 0; x < totX; x += 4)
            {
                for (int y = 0; y < totY; y += 4)
                {
                    if (occupied[x, y] == 1)
                    {
                        Gizmos.color = Color.yellow;
                    }
                    else if (visited[x, y] == true && gz.drawPathGrid)
                    {
                        Gizmos.color = Color.green;
                    }
                    else if (futureOccupied[0][x, y] == true && gz.drawFutureOccupancy)
                    {
                        Gizmos.color = Color.green;
                    }
                    else if (futureOccupied[1][x, y] == true && gz.drawFutureOccupancy)
                    {
                        Gizmos.color = Color.magenta;
                    }
                    else if (futureOccupied[2][x, y] == true && gz.drawFutureOccupancy)
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Color.blue;
                    }

                    Gizmos.DrawWireCube(new Vector3(x - 45, y - 25, 0), new Vector3(4f, 4f, 0));
                }
            }
        }

        // Draw route in gizmo
        if (found && gz.drawPath)
        {
            for (int i = 0; i < route.Count; i++)
            {
                if (i != route.Count - 1)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(route[i], route[i + 1]);
                }
            }
        }

    }

    // Sets up gizmo objects to draw the same information as gizmos, however as it uses a line renderer it works in the build
    void GizmoDraw()
    {
        if (gz.drawGrid)
        {
            if (!debugSpawned)
            {
                debugSpawn = new GameObject[(int)totX, (int)totY];
                for (int x = 0; x < totX; x += 4)
                {
                    for (int y = 0; y < totY; y += 4)
                    {
                        debugSpawn[x, y] = Instantiate(debugObj, this.transform.position, this.transform.rotation);
                        debugSpawn[x, y].transform.SetParent(this.transform);
                        dblr = debugSpawn[x, y].GetComponent<LineRenderer>();

                        List<Vector3> debugVerts = new List<Vector3>();
                        debugVerts.Add(new Vector3(x - 45 + 2, y - 25 + 2));
                        debugVerts.Add(new Vector3(x - 45 + 2, y - 25 - 2));
                        debugVerts.Add(new Vector3(x - 45 - 2, y - 25 - 2));
                        debugVerts.Add(new Vector3(x - 45 - 2, y - 25 + 2));

                        dblr.positionCount = 4;
                        dblr.SetPositions(ListToArrayVec3(debugVerts));
                        localDebug = ListToArrayVec3(debugVerts);
                        dblr.loop = true;
                    }
                }

                debugSpawned = true;
            }
            else
            {
                for (int x = 0; x < totX; x += 4)
                {
                    for (int y = 0; y < totY; y += 4)
                    {
                        Gradient gradient = new Gradient();
                        dblr = debugSpawn[x, y].GetComponent<LineRenderer>();
                        if (occupied[x, y] == 1)
                        {
                            gradient.SetKeys(
                            new GradientColorKey[] { new GradientColorKey(Color.yellow, 0.0f), new GradientColorKey(Color.yellow, 1f) },
                            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1f) }
                            );
                            dblr.colorGradient = gradient;
                        }
                        else if (visited[x, y] == true && gz.drawPathGrid)
                        {
                            gradient.SetKeys(
                            new GradientColorKey[] { new GradientColorKey(Color.green, 0f), new GradientColorKey(Color.green, 1f) },
                            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1f) }
                            );
                            dblr.colorGradient = gradient;
                        }
                        else if (futureOccupied[0][x, y] == true && gz.drawFutureOccupancy)
                        {
                            gradient.SetKeys(
                            new GradientColorKey[] { new GradientColorKey(Color.green, 0f), new GradientColorKey(Color.green, 1f) },
                            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1f) }
                            );
                            dblr.colorGradient = gradient;
                        }
                        else if (futureOccupied[1][x, y] == true && gz.drawFutureOccupancy)
                        {
                            gradient.SetKeys(
                            new GradientColorKey[] { new GradientColorKey(Color.magenta, 0f), new GradientColorKey(Color.magenta, 1f) },
                            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1f) }
                            );
                            dblr.colorGradient = gradient;
                        }
                        else if (futureOccupied[2][x, y] == true && gz.drawFutureOccupancy)
                        {
                            gradient.SetKeys(
                            new GradientColorKey[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.red, 1f) },
                            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1f) }
                            );
                            dblr.colorGradient = gradient;
                        }
                        else
                        {
                            gradient.SetKeys(
                            new GradientColorKey[] { new GradientColorKey(Color.blue, 0f), new GradientColorKey(Color.blue, 1f) },
                            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1f) }
                            );
                            dblr.colorGradient = gradient;
                        }
                    }
                }


                
            }
        }

        if (!gz.drawGrid && debugSpawned)
                {
                    for (int x = 0; x < totX; x += 4)
                    {
                        for (int y = 0; y < totY; y += 4)
                        {
                            Destroy(debugSpawn[x, y]);
                        }
                    }
                    debugSpawned = false;
                }

        if (found && gz.drawPath)
        {
            if (!pathDebugSpawned)
            {
                pathDebugSpawn = Instantiate(debugObj, this.transform.position, this.transform.rotation);
                pathDebugSpawn.transform.SetParent(this.transform);
                pathDebugSpawned = true;
            }
            else{
                Vector3[] routeDebug = new Vector3[route.Count];
                for (int i = 0; i < route.Count; i++)
                {
                    if (i != route.Count - 1)
                    {                        
                        routeDebug[i] = route[i];
                    }
                }
                LineRenderer dblr2 = pathDebugSpawn.GetComponent<LineRenderer>();
                dblr2.positionCount = route.Count-1;
                dblr2.SetPositions(routeDebug);
                dblr2.loop = false;
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.cyan, 0f), new GradientColorKey(Color.red, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1f) }
                    );
                dblr2.colorGradient = gradient;
            }
            
        }
        if (!gz.drawPath && pathDebugSpawned)
            {
                Destroy(pathDebugSpawn);
                pathDebugSpawned = false;
            }
    }
    #endregion
    
    private UnityEngine.Vector3[] ListToArrayVec3(List<UnityEngine.Vector3> l)
    {
        UnityEngine.Vector3[] temp = new UnityEngine.Vector3[l.Count];
        for (int i = 0; i < l.Count; i++)
        {
            temp[i] = l[i];
        }
        return temp;
    }
}