using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UI;
using pxr;

public class EnemyAI : MonoBehaviour
{
    public float timeBetweenAttack = 1f;
    bool alreadyAttacekd;

    private Vector3 startPos;
    public float sightRange;
    public bool moveForward;
    public bool playerVisible;
    private bool isDead;
    
    private Transform player; // Player
    private PlayerHealth playerHealth; // reference to player's health
    public ParticleSystem muzzleFlash;
    public TrailRenderer BulletTrail;
    private Animator animator;
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public AudioClip gunSound;
    public AudioClip movingSound;
    public List<AudioClip> DeathSound;

    GameManager gm;

    private void Awake()
    {
        GameObject go = GameObject.Find("GameManager");
        gm = go.GetComponent<GameManager>();
        isDead = false;
        audioSource1.clip = movingSound;
        audioSource2.clip = gunSound;
        audioSource1.Play();//PlayOneShot(gunSound, 5.0f); // 2.0f doubles perceived volume

        audioSource1.Pause();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        animator = GetComponent<Animator>();
        startPos = transform.position;
    }

    void Update()
    {
        if (!isDead)
        {
            if (moveForward)
            {
                audioSource1.UnPause();
                transform.Translate(Vector3.left * 3f * Time.deltaTime);

                if (Vector3.Distance(startPos, transform.position) >= 3f)
                {
                    audioSource1.Pause();
                    moveForward = false; // stop moving
                }
            }
            if (CanSeePlayer()) // If Player is in visible position
            {
                AttackPlayer();
            }
        }
    }


    private void AttackPlayer()
    {
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z); // Player Location

        transform.LookAt(targetPosition); // Make Bot look at Player

        if (!alreadyAttacekd)
        {
            animator.SetTrigger("shoot"); // Play shooting Animation

            if (playerHealth != null) // If Player is alive
            {     
                playerHealth.TakeDamage(15); // directly damage player
            }
            audioSource2.PlayOneShot(gunSound);

            alreadyAttacekd = true;
            Invoke(nameof(ResetAttack), timeBetweenAttack); // Attack Cooldown

            muzzleFlash.Play();
            TrailRenderer trail = Instantiate(BulletTrail, muzzleFlash.transform.position, Quaternion.identity);
            StartCoroutine(SpawnTrail(trail, player.position + Vector3.up * 0.8f));
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 target)
    {
        float duration = 0;
        Vector3 startPosition = Trail.transform.position;

        while (duration < 1)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, target, duration);

            duration += Time.deltaTime / Trail.time;

            yield return null;
        }
        Trail.transform.position = target;
        Destroy(Trail.gameObject, Trail.time);
    } 

    private void ResetAttack()
    {
        alreadyAttacekd = false;
    }

    bool CanSeePlayer()
    {
        Vector3[] targetPoints = new Vector3[]
        {
        player.position + Vector3.up * 0.8f, // head
        player.position + Vector3.up * 0.2f, // chest
        player.position + Vector3.up * -0.2f, // legs
        };

        foreach (var point in targetPoints)
        {
            Vector3 origin = transform.position + Vector3.up * 2f; // enemy eye height
            Vector3 dir = (point - origin).normalized;
            float dist = Vector3.Distance(origin, point);
            if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
            {
                if (hit.transform == player)
                {
                    Debug.DrawRay(origin, dir * dist, Color.green, 0.1f);
                    return true;

                }
                Debug.DrawRay(origin, dir * dist, Color.red, 0.1f);
            }
            Debug.DrawRay(origin, dir * dist, Color.red, 0.1f);
        }
        return false;
    }

    public void Die()
    {
        if (!isDead)
        {
            AudioClip DeathSoundChosen = DeathSound[Random.Range(0, DeathSound.Count)];
            player.GetComponent<AudioSource>().PlayOneShot(DeathSoundChosen);
            animator.SetTrigger("die");
            isDead = true;
            gm.AddDeath();
            Destroy(gameObject, 2f);
        }
    }
}
