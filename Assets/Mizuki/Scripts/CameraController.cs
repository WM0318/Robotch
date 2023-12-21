using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float cameraSensitivity = 175.0f;
    [SerializeField] Vector2 cameraPositionOffsets = new Vector2(1.0f, 1.0f);
    [SerializeField] float maxCameraDistance = 7.5f;
    [SerializeField] float cameraAngleLimitY = 67.5f;
    [SerializeField] LayerMask castLayer;

    [SerializeField] Transform cameraAxisY;
    [SerializeField] Transform cameraAxisX;
    [SerializeField] Transform followTarget;

    Vector2 cameraRotateInput;

    float cameraX;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        transform.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();
        MoveCamera();
    }



    void Inputs()
    {
        cameraRotateInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    void MoveCamera()
    {
        cameraAxisY.position = new Vector3(followTarget.position.x, followTarget.position.y + cameraPositionOffsets.y, followTarget.position.z);
        cameraAxisY.Rotate(Vector3.up, cameraRotateInput.x * cameraSensitivity * Time.deltaTime);

        if (maxCameraDistance == 0.0f) cameraAxisX.localPosition = Vector3.zero;
        else cameraAxisX.localPosition = new Vector3(cameraPositionOffsets.x, 0.0f, 0.0f);
        cameraX = Mathf.Clamp(cameraX - (cameraRotateInput.y * cameraSensitivity * Time.deltaTime), -cameraAngleLimitY, cameraAngleLimitY);
        cameraAxisX.localRotation = Quaternion.Euler(cameraX, 0.0f, 0.0f);

        if (Physics.Raycast(cameraAxisX.position, -cameraAxisX.forward, out RaycastHit cameraCollision, maxCameraDistance, castLayer))
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -cameraCollision.distance);
        }
        else transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -maxCameraDistance);
    }
}
