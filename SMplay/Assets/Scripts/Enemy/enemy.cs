using UnityEngine;
using System.Threading.Tasks;

public class enemy : MonoBehaviour
{
    public float speed;
    public Rigidbody2D target;
    bool isLive;
    bool on_action;
    int jumpForce = 5;
    Rigidbody2D rb;
    private Collider2D col;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private bool isGrounded;
    private SpriteRenderer spriteRenderer;
    
    public bool spritedir = false; // 스프라이트가 기본적으로 바라보는 방향, 좌측을 바라보고있으면 false, 우측을 바라보고있으면 true로 둘것
    public float stop_dist = 1.5f; // 적이 플레이어를 쫓아가는 거리, 근접몹이면 이 값을 작게, 원거리 공격몹이면 이 값을 크게
    void Start()
    {
        on_action = false;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, -col.bounds.extents.y, 0);
            groundCheck = checkObj.transform;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
    void Update()
    {
        Vector2 dist = target.position - (Vector2)transform.position;
        if (!on_action)
        {
            Follow(new Vector2(dist.x, 0f), new Vector2(0f,dist.y));
        }
    }

    void Follow(Vector2 horizontal, Vector2 vertical) //플레이어 방향으로 이동
    {
        if (horizontal.magnitude <= 20.0f && horizontal.magnitude > stop_dist)
        {
            transform.position += (Vector3)(horizontal.normalized * speed * Time.deltaTime);
            if (spritedir)
                spriteRenderer.flipX = horizontal.x < 0;
            else
                spriteRenderer.flipX = horizontal.x > 0;
        }
        else if (horizontal.magnitude <= 3.0f && vertical.y >=1)
        {
            CheckGround();
            if (isGrounded)
            {
                Jump();
            }
        }
        else if (horizontal.magnitude <= 3.0f && vertical.y < 1) //플레이어와 일정 거리만큼 가까워지면 공격하기
        {
            on_action = true;
            Attack();
        }
    }
    async void Attack()
    {
        await Task.Delay(3000); //호출시 공격까지의 딜레이 (ms)
        on_action = false;
    }
    void CheckGround()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        isGrounded = false;
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("Ground"))
            {
                isGrounded = true;
                break;
            }
        }
        Debug.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckRadius, 
                      isGrounded ? Color.green : Color.red);
    }
    void Jump()
    {
        Debug.Log("JUMPING!");
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}