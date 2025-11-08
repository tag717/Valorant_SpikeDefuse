using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor.Rendering;


public class Spike : MonoBehaviour
{   
    [Header("Reference")]
    PlayerInputHandler m_InputHandler;
    GameManager gm;

    [Header("SFX")]
    public AudioClip DefuseSound;

    [Header("UI")]
    public Slider DifuseUI;

    public GameObject onSpikeUI;
    public GameObject onTapUI;

    float m_gauge;

    public bool m_playerOnSpike = false;
    GameObject m_Spike;
    AudioSource AudioSource;

    JettSound JettAudio;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_InputHandler = GetComponent<PlayerInputHandler>();
        GameObject go = GameObject.Find("GameManager");
        gm = go.GetComponent<GameManager>();
        m_Spike = GameObject.Find("/Bomb");
        AudioSource = gameObject.AddComponent<AudioSource>();
        AudioSource.clip = DefuseSound;
        JettAudio = GetComponent<JettSound>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_gauge >= 7)
        {
            onSpikeUI.SetActive(false);
            onTapUI.SetActive(false);
            m_Spike.GetComponent<AudioSource>().Stop();
            StartCoroutine(gm.GameOver("SPIKE DEFUSED"));
            //JettAudio.PlayDefused();

        }
        else if (m_InputHandler.GetSpikeButtonDown())
        {
            AudioSource.Play();
        }
        else if (m_InputHandler.GetSpikeButtonHeld())
        {
            onSpikeUI.SetActive(false);
            onTapUI.SetActive(true);
            m_gauge += Time.deltaTime;
            //add stop other action code
            //need to add stop reload coroutine
        }
        else if (m_InputHandler.GetSpikeButtonUp())
        {
            StopDiffuse();
        }
        DifuseUI.value = m_gauge;
    }

    private void StopDiffuse()
    {
        AudioSource.Stop();
        onTapUI.SetActive(false);
        onSpikeUI.SetActive(true);
        if (m_gauge >= 3.5)
        {
            m_gauge = 3.5f;
        }
        else
        {
            m_gauge = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SPIKE"))
        {
            m_InputHandler.m_playerOnSpike = m_gauge < 7;
            m_playerOnSpike = m_gauge < 7;
            onSpikeUI.SetActive(true);
            //Debug.Log("Player entered spike zone");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SPIKE"))
        {
            StopDiffuse();
            m_InputHandler.m_playerOnSpike = false;
            m_playerOnSpike = false;
            onSpikeUI.SetActive(false);
            //Debug.Log("Player exited spike zone");
        }
    }

}
