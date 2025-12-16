using Unity.VisualScripting;
using UnityEngine;

    public enum EFState {normal, run}
    public enum AndyState {normal, run}
    public enum Bot1State {normal, walk, target, dead}
    public enum Bot2State {normal, walk, target, dead}
public class AnimationTesting : MonoBehaviour
{
    [Header("ㄷㄹ")]
    public Animator EFanim;
    public EFState EFstate;
    [Header("앤디")]
    public Animator Andyanim;
    public AndyState Andystate;
    [Header("앤디봇1")]
    public Animator Bot1anim;
    public Bot1State Bot1state;
    [Header("앤디봇2")]
    public Animator Bot2anim;
    public Bot2State Bot2state;
    

    void Update()
    {
        EFanim.SetInteger("State", (int)EFstate);
        Andyanim.SetInteger("State", (int)Andystate);
        Bot1anim.SetInteger("State", (int)Bot1state);
        Bot2anim.SetInteger("State", (int)Bot2state);
    }
}
