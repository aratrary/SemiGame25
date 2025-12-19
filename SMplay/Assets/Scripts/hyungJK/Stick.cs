using UnityEditor.Callbacks;
using UnityEngine;

public class Stick : MonoBehaviour
{
    SpriteRenderer sr;
    Rigidbody2D rigid;
    int face;
    public float V; // 날라가는 속도
    public bool isFlying = false; // 날라가냐?
    public Collider2D maincollider;
    public Collider2D childcollider;
    public LayerMask enemyMask; // 적
    public LayerMask groundMask; // 땅
    public LayerMask JKMask; // 형JK
    public bool hitEnemy; // 맞춘거 적임?
    public bool hitGround; // 맞춘거 땅임?
    public GameObject hittedEnemy; // 맞춘 적
    public int layer=0; // 맞춘 레이어
    public bool isReturning; // 돌아가는중임?
    public Moving JKscript; // 형JK의 스크립트
    public GameObject JK; // JK
    public Transform JKtrans; // 사실 JK 트랜스임ㄷㄷ
    public float returningspeed; // 돌아가는속도
    public enemy Enemyscript;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isFlying) // 날려지고 있음?
        {
            rigid.linearVelocityX = face * V;
        }

        else if (hitGround) // 맞춘게 땅임?
        {
            
        }

        else if (hitEnemy) // 맞춘게 적임?
        {
            
        }

        if (isReturning) // 돌아가는중임?
        {
            Vector2 toJK = (Vector2)JKtrans.position - rigid.position; // JK 위치에서 내 위치 빼서 벡터를 구해
            Vector2 step = (Vector2)(toJK / toJK.magnitude) * returningspeed * Time.fixedDeltaTime; // 그 벡터 방향으로 returningspeed로 날아가는 조금의 위치를 잡아
            rigid.MovePosition(rigid.position + step); // 그 위치로 이동하자
        }
    }
    public void Throwing(int facing) // 형JK쪽에서 트리거가 있음
    {
        rigid.constraints |= RigidbodyConstraints2D.FreezePositionY; // y 위치 고정
        sr.flipX = !(facing<0); // 구부러지는 방향 미리 조정
        face = facing; // 보는 방향 변수에 저장
        hitEnemy = false; // 적 맞췄는지 초기화
        hitGround = false; // 땅 맞췄는지 초기화
        hittedEnemy = null; // 맞춘 적 ㅊㄱㅎ
        Enemyscript = null;
        isFlying = true; // 날고있어
        childcollider.isTrigger = false; // 플랫폼처럼 밟히는거 활성화
    }
    public void Returning() // 형JK쪽에 ㅌㄹㄱㄱ ㅇㅇ
    {
        if (isReturning) // 이미 돌아가는중이면 말고
            return;
        rigid.gravityScale = 0;
        isReturning = true; // 돌아가는중이야
        childcollider.isTrigger = true; // 플랫폼처럼 밟히는건 끄자
        maincollider.isTrigger = true; // 맞는 판정 없애기
        maincollider.usedByEffector = false; // 형JK만 안맞는거 끄기
        rigid.constraints &= ~RigidbodyConstraints2D.FreezePositionY; // Y고정 끄고
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        layer = 1 << collision.gameObject.layer; // 맞은애 value (비트)
        // Use mask-friendly checks: support masks that contain multiple layers
        bool hitEnemyLayer = (layer & enemyMask.value) != 0;
        bool hitGroundLayer = (layer & groundMask.value) != 0;
        if (isFlying && (hitEnemyLayer || hitGroundLayer)) //맞은 애가 적이거나 땅이야?
        {
            isFlying = false; //일단 나는건 멈춰
            maincollider.isTrigger = true; // 맞는 판정은 이제 필요없어
            rigid.linearVelocityX = 0;
            Debug.Log($"[Stick] OnCollisionStay2D hit {collision.gameObject.name} (enemy:{hitEnemyLayer} ground:{hitGroundLayer})");
            if (hitEnemyLayer) // 적이 맞은거야?
            {
                hitEnemy = true; // 적이 맞았다고 상태를 정하자
                hittedEnemy = collision.gameObject;
                Enemyscript = hittedEnemy.GetComponent<enemy>();
                if (Enemyscript != null)
                {
                    Debug.Log($"[Stick] -> calling enemy.Death() on {hittedEnemy.name}");
                    Enemyscript.Death();
                }
                else
                {
                    // enemy 타입이 아닐 경우(예: 보스 스크립트) 다른 스크립트를 참조하여 처리
                    var boss = hittedEnemy.GetComponent<FloatingLibrary>();
                    if (boss != null)
                    {
                        Debug.Log($"[Stick] -> calling FloatingLibrary.Death() on {hittedEnemy.name}");
                        boss.Death();
                    }
                    else
                    {
                        Debug.LogWarning($"[Stick] hit object {hittedEnemy.name} is in enemyMask but has no enemy or FloatingLibrary script.");
                    }
                }
                rigid.gravityScale = 1;
            }
                else if (hitGroundLayer) // 땅이 맞은거야?
            {
                hitGround = true; // 땅이 맞았다고 상태를 정하자
            }
        }
        
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        layer = 1 << collision.gameObject.layer; // trigger로 맞은 애 value
        if (isReturning && layer == JKMask.value) // 내가 돌아가는 중인데 맞은 애가 형JK야?
        {
            isReturning = false; // 이제 돌아가는중 아님
            gameObject.SetActive(false); // 내가 없어..없어져볼게 하나 둘 셋 얏
            JKscript.catched = true;
            JKscript.havingStick = true; // JK의 우람한 막대기 생김
            maincollider.isTrigger = false; // 나 이제 안 맞는 판정 없어
            maincollider.usedByEffector = true; // 형JK는 맞지 않도록 함
        }
    }
}
