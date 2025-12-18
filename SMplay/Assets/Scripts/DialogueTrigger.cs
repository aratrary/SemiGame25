using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// NPC나 상호작용 오브젝트에 부착되어 대화 시퀀스를 시작시키는 트리거 클래스
/// GameStarter와 유사한 방식으로 여러 대화를 순차 재생
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    #region 설정 변수들
    [Header("대화 시퀀스 설정")]
    [Tooltip("재생할 대화 시퀀스 배열")]
    public DialogueSequence[] dialogueSequences;
    
    [Header("재생 설정")]
    [Tooltip("각 대화 사이 대기 시간 (초)")]
    public float delayBetweenDialogues = 0.5f;
    
    [Header("자동 시작")]
    [Tooltip("씬 시작 시 자동으로 대화를 시작할지 여부")]
    public bool autoStartOnLoad = true;
    
    [Header("상호작용 설정")]
    [HideInInspector]
    [Tooltip("상호작용 키 (기본값: E키) - 현재 비활성화됨")]
    public KeyCode interactionKey = KeyCode.E;
    
    [HideInInspector]
    [Tooltip("자동으로 대화를 시작할지 여부 - 현재 비활성화됨")]
    public bool autoTrigger = false;
    
    [Tooltip("대화를 한 번만 실행할지 여부")]
    public bool oneTimeOnly = false;
    
    [Header("상호작용 UI (선택사항)")]
    [Tooltip("E키를 눌러 대화하기 같은 상호작용 안내 UI")]
    public GameObject interactionUI;
    #endregion
    
    #region 내부 상태 변수들
    private DialogueSystem dialogueSystem;
    private bool isPlayingSequence = false;
    private bool playerInRange = false;
    private bool hasTriggered = false;
    private GameObject currentPlayer;
    #endregion
    
    #region Unity 생명주기 메서드들
    void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        
        if (dialogueSystem == null)
        {
            Debug.LogError($"{gameObject.name}: 씬에서 DialogueSystem을 찾을 수 없습니다!");
        }
        
        if (dialogueSequences == null || dialogueSequences.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: 대화 시퀀스가 설정되지 않았습니다!");
        }
        
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        
        ValidateTriggerSetup();
        
        // 씬 시작 시 자동으로 대화 시작
        if (autoStartOnLoad && CanTriggerDialogue())
        {
            Debug.Log($"{gameObject.name}: 씬 시작 시 자동으로 대화를 시작합니다.");
            TriggerDialogue();
        }
    }
    
    void Update()
    {
        // ===== 임시로 키보드 입력 비활성화 =====
        // 나중에 퀘스트/스테이지 조건으로 교체 예정
        // 현재는 버튼 OnClick으로만 TriggerDialogue() 호출
        
        /* 키보드 입력 활성화 시 주석 해제
        if (playerInRange && !autoTrigger && CanTriggerDialogue())
        {
            if (Input.GetKeyDown(interactionKey))
            {
                TriggerDialogue();
            }
        }
        */
    }
    #endregion
    
    #region 트리거 이벤트 메서드들
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerEnter(other.gameObject);
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerExit(other.gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerEnter(other.gameObject);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerExit(other.gameObject);
        }
    }
    #endregion
    
    #region 플레이어 상호작용 처리
    private void HandlePlayerEnter(GameObject player)
    {
        currentPlayer = player;
        playerInRange = true;
        
        ShowInteractionUI();
        
        if (autoTrigger && CanTriggerDialogue())
        {
            TriggerDialogue();
        }
        
        OnPlayerEnterRange();
    }
    
    private void HandlePlayerExit(GameObject player)
    {
        currentPlayer = null;
        playerInRange = false;
        
        HideInteractionUI();
        
        OnPlayerExitRange();
    }
    
    private bool CanTriggerDialogue()
    {
        if (dialogueSystem == null || dialogueSequences == null || dialogueSequences.Length == 0)
            return false;
        
        if (isPlayingSequence)
            return false;
        
        if (dialogueSystem.IsDialogueActive())
            return false;
        
        if (oneTimeOnly && hasTriggered)
            return false;
        
        return true;
    }
    #endregion
    
    #region 공개 메서드들
    /// <summary>
    /// 대화 시퀀스를 시작하는 메인 메서드
    /// 외부에서도 호출 가능 (버튼 OnClick 등)
    /// </summary>
    public void TriggerDialogue()
    {
        if (!CanTriggerDialogue())
        {
            Debug.LogWarning($"{gameObject.name}: 대화를 시작할 수 없는 상태입니다.");
            return;
        }
        
        OnDialogueTriggered();
        
        StartCoroutine(PlayDialogueSequence());
        
        if (oneTimeOnly)
        {
            hasTriggered = true;
            HideInteractionUI();
        }
        
        Debug.Log($"{gameObject.name}: 대화 시퀀스를 시작했습니다.");
    }
    
    /// <summary>
    /// 대화 시퀀스를 순차적으로 재생하는 코루틴
    /// </summary>
    private IEnumerator PlayDialogueSequence()
    {
        isPlayingSequence = true;
        
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
            // 컷씬 전용 모드 (대화 없이 컷씬만)
            if (sequence.isCutsceneOnly)
            {
                Debug.Log($"=== 컷씬 전용 모드 ===");
                Debug.Log($"컷씬 Duration 설정값: {sequence.cutsceneDuration}초");
                
                sequence.onCutscene?.Invoke();
                Debug.Log($"onCutscene 이벤트 실행됨");
                
                if (sequence.cutsceneDuration > 0f)
                {
                    Debug.Log($"{sequence.cutsceneDuration}초 대기 시작...");
                    yield return new WaitForSeconds(sequence.cutsceneDuration);
                    Debug.Log($"=== {sequence.cutsceneDuration}초 대기 완료 ===");
                }
                else
                {
                    Debug.LogWarning("cutsceneDuration이 0 이하입니다! 컷씬 스킵됨");
                }
                continue;
            }

            // 유효하지 않은 대화 건너뛰기
            if (sequence.dialogue == null || !sequence.dialogue.IsValid())
            {
                Debug.LogWarning("유효하지 않은 대화 데이터를 건너뜁니다.");
                continue;
            }
            
            dialogueIndex++;
            bool isLastDialogue = (dialogueIndex >= totalDialogues);
            
            // 컷씬과 대화 동시 실행 (onCutscene 이벤트가 있으면 실행)
            Debug.Log($"onCutscene 체크 중... (null: {sequence.onCutscene == null})");
            if (sequence.onCutscene != null)
            {
                Debug.Log($"=== 컷씬 + 대화 동시 실행 ===");
                sequence.onCutscene.Invoke();
            }
            else
            {
                Debug.LogWarning("onCutscene이 null입니다. 컷씬 없이 대화만 진행합니다.");
            }
            
            // 대화 시작 (첫 대화 아니면 패널 유지)
            bool keepPanelOpen = (dialogueIndex > 1);
            dialogueSystem.StartDialogue(sequence.dialogue, keepPanelOpen);
            
            // 대화 종료 대기
            yield return new WaitUntil(() => !dialogueSystem.IsDialogueActive());
            
            // 대화 종료 처리 (마지막이 아니면 패널 유지)
            if (isLastDialogue)
            {
                dialogueSystem.EndDialogue(keepPanelOpen: false);
            }
            else
            {
                dialogueSystem.EndDialogue(keepPanelOpen: true);
            }
            
            // 다음 대화까지 대기 (마지막이 아니면)
            if (!isLastDialogue && delayBetweenDialogues > 0f)
            {
                yield return new WaitForSeconds(delayBetweenDialogues);
            }
        }
        
        isPlayingSequence = false;
        Debug.Log($"{gameObject.name}: 모든 대화 시퀀스가 완료되었습니다.");
    }
    
    /// <summary>
    /// 대화 시퀀스 배열을 런타임에 변경
    /// </summary>
    public void SetDialogue(DialogueSequence[] newDialogueSequences)
    {
        dialogueSequences = newDialogueSequences;
        
        if (oneTimeOnly)
        {
            hasTriggered = false;
        }
        
        Debug.Log($"{gameObject.name}: 대화 시퀀스가 변경되었습니다.");
    }
    
    /// <summary>
    /// 단일 대화 데이터를 설정하는 간편 메서드 (하위 호환성)
    /// </summary>
    public void SetDialogue(DialogueData newDialogue)
    {
        dialogueSequences = new DialogueSequence[]
        {
            new DialogueSequence { dialogue = newDialogue }
        };
        
        if (oneTimeOnly)
        {
            hasTriggered = false;
        }
        
        Debug.Log($"{gameObject.name}: 대화 데이터가 변경되었습니다.");
    }
    
    /// <summary>
    /// 트리거를 다시 활성화
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        Debug.Log($"{gameObject.name}: 트리거가 리셋되었습니다.");
    }
    #endregion
    
    #region UI 관리 메서드들
    private void ShowInteractionUI()
    {
        if (interactionUI != null && CanTriggerDialogue() && !autoTrigger)
        {
            interactionUI.SetActive(true);
            UpdateInteractionUIText();
        }
    }
    
    private void HideInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }
    
    private void UpdateInteractionUIText()
    {
        if (interactionUI != null)
        {
            var textComponent = interactionUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            
            if (textComponent != null)
            {
                textComponent.text = $"{interactionKey}키를 눌러 대화하기";
            }
        }
    }
    #endregion
    
    #region 이벤트 메서드들 (확장 가능)
    protected virtual void OnPlayerEnterRange()
    {
        // 상속받아서 확장 가능
    }
    
    protected virtual void OnPlayerExitRange()
    {
        // 상속받아서 확장 가능
    }
    
    protected virtual void OnDialogueTriggered()
    {
        // 상속받아서 확장 가능
    }
    #endregion
    
    #region 유틸리티 및 디버그 메서드들
    private void ValidateTriggerSetup()
    {
        // ===== 임시로 Collider 체크 비활성화 =====
        // 버튼으로만 사용 중이므로 Collider 불필요
        // 나중에 플레이어 진입 감지가 필요하면 Collider 추가 후 주석 해제
        
        /* Collider 체크 활성화 시 주석 해제
        Collider2D collider2D = GetComponent<Collider2D>();
        Collider collider3D = GetComponent<Collider>();
        
        if (collider2D == null && collider3D == null)
        {
            Debug.LogError($"{gameObject.name}: 트리거 기능을 위해 Collider 컴포넌트가 필요합니다!");
            return;
        }
        
        bool isTrigger = false;
        if (collider2D != null)
        {
            isTrigger = collider2D.isTrigger;
        }
        else if (collider3D != null)
        {
            isTrigger = collider3D.isTrigger;
        }
        
        if (!isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: Collider의 'Is Trigger' 옵션이 체크되지 않았습니다!");
        }
        */
    }
    
    public string GetTriggerStatus()
    {
        return $"플레이어 범위 내: {playerInRange}, 실행됨: {hasTriggered}, 대화 가능: {CanTriggerDialogue()}";
    }
    #endregion
    
    #region Inspector 버튼들 (에디터 전용)
#if UNITY_EDITOR
    [ContextMenu("테스트 대화 실행")]
    private void TestTriggerDialogue()
    {
        if (Application.isPlaying)
        {
            TriggerDialogue();
        }
        else
        {
            Debug.Log("플레이 모드에서만 테스트 가능합니다.");
        }
    }
    
    [ContextMenu("트리거 리셋")]
    private void TestResetTrigger()
    {
        ResetTrigger();
    }
#endif
    #endregion
}
