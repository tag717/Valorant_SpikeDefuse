using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunInfo : MonoBehaviour
{
    public string Gun;
    public float accuracy; 
    public float fireRate; //higher = slower fire rate, lower = faster fire rate
    public float reloadTime; //reload time for each guns
    public int damage; //damage for each guns
    public float headshotM; //head shot damage multiplier by gun
    public float legshotM; //leg shot damage multiplier by gun
    public int reloadBulletCount; //number of bullets that needs to be reloaded
    public int currentBulletCount; 
    public int maxBulletCount;
    public int carryBulletCount;
    public Vector3 fineSightOriginPos;

    //recoil
    public float recoil;
    public float recoilscoped;

    public Animator anime;
    public Animator playerAnime;
    public GameObject bulletEffect;
    public GameObject HitEffect;
    public ParticleSystem muzzleFlash; //fire effect on gun
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip pullOutSound;

    public float equipDelay;

}
