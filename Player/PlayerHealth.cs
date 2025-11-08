using UnityEngine;
using TMPro;


public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100;
    public float maxShield = 50;
    public float currentHealth;
    public float currentShield;
    private GameManager GM;

    //HUD components
    [Header("health bar")]
    public TextMeshProUGUI HealthBar;
    public TextMeshProUGUI SheildBar;

    JettSound Js;

    void Start()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;
        GM = (GameObject.Find("GameManager")).GetComponent<GameManager>();
        Js = GetComponent<JettSound>();

    }

    void Update()
    {
        HealthBar.text = currentHealth.ToString();
        SheildBar.text = currentShield.ToString();
    }
    public void TakeDamage(int amount)
    {
        if (currentShield > 0)
        {
            float damageleft = 0;

            if (currentShield < Mathf.RoundToInt(amount * 0.66f))
            {
                damageleft = Mathf.RoundToInt(amount * 0.66f) - currentShield;
                currentHealth -= Mathf.RoundToInt(amount * 0.34f) + damageleft;
                currentShield = 0;
                Debug.Log(damageleft);
            }
            else
            {
                currentShield -= Mathf.RoundToInt(amount * 0.66f);
                currentHealth -= Mathf.RoundToInt(amount * 0.34f);
            }

        }
        else
        {
            currentHealth -= amount;
        }

        if (currentHealth <= 0)
        {
            Kill();
        }

        Js.PlayGrunt();

    }

    void Kill()
    {
        Debug.Log($"{gameObject.name} died");
        StartCoroutine(GM.GameOver("PLAYER DIED"));

    }
}
