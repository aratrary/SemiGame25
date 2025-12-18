using System;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

public class CameraMove : MonoBehaviour
{
    public GameObject player;

    //camera speed임 미니언아님
    public float cs = 5f; 
    
    // 0이어도 0.01같이 해놔야합니다 안그럼 초기화되어서 9999됨
    public float LeftCameraBound = 0;
    public float RightCameraBound = 0;
    
    private Vector3 dir; //카메라 움직여야하는 방향(player-camera로 구함)

    void Start()
    {
        // Bound값이 0이면 강제로 맨왼쪽 맨오른쪽으로 바꿈
        if (LeftCameraBound == 0) LeftCameraBound = -9999;
        if (RightCameraBound == 0) RightCameraBound = 9999;
        
    }
    void LateUpdate() // 플레이어 움직임 -> 카메라이동 이렇게해야 움직임에 버그 안난대요
    {
        // 경계 밖에선 y좌표만 따라가기, 경계 안에선 xyz 다따라가기(z는의미없음)
        if (LeftCameraBound >= player.transform.position.x || RightCameraBound <= player.transform.position.x)
            {//Debug.Log("BOUNDS");
            dir = new(0, player.transform.position.y - this.transform.position.y + 2, 0);}
        else
            dir = player.transform.position - this.transform.position + new Vector3(0, 2, 0);

        Vector3 moveVector = new(dir.x*cs*Time.deltaTime, dir.y*cs*Time.deltaTime, 0f);

        this.transform.Translate(moveVector);

    }
}
