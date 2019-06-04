using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimatorRendererPair : MonoBehaviour
{
    public Animator animator;
    public Renderer rend;


    void Start()
    {
        // totalWarZoom.Instance.AddAnimatorAndRenderer(animator, rend);
        //AnimatorOptimiser.Instance.AddAnimator(this);
    }

    void OnBecameVisible() 
    {
        AnimatorOptimiser.Instance.AnimatorEntersCameraView(this);
    }

    void OnBecameInvisible()
    {
        AnimatorOptimiser.Instance.AnimatorLeavesCameraView(this);
        animator.enabled = false;
    }
}
