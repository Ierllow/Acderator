using Cysharp.Threading.Tasks;
using Intense.Api;
using Intense.Master;
using Intense.UI;
using PlayFab.ClientModels;
using PlayFab.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ZLinq;

namespace Intense.Data
{
    internal class ScoreManager : SingletonMonoBehaviour<ScoreManager>
    {
        public bool IsInit { get; private set; }

        public List<ScoreData> ScoreDataList { get; private set; } = new();

        public void SetScoreData(IDictionary<string, UserDataRecord> dataDict = default)
        {
            if (IsInit) return;
            if (dataDict == default)
            {
                IsInit = true;
                return;
            }

            var deserializeDataDict = (dataDict.Values as object[]).AsValueEnumerable().Select(x => PlayFabSimpleJson.DeserializeObject(x.ToString()) as IDictionary<string, object>).ToList();
            var value = default(object);
            foreach (var deserializeData in deserializeDataDict)
            {
                var sid = 0;
                var score = 0;
                if (deserializeData.TryGetValue("sid", out value)) sid = int.Parse(value.ToString());
                if (deserializeData.TryGetValue("score", out value)) score = int.Parse(value.ToString());
                UpdateScoreData(sid, score);
            }
            IsInit = true;
        }

        public void UpdateScoreData(int sid, int scoreNum)
        {
            var target = ScoreDataList.AsValueEnumerable().FirstOrDefault(x => x.Sid == sid);
            if (target != default)
            {
                if (!IsInit) return;
                if (target.ScoreNum >= scoreNum) return;

                target.ScoreNum = scoreNum;
            }
            else
            {
                ScoreDataList.Add(new ScoreData(sid, scoreNum));
            }
        }

        public async UniTask<bool> RequestUpdateScoreAsync(int sid, int score)
        {
            var request = new UpdateScoreRequest();
            request.AddData("sid", sid);
            request.AddData("score", score);
            var response = await NetworkManager.Instance.RequestAsync(request);
            return response?.Error == null;
        }

        public int GetScore(int sid)
        {
            return ScoreDataList.AsValueEnumerable().FirstOrDefault(x => x.Sid == sid)?.ScoreNum ?? 0;
        }

#if UNITY_EDITOR
        public async UniTask<string> RequestChart(string url)
        {
            Loading.Instance.ShowLoading();
            // TODO
            var baseUrl = Application.platform.EnumEquals(RuntimePlatform.IPhonePlayer) ? "iOS_Japanese" : "Android_Japanese";
            using var www = UnityWebRequest.Get(baseUrl + url);
            await www.SendWebRequest();
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError("www Error:" + www.error);
                return "";
            }
            Loading.Instance.HideLoading();
            return www.downloadHandler.text;
        }
#endif
    }
}