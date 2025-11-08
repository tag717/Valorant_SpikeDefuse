using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Melee : MonoBehaviour
{
    public GameObject damagePopup;
    public GameObject HitEffect;


    public float range;
    public int primaryDamage;
    public int alternativeDamage;
    private int dmg;
    public float attackDelay; //attack delay time(motion time)
    public float attackHitTime; //time needs the weapon to have active damage
    public float attackDeacTime; //when the weapon is in motion but no damage is appling

    public Animator anime;

    private bool isAttack = false;
    private bool isSwing = false; //in swing motion
    private RaycastHit hitInfo; //get hitinfo if anything hit \
    public GameObject bulletEffect;
    public AudioClip swingSound;
    public AudioClip stabSound;
    public AudioClip pullOutSound;
    private AudioSource AudioSource;

    void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
    }

    public static bool isActivate = true;
    public void Equip()
    {
        AudioSource.PlayOneShot(pullOutSound);
    }

    public bool Attack(int type)
    {
        if (!isAttack)
        {
            if (type == 0)
                StartCoroutine(AttackCoroutine());
            else
                StartCoroutine(AttackCoroutine2());
            return true;
        }
        return false;
    }

    private IEnumerator AttackCoroutine() //primary slash (left click)
    {
        isAttack = true;
        //anime.SetTrigger("Pslash");
        AudioSource.PlayOneShot(swingSound);
        yield return new WaitForSeconds(attackDelay);
        isSwing = true; //after attack start motion, damage is applicable
        dmg = primaryDamage;
        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(attackDeacTime); //hit is done, in swing motion but no damage is applicable
        isSwing = false;

        yield return new WaitForSeconds(attackDelay - attackHitTime - attackDeacTime);
        isAttack = false;
    }

    private IEnumerator AttackCoroutine2() //alternative slash (left click)
    {
        isAttack = true;
        //anime.SetTrigger("Aslash");
        AudioSource.PlayOneShot(stabSound);
        yield return new WaitForSeconds(attackDelay);
        isSwing = true; //after attack start motion, damage is applicable
        dmg = alternativeDamage;
        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(attackDeacTime); //hit is done, in swing motion but no damage is applicable
        isSwing = false;

        yield return new WaitForSeconds(attackDelay - attackHitTime - attackDeacTime);
        isAttack = false;
    }

    private bool CheckObject()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        int layerMask = ~LayerMask.GetMask("Player");
        // use the CLASS FIELD hitInfo instead of redeclaring a new one!
        if (Physics.Raycast(ray, out hitInfo, range, layerMask, QueryTriggerInteraction.Ignore))
        {

            // Apply damage if the target has health
            if (hitInfo.collider.CompareTag("Head")|| hitInfo.collider.CompareTag("Body")|| hitInfo.collider.CompareTag("Leg"))
            {
                ShowDamagePopup(dmg, hitInfo);
            }
            else
            {
                Debug.Log("no bot hit");
                GameObject clone = Instantiate(bulletEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                Destroy(clone, 2f);
            }


            return true;
        }
        return false;
    }

    private void ShowDamagePopup(int dmg, RaycastHit hitInfo)
    {
        //damage pop up  message);

        if (damagePopup == null)
            Debug.Log("damagepopup null");

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

    private IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            if (CheckObject())
            {
                isSwing = false;
                Debug.Log(hitInfo.transform.name);
            }
            yield return null;
        }
    }

    public void ChangeisAttack()
    {
        isSwing = false;
        isAttack = false;
    }


}
