using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/*
注意点
> cameraAxisY の子に cameraAxisX の子に MainCamera を入れること
> MainCamera にこのスクリプトをつけること
> InputAction の辺りは文字列で参照しています
> 視野角は水平にしてください
*/

public class CameraController : MonoBehaviour
{
    [Header("Status")]
    [Tooltip("カメラのマウス感度")]
    [SerializeField] float mouseSensitivity = 17.5f;
    [Tooltip("カメラのスティック感度")]
    [SerializeField] float gamepadSensitivity = 175.0f;
    [Tooltip("カメラの位置オフセット")]
    [SerializeField] Vector2 cameraPotisionOffset = new Vector2(1.0f, 1.0f);
    [Tooltip("被写体からカメラまでの最大距離")]
    [SerializeField] float maxCameraDistance = 7.5f;
    [Tooltip("カメラの Y 軸回転角度上限")]
    [SerializeField] float cameraAngleLimitY = 67.5f;
    [Tooltip("カメラが貫通しないレイヤー")]
    [SerializeField] LayerMask castLayer;
    [Tooltip("ブースト時に増加するFOV")]
    [SerializeField] float boostFOV = 30.0f;

    [Header("Reference")]
    [Tooltip("カメラ")]
    [SerializeField] Camera playerCamera;
    [Tooltip("カメラが Y 軸で回転するための EmptyObject")]
    [SerializeField] Transform cameraAxisY;
    [Tooltip("カメラが X 軸で回転するための EmptyObject")]
    [SerializeField] Transform cameraAxisX;
    [Tooltip("被写体")]
    [SerializeField] Transform followTarget;
    [Tooltip("PlayerInput")]
    [SerializeField] PlayerInput playerInput;
    [Tooltip("マウス感度調整用スライダー")]
    [SerializeField] Slider mouseSensitivitySlider;
    [Tooltip("スティック感度調整用スライダー")]
    [SerializeField] Slider stickSensitivitySlider;

    // 内部処理用、現在のカメラ感度
    float cameraSensitivity;

    float defaultFOV;

    // カメラの回転用入力
    Vector2 cameraRotateInput;

    // 回転用
    float cameraX;

    // 入力用
    void OnEnable()
    {
        playerInput.actions["Look"].performed += OnLook;
        playerInput.actions["Look"].canceled += OnLook;
    }

    // カメラ回転用の入力があった場合は呼び出される
    void OnLook(InputAction.CallbackContext context)
    {
        // 入力された値を回転用の変数に
        cameraRotateInput = context.ReadValue<Vector2>();
    }



    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // マウスカーソルを固定
        Cursor.visible = false; // マウスカーソルを非表示

        transform.position = Vector3.zero; // MainCamera の位置を初期化
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f); // MainCamera の回転を初期化
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



    // 現在接続されているデバイスを確かめる
    void CurrentDeviceCheck()
    {
        if (playerInput.currentControlScheme == "Keyboard") cameraSensitivity = mouseSensitivity;
        if (playerInput.currentControlScheme == "Gamepad") cameraSensitivity = gamepadSensitivity;
    }

    void MoveCamera()
    {
        // Y 軸の処理
        cameraAxisY.position = new Vector3(followTarget.position.x, followTarget.position.y + cameraPotisionOffset.y, followTarget.position.z);
        cameraAxisY.Rotate(Vector3.up, cameraRotateInput.x * cameraSensitivity * Time.deltaTime);

        // X 軸の処理
        if (maxCameraDistance == 0.0f) cameraAxisX.localPosition = Vector3.zero;
        else cameraAxisX.localPosition = new Vector3(cameraPotisionOffset.x, 0.0f, 0.0f);
        cameraX = Mathf.Clamp(cameraX - (cameraRotateInput.y * cameraSensitivity * Time.deltaTime), -cameraAngleLimitY, cameraAngleLimitY);
        cameraAxisX.localRotation = Quaternion.Euler(cameraX, 0.0f, 0.0f);

        // オブジェクトを貫通しない用
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
