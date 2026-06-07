#nullable enable

using Intense.UI;
using TMPro;
using UnityEngine;

namespace Song
{
    public class SongIntroView : MonoBehaviour
    {
        [SerializeField] private AtlasImage atlas = default!;
        [SerializeField] private AtlasImage difficultyImage = default!;
        [SerializeField] private TextMeshProUGUI songName = default!;
        [SerializeField] private TextMeshProUGUI composer = default!;

        public void Show(SongInfo songInfo, Sprite jacket, Sprite difficulty)
        {
            atlas.SetSprite(jacket);
            difficultyImage.SetSprite(difficulty);
            songName.SetText(songInfo.Name);
            composer.SetText(songInfo.Composer);
        }
    }
}
