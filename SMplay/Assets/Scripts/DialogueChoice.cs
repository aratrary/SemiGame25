using UnityEngine;

/// <summary>
/// 분기형 대화를 위한 선택지 데이터 클래스
/// 플레이어가 선택할 수 있는 옵션과 그에 따른 결과 대화를 정의
/// </summary>
/// sex
[System.Serializable]
public class DialogueChoice
{
    [Header("선택지 정보")]
    /// <summary>
    /// 선택지 버튼에 표시될 텍스트
    /// 예: "네, 도와드리겠습니다.", "아니요, 거절합니다."
    /// </summary>
    [TextArea(2, 5)]
    public string choiceText;
    
    /// <summary>
    /// 이 선택지를 선택했을 때 이어질 대화 데이터
    /// null이면 대화 종료
    /// </summary>
    public DialogueData nextDialogue;
    
    [Header("선택지 조건 (선택사항)")]
    /// <summary>
    /// 이 선택지가 표시되기 위한 조건
    /// 예: 특정 아이템 소지, 스탯 수치, 퀘스트 진행 상황 등
    /// </summary>
    public DialogueCondition condition;
    
    [Header("선택지 결과 (선택사항)")]
    /// <summary>
    /// 선택지 선택 시 실행될 액션들
    /// 예: 아이템 획득, 경험치 증가, 퀘스트 진행 등
    /// </summary>
    public DialogueAction[] actions;
}

/// <summary>
/// 선택지 표시 조건을 정의하는 클래스
/// </summary>
[System.Serializable]
public class DialogueCondition
{
    /// <summary>
    /// 조건 타입 (아이템, 스탯, 퀘스트 등)
    /// </summary>
    public ConditionType type;
    
    /// <summary>
    /// 조건 대상의 ID 또는 이름
    /// </summary>
    public string targetId;
    
    /// <summary>
    /// 비교 연산자 (같음, 큼, 작음 등)
    /// </summary>
    public ComparisonOperator comparison;
    
    /// <summary>
    /// 비교할 값
    /// </summary>
    public int value;
}

/// <summary>
/// 선택지 선택 시 실행될 액션을 정의하는 클래스
/// </summary>
[System.Serializable]
public class DialogueAction
{
    /// <summary>
    /// 액션 타입
    /// </summary>
    public ActionType type;
    
    /// <summary>
    /// 액션 대상의 ID
    /// </summary>
    public string targetId;
    
    /// <summary>
    /// 액션 값 (수량, 증가량 등)
    /// </summary>
    public int value;
    
    /// <summary>
    /// 추가 문자열 매개변수
    /// </summary>
    public string stringParameter;
}

/// <summary>
/// 조건 타입 열거형
/// </summary>
public enum ConditionType
{
    None,           // 조건 없음
    HasItem,        // 특정 아이템 소지
    StatValue,      // 스탯 수치
    QuestStatus,    // 퀘스트 상태
    Flag            // 게임 플래그
}

/// <summary>
/// 비교 연산자 열거형
/// </summary>
public enum ComparisonOperator
{
    Equal,          // 같음
    GreaterThan,    // 큼
    LessThan,       // 작음
    GreaterEqual,   // 크거나 같음
    LessEqual       // 작거나 같음
}

/// <summary>
/// 액션 타입 열거형
/// </summary>
public enum ActionType
{
    None,           // 액션 없음
    GiveItem,       // 아이템 지급
    RemoveItem,     // 아이템 제거
    ChangeGold,     // 골드 변경
    ChangeExp,      // 경험치 변경
    SetFlag,        // 플래그 설정
    StartQuest,     // 퀘스트 시작
    CompleteQuest   // 퀘스트 완료
}