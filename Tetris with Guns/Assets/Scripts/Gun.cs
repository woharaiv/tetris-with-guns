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
            if(usingAmmo)
                ammoText.text = _ammo.ToString(); 
        } 
    }
    [SerializeField] int _ammo = 100;
    int ammoMax = 100;
    [SerializeField] TextMeshProUGUI ammoText;
    bool usingAmmo;

    [SerializeField] GameObject shootParticle;
    [SerializeField] AudioClip shootSound;
    [SerializeField] float screenShakeStrength;
    float shotSpread;

    [SerializeField] float fireRate = 0.2f;
    float fireTimer;
    bool doAutofire;

    [SerializeField] bool canSwitchWeapon;
    [SerializeField] WeaponType[] weapons;
    List<WeaponType> heldWeaponTypes;
    List<int> heldWeaponAmmos;
    [SerializeField] int activeWeaponIndex;

    InputAction shootInput;
    InputAction switchWeaponInput;

    Background bgManager;


    private void Awake()
    {
        input = actionAsset.FindActionMap("Gameplay");
        shootInput = input.FindAction("Shoot");
        shootInput.started += ShootAction;
        switchWeaponInput = input.FindAction("Switch Weapon");
        switchWeaponInput.started += SwitchWeapon;
        bgManager = FindAnyObjectByType<Background>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        if(!ammoText)
            usingAmmo = false;
        heldWeaponTypes = new List<WeaponType>();
        heldWeaponAmmos = new List<int>();
        foreach(WeaponType weapon in weapons)
        {
            heldWeaponTypes.Add(weapon);
            heldWeaponAmmos.Add(weapon.ammo);
        }
        activeWeaponIndex = 0;
        UpdateActiveWeapon();
        fireTimer = 0;
    }

    private void OnDestroy()
    {
        shootInput.started -= ShootAction;
        switchWeaponInput.started -= SwitchWeapon;
    }

    private void Update()
    {
        if(fireTimer > 0)
            fireTimer -= Time.deltaTime;
        if(doAutofire && shootInput.phase == InputActionPhase.Performed)
        {
            Shoot();
        }
    }


    void Shoot(InputAction.CallbackContext? ctx = null)
    {
        if (GameManager.instance != null && !GameManager.instance.acceptingInput)
            return;
        if (fireTimer > 0)
            return;
        if (usingAmmo)
        {
            if (heldWeaponAmmos[activeWeaponIndex] <= 0)
                return;
            else
                heldWeaponAmmos[activeWeaponIndex]--;
        }

        Vector2 clickLocation = (Vector2)Camera.main.ScreenToWorldPoint(shootInput.ReadValue<Vector2>()) + (Random.insideUnitCircle * shotSpread);

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

        fireTimer = fireRate;
    }
    void ShootAction(InputAction.CallbackContext ctx) { Shoot(ctx); }

    void SwitchWeapon(InputAction.CallbackContext ctx) 
    {
        if (!canSwitchWeapon || GameManager.instance != null && !GameManager.instance.acceptingInput)
            return;
        int newActiveIndex;
        int.TryParse(ctx.control.name, out newActiveIndex);
        
        if(newActiveIndex <= weapons.Length && newActiveIndex > 0)
            activeWeaponIndex = newActiveIndex - 1;
        Debug.Log(activeWeaponIndex);
        UpdateActiveWeapon();
    }

    void UpdateActiveWeapon()
    {
        fireTimer = fireRate = heldWeaponTypes[activeWeaponIndex].fireRate;
        screenShakeStrength = heldWeaponTypes[activeWeaponIndex].shakeStrength;
        doAutofire = heldWeaponTypes[activeWeaponIndex].autoFire;
        shotSpread = heldWeaponTypes[activeWeaponIndex].spreadRadius;
    }


    public void AddAmmo(int ammoToAdd)
    {
        heldWeaponAmmos[activeWeaponIndex] += ammoToAdd;
    }
    public void Reload()
    {
        heldWeaponAmmos[activeWeaponIndex] = heldWeaponTypes[activeWeaponIndex].ammo;
    }
}
