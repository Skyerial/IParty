using UnityEngine;
using UnityEngine.InputSystem;

public class DescrTabController : MonoBehaviour
{
    public Descr_Contr descriptionTab;
    public Descr_Contr controlsTab;

    public StartMinigameButton StartButton;

    private PlayerInput playerInput;
    private InputAction switchTabAction;
    private InputAction startAction;

    private bool isDescriptionActive = true;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        switchTabAction = playerInput.actions["Jump"];
        startAction = playerInput.actions["Sprint"];
    }

    private void OnEnable()
    {
        switchTabAction.performed += OnSwitchTab;
        startAction.performed += OnStart;
    }

    private void OnDisable()
    {
        switchTabAction.performed -= OnSwitchTab;
        startAction.performed -= OnStart;
    }

    private void OnSwitchTab(InputAction.CallbackContext context)
    {
        if (isDescriptionActive)
        {
            controlsTab.OnClick();
            isDescriptionActive = false;
        }
        else
        {
            descriptionTab.OnClick();
            isDescriptionActive = true;
        }
    }

    private void OnStart(InputAction.CallbackContext context)
    {
        Debug.Log("Start button pressed — start the game here!");
        StartButton.LoadSelectedMinigame();
    }
}
