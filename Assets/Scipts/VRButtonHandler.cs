using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

public class VRButtonHandler : MonoBehaviour
{
    public InputActionAsset inputActions;
    public Button targetButton;

    private InputAction selectAction;

    void Start()
    {
        var rightHandActionMap = inputActions.FindActionMap("XRI RightHand");
        selectAction = rightHandActionMap.FindAction("Select");


        selectAction.performed += OnSelectPerformed;
        selectAction.Enable();
    }

    void OnDestroy()
    {
        selectAction.performed -= OnSelectPerformed;
    }

    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        targetButton.onClick.Invoke();
    }
}
