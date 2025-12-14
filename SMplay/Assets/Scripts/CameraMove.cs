using System;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Analytics;

public class CameraMove : MonoBehaviour
{
    public GameObject player;

    //camera speed임 미니언아님
    public float cs = 5f; 
    
    // 카메라 경계(왼쪽/오른쪽)
    public float LeftCameraBound = -15;
    public float RightCameraBound = 9999;

    void LateUpdate() // 플레이어 움직임 -> 카메라이동 이렇게해야 움직임에 버그 안난대요
    {
        if (LeftCameraBound >= player.transform.position.x) return;
        if (RightCameraBound <= player.transform.position.y) return;

        Vector3 dir = player.transform.position - this.transform.position + new Vector3(0, 2, 0);
        Vector3 moveVector = new(dir.x*cs*Time.deltaTime, dir.y*cs*Time.deltaTime, 0f);

        this.transform.Translate(moveVector);
    }
}
