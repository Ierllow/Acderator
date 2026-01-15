using Cysharp.Text;
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

        public void Show(SongInfo songInfo)
        {
            atlas.SetAtlasFormat("{0}", songInfo.Group);
            difficultyImage.SetAtlasFormat("difficulty_{0}", songInfo.Difficulty, "song/difficulty");
            songName.SetTextFormat("{0}", songInfo.Name);
            composer.SetTextFormat("{0}", songInfo.Composer);
        }
    }
}