using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;
using TMPro;
using Cinemachine;

public class Gun : MonoBehaviour
{
    [SerializeField] InputActionAsset actionAsset;
    InputActionMap input;

    [SerializeField] RawImage screenFlashAsset;

    CinemachineImpulseSource impulseSource;

    public int ammoCount 
    { 
        get 
        { 
            return _ammo; 
        }
        set 
        { 
            _ammo = value; 
            ammoText.text = _ammo.ToString(); 
        } 
    }
    [SerializeField] int _ammo = 10;
    int ammoMax = 10;
    [SerializeField] TextMeshProUGUI ammoText;

    [SerializeField] GameObject shootParticle;
    [SerializeField] AudioClip shootSound;
    [SerializeField] float screenShakeStrength;

    Background bgManager;

    private void Awake()
    {
        input = actionAsset.FindActionMap("Gameplay");
        input.FindAction("Shoot").started += Shoot;
        bgManager = FindAnyObjectByType<Background>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    void Shoot(InputAction.CallbackContext ctx)
    {
        if (!GameManager.instance.acceptingInput)
            return;
        if (ammoCount <= 0)
            return;

        ammoCount--;

        Vector2 clickLocation = GameManager.camera.ScreenToWorldPoint(ctx.ReadValue<Vector2>());

        //Hit particle
        Instantiate(shootParticle, (Vector3)clickLocation + Vector3.back, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
        
        //Gun sound
        AudioSource.PlayClipAtPoint(shootSound, clickLocation);

        //Screen flash
        DOTween.Sequence()
            .Append(screenFlashAsset.DOColor(new Color(1, 1, 1, 0.25f), 0.05f))
            .Append(screenFlashAsset.DOColor(new Color(1, 1, 1, 0), 0.1f));

        //Screen shake
        impulseSource.GenerateImpulseAt(clickLocation, Random.insideUnitCircle.normalized * 0.1f * screenShakeStrength);

        bool hitBackground = true;
        Collider2D[] hit = Physics2D.OverlapPointAll(clickLocation);
        foreach (Collider2D col in hit)
        {
            if(col.CompareTag("Obstruction"))
                hitBackground = false;
            
            Tile tileCheck = col.gameObject.GetComponent<Tile>();
            if (tileCheck != null)
            {
                tileCheck.DamageTile();
            }
        }
        
        if (hitBackground)
            bgManager.ShootBackground(clickLocation);
    }
    public void AddAmmo(int ammoToAdd)
    {
        ammoCount += ammoToAdd;
    }
    public void Reload()
    {
        ammoCount = ammoMax;
    }
}
