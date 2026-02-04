using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    public float jumpForce;
    public float gravity = -9.81f;
    private float verticalVelocity; 
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

        verticalVelocity += gravity * Time.deltaTime;
        Collider2D groundCollider = physicsScript.Grounded(transform, transform.position, transform.localScale, 0);
        
        if (groundCollider && verticalVelocity <= 0)
        {
            // Debug.Log("setting velocity to 0");
            Vector2 groundPos = Physics2D.ClosestPoint(transform.position, groundCollider);
            transform.position = new Vector2(transform.position.x, groundPos.y + (transform.localScale.y / 2));
            verticalVelocity = 0.0f;

        }

        // Debug.Log("updating velocity");
        transform.Translate(new Vector2(0, verticalVelocity) * Time.deltaTime);

    }

    void OnJump ()
    {
        if (physicsScript.Grounded(transform, transform.position, transform.localScale, 0))
        {
            verticalVelocity += jumpForce;
        }
    }
}
