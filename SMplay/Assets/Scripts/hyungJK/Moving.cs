using Unity.VisualScripting;
using UnityEngine;

public class Moving : MonoBehaviour
{

    [Header("이동 힘")]
    float Iv; // 좌우 키 입력
    public float maxV ; // 최대속도
    public float StartJumppower; //점프 시작할때 힘
    public float Jumppower; // 점프 힘 초기화용
    float Jpower; //점프하는중에 작아지는 점프힘

    [Header("판단bool")]
    public bool isGround; //땅에있음?
    public bool isJumping; //점프함?
    Rigidbody2D rigid;
    SpriteRenderer sr;
    bool Ij; // 점프 인풋
    int face = 1; // 바라보는 방향 좌 : -1, 우 : 1
    public bool havingStick = true;
    public bool throwableStick;

    [Header("버퍼시간, 코요태시간")]
    public float bufferTime; // 바닥판정 전 점프 눌러도 점프가 인정되는 시간
    float bufferTimer; // 위에거 타이머
    bool Jumpkey = false; // 점프 실행시키는 트리거
    public float coyoteTime; // 코요테 시간(모르면 검색ㄱㄱ 간단한거임)
    float coyoteTimer; // 위에거 타이머

    [Header("바닥판정용")]
    public Collider2D mainCollider; // 자기 콜라이더
    public LayerMask groundMask; // 땅 레이어만 감지하도록 함
    public float groundDistance; // 땅 감지 거리
    public float groundSkin; // 콜라이더 맨 밑바닥보단 조금 위에서 시작함
    public float minGroundNormalY; // 인식된 땅의 기울기 벡터 정도로 생각하셈

    [Header("자스틱던지기")]

    public GameObject jaStick;
    public Stick stickscript;
    public Transform playerbody;
    public float stickspawnX;
    public float stickspawnY;


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        Jpower = Jumppower;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // ===============좌우이동=================    
        if ((Iv != 0) || (rigid.linearVelocityX != 0))
            rigid.linearVelocityX = Iv * maxV;

        // =================점프===================    
        if (Jumpkey) // 점프 받음
        {
            Jumpkey = false;
            isJumping = true; // 공중에서 점프키 한 번 누르면 땅에 닫자마자 점프하는거 방지
            rigid.linearVelocityY = 0;
            rigid.AddForce(Vector2.up * StartJumppower, ForceMode2D.Impulse); //처음 점프
            coyoteTimer = 0;
        }
        else if (Ij && isJumping) // 키 누르는데 점프중
        {
            Jpower = Mathf.Lerp(Jpower, 0, 0.1f); // 점프파워 10% 계속
            rigid.AddForce(Vector2.up * Jpower, ForceMode2D.Impulse);
            //Debug.Log(Jpower);
        }
        else if (!Ij && isJumping) //점프 키 안누르는데 점프중임
        {
            isJumping = false; 
            //Debug.Log("simai");
        }
    }
    void Update()
    {
        Iv = Input.GetAxis("Horizontal");

        Ij = Input.GetButton("Jump");
        //Debug.Log(Input.GetButton("Jump"));

        if (Input.GetButtonDown("Jump")) // 키 누르는데 점프중 아님
        {
            if (coyoteTimer > 0)
            {
                Jumpkey = true;
            }
            else
            {
                bufferTimer = bufferTime;
            }
        }
        if (bufferTimer > 0 && Input.GetButton("Jump")) // 점프키 누름 
        {
            bufferTimer -= Time.deltaTime; 
            if (coyoteTimer > 0)
            {
                bufferTimer = 0;
                Jumpkey = true;
            }
        }
        if (!isGround && coyoteTimer > 0)
        {
            coyoteTimer -= Time.deltaTime;
        }


        isGround = CheckGrounded_BoxCast();

        if (!havingStick)
        {
            throwableStick = false;
        }

        if (isGround)
        {
            if (coyoteTimer != coyoteTime)
            {
                coyoteTimer = coyoteTime;
            }

            if ((Jpower != Jumppower) && (!isJumping))
            {
                Jpower = Jumppower;
            }

            if (havingStick)
            {
                throwableStick = true;
            }
        }


        /* if (Input.GetAxisRaw("Horizontal") != 0)     // <- 입력으로 방향 바꾸기
            face = (Input.GetAxisRaw("Horizontal") > 0) ? 1 : -1; */ 
        if (rigid.linearVelocityX !=0)                  // <- 속도로 방향 바꾸기
            face = (rigid.linearVelocityX > 0) ? 1 : -1;

        sr.flipX = (face<0);

        
        if(Input.GetButtonDown("Throw"))
        {
            if (throwableStick)
            {
                jaStick.transform.position = playerbody.position + new Vector3(stickspawnX * face, stickspawnY, 0);
                jaStick.SetActive(true);
                stickscript.Throwing(face);
                havingStick = false;
            }
            else if (!havingStick)
            {
                stickscript.Returning();
            }
        }
    }

    private bool CheckGrounded_BoxCast()
    {
        // 1) 내 몸 콜라이더의 월드 크기/위치 정보
        Bounds b = mainCollider.bounds;

        // 2) 박스 시작 중심점(origin)
        //    - x는 몸 중앙
        //    - y는 발바닥(b.min.y)보다 살짝 위(groundSkin)
        Vector2 origin = new Vector2(b.center.x, b.min.y + groundSkin);

        // 3) 박스 크기(size)
        //    - 가로: 몸 폭보다 살짝 좁게 (벽 스침 오판정 줄이기)
        //    - 세로: 얇게
        Vector2 size = new Vector2(b.size.x * 1.0f, 0.1f);

        // 4) 아래로 짧게 쓸기(BoxCast)
        RaycastHit2D hit = Physics2D.BoxCast(
            origin,            // 시작 위치(박스 중심)
            size,              // 박스 크기
            0f,                // 회전각(0 = 회전 없음)
            Vector2.down,      // 방향
            groundDistance,    // 거리
            groundMask         // 레이어 필터(선택)
        );

        // 5) 아무것도 안 맞으면 공중
        if (hit.collider == null) return false;

        // 6) 트리거는 무시하고 싶으면(선택)
        // if (hit.collider.isTrigger) return false;

        // 7) '위쪽 면'만 바닥으로 인정 (벽은 normal.y가 0에 가까움)
        return hit.normal.y >= minGroundNormalY;
    }
}
