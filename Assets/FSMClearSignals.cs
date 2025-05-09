using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMClearSignals : StateMachineBehaviour
{
    public AudioClip soundClip;
    //清理进入动画的触发信号
    public string[] clearAtEnter;
    //清理退出动画的触发信号
    public string[] clearAtExit;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateinfo, int layerindex)
    {
        foreach (string signal in clearAtEnter)
        {
            animator.ResetTrigger(signal);
        }
        animator.gameObject.GetComponent<AudioSource>().clip = soundClip;
        animator.gameObject.GetComponent<AudioSource>().Play();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //for (int i = 0; i < clearAtExit.Length; i++)
        //{
        //    animator.ResetTrigger(clearAtExit[i]);
        //}
        foreach (string signal in clearAtExit)
        {
            animator.ResetTrigger(signal);
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
