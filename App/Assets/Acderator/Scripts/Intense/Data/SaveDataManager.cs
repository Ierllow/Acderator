using UnityEngine;

namespace Intense.Data
{
    internal class SaveDataManager : SingletonMonoBehaviour<SaveDataManager>
    {
        public float GetFloat(string key, float defaultValue = 0f) => PlayerPrefs.GetFloat(key, defaultValue);
        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
        public int GetInt(string key, int defaultValue = 0) => PlayerPrefs.GetInt(key, defaultValue);
        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
        public string GetString(string key, string defaultValue = "") => PlayerPrefs.GetString(key, defaultValue);
        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);
        public bool GetBool(string key, bool defaultValue = false) => GetInt(key, defaultValue ? 1 : 0) != 0;
        public void SetBool(string key, bool value) => SetInt(key, value ? 1 : 0);
        public void DeleteAll() => PlayerPrefs.DeleteAll();
        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
        public void Save() => PlayerPrefs.Save();
        public bool HasKey(string key) => PlayerPrefs.HasKey(key);
    }
}