
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{
    [Header("References")]
    public Camera PlayerCamera;
    public AudioSource AudioSource;
    public Animator Animator;

    [Header("General")]
    public float GravityDownForce = 20f;
    public float GroundCheckDistance = 0.05f;
    public LayerMask GroundCheckLayers = -1;

    [Header("Movement")]
    public float MaxSpeedOnGround = 10f;
    public float MovementSharpness = 15;
    public float MaxSpeedInAir = 10f;
    public float AccelerationSpeedInAir = 25f;

    [Header("Rotation")]
    public float RotationSpeed = 200f;

    [Header("Jump")]
    public float JumpForce = 9f;

    [Header("Audio")]
    public float FootstepSfxFrequency = 0.3f;

    public AudioClip FootstepSfx;
    public AudioClip JumpSfx;
    public AudioClip LandSfx;

    public Vector3 CharacterVelocity { get; set; }
    public bool IsGrounded; //{ get; private set; }
    public bool HasJumpedThisFrame { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsMoving { get; private set; }

    PlayerInputHandler m_InputHandler;
    CharacterController m_Controller;
    Vector3 m_GroundNormal;
    float m_LastTimeJumped = 0f;
    float m_LastTimeInAir = 0f;
    float m_CameraVerticalAngle = 0f;
    float m_FootstepDistanceCounter;

    const float k_JumpGroundingPreventionTime = 0.2f;
    const float k_GroundCheckDistanceInAir = 0.07f;


    void Start()
    {
        m_Controller = GetComponent<CharacterController>();
        m_InputHandler = GetComponent<PlayerInputHandler>();

        m_Controller.enableOverlapRecovery = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(m_InputHandler.GetSwitchInput());
        HasJumpedThisFrame = false;

        bool wasGrounded = IsGrounded;
        GroundCheck();
        Animator.SetBool("IsGrounded", IsGrounded);

        if (!IsGrounded && wasGrounded)
        {
            m_LastTimeInAir = Time.time;
        }

        if (IsGrounded && !wasGrounded && Time.time > m_LastTimeInAir + k_JumpGroundingPreventionTime)
        {
            Animator.SetTrigger("Land");
            AudioSource.PlayOneShot(LandSfx);
        }

        HandleCharacterMovement();
    }

    void GroundCheck()
    {
        IsGrounded = false;
        m_GroundNormal = Vector3.up;

        if (Time.time >= m_LastTimeJumped + k_JumpGroundingPreventionTime)
        {
            Debug.DrawLine(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(), Color.red);
            if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(),
                m_Controller.radius * 0.01f, Vector3.down, out RaycastHit hit, GroundCheckDistance, GroundCheckLayers,
                QueryTriggerInteraction.Ignore))
            {
                // storing the upward direction for the surface found
                m_GroundNormal = hit.normal;

                // Only consider this a valid ground hit if the ground normal goes in the same direction as the character up
                // and if the slope angle is lower than the character controller's limit
                if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                    IsNormalUnderSlopeLimit(m_GroundNormal))
                {
                    IsGrounded = true;

                    // handle snapping to the ground
                    /*
                    if (hit.distance > m_Controller.skinWidth)
                    {
                        m_Controller.Move(Vector3.down * hit.distance);
                    }
                    */
                }
            }
        }
    }

    void HandleCharacterMovement()
    {
        // horizontal character movement
        transform.Rotate(new Vector3(0f, (m_InputHandler.GetLookInputsHorizontal() * RotationSpeed), 0f), Space.Self);

        // vertical camera movement
        m_CameraVerticalAngle += m_InputHandler.GetLookInputsVertical() * RotationSpeed;
        m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle, -80f, 80f);
        PlayerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, 0);

        // character transform movement
        Vector3 worldspaceMoveInput = transform.TransformVector(m_InputHandler.GetMoveInput());
        IsMoving = m_InputHandler.GetMoveInput().magnitude != 0;
        Animator.SetBool("Moving", IsMoving);
        if (IsGrounded)
        {
            Vector3 targetVelocity = worldspaceMoveInput * MaxSpeedOnGround;
            CharacterVelocity = Vector3.Lerp(CharacterVelocity, targetVelocity,
                        MovementSharpness * Time.deltaTime);

            // jumping
            if (m_InputHandler.GetJumpInputDown())
            {
                CharacterVelocity = new Vector3(CharacterVelocity.x, 0f, CharacterVelocity.z);
                CharacterVelocity += Vector3.up * JumpForce;
                Animator.SetTrigger("Jump");
                AudioSource.PlayOneShot(JumpSfx);

                m_LastTimeJumped = Time.time;
                HasJumpedThisFrame = true;
                IsGrounded = false;
            }

            // footstep sound
            if (m_FootstepDistanceCounter >= 1f / FootstepSfxFrequency)
            {
                m_FootstepDistanceCounter = 0f;
                AudioSource.PlayOneShot(FootstepSfx);
            }

            m_FootstepDistanceCounter += CharacterVelocity.magnitude * Time.deltaTime;
        }
        else
        {
            CharacterVelocity += worldspaceMoveInput * AccelerationSpeedInAir * Time.deltaTime;
            CharacterVelocity += Vector3.down * GravityDownForce * Time.deltaTime;
        }

        m_Controller.Move(CharacterVelocity * Time.deltaTime);
    }

    bool IsNormalUnderSlopeLimit(Vector3 normal)
    {
        return Vector3.Angle(transform.up, normal) <= m_Controller.slopeLimit;
    }

    Vector3 GetCapsuleBottomHemisphere()
    {
        return Camera.main.transform.position - (transform.up * (m_Controller.height - 0.1f - m_Controller.radius));
    }

    Vector3 GetCapsuleTopHemisphere()
    {
        return Camera.main.transform.position - (transform.up * (m_Controller.radius - 0.1f));
    }

    
}
