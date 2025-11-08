using UnityEngine;

public class Recoil : MonoBehaviour
{
    private GunController player;
    private GunInfo gun;
    private Animator animator;
    private bool isFiring;
    private float t = 0;
    [Header("Recoil Time")]
    public float recoilSpeed;
    public float recoilRecoverySpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = gameObject.GetComponentInParent<GunController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        isFiring = Input.GetButton("Fire1");
        gun = player.CurrentGun;
        if (gun != null)
        {
            float target = isFiring ? 1f : 0f;
            float spd = isFiring ? recoilSpeed : recoilRecoverySpeed;
            t = Mathf.MoveTowards(t, target, spd * Time.deltaTime);
            if (t >= 0.99) t = 70 / 160;
            if (isFiring) animator.Play("Vandal", 0, t);
        }
        if (Input.GetButtonUp("Fire1"))
        {
            animator.SetTrigger("stopped");
        }
    }
}
