using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;

public class FloatingLibrary : MonoBehaviour
{
    public Transform target;
    public float speed;
    public GameObject prefab;
    public GameObject prefabray;
    private List<GameObject> minionList = new List<GameObject>();
    private List<float> angleList = new List<float>();
    private List<(GameObject ray,int booknum)> minionrayList = new List<(GameObject ray,int booknum)>();
    private List<Vector2> waypoints = new List<Vector2>() //보스랑 자식개체 움직이는 위치 - 좌표 리스트
    {
        new Vector2(-6.84f, 1.44f),//보스 첫번쨰 패턴 (카톡에 올라와있는 삼각형으로 움직이는 패턴)의 왼쪽 끝점 좌표
        new Vector2(0, 3f),//보스 첫번쨰 패턴 (카톡에 올라와있는 삼각형으로 움직이는 패턴)의 위쪽 좌표
        new Vector2(6.84f,1.44f), //보스 첫번쨰 패턴 (카톡에 올라와있는 삼각형으로 움직이는 패턴)의 오른쪽 좌표
        new Vector2(0f,0.2f),//책 소환패턴시 책장 이동위치
        new Vector2(-6.07f,1.37f), //책 소환패턴시 책 이동위치
        new Vector2(-3.51f,2.56f),//책 소환패턴시 책 이동위치
        new Vector2(3.51f,2.56f),//책 소환패턴시 책 이동위치
        new Vector2(6.07f,1.37f),//책 소환패턴시 책 이동위치
    };
    private int currentWaypoint = 0;
    public bool on_pattern = false; //패턴 실행중인지 체크용
    public int now_pattern = 1; //어떤 패턴을 실행중인지
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector2 direction = (Vector2)target.position - waypoints[i+4];
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angleList.Add(angle);
        }
        if (minionList.Count != 0 && minionrayList.Count != 0)
        {
            foreach(var (ray,num) in minionrayList)
            {
                ray.transform.position = (Vector2)minionList[num].transform.position + (Vector2)minionList[num].transform.right*6;
                
            }
           
        }
            
        if (!on_pattern) // 이미 패턴이 실행중이면 패턴 실행 x
        {
            animator.SetInteger("AnimState",2);
            int randomInt = Random.Range(0, 3);
            if (randomInt == 0)
                StartPattern(2); //공격패턴 확률 33%
            else
                StartPattern(1); // 이동패턴 확률 66%
        }
        else
            animator.SetInteger("AnimState",1);
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
        }
        yield return StartCoroutine(Wait(4.0f)); //책이 소환 후 공격할떄까지의 대기시간
        int k = 0;
        for (int i = 0; i<4;i++)
        {
            StartCoroutine(Summon(1,k)); //공격 광선 소환
            minionrayList[i].ray.transform.rotation = Quaternion.Euler(0, 0, angleList[i]+90);
            k++;
        }
        yield return StartCoroutine(Wait(2f));
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