using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using TMPro;


public class GunController : MonoBehaviour
{
    [Header("Information")]
    public static bool isActivate = true;
    public GameObject damagePopup;
    public PlayerMovement playerMovement;

    [SerializeField]

    [Header("Shoot Parameters")]
    [HideInInspector] public float EquipCooldown = 0f;
    public CameraRecoil Recoil_script;
    private float RecoilTime = 0f; //variable to measure sheriff's recoil time to set accuracy
    private float VanRecoilTime = 0f;
    private int RecoilCount = 0; //vandal allows 3 accurate shots without recoil time
    private float SprayTime = 0f;
    private float currentFireRate;
    
    public bool isFineSightMode = false;

    [Header("Audio & Visual")]
    public Animator playerAnimator;
    public Animator WeaponAnimator;
    public AudioSource audioSource;
    private AudioSource gunAudioSource; // for gunshots

    public GunInfo CurrentGun { get; set;}
    public bool IsReload { get; private set; }
    public float GunAccuracy { get; private set; }
    Vector3 m_originPos; //original gun poisition before finesight


    void Start()
    {
        m_originPos = Vector3.zero;

        WeaponAnimator = CurrentGun.anime;
        playerMovement = GetComponentInParent<PlayerMovement>();
        gunAudioSource = gameObject.AddComponent<AudioSource>(); // add extra AudioSource for gunshots
    }

    void Update()
    {
        if (!isActivate) return;

        if (EquipCooldown > 0f)
            EquipCooldown -= Time.deltaTime;

        if (RecoilTime > 0f)
            RecoilTime -= Time.deltaTime;

        if (VanRecoilTime > 0f)
            VanRecoilTime -= Time.deltaTime;


        GunFireRateCalc();
        TryFire();
        TryReload();
        //TryFineSight();
    }

    public Animator GetAnimator()
    {
        return WeaponAnimator;
    }

    public void SetAnimator(Animator newAnimator)
    {
        WeaponAnimator = newAnimator;
    }

    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime;  //reduces 1 sec by 1 sec
    }

    private void TryFire()  //got left click
    {
        if (EquipCooldown > 0f) return; //if in equip motion, do not allow fire 

        if (Input.GetButton("Fire1") && currentFireRate <= 0 && !IsReload)
        {
            Fire();
        }
    }

    private void Fire()  //process to fire
    {
        if (!IsReload)
        {
            if (CurrentGun.currentBulletCount > 0)
            {
                Shoot();
            }
            else
            {
                //CancelFineSight();
                StartCoroutine(reloadCoroutine());
            }
            
        }
    }

    private void Shoot()  
    {
        Recoil_script.RecoilFire();
        playerAnimator.SetTrigger("shoot");
        WeaponAnimator.SetTrigger("shoot");
        CurrentGun.currentBulletCount--;
        currentFireRate = CurrentGun.fireRate;
        gunAudioSource.PlayOneShot(CurrentGun.fireSound);
        CurrentGun.muzzleFlash.Play();

        Hit();
        if (CurrentGun.Gun == "Sheriff")
            RecoilTime = 0.5f;
        else if (CurrentGun.Gun == "Vandal") 
        {
            if (VanRecoilTime <= 0f) //first shot in a while
            {
                RecoilCount = 2;
                SprayTime = 0;
            }
            else if (RecoilCount > 0) //allow three shots without recoil time
            {
                Debug.Log(RecoilCount.ToString());
                RecoilCount--;
                SprayTime = 0;

            }
            else //spary
            {
                RecoilTime = 0.375f;
                SprayTime += 0.3f;   // count shots instead of seconds
            }
            VanRecoilTime = 0.375f;

        }
        
    }

    private void TryReload()
    {
        if (EquipCooldown > 0f) return; //if in equip motion, do not allow reload
        if (Input.GetKeyDown(KeyCode.R) && !IsReload && CurrentGun.currentBulletCount < CurrentGun.reloadBulletCount)
        {
            //CancelFineSight();
            StartCoroutine(reloadCoroutine());
        }
    }

    IEnumerator reloadCoroutine()
    {
        WeaponAnimator.SetTrigger("reload");
        playerAnimator.SetTrigger("reload");
        if (CurrentGun.carryBulletCount > 0)
        {
            IsReload = true;
            PlaySE(CurrentGun.reloadSound);
            //currentGun.anime.SetTrigger("Reload");



            yield return new WaitForSeconds(CurrentGun.reloadTime);  // according as reload animation time

            if (CurrentGun.carryBulletCount >= CurrentGun.reloadBulletCount)
            {
                int reloadCount = CurrentGun.reloadBulletCount - CurrentGun.currentBulletCount;
                CurrentGun.currentBulletCount += reloadCount;
                CurrentGun.carryBulletCount -= reloadCount;
            }
            else
            {
                CurrentGun.currentBulletCount = CurrentGun.carryBulletCount;
                CurrentGun.carryBulletCount = 0;
            }

            IsReload = false;
        }
        else
        {
            Debug.Log("bullets ran out");
        }
    }

    private void Hit() //hit effect
    {
        //accuracy offset codes returns true when moving/jumping
        Vector3 spread = GetAccuracy();

        
        // normalize so spread doesn't skew vector length
        Vector3 direction = (Camera.main.transform.forward +
                    Camera.main.transform.TransformDirection(spread)).normalized;

        Ray ray = new Ray(Camera.main.transform.position, direction);
        //ray collide
        RaycastHit hitInfo = new RaycastHit();

        //if hitinfo = penetrable dmg decrease and layermask
        //ignore player
        int layerMask = ~LayerMask.GetMask("Player", "VFX", "UI");

        //if hit hit effect
        if (Physics.Raycast(ray, out hitInfo, 1000f, layerMask, QueryTriggerInteraction.Ignore))
        {
            int dmg = CurrentGun.damage; //get damage of current gun

            if (hitInfo.collider.CompareTag("Penetrable")) //if it hit a penetrable object, go through
            {
                int newLayerMask = ~LayerMask.GetMask("Player", "VFX", "UI", "Penetrable"); //ignore penetrable object
                dmg = dmg / 2; //reduce damage to 50% 
                GameObject clone = Instantiate(CurrentGun.bulletEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                Destroy(clone, 2f); //instantiate bullet effect on penetrable object 

                if (Physics.Raycast(ray, out hitInfo, 1000f, newLayerMask, QueryTriggerInteraction.Ignore))
                {
                    dmg = DamageCalc(hitInfo, dmg);
                    if (dmg == 0) //when no bot is hit
                    {
                        GameObject clone2 = Instantiate(CurrentGun.bulletEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                        Destroy(clone2, 2f);
                    }
                    else //when bot is hit
                    {
                        DamagePopup(dmg, hitInfo);
                    }
                }
            }
            else
            {
                dmg = DamageCalc(hitInfo, dmg); //returns dmg according to object hit
                if (dmg == 0) //when no bot is hit
                {
                    GameObject clone = Instantiate(CurrentGun.bulletEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                    Destroy(clone, 2f);
                }
                else //when bot is hit
                {
                    DamagePopup(dmg, hitInfo);
                }
            }
        }
    }

    private int DamageCalc(RaycastHit HI, int damage)
    {
        if (HI.collider.CompareTag("Head"))
        {
            Debug.Log("HeadShot!");
            damage = Mathf.CeilToInt(damage * CurrentGun.headshotM);
        }
        else if (HI.collider.CompareTag("Body"))
        {
            Debug.Log("BodyShot");
        }
        else if (HI.collider.CompareTag("Leg"))
        {
            Debug.Log("LegShot!");
            damage = Mathf.CeilToInt(damage * CurrentGun.legshotM);
        }
        else
        {
            Debug.Log("no bot hit");
            return 0;

        }
        return damage;

    }

    private void DamagePopup(int dmg, RaycastHit hitInfo)
    {
        //damage pop up  message
        Vector3 popupPos = hitInfo.point + hitInfo.normal * 0.02f;
        GameObject popup = Instantiate(damagePopup, popupPos, Quaternion.identity);
        popup.GetComponent<DamageDisplay>().Setup(dmg);

        GameObject clone = Instantiate(CurrentGun.HitEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
        Destroy(clone, 2f);

        //apply damage to object
        Health h = hitInfo.collider.GetComponentInParent<Health>();
        if (h != null)
        {
            h.TakeDamage(dmg);
        }
    
    }

    public void OutSound()
    {
        PlaySE(CurrentGun.pullOutSound);
    }

    private void PlaySE(AudioClip _clip)  
    {
        audioSource.clip = _clip;  
        audioSource.Play();      
    }

    public void StopClip(AudioClip clip)
    {
        if (audioSource.isPlaying && audioSource.clip == clip)
        {
            audioSource.Stop();
        }
    }

    public void CancelReload()
    {
        StopClip(CurrentGun.pullOutSound); //easy fix to cancel out pullout audio 
        if (IsReload)
        {
            StopAllCoroutines();
            StopClip(CurrentGun.reloadSound);
            IsReload = false;
        }
    }



    public Vector3 GetAccuracy()
    {
        Vector3 spread = Vector3.zero; //random spread

        if (playerMovement.MoveCheck())
        {
            GunAccuracy = 0.15f; //gun shot accuracy when walking
        }
        else if ((CurrentGun.Gun == "Sheriff") && RecoilTime > 0f)
        {
            GunAccuracy = 0.11f;
        }
        else if ((CurrentGun.Gun == "Vandal") && RecoilTime > 0f)
        {
            GunAccuracy = 0.03f; //TO IMPLEMENT SPARY PATTERN, DO IT HERE
            spread = new Vector3(UnityEngine.Random.Range(-GunAccuracy, GunAccuracy)
                    , GunAccuracy*SprayTime, 0f);
            return spread;
        }
        else
        {
            GunAccuracy = 0.001f;
        }
        spread = new Vector3(
            UnityEngine.Random.Range(-GunAccuracy, GunAccuracy),
            UnityEngine.Random.Range(-GunAccuracy, GunAccuracy),
            GunAccuracy
            );
        return spread;
    }

}


/*
private void TryFineSight()
{
    if (Input.GetButtonDown("Fire2") && !isReload)
        FineSight();

}

private void FineSight()
{
    isFineSightMode = !isFineSightMode;
    //currentGun.anime.SetBool("FineSightMode", isFineSightMode);

    if(isFineSightMode)
    {
        StopAllCoroutines();
        StartCoroutine(FineSightActivateCoroutine());
    }
    else
    {
        StopAllCoroutines();
        StartCoroutine(FineSightDeActivateCoroutine());
    }
}

public bool GetFineSightMode()
{
    return isFineSightMode;
}



public void CancelFineSight()
{
    if (isFineSightMode)
        FineSight();
}

IEnumerator FineSightActivateCoroutine()
{
    while(currentGun.transform.localPosition != currentGun.fineSightOriginPos)
    {
        currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.2f);
        yield return null;
    }
}

IEnumerator FineSightDeActivateCoroutine()
{
    while (currentGun.transform.localPosition != originPos)
    {
        currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
        yield return null;
    }
}
*/