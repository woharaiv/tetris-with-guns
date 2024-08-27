using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class Gun : MonoBehaviour
{
    [SerializeField] InputActionAsset actionAsset;
    InputActionMap input;

    [SerializeField] RawImage screenFlashAsset;

    [SerializeField] GameObject shootParticle;
    [SerializeField] AudioClip shootSound;

    Background bgManager;

    private void Awake()
    {
        input = actionAsset.FindActionMap("Gameplay");
        input.FindAction("Shoot").started += Shoot;
        bgManager = FindAnyObjectByType<Background>();
    }

    void Shoot(InputAction.CallbackContext ctx)
    {
        if (!GameManager.instance.acceptingInput)
            return;

        DOTween.Sequence()
            .Append(screenFlashAsset.DOColor(new Color(1, 1, 1, 0.25f), 0.05f))
            .Append(screenFlashAsset.DOColor(new Color(1, 1, 1, 0), 0.1f));

        Vector2 clickLocation = GameManager.camera.ScreenToWorldPoint(ctx.ReadValue<Vector2>());

        Instantiate(shootParticle, (Vector3)clickLocation + Vector3.back, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
        AudioSource.PlayClipAtPoint(shootSound, clickLocation);

        bool hitBackground = true;
        Collider2D[] hit = Physics2D.OverlapPointAll(clickLocation);
        foreach (Collider2D col in hit)
        {
            Tile tileCheck = col.gameObject.GetComponent<Tile>();
            if (tileCheck != null)
            {
                tileCheck.DamageTile();
                hitBackground = false;
            }
        }
        
        if (hitBackground)
            bgManager.ShootBackground(clickLocation);
    }
}
