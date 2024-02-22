using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Setting : MonoBehaviour
{
    [Header("Reference")]
    [Tooltip("ê›íËâÊñ ")]
    [SerializeField] GameObject settingMenu;

    PlayerInput playerInput;

    void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();

        playerInput.actions["OpenMenu"].started += OnOpenMenu;

        playerInput.actions["Reset"].started += OnReset;
    }

    void OnOpenMenu(InputAction.CallbackContext context)
    {
        if (settingMenu.activeInHierarchy)
        {
            settingMenu.SetActive(false);

            Time.timeScale = 1.0f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            settingMenu.SetActive(true);

            Time.timeScale = 0.0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnReset(InputAction.CallbackContext context)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
