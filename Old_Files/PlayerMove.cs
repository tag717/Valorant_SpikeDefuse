using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
public class PlayerMovement : MonoBehaviour
{
    [Header("Arm Animation")]
    public Animator animator;

    [Header("Player Setttings")]
    public float speed;
    public float acceleration;
    public float deceleration;
    public float gravity = 9.8f;
    public float jumpHeight = 2f;

    public Vector3 velocity;
    public bool isGrounded;

    private CharacterController controller;
    public GameObject Gun;
    private bool wasGrounded;
    private float lastHeight;
    [Header("Sound Effects")]
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public AudioSource audioSource3;
    public AudioClip movingSound;
    public AudioClip landingSound;
    public AudioClip jumpingSound;

    // Landing Parameter
    [Header("Laning SFX Tuning")]
    public float minAirTime = 0.12f;
    public float minFallSpeed = 3.0f;
    public float maxFallSpeed = 12.0f;
    public float landingCooldown = 0.10f;

    // Player Condition
    private float airTime;
    private float lastY;
    private float lastVy;
    private float lastLandingTime;

    //spike
    public float defuseTime = 3f;
    private bool isDefused = false;
    private float defuseProgress = 0f;



    void Start()
    {
        audioSource1.clip = movingSound;
        audioSource2.clip = jumpingSound;
        audioSource3.clip = landingSound;
        controller = GetComponent<CharacterController>();
        audioSource1.Play();
        audioSource1.Pause();
        lastY = transform.position.y;
    }

    void Update()
    {
        float dt = Mathf.Max(Time.deltaTime, 1e-5f);
        float y = transform.position.y;
        float vy = (y - lastY) / dt;

        // current landing condition
        isGrounded = controller.isGrounded;

        // count air time
        if (!isGrounded) airTime += dt;

        if (isGrounded && !wasGrounded)
        {
            float impactSpeed = -lastVy;
            bool enoughAir = airTime >= minAirTime;
            bool enoughSpeed = impactSpeed >= minFallSpeed;
            bool cooledDown = (Time.time - lastLandingTime) >= landingCooldown;

            if (enoughAir && enoughSpeed && cooledDown)
            {
                animator.SetTrigger("land");
                audioSource3.PlayOneShot(landingSound);
                lastLandingTime = Time.time;
            }
            airTime = 0f;
        }

        wasGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -1f;

        lastY = y;
        lastVy = vy;

        animator.SetBool("isGrounded", controller.isGrounded);


    }


    public bool MoveCheck()
    {
        return !isGrounded || controller.velocity.magnitude > 0.1f;
    }



    public void Move(Vector2 input)
    {
        Vector3 dir = (transform.right * input.x + transform.forward * input.y).normalized;
        Vector3 vel = new Vector3(velocity.x, 0f, velocity.z);

        if (input.magnitude > 0.01f)
        {
            Vector3 targetVel = dir * speed;
            vel = Vector3.MoveTowards(vel, targetVel, acceleration * Time.deltaTime);
        }
        else
        {
            vel = Vector3.MoveTowards(vel, Vector3.zero, deceleration * Time.deltaTime);
        }

        velocity.x = vel.x;
        velocity.z = vel.z;

        //Debug.Log(controller.velocity.magnitude);

        // Animation and SFX
        if (isGrounded)
        {
            animator.SetBool("isMoving", controller.velocity.magnitude > 0f);
            if (controller.velocity.magnitude >= speed - 1)
            {
                audioSource1.UnPause();
            }
            else
            {
                audioSource1.Pause();
            }
        }

        velocity.y -= gravity * Time.deltaTime * 2;
        controller.Move(velocity * Time.deltaTime);
    }

    public void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
            audioSource2.Play();
            lastHeight = transform.position.y;
            animator.SetTrigger("jump");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("SPIKE SP(IKE");
    }
}