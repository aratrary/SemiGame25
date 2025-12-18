using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
public class enemy : MonoBehaviour
{
    public float speed; //이동속도, Inspector 창에서 설정
    bool isLive;
    bool on_action;
    int jumpForce = 7; // 점프 강도
    Rigidbody2D rb;
    public Rigidbody2D target;
    private Collider2D col;
    public float attack_dmg; // 공격력, Inspector 창에서 설정
    public PlayerHealthController playerHealth; // PlayerHealthController에서 플레이어 피 가져오는 용도

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f; // 땅바닥에 닿은걸로 치는 거리
    private bool isGrounded;
    private SpriteRenderer spriteRenderer;
    
    public bool spritedir = false; // 스프라이트가 기본적으로 바라보는 방향, 좌측을 바라보고있으면 false, 우측을 바라보고있으면 true로 둘것
    public float stop_dist = 1.5f; // 적이 플레이어를 쫓아가는 거리, 근접몹이면 이 값을 작게, 원거리 공격몹이면 이 값을 크게
    private Animator animator;
    public RaycastHit2D hit;
    public LayerMask layerMask; 
    public float attack_dist;
    public float flip = 1f;
    private Collider2D targetCollider;
    void Start()
    {
        on_action = false;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, -col.bounds.extents.y, 0);
            groundCheck = checkObj.transform;
        }
        animator = GetComponent<Animator>();
        float EnemyMaxHealth = 100f;
        float EnemyCurrentHealth = EnemyMaxHealth;
    }
    void Update()
    {
        Vector2 dist = (Vector2)target.position - (Vector2)transform.position;
        if (!on_action)
        {
            Vector2 horizontal = new Vector2(dist.x, 0f);
            Follow(horizontal, new Vector2(0f,dist.y)); // Follow => 플레이어 쫓아가기
            if (animator.GetInteger("State") != 1) // 달리기 애니메이션
            {
                animator.SetInteger("State",1);
            }
            if (horizontal.magnitude >20.0f) // 너무 멀리 떨어져있으면 가만히 있는 애니메이션으로 전환
            {
                animator.SetInteger("State",0);
            }
        }
    }

    void Follow(Vector2 horizontal, Vector2 vertical) //플레이어 방향으로 이동
    {
        if (horizontal.magnitude <= 20.0f && horizontal.magnitude > stop_dist)
        {
            transform.position += (Vector3)(horizontal.normalized * speed * Time.deltaTime); //방향 따라 스프라이트 바라보는 방향 조정
            if (spritedir)
            {
                spriteRenderer.flipX = horizontal.x > 0;
                flip = 1;
            }
            else
            {
                spriteRenderer.flipX = horizontal.x < 0;
                flip = -1;
            }

        }
        
        else if (horizontal.magnitude <= stop_dist && vertical.y < 1) //플레이어와 일정 거리만큼 가까워지면 공격하기
        {
            Attack();
        }
        if (vertical.y>=1 && horizontal.magnitude<=3f) // 너무 멀지 않고 공격 거리 내로 안들어오면 이동하도록 설정
        {
            CheckGround();
            if (isGrounded)
            {
                Jump();
            }
        }
    }
    void Attack()
    {
        StartCoroutine(AttackCoroutine());
    }
    
    IEnumerator AttackCoroutine()
    {
        on_action = true; // 행동 중 다른 모션 방지
        yield return new WaitForSeconds(1f); // 공격 딜레이
        
        animator.SetTrigger("Attack"); // 공격 애니메이션
        
        Vector2 dist = (Vector2)target.position - (Vector2)transform.position; // 플레이어 - 적 간 거리 (벡터)
        if ((dist.x > 0 && spriteRenderer.flipX)|| (dist.x<0 && !spriteRenderer.flipX)) //벡터의 방향과 적 개체가 바라보는 방향이 같은지 확인)
        {
            if (dist.x < attack_dist && dist.y < 0.5) //공격 범위 이내에 플레이어가 있는지 확인
                playerHealth.TakeDamage(attack_dmg); //플레이어에 연결된 PlayerHealthController에서 TakeDamage를 불러와서 실행
                Debug.Log(playerHealth.currentHealth); //현재 플레이어 오브젝트 HP 확인 (디버깅용)
        }
        on_action = false;
    }
    void CheckGround() // 점프 부가기능 (지면에 닿아있을 때만 점프하도록 체크용도)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        isGrounded = false;
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("Ground")) // 땅에 닿아있는지 체크
            {
                isGrounded = true;
                break;
            }
        }
    }
    void Jump()
    {
        StartCoroutine(JumpCoroutine()); // 점프 구현
    }
    IEnumerator JumpCoroutine()
    {
        on_action = true;
        animator.SetTrigger("Jump"); // 점프 애니메이션
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        rb.AddForce(Vector2.right * flip * jumpForce*-0.3f, ForceMode2D.Impulse);
        yield return new WaitForSeconds(1f);
        on_action = false;
    }
}