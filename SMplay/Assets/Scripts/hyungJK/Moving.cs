using UnityEngine;

public class Moving : MonoBehaviour
{
    float Iv; // 좌우 키 입력
    public float maxV ; // 최대속도
    public float StartJumppower; //점프 시작할때 힘
    public float Jumppower; // 점프 힘 초기화용
    float Jpower; //점프하는중에 작아지는 점프힘
    public bool isGround; //땅에있음?
    public bool isJumping; //점프함?
    Rigidbody2D rigid;
    bool Ij; // 점프 인풋
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        Jpower = Jumppower;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if ((Iv != 0) || (rigid.linearVelocityX != 0))
            rigid.linearVelocityX = Iv * maxV;
            
        if (Ij && !isJumping) // 키 누르는데 점프중 아님
        {
            isJumping = true; // 공중에서 점프키 한 번 누르면 땅에 닫자마자 점프하는거 방지
            if (isGround) 
            {
                rigid.AddForce(Vector2.up * StartJumppower, ForceMode2D.Impulse); //처음 점프
            }
        }
        else if (Ij && isJumping) // 키 누르는데 점프중
        {
            Jpower = Mathf.Lerp(Jpower, 0, 0.1f); // 점프파워 0.1% 계속
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
    }

    void OnCollisionStay2D(Collision2D collision)
    {

        if ((Jpower != Jumppower) && (!isJumping))
        {
            Jpower = Jumppower;
        }
        isGround = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGround = false;
    }
}
