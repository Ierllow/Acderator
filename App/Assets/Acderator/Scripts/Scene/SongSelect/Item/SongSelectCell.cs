using EnhancedUI.EnhancedScroller;
using Intense.Master;
using Intense.UI;
using System;
using TMPro;
using UnityEngine;

namespace SongSelect
{
    public class SongSelectCell : EnhancedScrollerCellView
    {
        [SerializeField] private AtlasImage bg;
        [SerializeField] private TextMeshProUGUI songName;
        [SerializeField] private TextMeshProUGUI composer;

        public Action<SongSelectCell> selected;

        private Func<string, Sprite> spriteResolver;

        public SongMaster MSong { get; private set; }

        public void Setup(SongMaster mSong, Action<SongSelectCell> selected, Func<string, Sprite> spriteResolver)
        {
            this.selected = selected;
            this.spriteResolver = spriteResolver;
            MSong = mSong;
            SetSelectedCell(true);
            songName.SetText(mSong.Name);
            composer.SetText(mSong.Composer);
        }

        public void SetSelectedCell(bool value)
        {
            bg.SetAtlas(value ? "song_selected" : "song_not_selected");
            bg.ResolveSprite(spriteResolver);
        }

        public void OnTapCell() => selected?.Invoke(this);
    }
}