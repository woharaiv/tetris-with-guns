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
    public static Gun instance;
    [SerializeField] InputActionAsset actionAsset;
    InputActionMap input;
    public RawImage screenFlashAsset;

    public static CinemachineImpulseSource impulseSource;

    public int ammoCount 
    { 
        get 
        { 
            return heldWeaponAmmos[activeWeaponIndex]; 
        }
        set 
        {
            heldWeaponAmmos[activeWeaponIndex] = value;
            if(usingAmmo)
                ammoText.text = heldWeaponAmmos[activeWeaponIndex].ToString(); 
        } 
    }
    WeaponType activeWeapon
    {
        get
        {
            return heldWeaponTypes[activeWeaponIndex];
        }
    }
    [SerializeField] TextMeshProUGUI ammoText;
    bool usingAmmo = true;

    [SerializeField] float fireRate = 0.2f;
    float fireTimer;
    bool doAutofire;

    [SerializeField] bool canSwitchWeapon;
    [SerializeField] WeaponType[] weapons;
    List<WeaponType> heldWeaponTypes;
    List<int> heldWeaponAmmos;
    [SerializeField] int activeWeaponIndex;
    bool[] unlockedWeapons;

    InputAction shootInput;
    InputAction switchWeaponInput;

    private void Awake()
    {
        instance = this;
        input = actionAsset.FindActionMap("Gameplay");
        shootInput = input.FindAction("Shoot");
        shootInput.started += ShootAction;
        switchWeaponInput = input.FindAction("Switch Weapon");
        switchWeaponInput.started += SwitchWeapon;
        impulseSource = GetComponent<CinemachineImpulseSource>();
        if(ammoText == null)
            usingAmmo = false;
        heldWeaponTypes = new List<WeaponType>();
        heldWeaponAmmos = new List<int>();
        foreach(WeaponType weapon in weapons)
        {
            heldWeaponTypes.Add(weapon);
            heldWeaponAmmos.Add(weapon.ammo);
        }

        unlockedWeapons = new bool[weapons.Length];
        for (int i = 0; i < unlockedWeapons.Length; i++)
            unlockedWeapons[i] = false;
        unlockedWeapons[0] = true;

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
            if (ammoCount <= 0)
                return;
            else
                ammoCount--;
        }

        Vector2 clickLocation = (Vector2)Camera.main.ScreenToWorldPoint(shootInput.ReadValue<Vector2>());

        activeWeapon.Shoot(clickLocation);

        fireTimer = fireRate;
    }
    void ShootAction(InputAction.CallbackContext ctx) { Shoot(ctx); }

    void SwitchWeapon(InputAction.CallbackContext ctx) 
    {
        if (!canSwitchWeapon || GameManager.instance != null && !GameManager.instance.acceptingInput)
            return;
        int newActiveIndex;
        int.TryParse(ctx.control.name, out newActiveIndex);
        
        if(newActiveIndex <= weapons.Length && newActiveIndex > 0 && unlockedWeapons[newActiveIndex - 1])
            activeWeaponIndex = newActiveIndex - 1;
        Debug.Log(activeWeaponIndex);
        UpdateActiveWeapon();
    }

    void UpdateActiveWeapon()
    {
        fireTimer = fireRate = activeWeapon.fireRate;
        doAutofire = activeWeapon.autoFire;
        if (usingAmmo)
            ammoText.text = heldWeaponAmmos[activeWeaponIndex].ToString();
    }


    public void AddAmmo(int ammoToAdd)
    {
        ammoCount += ammoToAdd;
    }
    public void Reload()
    {
        ammoCount = heldWeaponTypes[activeWeaponIndex].ammo;
    }

    public string UnlockWeapon(int indexToUnlock)
    {
        if (indexToUnlock > 0 && indexToUnlock < unlockedWeapons.Length)
        {
            if (unlockedWeapons[indexToUnlock])
                return "";
            else
            {
                unlockedWeapons[indexToUnlock] = true;
                return ("Unlocked " + heldWeaponTypes[indexToUnlock].name + "! (Press " + indexToUnlock + ")");
            }
        }
        return "Unknown Gun";
    }
    public string UnlockRandomWeapon()
    {
        bool allWeaponsUnlocked = true;
        foreach (var unlocked in unlockedWeapons)
        {
            if(!unlocked)
            {
                allWeaponsUnlocked = false; 
                break;
            }
        }
        if (allWeaponsUnlocked)
            return "";
        else
        {
            int indexToUnlock = 0;
            do
            {
                indexToUnlock = Random.Range(1, unlockedWeapons.Length);
            } while (unlockedWeapons[indexToUnlock]);

            return UnlockWeapon(indexToUnlock);
        }
    }
}
