using Cysharp.Threading.Tasks;
using Intense.Api;
using Intense.Master;
using Intense.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;
using ZLinq;

namespace Intense.Data
{
    internal class ScoreManager : MonoBehaviour
    {
        [Inject] private NetworkManager networkManager;
        [Inject] private Loading loading;

        public bool IsInit { get; private set; }

        public List<ScoreData> ScoreDataList { get; private set; } = new();

        public void SetScoreData(Dictionary<string, object> dataDict)
        {
            if (IsInit) return;
            foreach (var (key, value) in dataDict)
            {
                var sid = int.Parse(key);
                var score = int.Parse(value.ToString());
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
                ScoreDataList.Add(new() { Sid = sid, ScoreNum = scoreNum });
            }
        }

        public async UniTask<bool> RequestUpdateScoreAsync(string sessionId, int score)
        {
            var request = new ScoreSubmitRequest();
            request.PostData.Add("session_id", sessionId);
            request.PostData.Add("sid", score);
            var response = await networkManager.RequestAsync(request);
            return response.Status == 200;
        }

        public int GetScore(int sid) => ScoreDataList.AsValueEnumerable().FirstOrDefault(x => x.Sid == sid)?.ScoreNum ?? 0;

#if UNITY_EDITOR
        public async UniTask<string> RequestChart(string url)
        {
            loading.ShowLoading();
            // TODO
            var baseUrl = Application.platform.EnumEquals(RuntimePlatform.IPhonePlayer) ? "iOS_Japanese" : "Android_Japanese";
            using var www = UnityWebRequest.Get(baseUrl + url);
            await www.SendWebRequest();
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError("www Error:" + www.error);
                return "";
            }
            loading.HideLoading();
            return www.downloadHandler.text;
        }
#endif
    }
}
