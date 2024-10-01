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
    [SerializeField] int _ammo = 100;
    int ammoMax = 100;
    [SerializeField] TextMeshProUGUI ammoText;

    [SerializeField] GameObject shootParticle;
    [SerializeField] AudioClip shootSound;
    [SerializeField] float screenShakeStrength;

    [SerializeField] float fireRate = 0.2f;
    float fireTimer;
    InputAction shootInput;

    Background bgManager;

    private void Awake()
    {
        input = actionAsset.FindActionMap("Gameplay");
        shootInput = input.FindAction("Shoot");
        shootInput.started += ShootAction;
        bgManager = FindAnyObjectByType<Background>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        fireTimer = fireRate;
    }

    private void Update()
    {
        if(shootInput.phase == InputActionPhase.Performed)
        {
            fireTimer -= Time.deltaTime;
            if(fireTimer <= 0)
            {
                Shoot();
                fireTimer = fireRate;
            }
        }
    }


    void Shoot(InputAction.CallbackContext? ctx = null)
    {
        if (!GameManager.instance.acceptingInput)
            return;
        if (ammoCount <= 0)
            return;

        ammoCount--;

        Vector2 clickLocation = GameManager.camera.ScreenToWorldPoint(shootInput.ReadValue<Vector2>());

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
    void ShootAction(InputAction.CallbackContext ctx) { Shoot(ctx); }
    public void AddAmmo(int ammoToAdd)
    {
        ammoCount += ammoToAdd;
    }
    public void Reload()
    {
        ammoCount = ammoMax;
    }
}
