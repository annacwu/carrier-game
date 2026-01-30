using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    public float jumpForce;
    public int numRays; 
    public float gravity = -9.81f;
    public bool useForgivingJumps = false;
    public int forgivenessFactor = 1; //determines how many frames before hitting the ground u can press jump

    private bool jumpSaved = false;
    private int tick = 0;

    private float verticalVelocity; //stores vertical velocity
    private float horizontalVelocity = 0; //stores horizontal velocity - currently set to 0 since we don't have that, BUT MAKE SURE TO USE THIS VARIABLE ONCE WE DO!!
    public GameObject physicsSystem;
    private Physics physicsScript;

    //public float playerHeight; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        physicsScript = physicsSystem.GetComponent<Physics>();

    }

    // Update is called once per frame
    void Update()
    {
        //logic for forgetting saved jumps if they are not executed in time
        if (jumpSaved && tick <= forgivenessFactor)
        {
            tick += 1;
        } else if (jumpSaved)
        {
            tick = 0;
            jumpSaved = false;
        }

        verticalVelocity += gravity * Time.deltaTime;
        
        //calculated distance object will move in next frame
        Vector2 prelimTravel = new Vector2(horizontalVelocity * Time.deltaTime, verticalVelocity * Time.deltaTime);
        Vector2 hitLoc = physicsScript.VerticalImpact(transform, prelimTravel, numRays); //gets point of potential impact

        //Collider2D groundCollider = physicsScript.Grounded(transform, transform.position, transform.localScale, 0);
        
        if (hitLoc != Vector2.zero && verticalVelocity <= 0)
        {
            // Debug.Log("setting velocity to 0");
            //Vector2 groundPos = Physics2D.ClosestPoint(transform.position, groundCollider);
            transform.position = new Vector2(hitLoc.x, hitLoc.y + (transform.localScale.y / 2));
            verticalVelocity = 0.0f;

            //if there is a 'saved' jump, execute it
            if (jumpSaved)
            {
                jumpSaved = false;
                tick = 0;
                verticalVelocity += jumpForce;
            }

        }

        // Debug.Log("updating velocity");
        transform.Translate(new Vector2(0, verticalVelocity) * Time.deltaTime);

    }

    //onJump still uses Grounded since we want u to be currently on the floor and not just about to hit it
    //but doesn't need to, especially if we want to do something like forgiving jumps
    //(i added a toggle to test this)
    void OnJump ()
    {
        //if player presses jump button and they are currently in the air, 'save' their input for a few frames (based on forgiveness factor)
        //if they end up on the ground in any of those frames, "forgive" their jump (i.e. execute it)
        if (useForgivingJumps)
        {
            jumpSaved = true;

        } else
        {
            if (physicsScript.Grounded(transform, transform.position, transform.localScale, 0))
            {
                verticalVelocity += jumpForce;
            }
        }

    }
}
