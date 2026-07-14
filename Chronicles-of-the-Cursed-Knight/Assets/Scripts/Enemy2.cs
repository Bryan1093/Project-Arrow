using UnityEngine;

public class Enemy2 : MonoBehaviour {

    public int maxHealth = 3;
    private bool facingLeft;

    [Header("Rangos de Ataque")]
    public float attackRangeRadius = 6f;
    public LayerMask whatIsPlayer;
    private Animator animator;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    
    [Header("UI - Feedback")]
    public GameObject floatingTextPrefab;
    public Transform textSpawnPoint;

    [Header("Ataque y Proyectiles")]
    public Transform firePoint;
    public GameObject arrowPrefab;
    public float arrowVelocity = 10f;
    public Vector3 offset;

    [Header("Cooldown de Ataque (Tiempos)")]
    public float attackCooldown = 1.5f; // Segundos de espera entre cada flecha
    private float nextAttackTime = 0f;

    // Constantes para parámetros del Animator
    private const string ANIM_ATTACK = "Attack"; // Parámetro Bool en el Animator
    private const string ANIM_DEATH = "Death";
    private const string ANIM_HURT = "Hurt";

    void Start() {
        facingLeft = true;
        nextAttackTime = 0f; // Aseguramos que puede disparar al inicio
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        animator = this.gameObject.GetComponent<Animator>();
        boxCollider2D = this.gameObject.GetComponent<BoxCollider2D>();
    }

    void Update() {
        // Optimización masiva: Accedemos directamente a la instancia Singleton del Player
        if (Player.instance == null) {
            animator.SetBool(ANIM_ATTACK, false);
            return;
        }

        Transform playerTransform = Player.instance.transform;
    
        if (maxHealth <= 0) {
            animator.SetBool(ANIM_DEATH, true);
            Die();
            return;
        }

        // Verificamos si el jugador está en rango circular
        Collider2D collInfo = Physics2D.OverlapCircle(transform.position + offset, attackRangeRadius, whatIsPlayer);
        bool isPlayerInAttackRange = (collInfo != null);

        // --- NUEVA LÓGICA DE CONTROL DE ANIMACIÓN ---

        // Solo activamos la animación de ATAQUE si:
        // 1. El jugador está en rango.
        // 2. Ya ha pasado el tiempo de cooldown (estamos listos para disparar).
        if (isPlayerInAttackRange && Time.time >= nextAttackTime) {
            animator.SetBool(ANIM_ATTACK, true);
        }
        else {
            // En cualquier otro caso (cooldown cargando o fuera de rango), apagamos la animación.
            animator.SetBool(ANIM_ATTACK, false);
        }

        // --- Lógica de Flip (Girar) ---
        // Seguimos girando para mirar al jugador si está en rango
        if (isPlayerInAttackRange) {
            if (transform.position.x < playerTransform.position.x && facingLeft) {
                transform.eulerAngles = new Vector3(0f, -180f, 0f);
                facingLeft = false;
            }
            else if (transform.position.x > playerTransform.position.x && !facingLeft){
                transform.eulerAngles = Vector3.zero;
                facingLeft = true;
            }
        }
    }

    // Esta función se llama desde un EVENTO de animación en el frame exacto del disparo.
    public void FireArrow() {
        // Doble verificación por seguridad (por si el evento de animación se dispara antes)
        if (Time.time >= nextAttackTime) {
            // Creamos la flecha
            GameObject tempArrowPrefab = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
            // Le damos velocidad
            tempArrowPrefab.GetComponent<Rigidbody2D>().linearVelocity = -firePoint.right * arrowVelocity;
            // La destruimos tras 5 segundos si no choca
            Destroy(tempArrowPrefab, 5f);
            
            // Registramos el tiempo en que podremos volver a disparar
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (maxHealth <= 0) return;

        maxHealth -= damageAmount;
        animator.SetTrigger(ANIM_HURT);
        CameraShake.instance.Shake(2.5f, .15f);

        // feedback visual
        if (floatingTextPrefab != null && textSpawnPoint != null)
        {
            Instantiate(floatingTextPrefab, textSpawnPoint.position, Quaternion.identity);
        }

        // --- NUEVO: RECOMPENSA DE TIEMPO AL MORIR ---
        if (maxHealth <= 0)
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.SumarTiempo(5f);
            }
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + offset, attackRangeRadius);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Arrow")) {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    public void ShakeCamera() {
        CameraShake.instance.Shake(4f, .18f);
    }

    void Die() {
        rb.gravityScale = 0f;
        boxCollider2D.enabled = false;
        Destroy(this.gameObject, 5f);
    }
}