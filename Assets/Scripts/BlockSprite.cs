using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSprite : MonoBehaviour
{
    [SerializeField]
    Transform pivot;

    [SerializeField]
    private SpriteRenderer borderSprite;

    [SerializeField]
    private SpriteRenderer fillSprite;

    Coroutine animationRoutine;

    public const float AnimationTransformScale = 1.5f;

    public bool isPlayingAnimation => (animationRoutine != null);

    public Color fillColor
    {
        get => fillSprite.color;
        set => fillSprite.color = value;
    }

    public void PlayAnimation(float duration, bool loop = false)
    {
        StopAnimation();
        animationRoutine = StartCoroutine(AnimationRoutine(duration, AnimationTransformScale, loop));
    }

    public void StopAnimation()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }
        pivot.localScale = Vector3.one;
    }

    IEnumerator AnimationRoutine(float duration, float transformScale = 1.2f, bool loop = false)
    {
        do
        {
            float ratio = 0f;
            while (ratio < 1f)
            {
                ratio += Time.deltaTime / duration;
                ratio = Mathf.Clamp(ratio, 0f, 1f);
                float scale = Mathf.Lerp(1f, transformScale, Mathf.Sin(ratio * Mathf.PI));
                pivot.localScale = Vector3.one * scale;
                yield return new WaitForEndOfFrame();
            }
        } while (loop);
        StopAnimation();
    }
}
