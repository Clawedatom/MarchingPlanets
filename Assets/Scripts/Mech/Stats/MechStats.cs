using UnityEngine;

public class MechStats : MonoBehaviour
{
    #region Private Fields

    [Header("Mech Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isDead;

    [Header("Mech Heating")]
    [SerializeField] private float maxHeating = 100f;
    [SerializeField] private float currentHeating;
    [SerializeField] private bool isOverheating;

    #endregion

    #region Properties

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public float CurrentHeating => currentHeating;
    public float MaxHeating => maxHeating;
    public bool IsOverheating => isOverheating;

    #endregion

    #region Start Up

    public void OnStart()
    {
        SetInitialStats();
    }

    private void SetInitialStats()
    {
        currentHealth = maxHealth;
        currentHeating = 0f;
        isDead = false;
    }

    #endregion

    #region Class Methods

    public void LoseHealth(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath();
        }
    }

    public void GainHealth(float amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    private void OnDeath()
    {
        isDead = true;
    }

    #endregion
}