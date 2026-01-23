using Cysharp.Threading.Tasks;
using Element.UI;
using Intense.UI;
using System;

namespace Intense.Asset
{
    public static class AssetBundleErrorKindExtensions
    {
        public static string GetErrorMessage(this EAssetBundleErrorKind kind) => kind switch
        {
            EAssetBundleErrorKind.Error or EAssetBundleErrorKind.NotFoundManifest => "エラーが発生しました。",
            EAssetBundleErrorKind.ConnectionError => "通信環境をご確認ください。",
            EAssetBundleErrorKind.ProtocolError => "通信に失敗しました。",
            _ => "",
        };
    }
}