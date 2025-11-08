using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;


public class GameManager: MonoBehaviour
{
    public GameObject botPrefab;
    public Transform spawnPoint1;
    public List<Transform> DFSpawnPoints = new List<Transform>();


    [Header("SPIKE")]
    public float SpikeTimer = 0f;
    public GameObject spike;

    [Header("Game Guide UI")]
    public TextMeshProUGUI guide;
    public Image img;
    public Image img2;
    private float m_GuideCount = 0f;

    [Header("Game over UI")]
    public GameObject gameOverScreen;
    public GameObject gameOverBlackScreen;
    public TextMeshProUGUI message;
    public TextMeshProUGUI W_Lmessage;


    AudioSource SpikeAudioSource;
    public int BotCount = 5;
    public int m_deathCount = 0;
    public bool m_gameOver = false;
    bool m_GameStarted = false;
    static float k_startOverTime = 1.0f;

    //jett sound
    JettSound JettAudio;

    void Start()
    {
        Time.timeScale = 1f;
        GameObject SPIKE = GameObject.Find("Bomb");
        GameObject Jett = GameObject.Find("Jett");
        JettAudio = Jett.GetComponent<JettSound>();
        SpikeAudioSource = SPIKE.GetComponent<AudioSource>();


    }

    void Update()
    {
        if (m_GameStarted)
        {
            if (SpikeTimer < 100f)
            {
                SpikeTimer += Time.deltaTime;
            }
            else if (SpikeTimer > 100f)
            {
                if (spike != null)
                {
                    Destroy(spike, 2f);
                    StartCoroutine(GameOver("SPIKE DETONATED"));
                }
            }
        }

        if (m_deathCount == 2)
            {
            HeavenBotSpawn();
            m_deathCount ++; //need this line for stopping unlimited spawns
        }
        if (m_GuideCount > 3f)
        {
            removeGuide();
            m_GuideCount = 3f; //stops incrementing
        }
        else if (m_GuideCount < 3f)
        {
            m_GuideCount += Time.deltaTime;
        }
        else if (m_deathCount == 7)
        {
            JettAudio.PlayAce();
            m_deathCount ++; //need this to play sound once
        } 

        if (m_gameOver)
        {
            var tempColor = gameOverBlackScreen.GetComponent<Image>().color;
            tempColor.a += 0.8f * Time.deltaTime;
            gameOverBlackScreen.GetComponent<Image>().color = tempColor;
        }
    }

    public void HeavenBotSpawn() 
    {
        var newBot = Instantiate(botPrefab, spawnPoint1.position, spawnPoint1.rotation);

        //make bot move forward
        EnemyAI nb = newBot.GetComponent<EnemyAI>();
        nb.moveForward = true;

    }

    public void DefaultBotSpawn()
    {
        for (int i = 0; i < DFSpawnPoints.Count; i++) //gives random spawnPoints array back
        {
            Transform temp = DFSpawnPoints[i];
            int rand = Random.Range(i, DFSpawnPoints.Count);
            DFSpawnPoints[i] = DFSpawnPoints[rand];
            DFSpawnPoints[rand] = temp;
        }

        for (int i = 0; i < BotCount; i++) //spawns five bots in scene
        {
            Instantiate(botPrefab, DFSpawnPoints[i].position, DFSpawnPoints[i].rotation);
        }

    }

    public void AddDeath()
    {
        m_deathCount++;
        if (JettAudio == null) Debug.Log("NULL");
        //JettAudio.PlayKill();
    }

    private void removeGuide()
    {
        if ((img != null) && (img2 != null) && (guide != null))
        {
            img.enabled = false;
            img2.enabled = false;
            guide.enabled = false;

        }
    }

    public void GameStart()
    {
        if (!m_GameStarted)
        {
            DefaultBotSpawn();
            SpikeAudioSource.Play();
            m_GameStarted = true;
        }
    }

    public IEnumerator GameOver(string condition)
    {

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            Image gameover = gameOverScreen.GetComponentInChildren<Image>();
            if (condition == "SPIKE DETONATED" || condition == "PLAYER DIED") //on lost conditions
            {
                gameover.color = Color.red;
                W_Lmessage.text = "LOST";


            }
            else //on won conditions
            {
                gameOverScreen.SetActive(true);
                W_Lmessage.text = "WON";
            }
            message.text = condition;

            
        }
  
        Time.timeScale = 0.4f; // Slow-down Effect
        m_gameOver = true;
        yield return new WaitForSeconds(k_startOverTime);
        GameRestart();
        //Cursor.visible = true;
        //Cursor.lockState = CursorLockMode.None;
    }

    public void GameRestart()
    {
        SceneManager.LoadScene("SampleScene"); // Reloads the current scene
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
