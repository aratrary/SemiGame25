using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
public class enemy : MonoBehaviour
{
    public float speed; //이동속도, Inspector 창에서 설정
    bool isLive = true;
    bool on_action;
    public int jumpForce = 7; // (현재 사용 안 함) 점프 제거했지만, 변수/코루틴은 원본 유지
    Rigidbody2D rb;
    public Rigidbody2D target;
    private Collider2D col;
    public float attack_dmg; // 공격력, Inspector 창에서 설정
    public Moving playerHealth; // PlayerHealthController에서 플레이어 피 가져오는 용도

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f; // 땅바닥에 닿은걸로 치는 거리
    public bool isGrounded;
    private SpriteRenderer spriteRenderer;

    public bool spritedir = false; // 스프라이트가 기본적으로 바라보는 방향, 좌측을 바라보고있으면 false, 우측을 바라보고있으면 true로 둘것
    public float stop_dist = 1.5f; // 적이 플레이어를 쫓아가는 거리, 근접몹이면 이 값을 작게, 원거리 공격몹이면 이 값을 크게
    private Animator animator;
    public RaycastHit2D hit;
    public LayerMask layerMask;
    public float attack_dist;
    public float flip = 1f;
    private Collider2D targetCollider;
    Vector2 hor;

    public float attackdelay;
    public float attackdelaydummy;
    public bool attacking;
    public float agrowexitdis;
    public float agrowenterdis;
    public float shotdelay;
    public int Health;

    // ===================== [추가] 플랫폼 끝/앞막힘 멈춤용 =====================
    [Header("Edge Stop Sensors")]
    public Transform frontCheck;        // 앞(옆) 막힘 체크 기준점(보통 몸통 높이)
    public Transform ledgeCheck;        // 앞-아래 바닥 체크 기준점(보통 발 근처)

    [Header("Edge Stop Masks")]
    public LayerMask groundMask;        // 바닥으로 인정할 레이어
    public LayerMask wallMask;          // 옆 막힘(벽/지형)으로 인정할 레이어

    [Header("Edge Stop Tuning")]
    public Vector2 frontCheckBoxSize = new Vector2(0.15f, 0.6f); // 앞 막힘 판정 박스 크기
    public float frontCheckForwardOffset = 0.05f;               // 콜라이더 앞쪽으로 얼마나 더 나가서 체크할지

    public float ledgeRayDownDistance = 0.35f;                  // 아래로 레이 길이
    public float ledgeForwardOffset = 0.05f;                    // 콜라이더 앞쪽으로 얼마나 더 나가서 발앞을 볼지

    public bool drawSensors = true;                             // 씬에서 디버그 보기

    bool blockedByEdge;                                         // 현재 프레임에 멈춰야 하는지(애니메이션 제어용)
    // ================================================================

    void Start()
    {
        isLive = true;
        GameObject player = GameObject.FindGameObjectWithTag("JK");
        playerHealth = player.GetComponent<Moving>();
        target = player.GetComponent<Rigidbody2D>();
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

        // ===================== [추가] 센서 자동 생성(없으면) =====================
        if (frontCheck == null)
        {
            GameObject fc = new GameObject("FrontCheck");
            fc.transform.parent = transform;
            fc.transform.localPosition = new Vector3(0f, 0f, 0f); // 기준은 중앙(방향 오프셋은 코드로 처리)
            frontCheck = fc.transform;
        }
        if (ledgeCheck == null)
        {
            GameObject lc = new GameObject("LedgeCheck");
            lc.transform.parent = transform;
            // 발 근처(바닥보다 살짝 위)
            lc.transform.localPosition = new Vector3(0f, -col.bounds.extents.y + 0.05f, 0f);
            ledgeCheck = lc.transform;
        }
        // =====================================================================

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isLive == false)
            return;

        Vector2 dist = (Vector2)target.position - (Vector2)transform.position;

        if (!on_action)
        {
            Vector2 horizontal = new Vector2(dist.x, 0f);
            hor = horizontal;

            Follow(horizontal, new Vector2(0f, dist.y)); // Follow => 플레이어 쫓아가기

            // ===================== [수정] 멈춤 상태면 Idle, 아니면 Run =====================
            if (horizontal.magnitude > agrowenterdis)
            {
                animator.SetInteger("State", 0);
            }
            else if (blockedByEdge)
            {
                animator.SetInteger("State", 0);
            }
            else
            {
                if (animator.GetInteger("State") != 1 && !attacking)
                    animator.SetInteger("State", 1);
            }
            // =====================================================================
        }

        // 바라보는 방향(원본 유지)
        if (spritedir)
        {
            spriteRenderer.flipX = hor.x < 0;
            flip = -1;
        }
        else
        {
            spriteRenderer.flipX = hor.x > 0;
            flip = 1;
        }
    }

    void Follow(Vector2 horizontal, Vector2 vertical) //플레이어 방향으로 이동
    {
        blockedByEdge = false; // 기본은 이동 가능

        if (horizontal.magnitude <= agrowenterdis && !attacking)
        {
            // ===================== [추가] 플랫폼 끝/앞막힘이면 이동 금지 =====================
            int dir = (horizontal.x >= 0f) ? 1 : -1;
            if (ShouldStopAtEdge(dir))
            {
                blockedByEdge = true;
                return; // 이동/점프 없이 멈춤
            }
            // =====================================================================

            transform.position += (Vector3)(horizontal.normalized * speed * Time.deltaTime);
        }
        if (horizontal.magnitude <= stop_dist && vertical.y < 1 && playerHealth.isGround == true) //플레이어와 일정 거리만큼 가까워지면 공격하기
        {
            attacking = true;
        }
        else if (attacking &&(!(hor.magnitude <= agrowexitdis) || vertical.y > 2.21 || (vertical.y > 1 && playerHealth.isGround == true)))
        {
            attacking = false;
            attackdelay = 0;
        }
        

        // ===================== [수정] 점프 로직 호출 제거 =====================
        // if (vertical.y>=1 && horizontal.magnitude<=3f) ... Jump();  <= 삭제(행동만 제거)
        // =====================================================================
    }

    void FixedUpdate()
    {
        if (isLive == false)
            return;
        attackdelaydummy = (attackdelay <= 0f) ? 0f : (attackdelay >= shotdelay) ? shotdelay : attackdelay;
        spriteRenderer.color = new Color(1f, (shotdelay-attackdelaydummy)/shotdelay, (shotdelay-attackdelaydummy)/shotdelay, 1f);
        if (attacking)
        {
            animator.SetInteger("State", 2); // 공격 애니메이션
            attackdelay += Time.fixedDeltaTime;
            if (attackdelay >= shotdelay)
            {
                attackdelay -= shotdelay;
                playerHealth.TakeDamage();
                if (!(hor.magnitude <= stop_dist))
                {
                    attacking = false;
                    attackdelay = 0;
                }
            }
        }
        
    }

    // ===================== [추가] edge-stop 핵심 로직 =====================
    bool ShouldStopAtEdge(int dir)
    {
        // 1) 옆(앞)이 막혔는지
        bool frontBlocked = IsFrontBlocked(dir);

        // 2) 앞-아래에 바닥이 없는지(낭떠러지)
        bool noGroundAhead = IsNoGroundAhead(dir);

        // 둘 중 하나라도 해당하면 멈춤
        return frontBlocked || noGroundAhead;
    }

    bool IsFrontBlocked(int dir)
    {
        // frontCheck는 중앙 기준점. 콜라이더 extents + 오프셋만큼 앞쪽으로 이동한 지점에 박스 오버랩
        float forward = col.bounds.extents.x + frontCheckForwardOffset;
        Vector2 origin = (Vector2)frontCheck.position + Vector2.right * dir * forward;

        Collider2D hitCol = Physics2D.OverlapBox(origin, frontCheckBoxSize, 0f, wallMask);
        return hitCol != null && hitCol.gameObject != gameObject;
    }

    bool IsNoGroundAhead(int dir)
    {
        // ledgeCheck는 발 근처 기준점. 콜라이더 extents + 오프셋만큼 앞쪽 지점에서 아래로 레이
        float forward = col.bounds.extents.x + ledgeForwardOffset;
        Vector2 origin = (Vector2)ledgeCheck.position + Vector2.right * dir * forward;

        RaycastHit2D groundHit = Physics2D.Raycast(origin, Vector2.down, ledgeRayDownDistance, groundMask);
        return groundHit.collider == null;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawSensors) return;
        if (frontCheck == null || ledgeCheck == null) return;

        // dir은 현재 스프라이트 flip 기준으로 대충 잡아줌(디버그용)
        int dir = 1;
        if (spriteRenderer != null)
            dir = spriteRenderer.flipX ? 1 : -1;

        // front overlap
        if (col != null)
        {
            float fwdFront = col.bounds.extents.x + frontCheckForwardOffset;
            Vector2 frontOrigin = (Vector2)frontCheck.position + Vector2.right * dir * fwdFront;
            Gizmos.DrawWireCube(frontOrigin, frontCheckBoxSize);

            float fwdLedge = col.bounds.extents.x + ledgeForwardOffset;
            Vector2 ledgeOrigin = (Vector2)ledgeCheck.position + Vector2.right * dir * fwdLedge;
            Gizmos.DrawLine(ledgeOrigin, ledgeOrigin + Vector2.down * ledgeRayDownDistance);
        }
    }
    // =====================================================================

    public void Death()
    {
        Health -= 1;
        if (Health == 0)
        {
            rb.bodyType = RigidbodyType2D.Static;
            col.enabled = false;
            animator.SetInteger("State", 3);
            isLive = false;
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            attackdelay = 0;
            attacking = false;
        }
    }
    void Attack()
    {

    }

 /*    IEnumerator AttackCoroutine()
    {
        on_action = true; // 행동 중 다른 모션 방지
        yield return new WaitForSeconds(1f); // 공격 딜레이

        animator.SetInteger("State", 2); // 공격 애니메이션

        Vector2 dist = (Vector2)target.position - (Vector2)transform.position; // 플레이어 - 적 간 거리 (벡터)
        if ((dist.x > 0 && spriteRenderer.flipX) || (dist.x < 0 && !spriteRenderer.flipX)) //벡터의 방향과 적 개체가 바라보는 방향이 같은지 확인)
        {
            if (dist.x < attack_dist && dist.y < 0.5) //공격 범위 이내에 플레이어가 있는지 확인
                playerHealth.TakeDamage(); //플레이어에 연결된 PlayerHealthController에서 TakeDamage를 불러와서 실행
            Debug.Log(playerHealth.currentHealth); //현재 플레이어 오브젝트 HP 확인 (디버깅용)
        }
        on_action = false;
    } */

    // ===================== 원본 함수 유지(현재 Follow에서 호출 안 함) =====================
    void CheckGround()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            groundCheck.position,
            groundCheckRadius,
            layerMask
        );

        isGrounded = false;

        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject) // 혹시 본인 잡힐 경우 대비
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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        rb.AddForce(Vector2.right * flip * jumpForce * -0.3f, ForceMode2D.Impulse);
        yield return new WaitForSeconds(1f);
        on_action = false;
    }
    // =====================================================================
}
