using UnityEngine;
using System.Threading.Tasks;
using System.Collections;

public class enemyGPT : MonoBehaviour
{
    public float speed; //이동속도, Inspector 창에서 설정
    bool isLive = true;
    bool on_action;
    Rigidbody2D rb;
    public Rigidbody2D target;
    private Collider2D col;
    public Moving playerHealth; // PlayerHealthController에서 플레이어 피 가져오는 용도
    private SpriteRenderer spriteRenderer;

    public bool spritedir = false; // 스프라이트 기본 방향(좌:false / 우:true)
    public float stop_dist = 1.5f; // 공격/정지 거리
    private Animator animator;
    public float flip = 1f;
    Vector2 hor;

    public float attackdelay;
    public float attackdelaydummy;
    public bool attacking;
    public float agrowexitdis;
    public float agrowenterdis;
    public float shotdelay;
    public int Health;

    // ===================== [간단화] groundMask 하나로 벽+바닥 판정 =====================
    [Header("Ground Mask (Wall + Floor)")]
    public LayerMask groundMask; // 벽/바닥 레이어가 같다면 이거 하나만 체크

    [Header("Edge/Wall Stop Tuning")]
    public float wallRayDistance = 0.12f;       // 앞에 벽이 있는지 보는 거리(짧게)
    public float ledgeRayDownDistance = 0.6f;   // 발앞 아래로 바닥을 찾는 레이 길이
    public float ledgeForwardOffset = 0.05f;    // 발앞을 얼마나 앞으로 볼지
    public float skin = 0.02f;                  // 레이가 콜라이더 경계에 걸리는 걸 줄이는 여유

    public bool drawSensors = true;             // 씬에서 디버그 레이 보기
    bool blockedByEdge;                         // 현재 프레임에 멈춰야 하는지
    // ================================================================

    [Header("DEBUG")]
    public bool debugAI = true;
    public float debugInterval = 0.5f;
    float debugTimer;
    bool debugWarnedMasks;

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
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isLive) return;

        Vector2 dist = (Vector2)target.position - (Vector2)transform.position;

        // ===== Debug 출력 =====
        if (debugAI)
        {
            if (!debugWarnedMasks && groundMask.value == 0)
            {
                debugWarnedMasks = true;
                Debug.LogWarning($"[enemy:{name}] groundMask가 비어있음(0). 프리팹 Inspector에서 Ground 레이어 체크 필요!");
            }

            debugTimer += Time.deltaTime;
            if (debugTimer >= debugInterval)
            {
                debugTimer = 0f;

                int dir = (dist.x >= 0f) ? 1 : -1;
                bool front = IsFrontBlocked(dir);
                bool noGround = IsNoGroundAhead(dir);

                Debug.Log(
                    $"[enemy:{name}] timeScale={Time.timeScale} dt={Time.deltaTime:F4} " +
                    $"dist=({dist.x:F2},{dist.y:F2}) agrowEnter={agrowenterdis} stopDist={stop_dist} speed={speed} " +
                    $"attacking={attacking} blockedByEdge={blockedByEdge} frontBlocked={front} noGroundAhead={noGround} " +
                    $"playerIsGround={(playerHealth != null ? playerHealth.isGround : false)} " +
                    $"mask g={groundMask.value} shotDelay={shotdelay}"
                );
            }
        }
        // =====================

        if (!on_action)
        {
            Vector2 horizontal = new Vector2(dist.x, 0f);
            hor = horizontal;

            Follow(horizontal, new Vector2(0f, dist.y));

            // 애니메이션: 멀면 Idle, 막혔으면 Idle, 아니면 Run(단 공격 중 제외)
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

    void Follow(Vector2 horizontal, Vector2 vertical)
    {
        blockedByEdge = false;

        // (1) 추격 이동 (공격 중에는 이동 안 함)
        if (horizontal.magnitude <= agrowenterdis && !attacking)
        {
            int dir = (horizontal.x >= 0f) ? 1 : -1;

            // [핵심 수정] 막혀도 return 하지 않고 "이 프레임 이동만" 막는다.
            // (막히면 공격 판정까지 못 가는 문제 해결)
            if (ShouldStopAtEdgeOrWall(dir))
            {
                blockedByEdge = true;
            }
            else
            {
                transform.position += (Vector3)(horizontal.normalized * speed * Time.deltaTime);
            }
        }

        // (2) 공격 시작/해제 (원본 로직 유지)
        if (horizontal.magnitude <= stop_dist && vertical.y < 1 && playerHealth.isGround == true)
        {
            attacking = true;
        }
        else if (attacking && (!(hor.magnitude <= agrowexitdis) || vertical.y > 2.21 || (vertical.y > 1 && playerHealth.isGround == true)))
        {
            attacking = false;
            attackdelay = 0;
        }
    }

    void FixedUpdate()
    {
        if (!isLive) return;

        // shotdelay 0이면 색 계산이 망가지니까 안전장치(최소 수정)
        float sd = Mathf.Max(shotdelay, 0.0001f);

        attackdelaydummy = (attackdelay <= 0f) ? 0f : (attackdelay >= sd) ? sd : attackdelay;
        spriteRenderer.color = new Color(1f, (sd - attackdelaydummy) / sd, (sd - attackdelaydummy) / sd, 1f);

        if (attacking)
        {
            animator.SetInteger("State", 2);
            attackdelay += Time.fixedDeltaTime;

            if (attackdelay >= sd)
            {
                attackdelay -= sd;
                playerHealth.TakeDamage();

                if (!(hor.magnitude <= stop_dist))
                {
                    attacking = false;
                    attackdelay = 0;
                }
            }
        }
    }

    // ===================== [간단한 땅 판정] Raycast 2개 + groundMask 하나 =====================
    // - 앞 벽이 있으면 멈춤
    // - 발앞 아래에 바닥이 없으면(낭떠러지) 멈춤
    bool ShouldStopAtEdgeOrWall(int dir)
    {
        bool frontBlocked = IsFrontBlocked(dir);
        bool noGroundAhead = IsNoGroundAhead(dir);
        return frontBlocked || noGroundAhead;
    }

    bool IsFrontBlocked(int dir)
    {
        // 콜라이더 중앙 높이에서, 앞쪽으로 짧게 Raycast
        // groundMask에 닿으면 "앞이 막혔다"로 판단
        Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.center.y);
        origin += Vector2.right * dir * (col.bounds.extents.x + skin);

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * dir, wallRayDistance, groundMask);
        return hit.collider != null && hit.collider.gameObject != gameObject;
    }

    bool IsNoGroundAhead(int dir)
    {
        // 콜라이더 바닥 근처(minY) + 발앞(앞으로 조금) 지점에서 아래로 Raycast
        // groundMask를 못 맞추면 "앞에 바닥이 없다"(낭떠러지)
        float forward = col.bounds.extents.x + ledgeForwardOffset;
        Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y + skin);
        origin += Vector2.right * dir * forward;

        RaycastHit2D groundHit = Physics2D.Raycast(origin, Vector2.down, ledgeRayDownDistance, groundMask);
        return groundHit.collider == null;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawSensors) return;
        if (col == null) col = GetComponent<Collider2D>();
        if (col == null) return;

        int dir = 1;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            dir = sr.flipX ? 1 : -1;

        // 앞 벽 레이
        Vector2 o1 = new Vector2(col.bounds.center.x, col.bounds.center.y) + Vector2.right * dir * (col.bounds.extents.x + skin);
        Gizmos.DrawLine(o1, o1 + Vector2.right * dir * wallRayDistance);

        // 낭떠러지 레이
        Vector2 o2 = new Vector2(col.bounds.center.x, col.bounds.min.y + skin) + Vector2.right * dir * (col.bounds.extents.x + ledgeForwardOffset);
        Gizmos.DrawLine(o2, o2 + Vector2.down * ledgeRayDownDistance);
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
}
