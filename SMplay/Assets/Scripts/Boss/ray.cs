using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;

public class NewMonoBehaviourScript : MonoBehaviour
{
}

public class nvm : MonoBehaviour
{
    // 광선 데미지 (PlayerHealthController가 있다면 float로, Moving 스크립트가 있다면 호출수 1)
    public float dmg = 1f;

    // 2D 트리거로 플레이어 맞을 때 호출
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        Debug.Log($"[ray] OnTriggerEnter2D with {other.gameObject.name} (layer:{LayerMask.LayerToName(other.gameObject.layer)})");

        // 우선 Moving 스크립트가 붙어 있는지 확인 (프로젝트에서 적들이 쓰는 방식)
        var moving = other.GetComponent<Moving>();
        if (moving != null)
        {
            Debug.Log($"[ray] -> calling Moving.TakeDamage() on {other.gameObject.name}");
            moving.TakeDamage(); // Moving.TakeDamage는 파라미터 없음으로 1 깎음
            return;
        }

        // PlayerHealthController가 붙어 있으면 데미지 전달
        var phc = other.GetComponent<PlayerHealthController>();
        if (phc != null)
        {
            Debug.Log($"[ray] -> calling PlayerHealthController.TakeDamage({dmg}) on {other.gameObject.name}");
            phc.TakeDamage(dmg);
            return;
        }

        // 태그가 JK/Player인데도 스크립트가 없는 경우 경고
        if (other.CompareTag("JK") || other.CompareTag("Player"))
        {
            Debug.LogWarning($"[ray] Player object '{other.gameObject.name}' has no Moving or PlayerHealthController attached.");
        }
    }

    // 혹시 광선이 콜리전(비트리거)으로 설정돼 있다면 충돌으로도 처리
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;
        var other = collision.collider;
        if (other == null) return;

        Debug.Log($"[ray] OnCollisionEnter2D with {other.gameObject.name} (layer:{LayerMask.LayerToName(other.gameObject.layer)})");

        var moving = other.GetComponent<Moving>();
        if (moving != null)
        {
            Debug.Log($"[ray] -> calling Moving.TakeDamage() on {other.gameObject.name}");
            moving.TakeDamage();
            return;
        }

        var phc = other.GetComponent<PlayerHealthController>();
        if (phc != null)
        {
            Debug.Log($"[ray] -> calling PlayerHealthController.TakeDamage({dmg}) on {other.gameObject.name}");
            phc.TakeDamage(dmg);
            return;
        }
    }

    // --- 3D collider compatibility: call same logic when using 3D colliders ---
    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        // reuse 2D logic via GameObject
        var go = other.gameObject;
        if (go == null) return;

        var moving = go.GetComponent<Moving>();
        if (moving != null)
        {
            moving.TakeDamage();
            return;
        }
        var phc = go.GetComponent<PlayerHealthController>();
        if (phc != null)
        {
            phc.TakeDamage(dmg);
            return;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;
        var go = collision.collider != null ? collision.collider.gameObject : null;
        if (go == null) return;
        var moving = go.GetComponent<Moving>();
        if (moving != null)
        {
            moving.TakeDamage();
            return;
        }
        var phc = go.GetComponent<PlayerHealthController>();
        if (phc != null)
        {
            phc.TakeDamage(dmg);
            return;
        }
    }
}
