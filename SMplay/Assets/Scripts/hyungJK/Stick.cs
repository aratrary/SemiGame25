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
    public LayerMask enemyMask;
    public LayerMask groundMask;
    public LayerMask JKMask;
    public bool hitEnemy;
    public bool hitGround;
    public GameObject hittedEnemy;
    public int layer=0;
    public bool isReturning;
    public Moving JKscript;
    public GameObject JK;
    public Transform JKtrans;
    public float returningspeed;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isFlying)
        {
            rigid.linearVelocityX = face * V;
        }

        else if (hitGround)
        {
            
        }
        else if (hitEnemy)
        {
            
        }

        if (isReturning)
        {
            Vector2 toJK = (Vector2)JKtrans.position - rigid.position;
            Vector2 step = (Vector2)(toJK / toJK.magnitude) * returningspeed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + step);
        }
    }
    public void Throwing(int facing)
    {
        maincollider.usedByEffector = true;
        rigid.constraints |= RigidbodyConstraints2D.FreezePositionY;
        sr.flipX = !(facing<0);
        face = facing;
        hitEnemy = false;
        hitGround = false;
        hittedEnemy = null;
        isFlying = true;
        childcollider.isTrigger = false;
    }
    public void Returning()
    {
        if (isReturning)
            return;
        isReturning = true;
        childcollider.isTrigger = true;
        maincollider.isTrigger = true;
        maincollider.usedByEffector = false;
        rigid.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        layer = 1 << collision.gameObject.layer;
        if (isFlying && (layer == enemyMask.value || layer == groundMask.value))
        {
            isFlying = false;
            maincollider.isTrigger = true;
            if (layer == enemyMask.value)
            {
                hitEnemy = true;
            }
            else if (layer == groundMask.value)
            {
                hitGround = true;
            }
        }
        
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        layer = 1 << collision.gameObject.layer;
        if (isReturning && layer == JKMask.value)
        {   
            isReturning = false;
            gameObject.SetActive(false);
            JKscript.havingStick = true;
            maincollider.isTrigger = false; 
            JKscript.anim.SetInteger("State", 2);
        }
    }
}
