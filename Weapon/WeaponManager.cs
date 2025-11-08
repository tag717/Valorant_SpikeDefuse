using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using pxr;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapons")]
    public List<GameObject> poses = new List<GameObject>(); // Order: vandal = 0, sheriff = 1, melee = 2

    [SerializeField]
    private GunController2 GunController; //when holding gun, activate GunController.cs 
    private Melee MeleeController; //when holding melee, active MeleeController.cs
    private GameObject ActiveWeapon; //gun control sets the current weapon which is sheriff at beginning
    private GameObject ActivePose;

    //HUD components
    [Header("Ammo")]
    public TextMeshProUGUI CurrentBullet;
    public TextMeshProUGUI CarryBullet;
    public Image img;
    public Image Vandal;
    public Image Sheriff;
    public Image Melee;
    public Image weaponsBar;
    private float UiLastingTime = 4.0f; //disable weaponsList UI after 4 seconds

    PlayerInputHandler m_InputHandler;
    PlayerCharacterController m_PlayerController;
    private int CurrentIndex = 1; // start with sheriff
    private int PreviousIndex; // previous weapon
    bool IsReloading
    {
        get { return GunController.IsReloading; }
        set { IsReloading = value; }
    }
    bool IsFullyLoaded
    {
        get { return GunController.currentBulletCount < GunController.reloadBulletCount; }
        set { IsFullyLoaded = value; }
    }
    bool IsOutOfAmmo
    {
        get { return GunController.currentBulletCount == 0; }
        set { IsOutOfAmmo = value; }
    }
    bool IsOutOfSpareAmmo
    {
        get { return GunController.carryBulletCount == 0; }
        set { IsOutOfSpareAmmo = value; }
    }
    float EquipCooldown;

    // holds sheriff at start 
    void Start()
    {
        m_PlayerController = GetComponent<PlayerCharacterController>();
        m_InputHandler = GetComponent<PlayerInputHandler>();
        EquipWeapon(CurrentIndex);
    }

    void Update()
    {
        if (EquipCooldown > 0f)
            EquipCooldown -= Time.deltaTime;

        // Switch Weapon
        {
            if (m_InputHandler.GetPrimaryWeaponButtonDown() && ActiveWeapon.name != "Vandal") EquipWeapon(0); // vandal
            if (m_InputHandler.GetSideArmButtonDown() && ActiveWeapon.name != "Sheriff") EquipWeapon(1); // sheriff
            if (m_InputHandler.GetMeleeButtonDown() && ActiveWeapon.name != "Melee") EquipWeapon(2); // melee
            if (m_InputHandler.GetSpikeButtonDown() && CurrentIndex != 3) EquipWeapon(3); // Defuser
            if (m_InputHandler.GetSpikeButtonUp() && CurrentIndex == 3) EquipWeapon(PreviousIndex); // Equip Last Used Weapon

            //Debug.Log(m_InputHandler.GetSpikeButtonUp());

            float scroll = m_InputHandler.GetSwitchInput();
            if (scroll > 0f) // scroll up
            {
                CurrentIndex--;
                if (CurrentIndex < 0) CurrentIndex = poses.Count - 1;
                EquipWeapon(CurrentIndex);
            }
            else if (scroll < 0f) // scroll down
            {
                CurrentIndex++;
                if (CurrentIndex >= poses.Count) CurrentIndex = 0;
                EquipWeapon(CurrentIndex);
            }
        }

        // Handle Attack and Reload
        GunController.IsMoving = m_PlayerController.IsMoving;

        if (EquipCooldown <= 0)
        {
            if ((m_InputHandler.GetFireInputDown() || m_InputHandler.GetFireInputHeld()) && !IsReloading)
            {
                TryAttack();
            }

            if (m_InputHandler.GetReloadButtonDown() && !IsReloading && IsFullyLoaded)
            {
                TryReload();
            }

            // Update UI
            {
                if (GunController != null)
                    UpdateAmmoUI();

                if (UiLastingTime > 0f)
                    UiLastingTime -= Time.deltaTime;
                else
                    DisableWeaponListUI();
            }
        }
    }

    private void EquipWeapon(int index)
    {
        // unequip current Weapon
        Destroy(ActivePose);

        // enable chosen
        CurrentIndex = index;
        ActivePose = Instantiate(poses[index], transform.GetChild(0).GetChild(0));
        ActiveWeapon = ActivePose.transform.GetChild(0).Find("Skeleton/Root/MasterWeapon").GetChild(1).gameObject;
        m_PlayerController.Animator = ActivePose.GetComponent<Animator>();

        if (index != 3)
        {
            if (index == 2)
            {
                MeleeController = ActiveWeapon.GetComponent<Melee>();
                MeleeController.Equip();
                DisableAmmoUI();
                EnableWeaponListUI();
                SetStatus(1f, Melee);
                SetStatus(0.5f, Vandal);
                SetStatus(0.5f, Sheriff);
            }
            else
            {
                GunController = ActiveWeapon.GetComponent<GunController2>();
                GunController.Equip();
                if (index == 0)
                {
                    SetStatus(1f, Vandal);
                    SetStatus(0.5f, Sheriff);

                }
                else if (index == 1)
                {
                    SetStatus(1f, Sheriff);
                    SetStatus(0.5f, Vandal);
                }
                EnableAmmoUI();
                UpdateAmmoUI(); //added here for faster update 
                SetStatus(0.5f, Melee);
                EnableWeaponListUI();

            }
        }
        

        EquipCooldown = 1f;
    }

    private void TryAttack()
    {
        bool Attacked = false;
        if (CurrentIndex != 2)
        {
            if (!IsOutOfAmmo)
            {
                Attacked = GunController.Shoot();
            }
            else
            {
                TryReload();
            }
        }
        else
        {
            Attacked = MeleeController.Attack(0);
        }
        if(Attacked) m_PlayerController.Animator.SetTrigger("Attack");
    }

    private void TryReload()
    {
        if (CurrentIndex != 2 && !IsOutOfSpareAmmo)
        {
            StartCoroutine(GunController.ReloadCoroutine());
            m_PlayerController.Animator.SetTrigger("Reload");
        }
    }
    private void DisableAmmoUI()
    {
        CurrentBullet.text = " ";
        CarryBullet.text = " ";
        if (img != null)
            img.enabled = false; // this hides the image
    }

    private void UpdateAmmoUI()
    {
        CurrentBullet.text = GunController.currentBulletCount.ToString();
        CarryBullet.text = GunController.carryBulletCount.ToString();
    }

    public void EnableAmmoUI()
    {
        if (img != null)
            img.enabled = true;
    }

    public void EnableWeaponListUI()
    {
        if (Vandal != null && Sheriff != null && Melee != null && weaponsBar != null)
        {
            Vandal.enabled = true;
            Sheriff.enabled = true;
            Melee.enabled = true;
            weaponsBar.enabled = true;
            UiLastingTime = 4.0f;
        }
    }

    public void DisableWeaponListUI()
    {
        if (Vandal != null && Sheriff != null && Melee != null && weaponsBar != null)
        {
            Vandal.enabled = false;
            Sheriff.enabled = false;
            Melee.enabled = false;
            weaponsBar.enabled = false;
            UiLastingTime = 0f;
        }
    }

    public void SetStatus(float alpha, Image im)
    {
        if (im != null)
        {
            if (alpha < 1f)
            {
                im.color = Color.black;
            }
            else
            {
                im.color = Color.white;
            }
            Color c = im.color;
            c.a = Mathf.Clamp01(alpha);
            im.color = c;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SPIKE"))
        {
            if (CurrentIndex == 3)
                EquipWeapon(PreviousIndex);
        }
    }
}
