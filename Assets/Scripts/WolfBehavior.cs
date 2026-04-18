using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WolfBehavior : MonoBehaviour
{
    public UnityEvent<WolfBehavior> OnSoundTriggered;

    [Header("Sound Settings")]
    [SerializeField] private float howlHearingRadius = 10f;
    [SerializeField, Range(0f, 1f)] private float howlChanceOnDamage = 0.2f;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("UI")]
    [SerializeField] private Slider healthBar;

    private AudioSource source;

    private Vector3 knockbackVelocity;
    private float knockbackTimer = 0f;

    private bool isDead = false;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    private void Update()
    {
        if (knockbackTimer > 0f)
        {
            transform.position += knockbackVelocity * Time.deltaTime;
            knockbackTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(float damageAmount, Vector3 damageSourcePosition)
    {
        if (isDead)
            return;

        if (damageAmount <= 0f)
            return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        ApplyKnockback(damageSourcePosition);

        if (Random.value <= howlChanceOnDamage)
        {
            Howl();
        }

        if (currentHealth <= 0f)
        {
            isDead = true;
            Die();
        }
    }

    void ApplyKnockback(Vector3 damageSourcePosition)
    {
        Vector3 awayFromHit = (transform.position - damageSourcePosition).normalized;
        awayFromHit.y = 0f;

        knockbackVelocity = awayFromHit * knockbackForce;
        knockbackTimer = knockbackDuration;
    }

    public void Howl()
    {
        if (source != null)
        {
            source.Play();
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, howlHearingRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject == gameObject)
                continue;

            WolfBehavior otherWolf = hits[i].GetComponentInParent<WolfBehavior>();
            if (otherWolf != null)
            {
                otherWolf.ReceiveHowl(this);
            }
        }
    }

    public void ReceiveHowl(WolfBehavior sourceWolf)
    {
        OnSoundTriggered?.Invoke(sourceWolf);
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    [SerializeField] private float deathDelay = 0.2f;

    void Die()
    {
        Destroy(gameObject, deathDelay);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, howlHearingRadius);
    }
}
