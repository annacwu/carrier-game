using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] Rigidbody2D playerRigidBody;
    [SerializeField] Transform playerTransform;

    InputAction moveAction;
    InputAction jumpAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 newVelocity = moveAction.ReadValue<Vector2>();

        if (jumpAction.IsPressed())
        {
            newVelocity += new Vector2(0.0f, 10.0f); //TODO: should only jump if u are touching the ground currently
        }

        

        Vector3 totalNewVelocity = newVelocity;
        playerTransform.position += totalNewVelocity * Time.deltaTime;


    }
}
