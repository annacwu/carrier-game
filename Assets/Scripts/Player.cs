using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float jumpForce;
    public float moveSpeed = 5;
    public int numRays;
    public float gravity = -9.81f;
    public bool useForgivingJumps = false;
    public int forgivenessFactor = 1; //determines how many frames before hitting the ground u can press jump
    private bool jumpSaved = false;
    public Vector2 moveVal;
    private int tick = 0;

    private float verticalVelocity; //stores vertical velocity
    private float horizontalVelocity; //stores horizontal velocity - currently set to 0 since we don't have that, BUT MAKE SURE TO USE THIS VARIABLE ONCE WE DO!!
    public GameObject physicsSystem;
    private Physics physicsScript;

    //public float playerHeight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        physicsScript = physicsSystem.GetComponent<Physics>();
    }

    void Update()
    {
        HandleForgivingJumps();
        ApplyGravityAndMove();
        HandleMovement();
    }

    void HandleMovement()
    {
        transform.Translate(new Vector3(moveVal.x, moveVal.y, 0) * moveSpeed * Time.deltaTime);
    }

    void HandleForgivingJumps()
    {
        //logic for forgetting saved jumps if they are not executed in time
        if (jumpSaved && tick <= forgivenessFactor)
        {
            tick += 1;
        }
        else if (jumpSaved)
        {
            tick = 0;
            jumpSaved = false;
        }

        //if there is a 'saved' jump, execute it
        Collider2D groundCollider = physicsScript.Grounded(
            transform.position,
            transform.localScale,
            0
        );
        if (jumpSaved && groundCollider)
        {
            jumpSaved = false;
            tick = 0;
            verticalVelocity += jumpForce;
        }
    }

    void ApplyGravityAndMove()
    {
        verticalVelocity += gravity * Time.deltaTime;

        //calculated distance object will move in next frame
        Vector2 prelimTravel = new Vector2(
            horizontalVelocity * Time.deltaTime,
            verticalVelocity * Time.deltaTime
        );
        CollisionDirection impact;
        Vector2 hitLoc = physicsScript.Impact(transform, prelimTravel, numRays, out impact); // gets point of potential impact

        if (impact != CollisionDirection.None)
        {
            ResolveCollision(prelimTravel, hitLoc, impact);
        }
        else
        {
            transform.Translate(new Vector2(horizontalVelocity, verticalVelocity) * Time.deltaTime); //if no impact, move as normal
        }
    }

    void ResolveCollision(Vector2 prelimTravel, Vector2 hitLoc, CollisionDirection impact)
    {
        Vector2 oldPos = transform.position;
        Vector2 snapTravel = hitLoc - oldPos; // distance that (if we hit something) was travelled

        transform.position = hitLoc; //make snap

        float remainingVertical = prelimTravel.y - snapTravel.y;
        float remainingHorizontal = prelimTravel.y - snapTravel.y;
        Vector2 remainingTravel = new Vector2(remainingHorizontal, remainingVertical); //calculate remaining distance you would have moved if not for the snap

        Vector2 secondHitLoc;
        CollisionDirection secondImpact;

        if (impact == CollisionDirection.Horizontal)
        {
            horizontalVelocity = 0.0f;
            remainingTravel.x = 0; //if we made a horizontal impact, we should not travel any more on that axis
            secondHitLoc = physicsScript.Impact(
                transform,
                new Vector2(0, remainingVertical),
                numRays,
                out secondImpact
            );
        }
        else
        {
            verticalVelocity = 0.0f;
            remainingTravel.y = 0;
            secondHitLoc = physicsScript.Impact(
                transform,
                new Vector2(remainingHorizontal, 0),
                numRays,
                out secondImpact
            );
        }

        //if there was a second impact, we can safely set all velocity to 0, since we know we hit colliders in both directions of travel
        if (secondImpact != CollisionDirection.None)
        {
            transform.position = secondHitLoc; //add new snap travel
            verticalVelocity = 0;
            horizontalVelocity = 0;
            remainingTravel = Vector2.zero; //set all velocity things to 0
        }

        //move only in direction where we didn't initially make impact
        transform.Translate(remainingTravel);
    }

    //onJump still uses Grounded since we want u to be currently on the floor and not just about to hit it
    //but doesn't need to, especially if we want to do something like forgiving jumps
    //(i added a toggle to test this)
    void OnJump()
    {
        //if player presses jump button and they are currently in the air, 'save' their input for a few frames (based on forgiveness factor)
        //if they end up on the ground in any of those frames, "forgive" their jump (i.e. execute it)
        if (useForgivingJumps)
        {
            jumpSaved = true;
        }
        else
        {
            if (physicsScript.Grounded(transform.position, transform.localScale, 0))
            {
                verticalVelocity += jumpForce;
            }
        }
    }

    void OnMove(InputValue val)
    {
        moveVal = val.Get<Vector2>();
        Debug.Log("OnMove called: " + moveVal);
    }
}
