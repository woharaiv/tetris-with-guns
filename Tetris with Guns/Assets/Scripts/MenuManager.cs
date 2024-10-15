using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] InputActionAsset actionAsset;
    InputActionMap input;

    [SerializeField] RawImage screenFlashAsset;

    public static MenuManager instance;

    private void Awake()
    {
        instance = this;
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
    public void StartButtonShot()
    {
        input.Disable();
        screenFlashAsset.DOColor(new Color(0, 0, 0, 1f), 0.5f).OnComplete(StartGame);
    }
    public void StartGame()
    {
        SceneManager.LoadScene("Gameplay");
    }
}
