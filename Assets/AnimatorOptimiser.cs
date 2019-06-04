using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AnimatorOptimiser : MonoBehaviour
{
    /*
     * all our animators should be set up to cull update transform
     * if there are to many aniamtions on screen we play the animations in a smaller framerate
     * workaround for adjusting framerate: enable and disable animators every few seconds
     * we adjust the framerate by seperating all animators in groups - the more groups - the better the performance
     * example: 4 groups, so every group gets updated 15 times per second ->60/4
     */

    [SerializeField]
    [Tooltip("if the number of animations on screen is bigger than this, we start optimising - 50 should be good for 0.5ms, could also be named animatorsPerGroup, maximal number of animators enabled per frame")]
    int animatorsOnScreenThreshold;

    [SerializeField]
    [Tooltip("we seperate the animators in smaller groups and update them after each other")]
    int animatorGroups;


    int animatorGroupsLastFrame; //use to check if we need to change the list

    int groupTraverser; //our index fro our animationGroupsArray, if we change our groups, we always put a modulo on this index - so itr is possible that some animations get played 2 framesin a row

    [SerializeField]
    int animatorsOnScreen = 0;
    int animatorsOnScreenLastFrame;



    List<AnimatorRendererPair> visibleAnimators = new List<AnimatorRendererPair>();
    HashSet<AnimatorRendererPair> visibleAnimatorsSet = new HashSet<AnimatorRendererPair>();
    //List<AnimatorRendererPair> allAnimators = new List<AnimatorRendererPair>(); 


    #region singletonCode

    public static AnimatorOptimiser Instance;

    public void Awake() // wir setzen sicher dass es immer existier aber immer nur eins
    {
        if (Instance != null)
        {
            DestroyImmediate(Instance); // es kann passieren wenn wir eine neue Scene laden dass immer noch eine Instanz existiert
        }
        else
        {
            Instance = this;
        }


        animatorGroups = 1;
        animatorGroupsLastFrame = 1;
    }

    #endregion

    /*public void AddAnimator(AnimatorRendererPair animRenderPair)
    {
        allAnimators.Add(animRenderPair);
    }*/

    public void AnimatorEntersCameraView(AnimatorRendererPair animRenderPair)
    {
        animatorsOnScreen++;
        visibleAnimatorsSet.Add(animRenderPair);
    }

    public void AnimatorLeavesCameraView(AnimatorRendererPair animRenderPair)
    {
        animatorsOnScreen--;
        visibleAnimatorsSet.Remove(animRenderPair);
    }



    void Update()
    {
        visibleAnimators = visibleAnimatorsSet.ToList();

        #region animator optimisation
        //if therer are too many animators on screen -> optimise
        if (animatorsOnScreen > animatorsOnScreenThreshold)
        {
            //1.disable the animatorsy enabled last frame
            int from = groupTraverser * animatorsOnScreenThreshold;
            int to = from + Mathf.Clamp(animatorsOnScreen - groupTraverser * animatorsOnScreenThreshold, 0, animatorsOnScreenThreshold);

            //Debug.Log(" Before:: group: id" + groupTraverser + " Raqnge: " + from + ", " + to);

            for (int i = from; i < to; i++)
            {
                visibleAnimators[i].animator.enabled = false;
            }


            //2.calculate the current groups size

            animatorGroups = Mathf.CeilToInt(animatorsOnScreen / (float)animatorsOnScreenThreshold);

            

            //3. enable the next ones according to current group size and animators on screen

            groupTraverser++;
            groupTraverser = groupTraverser % animatorGroups;

            //Debug.Log("group: id" + groupTraverser + " Raqnge: " + groupTraverser * animatorsOnScreenThreshold + ", " + Mathf.Clamp(visibleAnimators.Count - groupTraverser * animatorsOnScreenThreshold, 0, animatorsOnScreenThreshold));

            from = groupTraverser * animatorsOnScreenThreshold;
            to = from  + Mathf.Clamp(animatorsOnScreen - groupTraverser * animatorsOnScreenThreshold, 0, animatorsOnScreenThreshold);

            //Debug.Log(" After:: group: id" + groupTraverser + " Raqnge: "+ from + ", " + to);

            for (int i = from; i < to; i++)
            {
                 visibleAnimators[i].animator.speed = animatorGroups;
                 visibleAnimators[i].animator.enabled = true;
            }


            //prepare variables for next frame
            animatorGroupsLastFrame = animatorGroups;
            animatorsOnScreenLastFrame = animatorsOnScreen;
        }
        else
        {
            //if there are few animators on screen - reset everything
            animatorGroups = 1;
            for (int i = 0; i < visibleAnimators.Count; i++)
            {
                visibleAnimators[i].animator.speed = 1;
                visibleAnimators[i].animator.enabled = true;
            }
        }
        #endregion

      
    }
}
