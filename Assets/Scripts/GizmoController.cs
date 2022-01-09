using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoController : MonoBehaviour
{
    public bool drawGrid = false;
    public bool drawPathGrid = false;
    public bool drawPath = false;
    public bool drawFutureVector = false;
    public bool drawFutureOccupancy = false;
    public bool drawCollisionBounds = false;
    public bool drawBroadPhaseBounds = false;
    public bool drawPickups = false;
    public bool drawEnemyView = false;

    bool doDebug = false;

    void Update()
    {
        if(Input.GetKeyDown("d")){
            if(!doDebug){
                doDebug = true;
            }
            else{
                doDebug = false;
            }
        }
    }
    void OnGUI ()
    {
        if(doDebug){
            // Make a background box
            GUI.Box(new Rect(10,10,150,210), "Debug Menu");

            if(GUI.Button(new Rect(20,40,130,20), "Broad Phase")) 
            {
                if(drawBroadPhaseBounds){
                    drawBroadPhaseBounds = false;
                }
                else{
                    drawBroadPhaseBounds = true;
                }
            }

            if(GUI.Button(new Rect(20,70,130,20), "Draw Grid")) 
            {
                if(drawGrid){
                    drawGrid = false;
                }
                else{
                    drawGrid = true;
                }
            }

            if(GUI.Button(new Rect(20,100,130,20), "Draw Path")) 
            {
                if(drawPath){
                    drawPath = false;
                }
                else{
                    drawPath = true;
                }
            }

            if(GUI.Button(new Rect(20,130,130,20), "Pathfind Grid")) 
            {
                if(drawPathGrid){
                    drawPathGrid = false;
                }
                else{
                    drawPathGrid = true;
                }
            }

            if(GUI.Button(new Rect(20,160,130,20), "Future Occupancy")) 
            {
                if(drawFutureOccupancy){
                    drawFutureOccupancy = false;
                }
                else{
                    drawFutureOccupancy = true;
                }
            }

            if(GUI.Button(new Rect(20,190,130,20), "Future Vector"))
            {
                if(drawFutureVector){
                    drawFutureVector = false;
                }
                else{
                    drawFutureVector = true;
                }
            }
        }        
    }
}
