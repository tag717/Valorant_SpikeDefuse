using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class WeaponM : MonoBehaviour
{
    [Header("Weapon")]
    public GameObject sheriff;
    public GameObject vandal;
    public GameObject melee;
    [Header("Arms")]
    public GameObject sheriffPose;
    public GameObject vandalPose;
    public GameObject meleePose;

    private GameObject currentWeapon; //gun control sets the current weapon which is sheriff at beginning
    private GameObject currentPose;

    private GunInfo currentPlayerF; // GunInfo.cs applies to current gun

    [SerializeField]
    private GunController GunControl; //when holding gun, activate GunController.cs 
    [SerializeField]
    private Melee mel;

    private int currentIndex = 1; // start with sheriff

    private List<GameObject> weapons = new List<GameObject>();
    private List<GameObject> poses = new List<GameObject>();

    //HUD components
    [Header("Ammo")]
    public TextMeshProUGUI CurrentBullet;
    public TextMeshProUGUI CarryBullet;
    public Image img;

    public Image Vandal;
    public Image Sheriff;
    public Image Melee;
    public Image weaponsBar;

    public float UITimer; //disable weaponsList UI after 4 seconds

    // holds sheriff at start 
    void Start()
    {
        // Order: vandal = 0, sheriff = 1, melee = 2
        weapons.Add(vandal);
        weapons.Add(sheriff);
        weapons.Add(melee);
        poses.Add(vandalPose);
        poses.Add(sheriffPose);
        poses.Add(meleePose);

        currentWeapon = weapons[currentIndex];
        EquipWeapon(currentIndex);

        if (CurrentBullet == null)
            CurrentBullet = GameObject.Find("Current Bullet").GetComponent<TextMeshProUGUI>();
        if (CarryBullet == null)
            CarryBullet = GameObject.Find("Carry Bullet").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // number keys
        if (Input.GetKeyDown(KeyCode.Alpha1) && (currentWeapon.name != "Vandal")) EquipWeapon(0); // vandal
        if (Input.GetKeyDown(KeyCode.Alpha2) && (currentWeapon.name != "Sheriff")) EquipWeapon(1); // sheriff
        if (Input.GetKeyDown(KeyCode.Alpha3) && (currentWeapon.name != "Melee")) EquipWeapon(2); // melee

        // scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) // scroll up
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = weapons.Count - 1;
            EquipWeapon(currentIndex);
        }
        else if (scroll < 0f) // scroll down
        {
            currentIndex++;
            if (currentIndex >= weapons.Count) currentIndex = 0;
            EquipWeapon(currentIndex);
        }

        if (currentPlayerF != null)
        {
            UpdateAmmoUI();
        }

        if (UITimer > 0f) 
        {
            UITimer -= Time.deltaTime;
        }
        else if (UITimer < 0f)
        {
            DisableWeaponListUI();
        }
    }

    private void EquipWeapon(int index)
    {
        // unequip current Weapon
        Destroy(currentPose);

        // enable chosen
        currentPose = Instantiate(poses[index], transform.GetChild(0).GetChild(0));
        currentWeapon = currentPose.transform.GetChild(0).Find("Skeleton/Root/MasterWeapon").GetChild(1).gameObject;
        GunControl.playerAnimator = currentPose.GetComponent<Animator>();
        GunControl.SetAnimator(currentWeapon.GetComponent<Animator>());

        // check for gun
        currentPlayerF = currentWeapon.GetComponent<GunInfo>();
        if (currentPlayerF != null)
        {
            GetComponent<PlayerMovement>().animator = currentWeapon.GetComponent<GunInfo>().playerAnime;
            if (GunControl != null)
            {
                GunControl.CancelReload();
                GunControl.enabled = true;
                GunControl.CurrentGun = currentPlayerF;
                GunControl.EquipCooldown = currentPlayerF.equipDelay; //set equip time for gun
                GunControl.OutSound(); //play equip sound of gun
                EnableAmmoUI();
                EnableWeaponListUI();
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

            }
            if (mel != null)
            {
                mel.ChangeisAttack(); //when isSwing, isAttack is not finished, change to false since gun is active
                mel.enabled = false;
                SetStatus(0.5f, Melee);
            }
            // change animation
            GunControl.SetAnimator(currentPlayerF.anime);
        }

        // check for melee
        mel = currentWeapon.GetComponent<Melee>();
        if (mel != null)
        {
            //GetComponent<PlayerMovement>().animator = currentWeapon.GetComponent<Melee>().playerAnime;
            GunControl.CancelReload();

            if (GunControl != null) GunControl.enabled = false;
            if (currentPlayerF != null) currentPlayerF.enabled = false;
            mel.enabled = true;
            mel.Equip();
            DisableAmmoUI();
            EnableWeaponListUI();
            SetStatus(1f, Melee);
            SetStatus(0.5f, Vandal);
            SetStatus(0.5f, Sheriff);
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
        CurrentBullet.text = currentPlayerF.currentBulletCount.ToString();
        CarryBullet.text = currentPlayerF.carryBulletCount.ToString();
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
            UITimer = 4.0f;
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
            UITimer = 0f;
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
}
