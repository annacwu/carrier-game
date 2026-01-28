using Unity.VisualScripting;
using UnityEngine;

public class Physics : MonoBehaviour
{

    public float distanceToCheck = 0.25f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    //returns T/F if object is grounded
    public bool Grounded (Transform objTransform, Vector2 boxPoint, Vector2 boxSize, float boxAngle)
    {   

        // Debug.DrawRay(objTransform.position, Vector3.down, Color.black);
        //Debug.Log(objTransform.position);

        return Physics2D.OverlapBox(boxPoint, boxSize, boxAngle);
    }
}
