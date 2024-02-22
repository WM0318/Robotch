using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/*
���ӓ_
> cameraAxisY �̎q�� cameraAxisX �̎q�� MainCamera �����邱��
> MainCamera �ɂ��̃X�N���v�g�����邱��
> InputAction �̕ӂ�͕�����ŎQ�Ƃ��Ă��܂�
> ����p�͐����ɂ��Ă�������
*/

public class CameraController : MonoBehaviour
{
    [Header("Status")]
    [Tooltip("�J�����̃}�E�X���x")]
    [SerializeField] float mouseSensitivity = 17.5f;
    [Tooltip("�J�����̃X�e�B�b�N���x")]
    [SerializeField] float gamepadSensitivity = 175.0f;
    [Tooltip("�J�����̈ʒu�I�t�Z�b�g")]
    [SerializeField] Vector2 cameraPotisionOffset = new Vector2(1.0f, 1.0f);
    [Tooltip("��ʑ̂���J�����܂ł̍ő勗��")]
    [SerializeField] float maxCameraDistance = 7.5f;
    [Tooltip("�J������ Y ����]�p�x���")]
    [SerializeField] float cameraAngleLimitY = 67.5f;
    [Tooltip("�J�������ђʂ��Ȃ����C���[")]
    [SerializeField] LayerMask castLayer;
    [Tooltip("�u�[�X�g���ɑ�������FOV")]
    [SerializeField] float boostFOV = 30.0f;

    [Header("Reference")]
    [Tooltip("�J����")]
    [SerializeField] Camera playerCamera;
    [Tooltip("�J������ Y ���ŉ�]���邽�߂� EmptyObject")]
    [SerializeField] Transform cameraAxisY;
    [Tooltip("�J������ X ���ŉ�]���邽�߂� EmptyObject")]
    [SerializeField] Transform cameraAxisX;
    [Tooltip("��ʑ�")]
    [SerializeField] Transform followTarget;
    [Tooltip("PlayerInput")]
    [SerializeField] PlayerInput playerInput;
    [Tooltip("�}�E�X���x�����p�X���C�_�[")]
    [SerializeField] Slider mouseSensitivitySlider;
    [Tooltip("�X�e�B�b�N���x�����p�X���C�_�[")]
    [SerializeField] Slider stickSensitivitySlider;

    // ���������p�A���݂̃J�������x
    float cameraSensitivity;

    float defaultFOV;

    // �J�����̉�]�p����
    Vector2 cameraRotateInput;

    // ��]�p
    float cameraX;

    // ���͗p
    void OnEnable()
    {
        playerInput.actions["Look"].performed += OnLook;
        playerInput.actions["Look"].canceled += OnLook;
    }

    // �J������]�p�̓��͂��������ꍇ�͌Ăяo�����
    void OnLook(InputAction.CallbackContext context)
    {
        // ���͂��ꂽ�l����]�p�̕ϐ���
        cameraRotateInput = context.ReadValue<Vector2>();
    }



    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // �}�E�X�J�[�\�����Œ�
        Cursor.visible = false; // �}�E�X�J�[�\�����\��

        transform.position = Vector3.zero; // MainCamera �̈ʒu��������
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f); // MainCamera �̉�]��������
        defaultFOV = playerCamera.fieldOfView;
    }

    void Update()
    {
        CurrentDeviceCheck();
        CheckBoost();
        SetSensitivity();
    }

    void LateUpdate()
    {
        MoveCamera();
    }



    // ���ݐڑ�����Ă���f�o�C�X���m���߂�
    void CurrentDeviceCheck()
    {
        if (playerInput.currentControlScheme == "Keyboard") cameraSensitivity = mouseSensitivity;
        if (playerInput.currentControlScheme == "Gamepad") cameraSensitivity = gamepadSensitivity;
    }

    void MoveCamera()
    {
        // Y ���̏���
        cameraAxisY.position = new Vector3(followTarget.position.x, followTarget.position.y + cameraPotisionOffset.y, followTarget.position.z);
        cameraAxisY.Rotate(Vector3.up, cameraRotateInput.x * cameraSensitivity * Time.deltaTime);

        // X ���̏���
        if (maxCameraDistance == 0.0f) cameraAxisX.localPosition = Vector3.zero;
        else cameraAxisX.localPosition = new Vector3(cameraPotisionOffset.x, 0.0f, 0.0f);
        cameraX = Mathf.Clamp(cameraX - (cameraRotateInput.y * cameraSensitivity * Time.deltaTime), -cameraAngleLimitY, cameraAngleLimitY);
        cameraAxisX.localRotation = Quaternion.Euler(cameraX, 0.0f, 0.0f);

        // �I�u�W�F�N�g���ђʂ��Ȃ��p
        if (Physics.CheckBox(new Vector3(cameraAxisX.position.x, cameraAxisX.position.y, cameraAxisX.position.z),
                             new Vector3(playerCamera.nearClipPlane * Mathf.Tan((playerCamera.fieldOfView * Mathf.Deg2Rad) / 2.0f) * playerCamera.aspect,
                                         playerCamera.nearClipPlane * Mathf.Tan((playerCamera.fieldOfView * Mathf.Deg2Rad) / 2.0f),
                                         0.0f),
                             cameraAxisX.rotation,
                             castLayer))
        {
            transform.localPosition = new Vector3(-cameraPotisionOffset.x, transform.localPosition.y, 0.0f);
        }
        else if (Physics.BoxCast(new Vector3(cameraAxisX.position.x, cameraAxisX.position.y, cameraAxisX.position.z),
                                 new Vector3(playerCamera.nearClipPlane * Mathf.Tan(playerCamera.fieldOfView * Mathf.Deg2Rad / 2.0f) * playerCamera.aspect,
                                             playerCamera.nearClipPlane * Mathf.Tan(playerCamera.fieldOfView * Mathf.Deg2Rad / 2.0f),
                                             0.0f),
                                 -cameraAxisX.forward,
                                 out RaycastHit cameraCollision,
                                 cameraAxisX.rotation,
                                 maxCameraDistance,
                                 castLayer))
        {
            transform.localPosition = new Vector3(0.0f, transform.localPosition.y, -cameraCollision.distance - playerCamera.nearClipPlane);
        }
        else transform.localPosition = new Vector3(0.0f, transform.localPosition.y, -maxCameraDistance - playerCamera.nearClipPlane);
    }

    void CheckBoost()
    {
        if (followTarget.GetComponent<PlayerController>().isBoosting)
        {
            if (playerCamera.fieldOfView < defaultFOV + boostFOV)
            {
                playerCamera.fieldOfView += boostFOV * Time.deltaTime;
            }
            else playerCamera.fieldOfView = defaultFOV + boostFOV;
        }
        else
        {
            if (playerCamera.fieldOfView > defaultFOV)
            {
                playerCamera.fieldOfView -= boostFOV * Time.deltaTime;
            }
            else playerCamera.fieldOfView = defaultFOV;
        }
    }

    void SetSensitivity()
    {
        mouseSensitivity = mouseSensitivitySlider.value;
        gamepadSensitivity = stickSensitivitySlider.value;
    }
}
