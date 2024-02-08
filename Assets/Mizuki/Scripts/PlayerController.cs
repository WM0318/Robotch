using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/*
���ӓ_
 */

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("���s���x")]
    [SerializeField] float walkSpeed = 5.0f / 3.0f;
    [Tooltip("���@�̉�]���x")]
    [SerializeField] float turnSpeed = 0.1f;
    [Tooltip("�ڒn����p���C���[")]
    [SerializeField] LayerMask groundLayer;
    [Tooltip("�ڒn���Ă�ۂ̖��C")]
    [SerializeField] float groundDrag = 3.0f;
    Vector2 movementInput;
    float movementSpeed;
    float turnSmoothVelocity;
    bool isGround;
    const float CHECK_SPHERE_SCALE = 0.1f;

    [Header("Boost")]
    [Tooltip("�u�[�X�g���x")]
    [SerializeField] float sprintSpeed = 5.0f;
    [Tooltip("�u�[�X�g�̏����")]
    [SerializeField] float boostConsumption = 20.0f;
    bool isPressedBoost;
    bool isBoost;
    float boostGauge;
    public float boostCharge { set { boostGauge += value; } }
    public bool isBoosting { get { return isBoost; } }
    const float MAX_BOOST_GAUGE = 100.0f;

    [Header("Battery")]
    [Tooltip("�[�d���x")]
    [SerializeField] float chargeSpeed = 5.0f;
    [Tooltip("�o�b�e���[�����")]
    [SerializeField] float batteryConsumption = 2.0f;
    [Tooltip("�u�[�X�g���̃o�b�e���[�����")]
    [SerializeField] float boostingBatteryConsumption = 3.0f;
    [Tooltip("�[�d��̃��C���[")]
    [SerializeField] LayerMask chargerLayer;
    float batteryGauge;
    const float MAX_BATTERY_GAUGE = 100.0f;

    [Header("Garbage")]
    [Tooltip("�S�~�^���N�̗e��")]
    [SerializeField] float garbageCapacity = 55.0f;
    [Tooltip("�S�~�̔p�����x")]
    [SerializeField] float garbageDisposalSpeed = 6.0f;
    [Tooltip("�S�~���̃��C���[")]
    [SerializeField] LayerMask trashCanLayer;
    float possessionGarbage;
    public float addGarbage { set { possessionGarbage += value; } }
    bool fullyGarbage;
    public bool isFullyGarbage { get { return fullyGarbage; } }

    [Header("Reference")]
    [Tooltip("�J������ Y ���ŉ�]���邽�߂� EmptyObject")]
    [SerializeField] Transform cameraAxisY;
    [Tooltip("�u�[�X�g�Q�[�W�p�X���C�_�[")]
    [SerializeField] Slider boostGaugeSlider;
    [Tooltip("�o�b�e���[�Q�[�W�p�e�L�X�g")]
    [SerializeField] Text batteryGaugeText;
    [Tooltip("�o�b�e���[�Q�[�W�p�X���C�_�[")]
    [SerializeField] Slider batteryGaugeSlider;
    [Tooltip("�S�~�^���N�p�X���C�_�[")]
    [SerializeField] Slider garbageGaugeSlider;
    Rigidbody playerBody;
    PlayerInput playerInput;

    // ���͗p
    void OnEnable()
    {
        playerBody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;

        playerInput.actions["Boost"].started += OnBoost;
        playerInput.actions["Boost"].canceled += OnBoost;
    }

    // �ړ��p�̓��͂��������ꍇ�ɌĂяo��
    void OnMove(InputAction.CallbackContext context)
    {
        // ���͂��ꂽ�l���ړ��p�ϐ���
        movementInput = context.ReadValue<Vector2>();
    }

    // �W�����v�L�[�����͂��ꂽ�ꍇ�ɌĂяo��
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
        playerBody.freezeRotation = true; // Rigidbody �̉�]�Œ�
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



    // �ڒn����p
    void CheckGround()
    {
        // �ڒn����
        isGround = Physics.CheckSphere(new Vector3(transform.position.x, transform.position.y - transform.lossyScale.y, transform.position.z), CHECK_SPHERE_SCALE, groundLayer);

        if(isGround) playerBody.drag = groundDrag; // �ڒn���Ă���ꍇ�͖��C�� groundDrag ��
        else playerBody.drag = 0.0f; // �ڒn���Ă��Ȃ��ꍇ�͖��C�� 0 ��
    }

    // �v���C���[��]�p
    void RotatePlayer()
    {
        // ���͂��������ꍇ��
        if (movementInput != Vector2.zero)
        {
            // ���͒l����������Z�o
            float targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + cameraAxisY.eulerAngles.y;

            // ��L�ŎZ�o���ꂽ�����Ɍ������Ă�����Ɖ�]
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSpeed);
            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
        }
    }

    // �v���C���[�ړ��p
    void MovePlayer()
    {
        // �ړ��������������Z�o���Ĉړ�
        Vector3 moveDirection = cameraAxisY.right * movementInput.x + cameraAxisY.forward * movementInput.y;
        playerBody.AddForce(moveDirection * movementSpeed * 10.0f, ForceMode.Force);
    }

    // �ړ����x�����p
    void SpeedControl()
    {
        // X����Z������ Vector3 �Ŏ擾
        Vector3 flatVelocity = new Vector3(playerBody.velocity.x, 0.0f, playerBody.velocity.z);

        // ���݂̑��x�� movementSpeed �ȏゾ������
        if (flatVelocity.magnitude > movementSpeed)
        {
            // ������x���Z�o���� Velocity �ɑ��
            Vector3 limitedVelocity = flatVelocity.normalized * movementSpeed;
            playerBody.velocity = new Vector3(limitedVelocity.x, playerBody.velocity.y, limitedVelocity.z);
        }
    }

    // �W�����v�p
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
