using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;

    private int _currentHealth;

    public int CurrentHealth => _currentHealth;

    public float deathDelay = 1f;

    [Header("UI")]
    public Image healthBarFill;
    private bool _isDead = false;

    private void Awake()
    {
        _currentHealth = maxHealth;
        UpdateHealthUI();
        _controller = GetComponent<PlayerControllerBasic>();
    }

    [Header("Respawn")]
    public float respawnDelay = 5f;
    public Transform respawnPoint;
    private PlayerControllerBasic _controller;

    // Player Damage and Death Check
    public void TakeDamage(int amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;

        if (_currentHealth < 0)
            _currentHealth = 0;

        Debug.Log("Player Health: " + _currentHealth);

        UpdateHealthUI();

        if (_currentHealth <= 0)
        {
            _isDead = true;
            Invoke(nameof(Die), deathDelay);
        }
    }

    // UI connection to Player HP
    private void UpdateHealthUI()
    {
        if (healthBarFill == null) return;

        healthBarFill.fillAmount = (float)_currentHealth / maxHealth;
    }

    //Die and respawn Functions. Does not reset scene, only player position and state:
    private void Die()
    {
        Debug.Log("Player died");

        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (_controller != null)
        {
            _controller.enabled = false;
        }

        Invoke(nameof(Respawn), 5f);
    }

    private void Respawn()
    {
        _currentHealth = maxHealth;
        UpdateHealthUI();

        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }

        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        if (_controller != null)
        {
            _controller.enabled = true;
        }

        _isDead = false;
    }
}
