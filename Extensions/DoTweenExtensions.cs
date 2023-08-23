using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class DoTweenExtensions
{
    public static Tween DoFade(this List<SpriteRenderer> sprites, float endValue, float duration, Ease ease = Ease.Unset)
    {
        var sequence = DOTween.Sequence();
        
        sprites.ForEach(sprite => sequence.Insert(0f, 
            sprite.DOFade(endValue, duration)
                .SetEase(ease)));

        return sequence;
    }
}
