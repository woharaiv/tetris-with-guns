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
            {
                ammoText.text = heldWeaponAmmos[activeWeaponIndex].ToString();
                ammoMeters[activeWeaponIndex].value = (float)value/weapons[activeWeaponIndex].ammo;
            }
        } 
    }
    WeaponType activeWeapon
    {
        get
        {
            return weapons[activeWeaponIndex];
        }
    }
    [SerializeField] TextMeshProUGUI ammoText;
    bool usingAmmo = true;

    [SerializeField] float fireRate = 0.2f;
    float fireTimer;
    bool doAutofire;

    [SerializeField] bool canSwitchWeapon;
    [SerializeField] public WeaponType[] weapons;
    List<int> heldWeaponAmmos;
    [SerializeField] int activeWeaponIndex;
    bool[] unlockedWeapons;
    [HideInInspector] public List<Slider> ammoMeters = new List<Slider>();

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
        heldWeaponAmmos = new List<int>();
        foreach(WeaponType weapon in weapons)
        {
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
    public void AddAmmo(int ammoToAdd, int indexToAddTo)
    {
        heldWeaponAmmos[indexToAddTo] += ammoToAdd;
        if (usingAmmo)
        {
            ammoMeters[indexToAddTo].value = (float)heldWeaponAmmos[indexToAddTo] / weapons[indexToAddTo].ammo;
            if(indexToAddTo == activeWeaponIndex)
                ammoText.text = heldWeaponAmmos[activeWeaponIndex].ToString();
        }
    }

    /// <summary>
    /// Moves particles to the ammo bar of the gun active at the time of running this code, and makes some of them reload ammo of that weapon when they reach the bar.
    /// When each particle moves is randomly determined, so exactly when the ammo will be given to the player is unclear.
    /// </summary>
    /// <remarks>If totalAmountToReload is less than 0, each particle will reload 1 ammo. If it's less than or equal to the length of particlesToMove,
    /// that many particles will reload 1 ammo each when they reach the meter. If it's greater, the amount to reload will be distributed among the particles,
    /// with each particle reloading at least the truncated quotient and the remainder split evenly among some particles.</remarks>
    public void ScheduleMoveAmmoToReload(List<Particle> particlesToMove, int totalAmountToReload = -1)
    {
        float delayRange = (float)particlesToMove.Count / 50;
        int excessAmmo = 0;
        int ammoPerParticle = 1;
        if (totalAmountToReload >= 0)
        {
            ammoPerParticle = totalAmountToReload/particlesToMove.Count;
            excessAmmo = totalAmountToReload % particlesToMove.Count;
        }
        foreach (Particle p in particlesToMove)
        {
            p.ScheduleMoveToPoint(ActiveAmmoMeterPos(), 1f + Random.Range(Mathf.Max(-delayRange, -0.9f), delayRange))
                .AppendCallback(()=>AddAmmo(ammoPerParticle + (excessAmmo-- >= 0? 1 : 0), activeWeaponIndex));
        }
    }

    public void Reload()
    {
        ammoCount = weapons[activeWeaponIndex].ammo;
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
                return ("Unlocked " + weapons[indexToUnlock].name + "! (Press " + indexToUnlock + ")");
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

    public Vector3 ActiveAmmoMeterPos()
    {
        return ammoMeters[activeWeaponIndex].transform.position;
    }
}
