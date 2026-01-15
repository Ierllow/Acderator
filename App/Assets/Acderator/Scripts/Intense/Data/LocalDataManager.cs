using System;

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

                UserName = SaveDataManager.Instance.GetString("UserName", "");
                UserId = SaveDataManager.Instance.GetString("UserId", "");
                PassWard = SaveDataManager.Instance.GetString("PassWard", "");

                IsInit = true;
            }

            public override void Save()
            {
                SaveDataManager.Instance.SetString("UserName", UserName);
                SaveDataManager.Instance.SetString("UserId", UserId);
                SaveDataManager.Instance.SetString("PassWard", PassWard);
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

                BgmVolume = SaveDataManager.Instance.GetFloat("bgmVolume", 1f);
                SeVolume = SaveDataManager.Instance.GetFloat("seVolume", 1f);
                SongVolume = SaveDataManager.Instance.GetFloat("songVolume", 1f);
                IsMuteBgm = SaveDataManager.Instance.GetBool("bgmMute");
                IsMuteSe = SaveDataManager.Instance.GetBool("seMute");
                IsMuteSong = SaveDataManager.Instance.GetBool("songMute");
                NoteSpeed = SaveDataManager.Instance.GetFloat("noteSpeedConfig", 1f);
                TapTimingNum = SaveDataManager.Instance.GetFloat("tapTimingNum", 0f);

                IsInit = true;
            }

            public override void Save()
            {
                SaveDataManager.Instance.SetFloat("bgmVolume", BgmVolume);
                SaveDataManager.Instance.SetFloat("seVolume", SeVolume);
                SaveDataManager.Instance.SetFloat("songVolume", SongVolume);
                SaveDataManager.Instance.SetBool("bgmMute", IsMuteBgm);
                SaveDataManager.Instance.SetBool("seMute", IsMuteSe);
                SaveDataManager.Instance.SetBool("songMute", IsMuteSong);
                SaveDataManager.Instance.SetFloat("noteSpeedConfig", NoteSpeed);
                SaveDataManager.Instance.SetFloat("tapTimingNum", TapTimingNum);
            }
        }

        internal class SongSelectSavaData : Base
        {
            public int SelectedGroup { get; set; }
            public int SelectedDifficulty { get; set; }

            public override void Load()
            {
                if (IsInit) return;

                SelectedGroup = SaveDataManager.Instance.GetInt("SelectedGroup", 1);
                SelectedDifficulty = SaveDataManager.Instance.GetInt("SelectedDifficulty", 1);

                IsInit = true;
            }

            public override void Save()
            {
                SaveDataManager.Instance.SetInt("SelectedGroup", SelectedGroup);
                SaveDataManager.Instance.SetInt("SelectedDifficulty", SelectedDifficulty);
            }
        }

        internal class SongSelectSortData : Base
        {
            public int OrderType { get; set; }

            public override void Load()
            {
                if (IsInit) return;

                OrderType = SaveDataManager.Instance.GetInt("OrderType", 0);
                IsInit = true;
            }

            public void Save(int orderType)
            {
                OrderType = orderType;
                Save();
            }

            public override void Save() => SaveDataManager.Instance.SetInt("OrderType", OrderType);
        }
    }
}