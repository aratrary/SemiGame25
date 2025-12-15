using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    
    [Header("대화 시퀀스 설정")]
    [Tooltip("대화가 진행될 순서대로 배열")]
    public DialogueSequence[] dialogueSequences;
    
    [Header("딜레이 설정")]
    [Tooltip("각 대화 사이의 대기 시간 (초)")]
    public float delayBetweenDialogues = 0.5f;
    
    void Start()
    {
        StartCoroutine(StartSequence());
    }
       
    IEnumerator StartSequence()
    {
        foreach (DialogueSequence sequence in dialogueSequences)
        {
            if (sequence.dialogue == null || !sequence.dialogue.IsValid())
            {
                Debug.LogWarning("유효하지 않은 대화 데이터를 건너뜁니다.");
                continue;
            }
            
            // 대화 전 트리거 비활성화
            if (sequence.disableTriggerBefore != null)
            {
                sequence.disableTriggerBefore.enabled = false;
            }
            
            // 대화 시작
            dialogueSystem.StartDialogue(sequence.dialogue);
            
            // 대화 완료 대기
            yield return new WaitUntil(() => !dialogueSystem.IsDialogueActive());
            
            // 대화 후 트리거 활성화
            if (sequence.enableTriggerAfter != null)
            {
                if (sequence.setDialogueData != null)
                {
                    sequence.enableTriggerAfter.SetDialogue(sequence.setDialogueData);
                }
                sequence.enableTriggerAfter.enabled = true;
            }
            
            // 다음 대화까지 딜레이
            yield return new WaitForSeconds(delayBetweenDialogues);
        }
        
        Debug.Log("모든 대화 시퀀스가 완료되었습니다.");
    }
}

/// <summary>
/// 개별 대화 시퀀스 데이터
/// </summary>
[System.Serializable]
public class DialogueSequence
{
    [Header("대화 데이터")]
    [Tooltip("재생할 대화")]
    public DialogueData dialogue;
    
    [Header("트리거 제어 (선택사항)")]
    [Tooltip("이 대화 전에 비활성화할 트리거")]
    public DialogueTrigger disableTriggerBefore;
    
    [Tooltip("이 대화 후에 활성화할 트리거")]
    public DialogueTrigger enableTriggerAfter;
    
    [Tooltip("활성화할 트리거에 설정할 대화 데이터")]
    public DialogueData setDialogueData;
}