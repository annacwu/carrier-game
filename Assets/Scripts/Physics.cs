using Unity.VisualScripting;
using UnityEngine;

public class Physics : MonoBehaviour
{

    Collider2D[] results; //array that stores colliders the box comes into contact with
    
    //filter for filtering out types of colliders that player can come into contact with
    //can use to, for example, filter obstacle vs wall colliders for different behaviors 
    //(currently just ignored, as no colliders have a filter yet)
    //ContactFilter2D filter; 


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
    
    //returns T/F if object is grounded

    public Collider2D Grounded (Transform objTransform, Vector2 boxPoint, Vector2 boxSize, float boxAngle)
    {   

        if (Physics2D.OverlapBox(boxPoint, boxSize, boxAngle, ContactFilter2D.noFilter, results) > 0)
        {
            return results[0];
        }


        return null;
        // Debug.DrawRay(objTransform.position, Vector3.down, Color.black);
        //Debug.Log(objTransform.position);
    }

    //sends rays out from object based on velocity to see whether it will hit anything this frame
    //currently gets all colliders, but should be modified to filter only ground once we implement that
    //returns coordinate of impact point (i.e. where the player should snap to)
    public Vector2 VerticalImpact (Transform objTransform, Vector2 objVelocity, int numRays)
    {
        //distance: pythagorean theorem
        //float maxDist = Mathf.Sqrt((objVelocity.x * objVelocity.x) + (objVelocity.y * objVelocity.y));
        float maxDist = objVelocity.magnitude;
        //Debug.Log("max distance is " + maxDist);

        //distance that closest collider is to the player
        float minDist = Mathf.Infinity;
        //Vector2 will return a zero vector if there is no result
        Vector2 returnVector = Vector2.zero;

        //get height and width beforehand for readability        
        float height = objTransform.localScale.y;
        float width = objTransform.localScale.x;
        
        for (int i = 0; i < numRays; i++)
        {   
            //Debug.Log("creating rays!");
            float offset = -(width / 2.0f) + (width / numRays * i);
            //where each ray should start - basically, start at left corner of box and then move right in increments based on numRays
            Vector2 origin =  new Vector2(objTransform.position.x + offset, objTransform.position.y - (height / 2.0f));

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
                    float newX = hitPoint.x - offset;
                    returnVector = new Vector2(newX, hitPoint.y);
                }
            }
        }
        
        return returnVector;
    }


}
