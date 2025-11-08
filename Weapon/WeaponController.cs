using UnityEngine;
using UnityEngine.Events;

public enum WeaponShootType
{
    Manual,
    Automatic,
}

public class WeaponController : MonoBehaviour
{

    [Header("Information")]
    public string WeaponName;
    public GameObject WeaponRoot;
    public Transform WeaponMuzzle;

    [Header("Shoot Parameters")]
    public WeaponShootType ShootType;
    public float DelayBetweenShots = 0.5f;
    public float BulletSpreadAngle = 0f;
    public int BulletsPerShot = 1;
    public float RecoilForce = 1;

    [Header("Ammo Parameters")]
    public bool AutomaticReload = true;
    public int ClipSize = 50;
    public float AmmoReloadRate = 1f;
    public float AmmoReloadDelay = 2f;
    public int MaxAmmo = 25;

    [Header("Audio & Visual")]
    public Animator WeaponAnimator;
    public GameObject MuzzleFlashPrefab;
    public AudioClip ShootSfx;
    public AudioClip ChangeWeaponSfx;
    bool m_WantsToShoot = false;
    public UnityAction OnShoot;
    int m_CarriedPhysicalBullets;
    int m_CurrentAmmo;
    float m_LastTimeShot = Mathf.NegativeInfinity;
    Vector3 m_LastMuzzlePosition;
    public int GetCurrentAmmo() => m_CurrentAmmo;
    AudioSource m_ShootAudioSource;
    public bool IsReloading { get; private set; }
    const string k_AnimAttackParameter = "Attack";

    void Awake()
    {
        m_CurrentAmmo = MaxAmmo;
        m_LastMuzzlePosition = WeaponMuzzle.position;
        m_CarriedPhysicalBullets = ClipSize;
        m_ShootAudioSource = GetComponent<AudioSource>();
    }

    void Reload()
    {
        if (m_CarriedPhysicalBullets > 0)
        {
            m_CurrentAmmo = Mathf.Min(m_CarriedPhysicalBullets, ClipSize);
        }

        IsReloading = false;
    }

    public void StartReloadAnimation()
    {
        if (m_CurrentAmmo < m_CarriedPhysicalBullets)
        {
            GetComponent<Animator>().SetTrigger("Reload");
            IsReloading = true;
        }
    }

    public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
    {
        float spreadAngleRatio = BulletSpreadAngle / 180f;
        Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere,
            spreadAngleRatio);

        return spreadWorldDirection;
    }
}
