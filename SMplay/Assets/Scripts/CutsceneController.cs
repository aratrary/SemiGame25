using UnityEngine;
using System.Collections;

/// <summary>
/// 임시 컷씬 연출을 관리하는 스크립트
/// DialogueSequence의 onCutscene 이벤트에 연결해서 사용
/// </summary>
public class CutsceneController : MonoBehaviour
{
    /// <summary>
    /// tempPlayer를 오른쪽으로 움직이는 컷씬 (기본값: 2칸, 2초)
    /// OnCutscene 이벤트에 직접 연결 가능
    /// </summary>
    public void MovePlayerRight()
    {
        MovePlayer(2f, 2f);
    }

    /// <summary>
    /// tempPlayer를 왼쪽으로 움직이는 컷씬 (기본값: 2칸, 2초)
    /// OnCutscene 이벤트에 직접 연결 가능
    /// </summary>
    public void MovePlayerLeft()
    {
        MovePlayer(-2f, 2f);
    }

    /// <summary>
    /// tempPlayer를 움직이는 내부 구현
    /// </summary>
    /// <param name="distance">이동 거리 (양수=오른쪽, 음수=왼쪽)</param>
    /// <param name="duration">이동 시간(초)</param>
    private void MovePlayer(float distance, float duration)
    {
        // 여러 가능한 이름으로 플레이어를 찾음
        Transform player = GameObject.Find("TempPlayer")?.transform;
        if (player == null) player = GameObject.Find("tempPlayer")?.transform;
        if (player == null) player = GameObject.Find("Player")?.transform;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player == null)
        {
            Debug.LogError("플레이어를 찾을 수 없습니다! (TempPlayer, tempPlayer, Player 또는 Player 태그)");
            return;
        }

        Debug.Log($"플레이어 발견: {player.name}, 이동 시작 ({distance}칸, {duration}초)");
        StartCoroutine(MoveRoutine(player, new Vector3(distance, 0, 0), duration));
    }

    /// <summary>
    /// 실제 이동 처리 코루틴
    /// </summary>
    private IEnumerator MoveRoutine(Transform player, Vector3 movement, float duration)
    {
        Vector3 startPos = player.position;
        Vector3 endPos = startPos + movement;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration; // 0 ~ 1
            player.position = Vector3.Lerp(startPos, endPos, progress);
            yield return null;
        }

        player.position = endPos; // 최종 위치 확실히 설정
        Debug.Log($"tempPlayer 이동 완료: {endPos}");
    }
}
