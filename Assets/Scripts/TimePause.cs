using System.Collections;
using System.Linq;
using UnityEngine;

public class TimePause : MonoBehaviour
{
    public Animator animator;
    public string animationName;
    private bool isAnimationPlaying = false;
    [SerializeField]
    private Transform player;

    void Update()
    {
        if (isAnimationPlaying)
        {
            // If you need specific updates during pause, use Time.unscaledDeltaTime
        }
    }

    public void PlayAnimationAndPauseTime(CamType camType)
    {
        if (animator != null)
        {
            isAnimationPlaying = true;
            
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            // Pause game time
            Time.timeScale = 0;

            // Play the animation
            animator.Play(camType.ToString());

            // Start a coroutine to wait for the animation to finish
            StartCoroutine(ResumeTimeAfterAnimation());
            
        }
    }

    private IEnumerator ResumeTimeAfterAnimation()
    {
        // Wait for the animation to complete
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        float animationLength = controller.animationClips.First(clip => clip.name == animationName).length + 1;
        StartCoroutine(RotateToTargetCoroutine(player, animationLength));
        // Use unscaled time to track the animation duration
        yield return new WaitForSecondsRealtime(animationLength);

        // Resume game time
        Time.timeScale = 1;
        animator.updateMode = AnimatorUpdateMode.Normal;
        isAnimationPlaying = false;
    }
    
    private IEnumerator RotateToTargetCoroutine(Transform targetObject, float rotationDuration)
    {
        Quaternion initialRotation = targetObject.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, -90);

        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            float t = elapsedTime / rotationDuration;
            targetObject.rotation = Quaternion.Lerp(initialRotation, targetRotation, t);
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time
            yield return null; // Wait for the next frame
        }

        // Ensure final rotation is exactly the target rotation
        targetObject.rotation = targetRotation;
    }
}
