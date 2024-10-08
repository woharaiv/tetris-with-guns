using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [SerializeField] InputActionAsset actionAsset;
    InputActionMap input;

    private void Awake()
    {
        input = actionAsset.FindActionMap("Gameplay");
    }
    void OnEnable()
    {
        input.Enable();
    }
    void OnDisable()
    {
        input.Disable();
    }
}
