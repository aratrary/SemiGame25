using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;
public class NewMonoBehaviourScript : MonoBehaviour
{
    float dmg; //광선 데미지

    private void OnTriggerEnter(Collider other)
    {
        // Player 태그를 가진 오브젝트인지 확인
        if (other.CompareTag("Player"))
        {
            // PlayerHealthController 컴포넌트 가져오기
            PlayerHealthController healthController = other.GetComponent<PlayerHealthController>();
            
            // 컴포넌트가 존재하면 TakeDamage 호출
            if (healthController != null)
            {
                healthController.TakeDamage(dmg);
            }
            else
            {
                Debug.LogWarning("PlayerHealthController를 찾을 수 없습니다!");
            }
        }
    }
}
