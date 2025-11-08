using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100;
    public float maxShield = 50;
    public float currentHealth;
    public float currentShield;


    void Start()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;


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
            GetComponent<EnemyAI>().Die();
        }
    }
}
