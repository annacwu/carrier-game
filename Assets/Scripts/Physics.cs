using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Physics : MonoBehaviour
{

    Collider2D[] results; //array that stores colliders the box comes into contact with

    //stores the offset between the corner of the collider and where the first ray will be sent out from
    //purpose: fixes some edge cases where Impact() wouldn't know whether the collision is vertical or horizontal
    //problems: makes effective 'collider' smaller, needs to be synced up with smaller Grounded() box (NOT CURRENTLY IMPLEMENTED)
    //alternative(s): idk but worth looking into
    public float rayOffset = 0.01f; 

    public float distanceToCheck = 0.25f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        results = new Collider2D[1]; //initialize length to 1
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    //returns collider that OverlapBox hits
    public Collider2D Grounded (Vector2 boxPoint, Vector2 boxSize, float boxAngle)
    {   

        if (Physics2D.OverlapBox(boxPoint, boxSize, boxAngle, ContactFilter2D.noFilter, results) > 0)
        {
            return results[0];
        }


        return null;

    }

    //sends rays out from object based on velocity to see whether it will hit anything this frame
    //returns coordinate of impact point (i.e. where the player should snap to)
    //if you do get an impact, you need to call this function one more time to ensure there isn't a second impact in the same frame
    //(e.g. if you're above to hit a corner, both horizontal and vertical velocity could be enough to impact in the same frame)
    //I think 2 checks is enough for any scenario
    //objTransform: transform of the object which is checking if it is gonna hit anything else (for now, the player's transform)
    //objVelocity: the velocity of said object
    //numRays: the # of rays sent out from each side of the object (e.g. a value of '3' will send out a maxiumum of 3 rays from the horizontal edge and 3 from the vertical edge)
    //out string direction: returns which axis Impact() detected a collision on ("horizontal", "vertical", or "none")
    public Vector2 Impact (Transform objTransform, Vector2 objVelocity, int numRays, out string direction)
    {
        //length of the ray
        float maxDist = objVelocity.magnitude;
        //Debug.Log("max distance is " + maxDist);

        //distance that closest collider is to the player
        float minDist = Mathf.Infinity;
        //Vector2 will return a zero vector if there is no result
        Vector2 returnVector = Vector2.zero;

        //get height and width beforehand for readability   
        //corrected for rayOffset     
        float height = objTransform.localScale.y - rayOffset * 2;
        float width = objTransform.localScale.x - rayOffset * 2;

        direction = "none";
        
        for (int i = 0; i < numRays; i++)
        {   

            //check for horizontal collisions
            if (objVelocity.x != 0) //there is horizontal movement
            {
                //if going right, this will be positive
                int side = objVelocity.x < 0 ? -1 : 1;

                float vertOffset = -(height / 2.0f) + (height / (numRays-1) * i); //position of bottom edge of the player
                Vector2 vertOrigin =  new Vector2(objTransform.position.x + (width / 2.0f * side), objTransform.position.y + vertOffset); //origin of each ray
                RaycastHit2D horizontalHit = Physics2D.Raycast(vertOrigin, objVelocity.normalized, maxDist);

                if (horizontalHit)
                {
                    if (minDist > horizontalHit.distance)
                    {
                        minDist = horizontalHit.distance;
                        //get point where that ray intersected with a collider
                        Vector2 horizontalHitPoint = horizontalHit.point;
                        float newY = horizontalHitPoint.y - vertOffset;
                        //return point where player should snap to (adjusted for initial vertical offest of ray that was sent out)
                        
                        returnVector = new Vector2(horizontalHitPoint.x - (width / 2 * side), newY);
                        direction = "horizontal";
                    }
                }

            }
            
            //check for vertical collisions
            if (objVelocity.y != 0)
            {

                int vert = objVelocity.y < 0 ? -1 : 1;
                
                //Debug.Log("creating rays!");
                float horizontalOffset = -(width / 2.0f) + (width / (numRays-1) * i);
                //where each ray should start - basically, start at left corner of box and then move right in increments based on numRays
                Vector2 origin =  new Vector2(objTransform.position.x + horizontalOffset, objTransform.position.y + (height / 2.0f * vert));

                //currently no layerMask bc we haven't implemented that, but seems to be an int - so colliders would have to be on different layers?
                RaycastHit2D hit = Physics2D.Raycast(origin, objVelocity.normalized, maxDist);
                
                //if we hit something, if it is the closest thing so far, we should return it
                if (hit)
                {
                    if (minDist > hit.distance)
                    {
                        minDist = hit.distance;
                        //get point where that ray intersected with a collider
                        Vector2 hitPoint = hit.point;
                        float newX = hitPoint.x - horizontalOffset;
                        returnVector = new Vector2(newX, hitPoint.y - (height / 2 * vert));
                        direction = "vertical";
                    }
                }
            }

        }
        
        return returnVector;
    }


}
