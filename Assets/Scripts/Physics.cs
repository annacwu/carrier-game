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

    /*
    //sends rays out from object based on velocity to see whether it will hit anything this frame
    //returns coordinate of impact point (i.e. where the player should snap to)
    //if you do get an impact, you need to call this function one more time to ensure there isn't a second impact in the same frame
    //(e.g. if you're above to hit a corner, both horizontal and vertical velocity could be enough to impact in the same frame)
    //I think 2 checks is enough for any scenario
    //objTransform: transform of the object which is checking if it is gonna hit anything else (for now, the player's transform)
    //objVelocity: the velocity of said object
    //numRays: the # of rays sent out from each side of the object (e.g. a value of '3' will send out a maxiumum of 3 rays from the horizontal edge and 3 from the vertical edge)
    out string direction: returns which axis Impact() detected a collision on ("horizontal", "vertical", or "none")
    */
    public Vector2 Impact (Transform objTransform, Vector2 objVelocity, int numRays, out string direction)
    {

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
            var (horizontalHit, side, vertOffset) = CheckAxis(0, objVelocity, objTransform, i, height, width, numRays);

            if (horizontalHit)
            {
                var (newReturn, newMin, updated) = CalculateHit(0, horizontalHit, side, vertOffset, minDist, height, width);
                if (updated) {
                    returnVector = newReturn;
                    minDist = newMin;
                    direction = "horizontal";
                }
            }

            //check for vertical collisions
            var (verticalHit, vert, horizontalOffset) = CheckAxis(1, objVelocity, objTransform, i, height, width, numRays);

            if (verticalHit)
            {
                var (newReturn, newMin, updated) = CalculateHit(1, verticalHit, vert, horizontalOffset, minDist, height, width);
                if (updated) {
                    returnVector = newReturn;
                    minDist = newMin;
                    direction = "vertical";
                }
            }

        }
        
        return returnVector;
    }
    

    // 0 for horizontal, 1 for vertical
    private (RaycastHit2D hit, int dir, float offset) CheckAxis(int axis, Vector2 objVelocity, Transform objTransform, int i, float height, float width, int numRays)
    {
        if (objVelocity[axis] != 0)
        {
            float maxDist = objVelocity.magnitude;
            // get direction (up/down, left/right)
            int dir = objVelocity[axis] < 0 ? -1 : 1; 
            // for x, the size of the player on the axis in the direction we are moving is the width. 
            float movementSize = axis == 0 ? width : height;
            // size of the player on the axis the rays will be spread
            float spreadSize = axis == 0 ? height : width;

            float offset = -(spreadSize / 2.0f) + (spreadSize / (numRays-1) * i);

            // calculate origin
            Vector2 origin = Vector2.zero; // initialize empty
            origin[axis] = objTransform.position[axis] + (movementSize / 2.0f * dir);
            origin[1 - axis] = objTransform.position[1 - axis] + offset;

            RaycastHit2D hit = Physics2D.Raycast(origin, objVelocity.normalized, maxDist);
            return (hit, dir, offset);
        }
        return (default, 0, 0);
    }

    // 0 for horizontal, 1 for vertical
    private (Vector2 collision, float minDist, bool updated) CalculateHit (int axis, RaycastHit2D hit, int dir, float offset, float currentMin, float height, float width)
    {
        if (currentMin > hit.distance) {
            //get point where that ray intersected with a collider
            Vector2 hitPoint = hit.point;
            float otherCoord = hitPoint[1 - axis] - offset;
            Vector2 returnVector = axis == 0 ?
                new Vector2(hitPoint[axis] - (width / 2 * dir), otherCoord) :
                new Vector2(otherCoord, hitPoint[axis] - (height / 2 * dir));

            return (returnVector, hit.distance, true);
        }
        return (default, currentMin, false);
    }   
}
