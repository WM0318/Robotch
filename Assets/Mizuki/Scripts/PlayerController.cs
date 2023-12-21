using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float movementSpeed = 5.0f;
    [SerializeField] float turnSpeed = 0.1f;

    [SerializeField] Rigidbody playerBody;
    [SerializeField] Transform cameraAxisY;

    Vector2 movementInput;

    float turnSmoothVelocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();
        RotatePlayer();
        SpeedControl();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }



    void Inputs()
    {
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    void RotatePlayer()
    {
        if (movementInput != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + cameraAxisY.eulerAngles.y;

            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSpeed);
            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
        }
    }

    void MovePlayer()
    {
        Vector3 moveDirection = cameraAxisY.right * movementInput.x + cameraAxisY.forward * movementInput.y;
        playerBody.AddForce(moveDirection * movementSpeed * 10.0f, ForceMode.Force);
    }

    void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(playerBody.velocity.x, 0.0f, playerBody.velocity.z);

        if (flatVelocity.magnitude > movementSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * movementSpeed;
            playerBody.velocity = new Vector3(limitedVelocity.x, playerBody.velocity.y, limitedVelocity.z);
        }
    }
}
