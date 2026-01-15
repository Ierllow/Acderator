using Cysharp.Text;
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

        public SongMaster MSong { get; private set; }

        public void Setup(SongMaster mSong, Action<SongSelectCell> selected)
        {
            this.selected = selected;
            MSong = mSong;
            SetSelectedCell(true);
            songName.SetTextFormat("{0}", mSong.Name);
            composer.SetTextFormat("{0}", mSong.Composer);
        }

        public void SetSelectedCell(bool value) => bg.SetAtlas(value ? "song_selected" : "song_not_selected");

        public void OnTapCell() => selected?.Invoke(this);
    }
}