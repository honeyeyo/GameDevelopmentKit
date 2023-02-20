using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;
using TaskStatus = GameFramework.TaskStatus;

namespace UnityGameFramework.Extension
{
    public static partial class Awaitable
    {
#if UNITY_EDITOR
        private static bool s_IsSubscribeEvent = false;
#endif

        /// <summary>
        /// 注册需要的事件 (需再流程入口处调用 防止框架重启导致事件被取消问题)
        /// </summary>
        public static void SubscribeEvent()
        {
            EventComponent eventComponent = GameEntry.GetComponent<EventComponent>();
            eventComponent.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            eventComponent.Subscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);

            eventComponent.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            eventComponent.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);

            eventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            eventComponent.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            eventComponent.Subscribe(UnloadSceneSuccessEventArgs.EventId, OnUnloadSceneSuccess);
            eventComponent.Subscribe(UnloadSceneFailureEventArgs.EventId, OnUnloadSceneFailure);

            eventComponent.Subscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
            eventComponent.Subscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

            eventComponent.Subscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            eventComponent.Subscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);
#if UNITY_EDITOR
            s_IsSubscribeEvent = true;
#endif
        }

#if UNITY_EDITOR
        private static void TipsSubscribeEvent()
        {
            if (!s_IsSubscribeEvent)
            {
                throw new Exception("Use await/async extensions must to subscribe event!");
            }
        }
#endif

        /// <summary>
        /// 打开界面（可等待）
        /// </summary>
        public static UniTask<UIForm> OpenUIFormAsync(this UIComponent uiComponent, string uiFormAssetName, string uiGroupName,
        int priority, bool pauseCoveredUIForm, object userData = default, CancellationTokenSource cancellationTokenSource = default)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tcs = AutoResetUniTaskCompletionSource<UIForm>.Create();
            int serialId = uiComponent.OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, AwaitDataWrap<UIForm>.Create(userData, tcs));
            if (cancellationTokenSource != default)
            {
                cancellationTokenSource.Token.Register(() =>
                {
                    if (uiComponent.IsLoadingUIForm(serialId))
                    {
                        uiComponent.CloseUIForm(serialId);
                        tcs.TrySetCanceled(cancellationTokenSource.Token);
                    }
                });
            }
            return tcs.Task;
        }

        private static void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
            if(ne.UserData is AwaitDataWrap<UIForm> awaitDataWrap)
            {
                var taskCompletionSource = awaitDataWrap.TaskCompletionSource;
                ReferencePool.Release(awaitDataWrap);
                taskCompletionSource.TrySetResult(ne.UIForm);
            }
        }

        private static void OnOpenUIFormFailure(object sender, GameEventArgs e)
        {
            OpenUIFormFailureEventArgs ne = (OpenUIFormFailureEventArgs)e;
            if (ne.UserData is AwaitDataWrap<UIForm> awaitDataWrap)
            {
                Log.Error(ne.ErrorMessage);
                var taskCompletionSource = awaitDataWrap.TaskCompletionSource;
                ReferencePool.Release(awaitDataWrap);
                taskCompletionSource.TrySetException(new GameFrameworkException(ne.ErrorMessage));
            }
        }

        /// <summary>
        /// 显示实体（可等待）
        /// </summary>
        public static UniTask<Entity> ShowEntityAsync(this EntityComponent entityComponent, int entityId, Type entityLogicType, string entityAssetName,
        string entityGroupName, int priority = 0, object userData = default, CancellationTokenSource cancellationTokenSource = default)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tcs = AutoResetUniTaskCompletionSource<Entity>.Create();
            entityComponent.ShowEntity(entityId, entityLogicType, entityAssetName, entityGroupName, priority, AwaitDataWrap<Entity>.Create(userData, tcs));
            if (cancellationTokenSource != default)
            {
                cancellationTokenSource.Token.Register(() =>
                {
                    if (entityComponent.IsLoadingEntity(entityId))
                    {
                        entityComponent.HideEntity(entityId);
                        tcs.TrySetCanceled(cancellationTokenSource.Token);
                    }
                });
            }
            return tcs.Task;
        }

        private static void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
            if (ne.UserData is AwaitDataWrap<Entity> awaitDataWrap)
            {
                var taskCompletionSource = awaitDataWrap.TaskCompletionSource;
                ReferencePool.Release(awaitDataWrap);
                taskCompletionSource.TrySetResult(ne.Entity);
            }
        }

        private static void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            ShowEntityFailureEventArgs ne = (ShowEntityFailureEventArgs)e;
            if (ne.UserData is AwaitDataWrap<Entity> awaitDataWrap)
            {
                var taskCompletionSource = awaitDataWrap.TaskCompletionSource;
                ReferencePool.Release(awaitDataWrap);
                taskCompletionSource.TrySetException(new GameFrameworkException(ne.ErrorMessage));
            }
        }

        /// <summary>
        /// 加载场景（可等待）
        /// </summary>
        public static UniTask LoadSceneAsync(this SceneComponent sceneComponent, string sceneAssetName, int priority = 0 , object userData = default)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tcs = AutoResetUniTaskCompletionSource.Create();
            try
            {
                sceneComponent.LoadScene(sceneAssetName, priority, AwaitDataWrap.Create(userData, tcs));
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                tcs.TrySetException(e);
            }
            return tcs.Task;
        }

        private static void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            LoadSceneSuccessEventArgs ne = (LoadSceneSuccessEventArgs)e;
            if (ne.UserData is AwaitDataWrap awaitDataWrap)
            {
                var taskCompletionSource = awaitDataWrap.TaskCompletionSource;
                ReferencePool.Release(awaitDataWrap);
                taskCompletionSource.TrySetResult();
            }
        }

        private static void OnLoadSceneFailure(object sender, GameEventArgs e)
        {
            LoadSceneFailureEventArgs ne = (LoadSceneFailureEventArgs)e;
            if (ne.UserData is AwaitDataWrap awaitDataWrap)
            {
                var taskCompletionSource = awaitDataWrap.TaskCompletionSource;
                ReferencePool.Release(awaitDataWrap);
                Log.Error(ne.ErrorMessage);
                taskCompletionSource.TrySetException(new GameFrameworkException(ne.ErrorMessage));
            }
        }

        /// <summary>
        /// 卸载场景（可等待）
        /// </summary>
        public static UniTask UnLoadSceneAsync(this SceneComponent sceneComponent, string sceneAssetName, object userData = default)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tcs = AutoResetUniTaskCompletionSource.Create();
            try
            {
                sceneComponent.UnloadScene(sceneAssetName, AwaitDataWrap.Create(userData, tcs));
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                tcs.TrySetException(e);
            }
            return tcs.Task;
        }

        private static void OnUnloadSceneSuccess(object sender, GameEventArgs e)
        {
            UnloadSceneSuccessEventArgs ne = (UnloadSceneSuccessEventArgs)e;
            if (ne.UserData is AwaitDataWrap awaitDataWrap)
            {
                var taskCompletionSource = awaitDataWrap.TaskCompletionSource;
                ReferencePool.Release(awaitDataWrap);
                taskCompletionSource.TrySetResult();
            }
        }

        private static void OnUnloadSceneFailure(object sender, GameEventArgs e)
        {
            UnloadSceneFailureEventArgs ne = (UnloadSceneFailureEventArgs)e;
            if (ne.UserData is AwaitDataWrap awaitDataWrap)
            {
                var taskCompletionSource = awaitDataWrap.TaskCompletionSource;
                ReferencePool.Release(awaitDataWrap);
                Log.Error($"Unload scene {ne.SceneAssetName} failure.");
                taskCompletionSource.TrySetException(new GameFrameworkException($"Unload scene {ne.SceneAssetName} failure."));
            }
        }

        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static UniTask<T> LoadAssetAsync<T>(this ResourceComponent resourceComponent, string assetName, int priority = 0,
        object userData = default, CancellationTokenSource cancellationTokenSource = default) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var loadAssetTcs = AutoResetUniTaskCompletionSource<T>.Create();
            resourceComponent.LoadAsset(assetName, typeof (T), priority,
                new LoadAssetCallbacks((tempAssetName, asset, duration, userdata) =>
                    {
                        var source = loadAssetTcs;
                        loadAssetTcs = null;
                        T tAsset = asset as T;
                        if (tAsset != null)
                        {
                            if (cancellationTokenSource is { IsCancellationRequested: true })
                            {
                                resourceComponent.UnloadAsset(tAsset);
                                source.TrySetCanceled(cancellationTokenSource.Token);
                            }
                            else
                            {
                                source.TrySetResult(tAsset);
                            }
                        }
                        else
                        {
                            Log.Error($"Load asset failure load type is {asset.GetType()} but asset type is {typeof (T)}.");
                            source.TrySetException(
                                new GameFrameworkException($"Load asset failure load type is {asset.GetType()} but asset type is {typeof (T)}."));
                        }
                    },
                    (tempAssetName, status, errorMessage, userdata) =>
                    {
                        Log.Error(errorMessage);
                        loadAssetTcs.TrySetException(new GameFrameworkException(errorMessage));
                    }),
                userData);
            return loadAssetTcs.Task;
        }

        /// <summary>
        /// 增加Web请求任务（可等待）
        /// </summary>
        public static UniTask<WebResult> WebRequestAsync(this WebRequestComponent webRequestComponent, string webRequestUri,
        WWWForm wwwForm = null, string tag = null, int priority = 0, object userData = default, CancellationTokenSource cancellationTokenSource = default)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tcs = AutoResetUniTaskCompletionSource<WebResult>.Create();
            int serialId = webRequestComponent.AddWebRequest(webRequestUri, wwwForm, tag, priority, AwaitDataWrap<WebResult>.Create(userData, tcs));
            if (cancellationTokenSource != default)
            {
                cancellationTokenSource.Token.Register(() =>
                {
                    var taskInfo = webRequestComponent.GetWebRequestInfo(serialId);
                    if (taskInfo.Status != TaskStatus.Done)
                    {
                        webRequestComponent.RemoveWebRequest(serialId);
                        tcs.TrySetCanceled(cancellationTokenSource.Token);
                    }
                });
            }
            return tcs.Task;
        }

        /// <summary>
        /// 增加Web请求任务（可等待）
        /// </summary>
        public static UniTask<WebResult> WebRequestAsync(this WebRequestComponent webRequestComponent, string webRequestUri, byte[] postData,
        string tag = null, int priority = 0, object userData = default, CancellationTokenSource cancellationTokenSource = default)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tcs = AutoResetUniTaskCompletionSource<WebResult>.Create();
            int serialId = webRequestComponent.AddWebRequest(webRequestUri, postData, tag, priority, AwaitDataWrap<WebResult>.Create(userData, tcs));
            if (cancellationTokenSource != default)
            {
                cancellationTokenSource.Token.Register(() =>
                {
                    var taskInfo = webRequestComponent.GetWebRequestInfo(serialId);
                    if (taskInfo.Status != TaskStatus.Done)
                    {
                        webRequestComponent.RemoveWebRequest(serialId);
                        tcs.TrySetCanceled(cancellationTokenSource.Token);
                    }
                });
            }
            return tcs.Task;
        }

        private static void OnWebRequestSuccess(object sender, GameEventArgs e)
        {
            WebRequestSuccessEventArgs ne = (WebRequestSuccessEventArgs)e;
            if (ne.UserData is AwaitDataWrap<WebResult> awaitDataWrap)
            {
                var taskCompletionSource = awaitDataWrap.TaskCompletionSource;
                WebResult result = WebResult.Create(ne.GetWebResponseBytes(), false, string.Empty, awaitDataWrap.UserData);
                ReferencePool.Release(awaitDataWrap);
                taskCompletionSource.TrySetResult(result);
            }
        }

        private static void OnWebRequestFailure(object sender, GameEventArgs e)
        {
            WebRequestFailureEventArgs ne = (WebRequestFailureEventArgs)e;
            if (ne.UserData is AwaitDataWrap<WebResult> awaitDataWrap)
            {
                var taskCompletionSource = awaitDataWrap.TaskCompletionSource;
                WebResult result = WebResult.Create(null, true, ne.ErrorMessage, awaitDataWrap.UserData);
                ReferencePool.Release(awaitDataWrap);
                taskCompletionSource.TrySetResult(result);
            }
        }

        /// <summary>
        /// 增加下载任务（可等待)
        /// </summary>
        public static UniTask<DownloadResult> AddDownloadAsync(this DownloadComponent downloadComponent,
        string downloadPath,string downloadUri, string tag = null, int priority = 0, object userdata = null)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tcs = AutoResetUniTaskCompletionSource<DownloadResult>.Create();
            downloadComponent.AddDownload(downloadPath, downloadUri, tag, priority, AwaitDataWrap<DownloadResult>.Create(userdata, tcs));
            return tcs.Task;
        }

        private static void OnDownloadSuccess(object sender, GameEventArgs e)
        {
            DownloadSuccessEventArgs ne = (DownloadSuccessEventArgs)e;
            if (ne.UserData is AwaitDataWrap<DownloadResult> awaitDataWrap)
            {
                DownloadResult result = DownloadResult.Create(false, string.Empty, awaitDataWrap.UserData);
                var tcs = awaitDataWrap.TaskCompletionSource;
                ReferencePool.Release(awaitDataWrap);
                tcs.TrySetResult(result);
            }
        }

        private static void OnDownloadFailure(object sender, GameEventArgs e)
        {
            DownloadFailureEventArgs ne = (DownloadFailureEventArgs)e;
            if (ne.UserData is AwaitDataWrap<DownloadResult> awaitDataWrap)
            {
                DownloadResult result = DownloadResult.Create(true, ne.ErrorMessage, awaitDataWrap.UserData);
                var tcs = awaitDataWrap.TaskCompletionSource;
                ReferencePool.Release(awaitDataWrap);
                tcs.TrySetResult(result);
            }
        }
    }
}