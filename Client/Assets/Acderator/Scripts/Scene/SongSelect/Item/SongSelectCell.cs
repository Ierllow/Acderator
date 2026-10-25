using EnhancedUI.EnhancedScroller;
using Intense.Master;
using Intense.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace SongSelect
{
    public class SongSelectCell : EnhancedScrollerCellView
    {
        [FormerlySerializedAs("bg"), SerializeField] private AtlasImage background;
        [SerializeField] private TextMeshProUGUI songName;
        [SerializeField] private TextMeshProUGUI composer;

        private Action<SongSelectCell> onSelected;
        private Func<string, Sprite> spriteResolver;

        public SongMaster Song { get; private set; }

        public void Setup(SongMaster song, Action<SongSelectCell> onSelected, Func<string, Sprite> spriteResolver)
        {
            this.onSelected = onSelected;
            this.spriteResolver = spriteResolver;
            Song = song;
            SetSelectedCell(true);
            songName.SetText(song.Name);
            composer.SetText(song.Composer);
        }

        public void SetSelectedCell(bool value)
        {
            background.SetAtlas(value ? "song_selected" : "song_not_selected");
            background.ResolveSprite(spriteResolver);
        }

        public void OnTapCell() => onSelected?.Invoke(this);
    }
}