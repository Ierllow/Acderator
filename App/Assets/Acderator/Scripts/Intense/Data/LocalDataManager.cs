using System;
using UnityEngine;

namespace Intense.Data
{
    internal partial class LocalDataManager : SingletonMonoBehaviour<LocalDataManager>
    {
        internal LocalUserData LocalUser { get; } = new();
        internal ConfigData Option { get; } = new();
        internal SongSelectSavaData SelectSavaData { get; } = new();
        internal SongSelectSortData SongSelectSort { get; } = new();

        protected override void Awake()
        {
            base.Awake();
            LocalUser.Load();
            Option.Load();
            SelectSavaData.Load();
            SongSelectSort.Load();
        }

        protected override void OnDestroy()
        {
            LocalUser.Save();
            Option.Save();
            SelectSavaData.Save();
            SongSelectSort.Save();
            base.OnDestroy();
        }

        [Serializable]
        internal abstract class Base
        {
            public bool IsInit { get; protected set; }

            public abstract void Load();
            public virtual void Save() { }
        }

        internal class LocalUserData : Base
        {
            public string UserName { get; set; }
            public string UserId { get; set; }
            public string PassWard { get; set; }

            public override void Load()
            {
                if (IsInit) return;

                UserName = PlayerPrefs.GetString("UserName", "");
                UserId = PlayerPrefs.GetString("UserId", "");
                PassWard = PlayerPrefs.GetString("PassWard", "");

                IsInit = true;
            }

            public override void Save()
            {
                PlayerPrefs.SetString("UserName", UserName);
                PlayerPrefs.SetString("UserId", UserId);
                PlayerPrefs.SetString("PassWard", PassWard);
            }
        }

        internal class ConfigData : Base
        {
            public float BgmVolume { get; set; }
            public float SeVolume { get; set; }
            public float SongVolume { get; set; }
            public bool IsMuteBgm { get; set; }
            public bool IsMuteSe { get; set; }
            public bool IsMuteSong { get; set; }
            public float NoteSpeed { get; set; }
            public float TapTimingNum { get; set; }

            public override void Load()
            {
                if (IsInit) return;

                BgmVolume = PlayerPrefs.GetFloat("bgmVolume", 1f);
                SeVolume = PlayerPrefs.GetFloat("seVolume", 1f);
                SongVolume = PlayerPrefs.GetFloat("songVolume", 1f);
                IsMuteBgm = PlayerPrefs.HasKey("bgmMute") ? PlayerPrefs.GetInt("bgmMute") == 1 : false;
                IsMuteSe = PlayerPrefs.HasKey("seMute") ? PlayerPrefs.GetInt("seMute") == 1 : false;
                IsMuteSong = PlayerPrefs.HasKey("songMute") ? PlayerPrefs.GetInt("songMute") == 1 : false;
                NoteSpeed = PlayerPrefs.GetFloat("noteSpeedConfig", 1f);
                TapTimingNum = PlayerPrefs.GetFloat("tapTimingNum", 0f);

                IsInit = true;
            }

            public override void Save()
            {
                PlayerPrefs.SetFloat("bgmVolume", BgmVolume);
                PlayerPrefs.SetFloat("seVolume", SeVolume);
                PlayerPrefs.SetFloat("songVolume", SongVolume);
                PlayerPrefs.SetInt("bgmMute", IsMuteBgm ? 1 : 0);
                PlayerPrefs.SetInt("seMute", IsMuteSe ? 1 : 0);
                PlayerPrefs.SetInt("songMute", IsMuteSong ? 1 : 0);
                PlayerPrefs.SetFloat("noteSpeedConfig", NoteSpeed);
                PlayerPrefs.SetFloat("tapTimingNum", TapTimingNum);
            }
        }

        internal class SongSelectSavaData : Base
        {
            public int SelectedGroup { get; set; }
            public int SelectedDifficulty { get; set; }

            public override void Load()
            {
                if (IsInit) return;

                SelectedGroup = PlayerPrefs.GetInt("SelectedGroup", 1);
                SelectedDifficulty = PlayerPrefs.GetInt("SelectedDifficulty", 1);

                IsInit = true;
            }

            public override void Save()
            {
                PlayerPrefs.SetInt("SelectedGroup", SelectedGroup);
                PlayerPrefs.SetInt("SelectedDifficulty", SelectedDifficulty);
            }
        }

        internal class SongSelectSortData : Base
        {
            public int OrderType { get; set; }

            public override void Load()
            {
                if (IsInit) return;

                OrderType = PlayerPrefs.GetInt("OrderType", 0);
                IsInit = true;
            }

            public void Save(int orderType)
            {
                OrderType = orderType;
                Save();
            }

            public override void Save() => PlayerPrefs.SetInt("OrderType", OrderType);
        }
    }
}