using UnityEngine;
using UnityEngine.Serialization;

namespace Intense.Api
{
    public class NetworkConfig : ScriptableObject
    {
        [SerializeField] private string apiServerUrl = "";
        [SerializeField] private string webViewServerUrl = "";

        public string ApiServerUrl => apiServerUrl;
        public string WebViewServerUrl => webViewServerUrl;
    }
}