using Intense.UI;
using TMPro;
using UnityEngine;

namespace Song
{
    public class SongIntroView : MonoBehaviour
    {
        [SerializeField] private AtlasImage atlas;
        [SerializeField] private AtlasImage difficultyImage;
        [SerializeField] private TextMeshProUGUI songName;
        [SerializeField] private TextMeshProUGUI composer;

        public void Show(SongInfo songInfo, Sprite jacket, Sprite difficulty)
        {
            atlas.SetSprite(jacket);
            difficultyImage.SetSprite(difficulty);
            songName.SetText(songInfo.Name);
            composer.SetText(songInfo.Composer);
        }
    }
}