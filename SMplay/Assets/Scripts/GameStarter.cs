using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameStarter : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    
    [Header("대화 시퀀스 설정")]
    [Tooltip("재생할 대화 시퀀스 배열")]
    public DialogueSequence[] dialogueSequences;
    
    [Header("재생 설정")]
    [Tooltip("각 대화 사이 대기 시간 (초)")]
    public float delayBetweenDialogues = 0.5f;
    
    void Start()
    {
        StartCoroutine(StartSequence());
    }
       
    IEnumerator StartSequence()
    {
        int dialogueIndex = 0;
        int totalDialogues = 0;
        
        // 전체 대화 개수 미리 계산 (컷씬 제외)
        foreach (var seq in dialogueSequences)
        {
            if (!seq.isCutsceneOnly && seq.dialogue != null && seq.dialogue.IsValid())
            {
                totalDialogues++;
            }
        }
        
        foreach (DialogueSequence sequence in dialogueSequences)
        {
            if (sequence.isCutsceneOnly)
            {
                sequence.onCutscene?.Invoke();
                if (sequence.cutsceneDuration > 0f)
                {
                    yield return new WaitForSeconds(sequence.cutsceneDuration);
                }
                continue;
            }

            if (sequence.dialogue == null || !sequence.dialogue.IsValid())
            {
                Debug.LogWarning("유효하지 않은 대화 데이터를 건너뜁니다.");
                continue;
            }
            
            dialogueIndex++;
            bool isLastDialogue = (dialogueIndex >= totalDialogues);
            
            Debug.Log($"=== 대화 {dialogueIndex}/{totalDialogues} 시작 (마지막: {isLastDialogue}) ===");
            
            // 대화 전 트리거 비활성화
            if (sequence.disableTriggerBefore != null)
            {
                sequence.disableTriggerBefore.enabled = false;
            }
            
            // 대화 시작 (첫 대화 아니면 패널 유지)
            bool keepPanelOpen = (dialogueIndex > 1);
            Debug.Log($"StartDialogue 호출 - keepPanelOpen: {keepPanelOpen}");
            dialogueSystem.StartDialogue(sequence.dialogue, keepPanelOpen);
            
            // 대화 종료 대기
            Debug.Log("대화 종료 대기 중...");
            yield return new WaitUntil(() => !dialogueSystem.IsDialogueActive());
            
            Debug.Log("대화 종료 감지됨");
            
            // 대화 종료 처리 (마지막이 아니면 패널 유지)
            if (isLastDialogue)
            {
                Debug.Log("마지막 대화 - 패널 닫기");
                dialogueSystem.EndDialogue(keepPanelOpen: false);
            }
            else
            {
                Debug.Log("중간 대화 - 패널 유지");
                dialogueSystem.EndDialogue(keepPanelOpen: true);
            }
            
            // 대화 후 트리거 활성화
            if (sequence.enableTriggerAfter != null)
            {
                if (sequence.setDialogueData != null)
                {
                    sequence.enableTriggerAfter.SetDialogue(sequence.setDialogueData);
                }
                sequence.enableTriggerAfter.enabled = true;
            }
            
            // 다음 대화까지 대기 (마지막이 아니면)
            if (!isLastDialogue && delayBetweenDialogues > 0f)
            {
                yield return new WaitForSeconds(delayBetweenDialogues);
            }
        }
        
        Debug.Log("모든 대화 시퀀스가 완료되었습니다.");
    }
}

/// <summary>
/// 게임 대화 시퀀스 구성 클래스
/// </summary>
[System.Serializable]
public class DialogueSequence
{
    [Header("컷씬 전용")]
    [Tooltip("대사 없는 컷씬 연출이면 체크. 이 경우 dialogue는 무시됩니다.")]
    public bool isCutsceneOnly = false;

    [Tooltip("컷씬 길이(초). 타임라인 등 다른 연출이 끝나면 0으로 두고 이벤트에서 처리하세요.")]
    public float cutsceneDuration = 0f;

    [Tooltip("컷씬 연출 콜백 (타임라인 재생 등)")]
    public UnityEvent onCutscene;

    [Header("대화 데이터")]
    [Tooltip("재생할 대화")]
    public DialogueData dialogue;
    
    [Header("트리거 설정 (선택사항)")]
    [Tooltip("이 대화 전에 비활성화할 트리거")]
    public DialogueTrigger disableTriggerBefore;
    
    [Tooltip("이 대화 후에 활성화할 트리거")]
    public DialogueTrigger enableTriggerAfter;
    
    [Tooltip("활성화될 트리거에 설정할 새 대화 데이터")]
    public DialogueData setDialogueData;
}