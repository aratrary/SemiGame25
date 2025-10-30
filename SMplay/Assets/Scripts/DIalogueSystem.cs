using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임의 대화 시스템을 관리하는 메인 클래스
/// 타이핑 효과, 대화 진행, UI 제어 등을 담당
/// </summary>
/// sex


public class DialogueSystem : MonoBehaviour
{
    #region UI 참조 변수들
    [Header("UI 참조")]
    /// <summary>
    /// 대화창 전체를 감싸는 패널 오브젝트
    /// 대화 시작/종료 시 활성화/비활성화됨
    /// </summary>
    public GameObject dialoguePanel;
    
    /// <summary>
    /// 캐릭터 이름을 표시하는 TextMeshPro 컴포넌트
    /// 대화창 상단에 위치
    /// </summary>
    public TextMeshProUGUI characterNameText;
    
    /// <summary>
    /// 실제 대화 내용을 표시하는 TextMeshPro 컴포넌트
    /// 타이핑 효과가 적용되는 메인 텍스트
    /// </summary>
    public TextMeshProUGUI dialogueText;
    
    /// <summary>
    /// "계속하려면 스페이스바를 누르세요" 같은 표시기
    /// 타이핑 완료 후에만 활성화됨
    /// </summary>
    public GameObject continueIndicator;

    /// <summary>
    /// 캐릭터 초상화를 표시하는 Image 컴포넌트 (선택사항)
    /// </summary>
    public Image characterPortrait;

    public GameObject skillInventoryPanel;
    #endregion
    
    #region 대화 설정 변수들
    [Header("대화 시스템 설정")]
    /// <summary>
    /// 타이핑 효과의 속도 (초 단위)
    /// 0.02f = 한 글자당 0.02초 간격으로 표시
    /// 값이 작을수록 빠르게 타이핑됨
    /// </summary>
    [Range(0.01f, 0.1f)]
    public float typingSpeed = 0.02f;
    
    /// <summary>
    /// 대화창 등장/퇴장 애니메이션 속도
    /// </summary>
    [Range(0.1f, 2.0f)]
    public float animationSpeed = 0.5f;
    #endregion
    
    #region 내부 상태 변수들
    /// <summary>
    /// 대화 문장들을 순서대로 관리하는 큐(Queue) 자료구조
    /// FIFO(First In, First Out) 방식으로 문장들을 처리
    /// </summary>
    private Queue<string> sentences;
    
    /// <summary>
    /// 현재 타이핑 효과가 진행 중인지 확인하는 플래그
    /// true: 타이핑 중, false: 타이핑 완료 또는 대기 중
    /// </summary>
    private bool isTyping = false;
    
    /// <summary>
    /// 대화 시스템이 현재 활성화되어 있는지 확인하는 플래그
    /// true: 대화 진행 중, false: 대화 종료 상태
    /// </summary>
    private bool dialogueActive = false;
    
    /// <summary>
    /// 현재 타이핑 중인 코루틴의 참조
    /// 타이핑 중단 시 사용
    /// </summary>
    private Coroutine typingCoroutine;
    private enum UIState { None, Dialogue, SkillInventory }
    // ⭐ UI 상태 정의 ⭐
    private UIState currentUIState = UIState.None; 
    // ⭐ 현재 UI 상태 변수 ⭐
    [Header("게임 대화 시퀀스 목록")]
    public List<DialogueData> gameDialogueSequences = new List<DialogueData>();
    #endregion
    
    #region Unity 생명주기 메서드들
    /// <summary>
    /// 게임 시작 시 한 번 호출되는 초기화 메서드
    /// </summary>
    void Start()
    {
        // 문장 큐 초기화 (빈 큐 생성)
        sentences = new Queue<string>();
        
        // 게임 시작 시 대화창을 숨김 상태로 설정
        dialoguePanel.SetActive(false);
        
        // 계속하기 표시기도 초기에는 숨김
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }

        // 초상화도 초기에는 숨김
        if (characterPortrait != null)
        {
            characterPortrait.gameObject.SetActive(false);
        }

        currentUIState = UIState.None; // ⭐ 시작 시 UI 상태는 None ⭐

        if (gameDialogueSequences.Count > 0)
    {
        StartDialogue(gameDialogueSequences[0]);
    }
    }
    
    /// <summary>
    /// 매 프레임마다 호출되는 업데이트 메서드
    /// 사용자 입력을 감지하고 대화 진행을 제어
    /// </summary>
    void Update()
    {
        // 대화가 활성화되어 있고 스페이스바가 눌렸을 때만 처리
        if (dialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpacebarInput();
        }

        if (Input.GetKeyDown(KeyCode.T))
    {
        Debug.Log("T 키 눌림 감지!"); // ⭐ 임시로 추가하여 T 키 입력 자체를 확인해 보세요 ⭐
        ToggleUIPanel();
    }
        
        // ESC 키로 대화 강제 종료 (선택사항)
        if (dialogueActive && Input.GetKeyDown(KeyCode.Escape))
        {
            EndDialogue();
        }
    }
    #endregion
    
    #region 공개 메서드들 (외부에서 호출 가능)
    /// <summary>
    /// 새로운 대화를 시작하는 메서드
    /// 외부 스크립트(DialogueTrigger 등)에서 호출
    /// </summary>
    /// <param name="dialogue">표시할 대화 데이터</param>
    public void StartDialogue(DialogueData dialogue)
    {
        // 이미 대화가 진행 중이면 새 대화 시작 방지
        if (dialogueActive)
        {
            Debug.LogWarning("대화가 이미 진행 중입니다!");
            return;
        }
        
        // 대화 데이터 유효성 검사
        if (dialogue == null || dialogue.sentences == null || dialogue.sentences.Length == 0)
        {
            Debug.LogError("유효하지 않은 대화 데이터입니다!");
            return;
        }


        // 대화 시스템 활성화
        dialogueActive = true;
        if (skillInventoryPanel != null) skillInventoryPanel.SetActive(false); // 혹시 몰라 항상 끕니다.
        currentUIState = UIState.Dialogue; // 대화 시작하면 무조건 대화창 상태
        
        // 대화창 패널 활성화 (화면에 표시)
        dialoguePanel.SetActive(true);
        
        // 캐릭터 이름 설정 (빈 문자열이면 "???" 표시)
        characterNameText.text = string.IsNullOrEmpty(dialogue.characterName) ? "???" : dialogue.characterName;
        
        // 캐릭터 초상화 설정
        SetCharacterPortrait(dialogue.characterPortrait);
        
        // 기존 큐에 남아있던 문장들을 모두 제거 (이전 대화 정리)
        sentences.Clear();
        
        // 새로운 대화의 모든 문장을 큐에 순서대로 추가
        foreach (string sentence in dialogue.sentences)
        {
            // 빈 문장이 아닌 경우에만 큐에 추가
            if (!string.IsNullOrEmpty(sentence.Trim()))
            {
                sentences.Enqueue(sentence);
            }
        }
        
        // 첫 번째 문장 표시 시작
        DisplayNextSentence();
        
        // 대화 시작 이벤트 (다른 시스템에서 사용할 수 있음)
        OnDialogueStart();
    }
    
    /// <summary>
    /// 현재 대화를 강제로 종료하는 메서드
    /// </summary>
    public void EndDialogue()
    {
        // 타이핑 코루틴이 실행 중이면 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // 모든 상태 플래그 리셋
        dialogueActive = false;
        isTyping = false;

        // UI 요소들 비활성화
        dialoguePanel.SetActive(false);
        if (skillInventoryPanel != null) skillInventoryPanel.SetActive(false); // 확실히 끔
        currentUIState = UIState.None; // 대화 종료하면 UI 상태는 None
        continueIndicator.SetActive(false);
        
        // 초상화 숨기기
        if (characterPortrait != null)
        {
            characterPortrait.gameObject.SetActive(false);
        }
        
        // 텍스트 초기화
        dialogueText.text = "";
        characterNameText.text = "";
        
        // 큐 정리
        sentences.Clear();
        
        // 대화 종료 이벤트
        OnDialogueEnd();
    }
    #endregion
    
    #region 내부 메서드들 (private)
    /// <summary>
    /// 스페이스바 입력 처리 로직
    /// 상황에 따라 다른 동작 수행
    /// </summary>
    private void HandleSpacebarInput()
    {
        if (isTyping)
        {
            // 타이핑 중이면 즉시 완성
            CompleteCurrentTyping();
        }
        else
        {
            // 타이핑 완료 상태면 다음 문장으로 진행
            DisplayNextSentence();
        }
    }
    
    /// <summary>
    /// 큐에서 다음 문장을 가져와서 표시하는 메서드
    /// 더 이상 문장이 없으면 대화 종료
    /// </summary>
    private void DisplayNextSentence()
    {
        // 큐가 비어있으면 (더 이상 표시할 문장이 없으면)
        if (sentences.Count == 0)
        {
            // 대화 종료
            EndDialogue();
            return;
        }
        
        // 큐에서 다음 문장을 추출 (FIFO 방식)
        string sentence = sentences.Dequeue();
        
        // 이전 타이핑 코루틴이 있다면 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // 새로운 문장 타이핑 시작
        typingCoroutine = StartCoroutine(TypeSentence(sentence));
    }
    
    /// <summary>
    /// 문장을 한 글자씩 타이핑하는 효과를 구현하는 코루틴
    /// </summary>
    /// <param name="sentence">타이핑할 문장</param>
    /// <returns>코루틴 IEnumerator</returns>
    private IEnumerator TypeSentence(string sentence)
    {
        // 타이핑 시작 상태 설정
        isTyping = true;
        
        // 계속하기 표시기 숨김 (타이핑 중에는 표시하지 않음)
        continueIndicator.SetActive(false);
        
        // 대화 텍스트 초기화 (빈 문자열로 시작)
        dialogueText.text = "";
        
        // 문장을 문자 배열로 변환하여 한 글자씩 처리
        foreach (char letter in sentence.ToCharArray())
        {
            // 현재 텍스트에 새 글자 추가
            dialogueText.text += letter;
            
            // 다음 글자까지 지정된 시간만큼 대기
            // yield return은 다음 프레임까지 실행을 일시 중단
            yield return new WaitForSeconds(typingSpeed);
        }
        
        // 타이핑 완료 상태로 변경
        isTyping = false;
        
        // 계속하기 표시기 활성화 (사용자가 다음으로 진행할 수 있음을 알림)
        continueIndicator.SetActive(true);
    }
    
    /// <summary>
    /// 현재 타이핑을 즉시 완성하는 메서드
    /// 사용자가 스페이스바를 눌러 타이핑을 건너뛸 때 호출
    /// </summary>
    private void CompleteCurrentTyping()
    {
        // 타이핑 코루틴 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // 타이핑 상태 해제
        isTyping = false;
        
        // 현재 문장의 전체 내용을 즉시 표시
        // Peek()는 큐에서 제거하지 않고 다음 요소만 확인
        if (sentences.Count > 0)
        {
            // 아직 큐에 문장이 남아있다면, 마지막으로 Dequeue된 문장을 완성
            // 실제로는 현재 타이핑 중인 문장을 완성해야 함
            // 이 부분은 구현 방식에 따라 다를 수 있음
        }
        
        // 계속하기 표시기 활성화
        continueIndicator.SetActive(true);
    }
    
    /// <summary>
    /// 캐릭터 초상화를 설정하는 메서드
    /// </summary>
    /// <param name="portrait">설정할 초상화 스프라이트</param>
    private void SetCharacterPortrait(Sprite portrait)
    {
        if (characterPortrait != null)
        {
            if (portrait != null)
            {
                // 초상화가 있으면 설정하고 활성화
                characterPortrait.sprite = portrait;
                characterPortrait.gameObject.SetActive(true);
            }
            else
            {
                // 초상화가 없으면 숨김
                characterPortrait.gameObject.SetActive(false);
            }
        }
    }
    #endregion
    
    #region 이벤트 메서드들 (확장 가능)
    /// <summary>
    /// 대화 시작 시 호출되는 이벤트 메서드
    /// 다른 시스템에서 대화 시작을 감지할 때 사용
    /// </summary>
    private void OnDialogueStart()
    {
        // 예: 게임 일시정지, BGM 변경, 캐릭터 이동 제한 등
        Debug.Log("대화가 시작되었습니다.");
        
        // 플레이어 이동 제한 (예시)
        // PlayerController.instance?.SetMovementEnabled(false);
    }
    
    /// <summary>
    /// 대화 종료 시 호출되는 이벤트 메서드
    /// </summary>
    private void OnDialogueEnd()
    {
        // 예: 게임 재개, 원래 BGM 복원, 캐릭터 이동 허용 등
        Debug.Log("대화가 종료되었습니다.");
        
        // 플레이어 이동 허용 (예시)
        // PlayerController.instance?.SetMovementEnabled(true);
    }
    #endregion
    
    #region 디버그 및 유틸리티 메서드들
    /// <summary>
    /// 현재 대화 시스템의 상태를 확인하는 메서드 (디버그용)
    /// </summary>
    /// <returns>대화 활성화 여부</returns>
    public bool IsDialogueActive()
    {
        return dialogueActive;
    }

    /// <summary>
    /// 남은 문장 수를 반환하는 메서드 (디버그용)
    /// </summary>
    /// <returns>큐에 남은 문장 수</returns>
    public int GetRemainingMessageCount()
    {
        return sentences.Count;
    }
    #endregion
    
        /// <summary>
    /// 'T' 키 입력에 따라 UI 패널들의 가시성 상태를 전환합니다.
    /// 순환: None -> Dialogue -> SkillInventory -> None -> ... (또는 Dialogue -> SkillInventory -> Dialogue 로 순환)
    /// </summary>
    private void ToggleUIPanel()
    {
        Debug.Log("ToggleUIPanel 호출됨, 현재 UIState (진입시): " + currentUIState + ", dialogueActive: " + dialogueActive);

        // 모든 UI 패널 일단 끄기 (여기서는 건드리지 않음)
        //dialoguePanel.SetActive(false); // 이건 대화창이 꺼진 상태여도 유지해야 하므로, 밑에서 개별적으로 제어
        //if (skillInventoryPanel != null) skillInventoryPanel.SetActive(false); // 이것도 마찬가지

        // 현재 대화가 진행 중일 때만 UI 전환 로직을 수행
        if (!dialogueActive)
        {
            // 대화가 진행 중이 아닐 때는 T를 눌러도 Skill/Inventory UI만 토글되게 하거나 아무것도 하지 않도록 할 수 있습니다.
            // 여기서는 대화가 진행 중일 때만 'T' 키가 Dialogue와 SkillInventory를 전환하도록 만듭니다.
            // 만약 대화와 무관하게 SkillInventory를 토글하고 싶다면 이 if문을 제거하고 로직을 수정해야 합니다.
            // 우선은 대화 중에만 UI 전환이 되도록 가정합니다.

            Debug.Log("대화가 진행 중이 아니라 UI 전환 로직을 건너뜁니다.");
            return; // 대화가 진행 중이 아닐 때는 'T' 키가 UI 전환을 하지 않도록
        }

        // 현재 UI 상태에 따라 다음 상태 결정
        switch (currentUIState)
        {
            case UIState.Dialogue: // 현재 대화창이 켜져 있었으면 스킬/인벤토리로
                dialoguePanel.SetActive(false); // 대화창 끄기
                if (skillInventoryPanel != null)
                {
                    skillInventoryPanel.SetActive(true); // 스킬/인벤토리 켜기
                    currentUIState = UIState.SkillInventory;
                }
                else // 스킬/인벤토리 패널이 없으면 다시 Dialogue로 (순환)
                {
                    dialoguePanel.SetActive(true); // 대화창 켜기
                    currentUIState = UIState.Dialogue;
                }
                break;

            case UIState.SkillInventory: // 현재 스킬/인벤토리가 켜져 있었으면 대화창으로
                if (skillInventoryPanel != null) skillInventoryPanel.SetActive(false); // 스킬/인벤토리 끄기
                dialoguePanel.SetActive(true); // 대화창 켜기
                currentUIState = UIState.Dialogue;
                break;

            default: // None 상태이거나 예측 불가능한 상태라면 기본적으로 대화창으로
                dialoguePanel.SetActive(true);
                if (skillInventoryPanel != null) skillInventoryPanel.SetActive(false); // 다른 UI는 끄고
                currentUIState = UIState.Dialogue;
                break;
        }

        Debug.Log("ToggleUIPanel 호출됨, 변경 후 UIState: " + currentUIState);
    }
}