using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public float LookSensitivity = 0.25f;
    PlayerCharacterController m_PlayerCharacterController;
    bool m_FireInputWasHeld;
    bool m_SpikeInputHeld;
    public bool m_playerOnSpike;


    private InputAction m_MoveAction;
    private InputAction m_LookAction;
    private InputAction m_JumpAction;
    private InputAction m_FireAction;
    private InputAction m_ReloadAction;
    private InputAction m_SwitchAction;
    private InputAction m_PrimaryWeaponAction;
    private InputAction m_SideArmAction;
    private InputAction m_MeleeAction;
    private InputAction m_SpikeAction;
    private InputAction m_SkillAction;

    void Start()
    {
        m_PlayerCharacterController = GetComponent<PlayerCharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        m_MoveAction = InputSystem.actions.FindAction("Player/Move");
        m_LookAction = InputSystem.actions.FindAction("Player/Look");
        m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
        m_FireAction = InputSystem.actions.FindAction("Player/Fire");
        m_ReloadAction = InputSystem.actions.FindAction("Player/Reload");
        m_SwitchAction = InputSystem.actions.FindAction("Player/Switch");
        m_PrimaryWeaponAction = InputSystem.actions.FindAction("Player/Primary");
        m_SideArmAction = InputSystem.actions.FindAction("Player/SideArm");
        m_MeleeAction = InputSystem.actions.FindAction("Player/Melee");
        m_SpikeAction = InputSystem.actions.FindAction("Player/SpikeInteraction");
        m_SkillAction = InputSystem.actions.FindAction("Player/Skills");



        m_MoveAction.Enable();
        m_LookAction.Enable();
        m_JumpAction.Enable();
        m_FireAction.Enable();
        m_ReloadAction.Enable();
        m_SwitchAction.Enable();
        m_PrimaryWeaponAction.Enable();
        m_SideArmAction.Enable();
        m_MeleeAction.Enable();
        m_SpikeAction.Enable();
        m_SkillAction.Enable();
    }

    void LateUpdate()
    {
        m_FireInputWasHeld = GetFireInputHeld();
        m_SpikeInputHeld = GetSpikeButtonHeld();
    }

    // Update is called once per f rame
    public bool CanProcessInput()
    {
        return Cursor.lockState == CursorLockMode.Locked;
    }

    public Vector3 GetMoveInput()
    {
        if (CanProcessInput() && !m_SpikeInputHeld)
        {
            var input = m_MoveAction.ReadValue<Vector2>();
            Vector3 move = new Vector3(input.x, 0f, input.y);

            move = Vector3.ClampMagnitude(move, 1);

            return move;
        }
        return Vector3.zero;
    }

    public float GetLookInputsHorizontal()
    {
        if (!CanProcessInput())
            return 0.0f;

        float input = m_LookAction.ReadValue<Vector2>().x;

        input *= LookSensitivity;

        return input;
    }

    public float GetLookInputsVertical()
    {
        if (!CanProcessInput())
            return 0.0f;

        float input = m_LookAction.ReadValue<Vector2>().y;

        input *= LookSensitivity;

        return input;
    }

    public bool GetJumpInputDown()
    {
        if (CanProcessInput() && !m_SpikeInputHeld)
        {
            return m_JumpAction.WasPressedThisFrame();
        }

        return false;
    }

    public bool GetJumpInputHeld()
    {
        if (CanProcessInput() && !m_SpikeInputHeld)
        {
            return m_JumpAction.IsPressed();
        }

        return false;
    }

    public bool GetFireInputDown()
    {
        return GetFireInputHeld() && !m_FireInputWasHeld;
    }

    public bool GetFireInputReleased()
    {
        return !GetFireInputHeld() && m_FireInputWasHeld;
    }

    public bool GetFireInputHeld()
    {
        if (CanProcessInput() && !m_SpikeInputHeld)
        {
            return m_FireAction.IsPressed();
        }

        return false;
    }

    public bool GetReloadButtonDown()
    {
        if (CanProcessInput() && !m_SpikeInputHeld)
        {
            return m_ReloadAction.WasPressedThisFrame();
        }

        return false;
    }

    public float GetSwitchInput()
    {
        if (CanProcessInput() && !m_SpikeInputHeld)
        {
            return m_SwitchAction.ReadValue<Vector2>().y;
        }
        return 0;
    }

    public bool GetPrimaryWeaponButtonDown()
    {
        if (CanProcessInput() && !m_SpikeInputHeld)
        {
            return m_PrimaryWeaponAction.WasPressedThisFrame();
        }
        return false;
    }

    public bool GetSideArmButtonDown()
    {
        if (CanProcessInput() && !m_SpikeInputHeld)
        {
            return m_SideArmAction.WasPressedThisFrame();
        }
        return false;
    }

    public bool GetMeleeButtonDown()
    {
        if (CanProcessInput() && !m_SpikeInputHeld)
        {
            return m_MeleeAction.WasPressedThisFrame();
        }
        return false;
    }

    public bool GetSpikeButtonDown()
    {
        return GetSpikeButtonHeld() && !m_SpikeInputHeld;
    }

    public bool GetSpikeButtonHeld()
    {
        if (CanProcessInput())
        {
            return m_SpikeAction.IsPressed() && m_playerOnSpike;
        }
        return false;
    }

    public bool GetSpikeButtonUp()
    {
        if (CanProcessInput())
        {
            bool StopDiffuse = m_SpikeAction.WasReleasedThisFrame() && m_playerOnSpike;
            return StopDiffuse;
        }
        return false;
    }

    public bool GetSkillInputDown()
    {
        if (CanProcessInput() && !m_SpikeInputHeld)
        {
            return m_SkillAction.WasPressedThisFrame();
        }

        return false;
    }

}
