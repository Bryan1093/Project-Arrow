using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 3;

    public float walkSpeed = 1.5f;
    public Transform groundCheckPoint;
    public float distance = .3f;
    public LayerMask whatIsGround;
    private bool facingLeft;

    public float attackRangeRadius = 6f;
    public LayerMask whatIsPlayer;
    public float chaseSpeed = 2f;
    public float retrieveDistance = 3f;
    private Animator animator;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    public GameObject floatingTextPrefab;
    public Transform textSpawnPoint;

    public Transform attackPoint;
    public float attackRadius = 1f;
    public Vector3 offset;

    private const string ANIM_ATTACK = "Attack";
    private const string ANIM_HURT = "Hurt";
    private const string ANIM_DEATH = "Death";

    void Start()
    {
        facingLeft = true;
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        animator = this.gameObject.GetComponent<Animator>();
        boxCollider2D = this.gameObject.GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // Optimización masiva: Accedemos directamente a la instancia Singleton del Player
        if (Player.instance == null) {
            animator.SetBool(ANIM_ATTACK, false);
            return;
        }

        Transform playerTransform = Player.instance.transform;

        if (maxHealth <= 0) {
            animator.SetBool(ANIM_ATTACK, false);
            Die();
            return;
        }

        Collider2D collInfo = Physics2D.OverlapCircle(transform.position + offset, attackRangeRadius, whatIsPlayer);

        if (collInfo) {
            if (playerTransform.position.x > transform.position.x && facingLeft) {
                transform.eulerAngles = new Vector3(0f, -180f, 0f);
                facingLeft = false;
            }
            else if (playerTransform.position.x < transform.position.x && !facingLeft){
                transform.eulerAngles = Vector3.zero;
                facingLeft = true;
            }

            Vector2 targetPos = new Vector2(playerTransform.position.x, transform.position.y);

            if (Vector2.Distance(transform.position, targetPos) > retrieveDistance) {
                animator.SetBool(ANIM_ATTACK, false);
                transform.position = Vector2.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
            }
            else {
                animator.SetBool(ANIM_ATTACK, true);
            }  
        }
        else {
            transform.Translate(Vector2.left * Time.deltaTime * walkSpeed);

            RaycastHit2D hitInfo = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, distance, whatIsGround);
            if (!hitInfo) {
                if (facingLeft) {
                    transform.eulerAngles = new Vector3(0f, -180f, 0f);
                    facingLeft = false;
                }
                else {
                    transform.eulerAngles = Vector3.zero;
                    facingLeft = true;
                }
            }
        }
    }

    public void Attack() {
        Collider2D collInfo = Physics2D.OverlapCircle(attackPoint.position, attackRadius, whatIsPlayer);
        if (collInfo && collInfo.gameObject.GetComponent<Player>() != null) {
            Player.instance.TakeDamage(1);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (maxHealth <= 0) return;

        maxHealth -= damageAmount;
        animator.SetTrigger(ANIM_HURT); // (O "Hurt" si no usaste la versión optimizada)
        CameraShake.instance.Shake(2.5f, .15f);

        // Guardamos el texto generado y le enviamos el daño exacto
        GameObject floatingText = Instantiate(floatingTextPrefab, textSpawnPoint.position, Quaternion.identity);
        floatingText.GetComponent<FloatingText>().SetDamageText(damageAmount);

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

        if (groundCheckPoint != null) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(groundCheckPoint.position, Vector2.down * distance);
        }

        if (attackPoint != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
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
        animator.SetBool(ANIM_DEATH, true);
        rb.gravityScale = 0f;
        boxCollider2D.enabled = false;
        Destroy(this.gameObject, 5f);
    }
}