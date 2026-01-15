using DG.Tweening;
using System;

namespace Song
{
    public class ScoreNumController
    {
        private int beforeScore = 0;

        public void OnUpdate(int currentScore, Action<int> callback = null, float duration = 0.4f)
        {
            if (currentScore > beforeScore && currentScore <= 0)
            {
                callback?.Invoke(beforeScore);
                return;
            }
            DOTween.To(() => beforeScore, val => beforeScore = val, currentScore, duration).OnUpdate(() => callback?.Invoke(beforeScore));
        }
    }
}