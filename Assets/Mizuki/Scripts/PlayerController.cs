using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/*
注意点
 */

public class PlayerController : MonoBehaviour
{
    [Header("Status")]
    [Tooltip("歩行速度")]
    [SerializeField] float walkSpeed = 5.0f / 3.0f;
    [Tooltip("疾走速度")]
    [SerializeField] float sprintSpeed = 5.0f;
    [Tooltip("ブーストの消費量")]
    [SerializeField] float boostConsumption = 20.0f;
    [Tooltip("自機の回転速度")]
    [SerializeField] float turnSpeed = 0.1f;
    [Tooltip("接地判定用レイヤー")]
    [SerializeField] LayerMask groundLayer;
    [Tooltip("接地してる際の摩擦")]
    [SerializeField] float groundDrag = 3.0f;

    [Header("Reference")]
    [Tooltip("自機の Rigidbody")]
    [SerializeField] Rigidbody playerBody;
    [Tooltip("カメラが Y 軸で回転するための EmptyObject")]
    [SerializeField] Transform cameraAxisY;
    [Tooltip("PlayerInput")]
    [SerializeField] PlayerInput playerInput;
    [Tooltip("ブーストゲージ用テキスト")]
    [SerializeField] Text boostGaugeText;
    [Tooltip("ブーストゲージ用スライダー")]
    [SerializeField] Slider boostGaugeSlider;

    // 入力用
    Vector2 movementInput;
    bool isPressedBoost;

    // 移動とか用
    float movementSpeed;
    float boostGauge;

    public float boostCharge { set { boostGauge += value; } }

    bool isBoost;

    public bool isBoosting { get { return isBoost; } }

    // 接地判定用
    bool isGround;
    const float CHECK_SPHERE_SCALE = 0.1f;

    // SmoothDampAngle用
    float turnSmoothVelocity;

    const float MAX_BOOST_GAUGE = 100.0f;

    // 入力用
    void OnEnable()
    {
        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;

        playerInput.actions["Boost"].started += OnBoost;
        playerInput.actions["Boost"].canceled += OnBoost;
    }

    // 移動用の入力があった場合に呼び出し
    void OnMove(InputAction.CallbackContext context)
    {
        // 入力された値を移動用変数に
        movementInput = context.ReadValue<Vector2>();
    }

    // ジャンプキーが入力された場合に呼び出し
    void OnBoost(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                isPressedBoost = true;
                break;
            case InputActionPhase.Canceled:
                isPressedBoost = false;
                break;
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        playerBody.freezeRotation = true; // Rigidbody の回転固定
        boostGaugeSlider.maxValue = MAX_BOOST_GAUGE;
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckGround();
        RotatePlayer();
        SpeedControl();
        Boost();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }



    // 接地判定用
    void CheckGround()
    {
        // 接地判定
        isGround = Physics.CheckSphere(new Vector3(transform.position.x, transform.position.y - transform.lossyScale.y, transform.position.z), CHECK_SPHERE_SCALE, groundLayer);

        if(isGround) playerBody.drag = groundDrag; // 接地している場合は摩擦を groundDrag に
        else playerBody.drag = 0.0f; // 接地していない場合は摩擦を 0 に
    }

    // プレイヤー回転用
    void RotatePlayer()
    {
        // 入力があった場合は
        if (movementInput != Vector2.zero)
        {
            // 入力値から方向を算出
            float targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + cameraAxisY.eulerAngles.y;

            // 上記で算出された方向に向かってじわっと回転
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSpeed);
            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
        }
    }

    // プレイヤー移動用
    void MovePlayer()
    {
        // 移動したい方向を算出して移動
        Vector3 moveDirection = cameraAxisY.right * movementInput.x + cameraAxisY.forward * movementInput.y;
        playerBody.AddForce(moveDirection * movementSpeed * 10.0f, ForceMode.Force);
    }

    // 移動速度制限用
    void SpeedControl()
    {
        // X軸とZ軸だけ Vector3 で取得
        Vector3 flatVelocity = new Vector3(playerBody.velocity.x, 0.0f, playerBody.velocity.z);

        // 現在の速度が movementSpeed 以上だったら
        if (flatVelocity.magnitude > movementSpeed)
        {
            // 上限速度を算出して Velocity に代入
            Vector3 limitedVelocity = flatVelocity.normalized * movementSpeed;
            playerBody.velocity = new Vector3(limitedVelocity.x, playerBody.velocity.y, limitedVelocity.z);
        }
    }

    // ジャンプ用
    void Boost()
    {
        if (boostGauge > 0.0f && isPressedBoost && movementInput != Vector2.zero)
        {
            movementSpeed = sprintSpeed;

            boostGauge -= boostConsumption * Time.deltaTime;

            isBoost = true;
        }
        else
        {
            movementSpeed = walkSpeed;

            isBoost = false;
        }

        boostGaugeText.text = boostGauge.ToString("f1");
        boostGaugeSlider.value = boostGauge;
    }
}
