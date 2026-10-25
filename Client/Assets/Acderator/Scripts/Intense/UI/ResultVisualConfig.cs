using UnityEngine;

namespace Intense.UI
{
    public class ResultVisualConfig : ScriptableObject
    {
        [Header("Song result telop gradients")]
        [SerializeField] private Gradient excellentGradient = new();
        [SerializeField] private Gradient clearGradient = new();
        [SerializeField] private Gradient failedGradient = new();

        [Header("Result scene background colors")]
        [SerializeField] private Color clearBackgroundColor = Color.white;
        [SerializeField] private Color failedBackgroundColor = new(1f, 1f, 1f, 0.4627451f);

        public Gradient ExcellentGradient => excellentGradient;
        public Gradient ClearGradient => clearGradient;
        public Gradient FailedGradient => failedGradient;
        public Color ClearBackgroundColor => clearBackgroundColor;
        public Color FailedBackgroundColor => failedBackgroundColor;
    }
}