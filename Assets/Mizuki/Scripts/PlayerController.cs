using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/*
���ӓ_
 */

public class PlayerController : MonoBehaviour
{
    [Header("Status")]
    [Tooltip("���s���x")]
    [SerializeField] float walkSpeed = 5.0f / 3.0f;
    [Tooltip("�������x")]
    [SerializeField] float sprintSpeed = 5.0f;
    [Tooltip("�u�[�X�g�̏����")]
    [SerializeField] float boostConsumption = 20.0f;
    [Tooltip("���@�̉�]���x")]
    [SerializeField] float turnSpeed = 0.1f;
    [Tooltip("�ڒn����p���C���[")]
    [SerializeField] LayerMask groundLayer;
    [Tooltip("�ڒn���Ă�ۂ̖��C")]
    [SerializeField] float groundDrag = 3.0f;

    [Header("Reference")]
    [Tooltip("���@�� Rigidbody")]
    [SerializeField] Rigidbody playerBody;
    [Tooltip("�J������ Y ���ŉ�]���邽�߂� EmptyObject")]
    [SerializeField] Transform cameraAxisY;
    [Tooltip("PlayerInput")]
    [SerializeField] PlayerInput playerInput;
    [Tooltip("�u�[�X�g�Q�[�W�p�e�L�X�g")]
    [SerializeField] Text boostGaugeText;
    [Tooltip("�u�[�X�g�Q�[�W�p�X���C�_�[")]
    [SerializeField] Slider boostGaugeSlider;

    // ���͗p
    Vector2 movementInput;
    bool isPressedBoost;

    // �ړ��Ƃ��p
    float movementSpeed;
    float boostGauge;

    public float boostCharge { set { boostGauge += value; } }

    bool isBoost;

    public bool isBoosting { get { return isBoost; } }

    // �ڒn����p
    bool isGround;
    const float CHECK_SPHERE_SCALE = 0.1f;

    // SmoothDampAngle�p
    float turnSmoothVelocity;

    const float MAX_BOOST_GAUGE = 100.0f;

    // ���͗p
    void OnEnable()
    {
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
