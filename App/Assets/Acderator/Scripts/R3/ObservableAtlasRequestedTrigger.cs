using System;
using UnityEngine;
using UnityEngine.U2D;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservableAtlasRequestedTrigger : ObservableTriggerBase
    {
        private Subject<(string, Action<SpriteAtlas>)> atlasRequested;

        private void Start() => SpriteAtlasManager.atlasRequested += AtlasRequested;

        private void AtlasRequested(string _, Action<SpriteAtlas> __) => atlasRequested?.OnNext((_, __));

        public Observable<(string, Action<SpriteAtlas>)> OnAtlasRequestedAsObservable() => atlasRequested ??= new Subject<(string, Action<SpriteAtlas>)>();

        protected override void RaiseOnCompletedOnDestroy()
        {
            SpriteAtlasManager.atlasRequested -= AtlasRequested;
            atlasRequested?.OnCompleted();
        }
    }
}