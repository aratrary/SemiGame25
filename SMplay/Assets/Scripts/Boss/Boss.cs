using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;
using Unity.VisualScripting;
using JetBrains.Annotations;

public class FloatingLibrary : MonoBehaviour
{
    public Transform target;
    public float speed;
    public GameObject prefab;
    public GameObject prefabray;
    private List<GameObject> minionList = new List<GameObject>();
    private List<float> angleList = new List<float>();
    private List<Quaternion> angleList_plzbee = new List<Quaternion>();
    private List<(GameObject ray,int booknum)> minionrayList = new List<(GameObject ray,int booknum)>();
    private List<Vector2> waypoints = new List<Vector2>() //보스랑 자식개체 움직이는 위치 - 좌표 리스트
    {
        new Vector2(-3.32f,-0.46f),//보스 첫번쨰 패턴 (카톡에 올라와있는 삼각형으로 움직이는 패턴)의 왼쪽 끝점 좌표
        new Vector2(4.14f,1.77f),//보스 첫번쨰 패턴 (카톡에 올라와있는 삼각형으로 움직이는 패턴)의 위쪽 좌표
        new Vector2(10.6f,-0.5f), //보스 첫번쨰 패턴 (카톡에 올라와있는 삼각형으로 움직이는 패턴)의 오른쪽 좌표
        new Vector2(4.03f,3.15f),//책 소환패턴시 책장 이동위치
        new Vector2(-0.02f,2.68f), //책 소환패턴시 책 이동위치
        new Vector2(8.81f,2.76f),//책 소환패턴시 책 이동위치
        new Vector2(-2.32f,-0.64f),//책 소환패턴시 책 이동위치
        new Vector2(10.6f,-0.68f),//책 소환패턴시 책 이동위치
    };
    private int currentWaypoint = 0;
    public bool on_pattern = false; //패턴 실행중인지 체크용
    public int now_pattern = 1; //어떤 패턴을 실행중인지
    public float boss_health;
    public int Health = 7; // 보스가 맞아야 죽는 히트 카운트 (자스틱으로 7대 맞으면 죽음)
    public float contactDamage = 1f; // 플레이어와 직접 닿았을 때 줄 데미지
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector2 direction = (Vector2)target.position - waypoints[i+4];
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angleList.Add(angle);
            angleList_plzbee.Add(Quaternion.Euler(0, 0, angle));
        }
        boss_health = 500f;
        
    }
    void TkDMG(float dmg)
    {
        boss_health = boss_health - dmg;
        if (boss_health <= 0)
            Destroy(this.gameObject);
    }

    // Stick.cs의 enemy.Death 호출을 받을 수 있게 Death 메서드 추가
    // 적 스크립트의 Death()와 유사하게 히트 카운트를 감소시키고 0이 되면 파괴합니다.
    public void Death()
    {
        Health -= 1;
        if (Health <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    // 플레이어와 직접 닿을 때 데미지를 주기 위한 2D 콜라이더 핸들러
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;
        var other = collision.collider;
        if (other == null) return;
        Debug.Log($"[Boss] OnCollisionEnter2D with {other.gameObject.name} (layer:{LayerMask.LayerToName(other.gameObject.layer)})");

        // Moving 스크립트(프로젝트에서 플레이어의 TakeDamage 호출 방식)를 우선 호출
        var moving = other.GetComponent<Moving>();
        if (moving != null)
        {
            Debug.Log($"[Boss] -> calling Moving.TakeDamage() on {other.gameObject.name}");
            moving.TakeDamage();
            return;
        }

        // PlayerHealthController가 있다면 float형 데미지로 호출
        var phc = other.GetComponent<PlayerHealthController>();
        if (phc != null)
        {
            Debug.Log($"[Boss] -> calling PlayerHealthController.TakeDamage({contactDamage}) on {other.gameObject.name}");
            phc.TakeDamage(contactDamage);
            return;
        }
    }

    // 트리거 충돌인 경우에도 대응
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        Debug.Log($"[Boss] OnTriggerEnter2D with {other.gameObject.name} (layer:{LayerMask.LayerToName(other.gameObject.layer)})");
        var moving = other.GetComponent<Moving>();
        if (moving != null)
        {
            Debug.Log($"[Boss] -> calling Moving.TakeDamage() on {other.gameObject.name}");
            moving.TakeDamage();
            return;
        }
        var phc = other.GetComponent<PlayerHealthController>();
        if (phc != null)
        {
            Debug.Log($"[Boss] -> calling PlayerHealthController.TakeDamage({contactDamage}) on {other.gameObject.name}");
            phc.TakeDamage(contactDamage);
            return;
        }
    }

    // 3D collider compatibility: if boss has 3D collider/rigidbody, handle those collisions too
    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;
        var other = collision.collider;
        if (other == null) return;

        var moving = other.GetComponent<Moving>();
        if (moving != null)
        {
            moving.TakeDamage();
            return;
        }

        var phc = other.GetComponent<PlayerHealthController>();
        if (phc != null)
        {
            phc.TakeDamage(contactDamage);
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        var moving = other.GetComponent<Moving>();
        if (moving != null)
        {
            moving.TakeDamage();
            return;
        }
        var phc = other.GetComponent<PlayerHealthController>();
        if (phc != null)
        {
            phc.TakeDamage(contactDamage);
            return;
        }
    }

    void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector2 direction = (Vector2)target.position - waypoints[i+4];
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angleList[i]=angle;
        }
        
            
        if (!on_pattern) // 이미 패턴이 실행중이면 패턴 실행 x
        {
            int randomInt = Random.Range(0, 3);
            if (randomInt == 0)
                StartPattern(1); //공격패턴 확률 33%
            else
                StartPattern(2); // 이동패턴 확률 66%
        }
    }
    void StartPattern(int pattern_num)
    {
        on_pattern=true;
        if (pattern_num == 1) //이동 패턴
            StartCoroutine(Pattern_1());
        else if (pattern_num ==2) //공격 패턴
            StartCoroutine(Pattern_2());
    }
    IEnumerator Pattern_1() // 이동패턴
    {
        yield return StartCoroutine(Move(this.gameObject,0));
        yield return StartCoroutine(Wait(3f));
        yield return StartCoroutine(Move(this.gameObject,1));
        yield return StartCoroutine(Wait(3f));
        yield return StartCoroutine(Move(this.gameObject,2));
        yield return StartCoroutine(Wait(3f));
        on_pattern=false;
    }
    IEnumerator Pattern_2() //공격패턴
    {
        yield return StartCoroutine(Move(this.gameObject,3)); //보스 공격위치로 이동
        for (int i = 0; i<4;i++) // 책 소환
        {
            yield return StartCoroutine(Summon(0,-1)); //소환
            StartCoroutine(Move(minionList[i],i+4)); //책 4권 각각 지정위치로 이동
            minionList[i].transform.rotation = Quaternion.Euler(0, 0, angleList[i]); //책 4권 모두 플레이어를 바라보게 회전
            angleList_plzbee[i] = minionList[i].transform.rotation;
        }
        yield return StartCoroutine(Wait(2.0f)); //책이 소환 후 공격할떄까지의 대기시간
        int k = 0;
        for (int i = 0; i<4;i++)
        {
            StartCoroutine(Summon(1,k)); //공격 광선 소환
            minionrayList[i].ray.transform.rotation =angleList_plzbee[i];
            k++;
        }
        foreach(var (ray,num) in minionrayList)
        {
            ray.transform.position = (Vector2)minionList[num].transform.position + (Vector2)minionList[num].transform.right*6;
            
        }
        yield return StartCoroutine(Wait(2f));
        foreach (GameObject book in minionList)
        {
            Destroy(book);
        }
        minionList.Clear();
        foreach (var item in minionrayList)
        {
            Destroy(item.ray);
        }
        minionrayList.Clear();
        on_pattern=false;
    }
    IEnumerator Move(GameObject obj, int pos_num) //이동 (오브젝트 넣고, 위쪽 포지션 리스트에 값만 잘 넣어두면 편하게 쓸수 있게 만들었습니다)
    {// Move(움직일 오브젝트, 좌표 리스트에서 원하는 좌표의 인덱스값)
        while (Vector2.Distance(obj.transform.position, waypoints[pos_num]) > 0.01f)
        {
            obj.transform.position = Vector2.MoveTowards(
                obj.transform.position, 
                waypoints[pos_num],
                speed * Time.deltaTime
            );yield return null;
        }
        
    }
    IEnumerator Wait(float time) //Wait(대기시간)
    {
        yield return new WaitForSeconds(time);
    }
    
    IEnumerator Summon(int summon_type,int booknum) //책, 광선 소환코드
    {
        if (summon_type == 0) //책소환
        {
            GameObject floatbook = Instantiate(prefab);
            minionList.Add(floatbook);
            floatbook.transform.position = transform.position; //책을 책장 위치로 이동(1회)
        }
        if (summon_type == 1) //광선소환
        {
            GameObject floatbook_ray = Instantiate(prefabray);
            minionrayList.Add((floatbook_ray,booknum));
        }
        ;yield return null;
    }
}