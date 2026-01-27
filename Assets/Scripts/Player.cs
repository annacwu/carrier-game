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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        physicsScript = physicsSystem.GetComponent<Physics>();

    }

    // Update is called once per frame
    void Update()
    {

        verticalVelocity += gravity * Time.deltaTime;
        
        if (physicsScript.Grounded(transform, transform.position, transform.localScale, 0) && verticalVelocity <= 0)
        {
            Debug.Log("setting velocity to 0");
            verticalVelocity = 0.0f;

        }

        Debug.Log("updating velocity");
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
