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
}
