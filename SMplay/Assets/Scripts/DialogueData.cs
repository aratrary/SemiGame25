using UnityEngine;

/// <summary>
/// 대화 데이터를 저장하는 클래스
/// Inspector에서 편집 가능하도록 Serializable 속성 추가
/// </summary>
/// sex
[System.Serializable]
public class DialogueData
{
    [Header("캐릭터 정보")]
    /// <summary>
    /// 대화하는 캐릭터의 이름
    /// UI 상단에 표시될 텍스트
    /// </summary>
    public string characterName;
    
    [Header("대화 내용")]
    /// <summary>
    /// Inspector에서 여러 줄 텍스트 입력을 위한 TextArea 속성
    /// 최소 3줄, 최대 10줄까지 표시
    /// </summary>
    [TextArea(3, 10)]
    /// <summary>
    /// 순서대로 표시될 대화 문장들의 배열
    /// 각 요소는 하나의 대화 문장을 의미
    /// </summary>
    public string[] sentences;
    
    [Header("캐릭터 초상화 (선택사항)")]
    /// <summary>
    /// 캐릭터의 초상화 이미지 (선택적)
    /// null이면 초상화를 표시하지 않음
    /// </summary>
    public Sprite characterPortrait;
}