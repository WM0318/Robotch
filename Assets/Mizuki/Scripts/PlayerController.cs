using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/*
注意点
 */

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("歩行速度")]
    [SerializeField] float walkSpeed = 5.0f / 3.0f;
    [Tooltip("自機の回転速度")]
    [SerializeField] float turnSpeed = 0.1f;
    [Tooltip("接地判定用レイヤー")]
    [SerializeField] LayerMask groundLayer;
    [Tooltip("接地してる際の摩擦")]
    [SerializeField] float groundDrag = 3.0f;
    Vector2 movementInput;
    float movementSpeed;
    float turnSmoothVelocity;
    bool isGround;
    const float CHECK_SPHERE_SCALE = 0.1f;

    [Header("Boost")]
    [Tooltip("ブースト速度")]
    [SerializeField] float sprintSpeed = 5.0f;
    [Tooltip("ブーストの消費量")]
    [SerializeField] float boostConsumption = 20.0f;
    bool isPressedBoost;
    bool isBoost;
    float boostGauge;
    public float boostCharge { set { boostGauge += value; } }
    public bool isBoosting { get { return isBoost; } }
    const float MAX_BOOST_GAUGE = 100.0f;

    [Header("Battery")]
    [Tooltip("充電速度")]
    [SerializeField] float chargeSpeed = 5.0f;
    [Tooltip("バッテリー消費量")]
    [SerializeField] float batteryConsumption = 2.0f;
    [Tooltip("ブースト時のバッテリー消費量")]
    [SerializeField] float boostingBatteryConsumption = 3.0f;
    [Tooltip("充電器のレイヤー")]
    [SerializeField] LayerMask chargerLayer;
    float batteryGauge;
    const float MAX_BATTERY_GAUGE = 100.0f;

    [Header("Garbage")]
    [Tooltip("ゴミタンクの容量")]
    [SerializeField] float garbageCapacity = 55.0f;
    [Tooltip("ゴミの廃棄速度")]
    [SerializeField] float garbageDisposalSpeed = 6.0f;
    [Tooltip("ゴミ箱のレイヤー")]
    [SerializeField] LayerMask trashCanLayer;
    float possessionGarbage;
    public float addGarbage { set { possessionGarbage += value; } }
    bool fullyGarbage;
    public bool isFullyGarbage { get { return fullyGarbage; } }

    [Header("Reference")]
    [Tooltip("カメラが Y 軸で回転するための EmptyObject")]
    [SerializeField] Transform cameraAxisY;
    [Tooltip("ブーストゲージ用スライダー")]
    [SerializeField] Slider boostGaugeSlider;
    [Tooltip("バッテリーゲージ用テキスト")]
    [SerializeField] Text batteryGaugeText;
    [Tooltip("バッテリーゲージ用スライダー")]
    [SerializeField] Slider batteryGaugeSlider;
    [Tooltip("ゴミタンク用スライダー")]
    [SerializeField] Slider garbageGaugeSlider;
    Rigidbody playerBody;
    PlayerInput playerInput;

    // 入力用
    void OnEnable()
    {
        playerBody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

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
        batteryGauge = MAX_BATTERY_GAUGE;
        batteryGaugeSlider.maxValue = MAX_BATTERY_GAUGE;
        garbageGaugeSlider.maxValue = garbageCapacity;
    }

    // Update is called once per frame
    void Update()
    {
        CheckGround();
        RotatePlayer();
        SpeedControl();
        Boost();
        Battery();
        Garbage();
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
        if (boostGauge > MAX_BOOST_GAUGE) boostGauge = MAX_BOOST_GAUGE;

        if (boostGauge > 0.1f && isPressedBoost && movementInput != Vector2.zero)
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

        boostGaugeSlider.value = boostGauge;
    }

    void Battery()
    {
        if (Physics.CheckSphere(transform.position, transform.lossyScale.x / 2, chargerLayer) && batteryGauge < MAX_BATTERY_GAUGE)
        {
            batteryGauge += chargeSpeed * Time.deltaTime;
        }
        else
        {
            if (isBoost) batteryGauge -= boostingBatteryConsumption * Time.deltaTime;
            else batteryGauge -= batteryConsumption * Time.deltaTime;
        }

        if (batteryGauge <= 0.0f)
        {
            GetComponent<GameManager>().GameOver();
            GetComponent<PlayerController>().enabled = false;
        }

        batteryGaugeText.text = batteryGauge.ToString("f1");
        batteryGaugeSlider.value = batteryGauge;
    }

    void Garbage()
    {
        if (possessionGarbage >= garbageCapacity) fullyGarbage = true;
        else fullyGarbage = false;

        if (Physics.CheckSphere(transform.position, transform.lossyScale.x / 2, trashCanLayer) && possessionGarbage > 0.0f)
        {
            possessionGarbage -= garbageDisposalSpeed * Time.deltaTime;
        }

        garbageGaugeSlider.value = possessionGarbage;
    }
}
