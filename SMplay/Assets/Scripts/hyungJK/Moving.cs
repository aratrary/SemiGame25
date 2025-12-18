using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class Moving : MonoBehaviour
{
    public bool testing;
    [Header("이동 힘")]
    float Iv; // 좌우 키 입력
    public float maxV ; // 최대속도
    public float StartJumppower; //점프 시작할때 힘
    public float Jumppower; // 점프 힘 초기화용
    float Jpower; //점프하는중에 작아지는 점프힘
    public float Wingpower; // 날개치는 힘

    [Header("판단bool")]
    public bool isGround; // 땅에있음?
    public bool isJumping; // 점프함?
    public bool havingWing; // 날개 가지고 있음?
    public bool Wingable; // 날개 펼칠 수 있음?
    public bool Stickable; // 스틱 밟고 점프할 수 있음?
    Rigidbody2D rigid; 
    SpriteRenderer sr;
    Animator anim;
    public enum State { Throw, Catch, Sit_1, Sit_2, normal, run, Jump, Wing} // 애니메이션 목록 편하게 나타내기
    bool Ij; // 점프 인풋
    int face = 1; // 바라보는 방향 좌 : -1, 우 : 1
    public bool havingStick = true; // 스틱 가지고 있음?
    public bool throwableStick; // 스틱 던질 수 있음?
    bool CatchAnimationDummy = true; // 스틱 모션을 위한 더미 bool임미다

    [Header("버퍼시간, 코요태시간")]
    public float bufferTime; // 바닥판정 전 점프 눌러도 점프가 인정되는 시간
    float bufferTimer; // 위에거 타이머
    bool Jumpkey = false; // 점프 실행시키는 트리거
    bool Wingkey = false; // 날개 펼치는 트리거
    public float coyoteTime; // 코요테 시간(모르면 검색ㄱㄱ 간단한거임)
    float coyoteTimer; // 위에거 타이머

    [Header("바닥판정용")]
    public BoxCollider2D mainCollider; // 자기 콜라이더
    public LayerMask groundMask; // 땅 레이어만 감지하도록 함
    public float groundDistance; // 땅 감지 거리
    public float groundSkin; // 콜라이더 맨 밑바닥보단 조금 위에서 시작함
    public float minGroundNormalY; // 인식된 땅의 기울기 벡터 정도로 생각하셈

    [Header("자스틱던지기")]
    public GameObject jaStick; 
    public Stick stickscript;
    public Transform playerbody;
    public float stickspawnX; // 스틱 소환되는 X오프셋
    public float stickspawnY; // 스틱 소환되는 Y오프셋
    public Animator Stickanim; // 스틱 애니메이션
    bool StickKey; // 스틱 밟고 점프하는 트리거
    float StickingLevel; // 스틱 밟은 정도 (1초미만/1초이상)
    public float Stickingpower1; // 스틱 밟고 점프하는 1단계 힘
    public float Stickingpower2; // 스틱 밟고 점프하는 2단계 힘
    bool Ist; // 스틱 밟기 시작했는지 보는거
    public LayerMask StickMask; // 스틱 Layer 분류
    public int Stickface; // stick이 바라보는 방향
    public Transform Sticktrans; // 사실 스틱 트랜스임ㄷㄷ

    [Header("체력")]

    public int currentHealth = 5;

    public float ?testing1 = null;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        Jpower = Jumppower;
        DontDestroyOnLoad(gameObject);
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
        
        // =================날개===================    
        if (Wingkey)
        {
            Wingkey = false;
            rigid.gravityScale = 0.4f;
            rigid.linearVelocityY = 0;
            rigid.AddForce(Vector2.up * Wingpower, ForceMode2D.Impulse);
        }

        // ===============스틱점프=================    
        if (StickKey)
        {
            StickKey = false;
            if (StickingLevel <1)
            {
                rigid.AddForce(Vector2.up * Stickingpower1, ForceMode2D.Impulse); // 1단계 스틱점프
            } 
            else
            {
                rigid.AddForce(Vector2.up * Stickingpower2, ForceMode2D.Impulse); // 2단계 스틱점프
            }
            StickingLevel = 0; // 스틱 밟기정도 초기화
            Stickanim.SetInteger("State", 0); // 스틱 애니메이션 변경 
            mainCollider.offset = new Vector2(0, -0.016f); // 콜라이더 초기화
            mainCollider.size = new Vector2(0.19f, 0.501f);
        }
    }
    void Update()
    {
        /* if (transform.position.y > testing1 || testing1 == null)
        {
            testing1 = transform.position.y;
            Debug.Log(testing1);
        } */

        CheckGrounded_BoxCast(); //isGround 체킹 + 밑이 스틱인지 체킹

        if (Input.GetButton("Vertical") && Stickable) // 아래로 누르고 있고 스틱점프가 가능하다면
        {
            if (!Ist)
                Ist = true; // 일단 이거 실행중이라는 뜻
        
            if (StickingLevel < 1)
            {
                if (StickingLevel == 0)
                {
                    sr.flipX = Stickface >0; // 스틱페이스 맞춰서 보는 방향 바꾸기
                    Iv = 0; //속도 0으로 둬야디
                    Ij = false; // 점프키같은거 없어야디
                    ChangeAnim(State.Sit_1); // 애니메이션 앉는거로 변경
                    Stickanim.SetInteger("State", 1); // 스틱 애니메이션 변경
                    mainCollider.offset = new Vector2(0, -0.06296875f); // 이거랑 아래거는 콜라이더 크기 변경하는거임
                    mainCollider.size = new Vector2(0.19f, 0.4070625f);
                    //if (testing) // 테스트기능이라는 뜻임
                    //playerbody.position = new Vector3(Sticktrans.position.x, playerbody.position.y, 0); // 스틱 스프라이트와 형JK스프라이트 위치 맞추기
                }
                StickingLevel += Time.deltaTime; // 스틱정도 시간에 따라 조절
            }
            else
            {
                ChangeAnim(State.Sit_2); // 더 앉는거로 변경
                Stickanim.SetInteger("State", 2); // 스틱도 애니메이션 변경함
            }
            return;
        } 
        else if (Ist)
        {
            StickKey = true; // 스틱 발사 트리거
            Ist = false; // 어 활동 끝났어
        }

        Iv = Input.GetAxis("Horizontal");

        Ij = Input.GetButton("Jump");
        //Debug.Log(Input.GetButton("Jump"));


        if (Input.GetButtonDown("Jump")) // 키 누르는데 점프중 아님
        {
            if (coyoteTimer > 0 || isGround) // 땅에 있거나 코요태시간 >0
            {
                Jumpkey = true; // 점프 트리거 on
            }
            else if(Wingable && !isGround) // 윙점프 가능하고 땅이 아님
            {
                Wingkey = true; // 날개 트리거on
                Wingable = false; //어 너 이제 날개점프 불가능해
            }
            else
            {
                bufferTimer = bufferTime; // 버퍼타이머 시작
            }
        }

        AnimationUpdate();

        if (bufferTimer > 0 && Input.GetButton("Jump")) // 점프키 누르면서 버퍼타이머가 시작함
        {
            
            bufferTimer -= Time.deltaTime; // 버퍼타이머 시간간다
            if (coyoteTimer > 0) // 코요태시간 안이라면(원래 코요태시간이 0이었으니까 0이상이라는건 땅을 밟았다는 소리)
            {
                bufferTimer = 0; // 버퍼시간 0 만들어야지
                Jumpkey = true; // 점프 트리거 on
            }
        }


        if (!isGround && coyoteTimer > 0) //땅이 아닌데 아직 코요태 안끝남
        {
            coyoteTimer -= Time.deltaTime; // 코요태 타이머 가는중
        }

        if (!havingStick) // 스틱이 없어?
        {
            throwableStick = false; // 그럼 던질수도 없겠지
        }

        if (isGround) // 땅이네
        {
            if (coyoteTimer != coyoteTime) // 코요태 타이머 초기화 안됐음?
            {
                coyoteTimer = coyoteTime; // 초기화
            }

            if ((Jpower != Jumppower) && (!isJumping)) // 점프상태가 아닌데 점프 파워가 초기화가 안됐어?
            {
                Jpower = Jumppower; //초기화
            }

            if (havingStick && !throwableStick) // 스틱을 가지고 있고 땅인데 스틱을 던질 수 없어?
            {
                throwableStick = true; // 던질 수 있어야지
            }
            if (!Wingable && havingWing) // 날개를 가지고 있고 땅는데 날개점프를 할 수 없어?
            {
                Wingable = true; // 날개점프 가능해야지
            }
            if (rigid.gravityScale != 1) // 날개점프때 초기화한 중력이 안돌아왔어?
            {
                rigid.gravityScale = 1; // 초기화해야지
            }
        }


        /* if (Input.GetAxisRaw("Horizontal") != 0)     // <- 입력으로 방향 바꾸기
            face = (Input.GetAxisRaw("Horizontal") > 0) ? 1 : -1; */ 
        if (rigid.linearVelocityX !=0)                  // <- 속도로 방향 바꾸기
            face = (rigid.linearVelocityX > 0) ? 1 : -1;

        sr.flipX = (face<0); // 방향 바꿔야지

        
        if(Input.GetButtonDown("Throw")) // 던지는 키를 눌렀네
        {
            if (throwableStick) // 스틱 던질 수 있음?
            {
                jaStick.transform.position = playerbody.position + new Vector3(stickspawnX * face, stickspawnY, 0); // 일단 자스틱 위치 초기화하고
                jaStick.SetActive(true); // 자스틱 on
                stickscript.Throwing(face); // 자스틱 던지자(자스틱 안 스크립트 안 함수 발동)
                havingStick = false; // 우람한 막대기 없음
                ChangeAnim(State.Throw); // 던지는 모션 on
                CatchAnimationDummy = false; // 애니메이션용 더미 변수 수정
                Stickface = face; // 스틱 방향 기억하고
            }
            else if (!havingStick) // 스틱을 안 가지고 있음?
            {
                stickscript.Returning(); // 돌아오거라
            }
        }

    }

    public void TakeDamage()
    {
        currentHealth -= 1;
        Debug.Log(currentHealth);
    }

    public void resetHealth()
    {
        currentHealth = 5;
    }
    public void AnimationUpdate()
    {
        if (havingStick && !CatchAnimationDummy) // 스틱이 있는데 아직 더미가 안켜졌네
        {
            ChangeAnim(State.Catch); // 잡는 모션
            CatchAnimationDummy = true; // 다시 더미 on
        }
        else if (!isGround) // 땅에 없네
        {
            if (!Wingable && havingWing) // 날개는 가지고 있는데 날개점프를 썼구나
                ChangeAnim(State.Wing); // 날개모션 on
            else //아니네
                ChangeAnim(State.Jump); // 점프모션 on
        }
        else if (Input.GetAxis("Horizontal") !=0) // 땅인데 좌우로 움직이냐?
        {
            ChangeAnim(State.run); // 달려야디
        }
        else
        {
            ChangeAnim(State.normal); // 아무것도 안하면 그냥 가마이 있어
        }
    }

    public void ChangeAnim(State state)
    {
        anim.SetInteger("State", (int)state + ((havingStick && (int)state > 3) ? 4 : 0)); // 애니메이션 바꾸기
    }

    private void CheckGrounded_BoxCast()
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
        if (hit.collider == null)
        {
            isGround = false;
            Stickable = false;
        }
        else
        {
            // 6) 트리거는 무시하고 싶으면(선택)
            // if (hit.collider.isTrigger) return false;

            // 7) '위쪽 면'만 바닥으로 인정 (벽은 normal.y가 0에 가까움)
            isGround = hit.normal.y >= minGroundNormalY;

            
            Stickable = (StickMask.value & ( 1 << hit.collider.gameObject.layer)) != 0; // 밑이 스틱이라면 스틱 던질 수 있어야지
        }

        
    }
}
