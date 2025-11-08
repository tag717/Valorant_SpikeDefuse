using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using TMPro;


public class GunController2 : MonoBehaviour
{
    [Header("Information")]
    public string Gun;
    public float accuracy; 
    public float fireRate; //higher = slower fire rate, lower = faster fire rate
    public float reloadTime; //reload time for each guns

    [Header("Bullet Parameters")]
    public int reloadBulletCount; //number of bullets that needs to be reloaded
    public int currentBulletCount; 
    public int maxBulletCount;
    public int carryBulletCount;

    [Header("Shoot Parameters")]
    private float RecoilTime = 0f; //variable to measure sheriff's recoil time to set accuracy
    private float VanRecoilTime = 0f;
    private int RecoilCount = 0; //vandal allows 3 accurate shots without recoil time
    private float SprayTime = 0f;
    private float currentFireRate;
    public bool isFineSightMode = false;

    [Header("Damage parameters")]
    public int damage; //damage for each guns
    public float headshotM; //head shot damage multiplier by gun
    public float legshotM; //leg shot damage multiplier by gun

    [Header("Audio & Visual")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip pullOutSound;
    public Animator WeaponAnimator;
    public TrailRenderer BulletTrail;

    [Header("VFX")]
    public GameObject damagePopup;
    public ParticleSystem muzzleFlash; //fire effect on gun
    public GameObject bulletEffect;
    public GameObject HitEffect;

    public bool IsReloading { get; private set; }
    public bool IsMoving { get; set; }
    public float GunAccuracy { get; private set; }
    public AudioSource AudioSource;
    GameManager GameManager;
    CameraRecoil Recoil_script;
    AudioSource GunAudioSource; // for gunshots
    Vector3 m_originPos; //original gun poisition before finesight

    JettSound Js;
    void Start()
    {
        m_originPos = Vector3.zero;
        WeaponAnimator = GetComponent<Animator>(); ;
        GunAudioSource = gameObject.AddComponent<AudioSource>(); // add extra AudioSource for gunshots
        Recoil_script = FindAnyObjectByType<CameraRecoil>();
        GameManager = FindAnyObjectByType<GameManager>();
        Js = GetComponentInParent<JettSound>();
    }

    void Update()
    {  
        if (RecoilTime > 0f)
            RecoilTime -= Time.deltaTime;

        if (VanRecoilTime > 0f)
            VanRecoilTime -= Time.deltaTime;


        GunFireRateCalc();
        //TryFineSight();
    }

    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime;  //reduces 1 sec by 1 sec
    }

    public bool Shoot()
    {
        if (currentFireRate > 0) return false;

        Recoil_script.RecoilFire();
        WeaponAnimator.SetTrigger("shoot");
        currentBulletCount--;
        currentFireRate = fireRate;
        GunAudioSource.PlayOneShot(fireSound);
        muzzleFlash.Play();

        Hit();
        if (Gun == "Sheriff")
            RecoilTime = 0.5f;
        else if (Gun == "Vandal")
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
        return true;
    }

    public IEnumerator ReloadCoroutine()
        {
            WeaponAnimator.SetTrigger("reload");
            if (carryBulletCount > 0)
            {
                IsReloading = true;
                PlaySE(reloadSound);
                //currentGun.anime.SetTrigger("Reload");

                yield return new WaitForSeconds(reloadTime);  // according as reload animation time

                if (carryBulletCount >= reloadBulletCount)
                {
                    int reloadCount = reloadBulletCount - currentBulletCount;
                    currentBulletCount += reloadCount;
                    carryBulletCount -= reloadCount;
                }
                else
                {
                    currentBulletCount = carryBulletCount;
                    carryBulletCount = 0;
                }

                IsReloading = false;
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
            int dmg = damage; //get damage of current gun

            //Bullet trace
            GameObject MuzzlePoint = GameObject.Find("MuzzlePoint");
            TrailRenderer trail = Instantiate(BulletTrail,MuzzlePoint.transform.position, Quaternion.identity);
            StartCoroutine(SpawnTrail(trail, hitInfo));


            if (hitInfo.collider.CompareTag("StartButton")) //if hit start button, start game
            {
                GameManager.GameStart();
            }
            
            if (hitInfo.collider.CompareTag("Penetrable")) //if it hit a penetrable object, go through
            {
                int newLayerMask = ~LayerMask.GetMask("Player", "VFX", "UI", "Penetrable"); //ignore penetrable object
                dmg = dmg / 2; //reduce damage to 50% 
                GameObject clone = Instantiate(bulletEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                Destroy(clone, 2f); //instantiate bullet effect on penetrable object 

                if (Physics.Raycast(ray, out hitInfo, 1000f, newLayerMask, QueryTriggerInteraction.Ignore))
                {
                    dmg = DamageCalc(hitInfo, dmg);
                    if (dmg == 0) //when no bot is hit
                    {
                        GameObject clone2 = Instantiate(bulletEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
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
                    GameObject clone = Instantiate(bulletEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                    Destroy(clone, 2f);
                }
                else //when bot is hit
                {
                    DamagePopup(dmg, hitInfo);
                }
            }
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, RaycastHit HitInfo)
    {
        float duration = 0;
        Vector3 startPosition = Trail.transform.position;

        while (duration < 1)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, HitInfo.point, duration);

            duration += Time.deltaTime / Trail.time;

            yield return null;
        }
        Trail.transform.position = HitInfo.point;
        Destroy(Trail.gameObject, Trail.time);
    }

    private int DamageCalc(RaycastHit HI, int damage)
    {
        if (HI.collider.CompareTag("Head"))
        {
            Debug.Log("HeadShot!");
            damage = Mathf.CeilToInt(damage * headshotM);

            //if (damage >= 150) Js.PlayHS();
        }
        else if (HI.collider.CompareTag("Body"))
        {
            Debug.Log("BodyShot");
        }
        else if (HI.collider.CompareTag("Leg"))
        {
            Debug.Log("LegShot!");
            damage = Mathf.CeilToInt(damage * legshotM);
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

        GameObject clone = Instantiate(HitEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
        Destroy(clone, 2f);

        //apply damage to object
        Health h = hitInfo.collider.GetComponentInParent<Health>();
        if (h != null)
        {
            h.TakeDamage(dmg);
        }
    
    }

    public void Equip()
    {
        AudioSource.PlayOneShot(pullOutSound);
    }

    public void OutSound()
    {
        PlaySE(pullOutSound);
    }

    private void PlaySE(AudioClip _clip)  
    {
        AudioSource.clip = _clip;  
        AudioSource.Play();      
    }

    public void StopClip(AudioClip clip)
    {
        if (AudioSource.isPlaying && AudioSource.clip == clip)
        {
            AudioSource.Stop();
        }
    }

    public void CancelReload()
    {
        StopClip(pullOutSound); //easy fix to cancel out pullout audio 
        if (IsReloading)
        {
            StopAllCoroutines();
            StopClip(reloadSound);
            IsReloading = false;
        }
    }



    public Vector3 GetAccuracy()
    {
        Vector3 spread = Vector3.zero; //random spread

        if (IsMoving)//playerMovement.MoveCheck())
        {
            GunAccuracy = 0.15f; //gun shot accuracy when walking
        }
        else if ((Gun == "Sheriff") && RecoilTime > 0f)
        {
            GunAccuracy = 0.11f;
        }
        else if ((Gun == "Vandal") && RecoilTime > 0f)
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