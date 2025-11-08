using UnityEngine;

public class JettSound : MonoBehaviour
{
    public AudioSource AudioSource;
    public AudioSource AudioSource2;

    public AudioClip killSound;
    public AudioClip AceSound;

    public AudioClip SkillsUnable;

    public AudioClip GruntSound;

    public AudioClip HeadShot;
    public AudioClip Defused;



    PlayerInputHandler m_InputHandler;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_InputHandler = GetComponent<PlayerInputHandler>();

    }

    // Update is called once per frame
    void Update()
    {
        if (m_InputHandler.GetSkillInputDown()) PlayUnable();
    }
/*
    public void PlayKill()
    {
        AudioSource.PlayOneShot(killSound);
    }
  */
    public void PlayAce()
    {
        AudioSource2.volume = 0.5f;
        AudioSource2.PlayOneShot(AceSound);
    }

    private void PlayUnable()
    {
        AudioSource.PlayOneShot(SkillsUnable);
    }

    public void PlayGrunt()
    {
        AudioSource.PlayOneShot(GruntSound);
    }
/*
    public void PlayHS()
    {
        Invoke("PlayHeadShot", 0.3f);

    }
  */
    private void PlayHeadShot()
    {
        AudioSource.PlayOneShot(HeadShot);

    }
/*
    public void PlayDefused()
    {
        AudioSource.PlayOneShot(Defused);

    }
    */
}
