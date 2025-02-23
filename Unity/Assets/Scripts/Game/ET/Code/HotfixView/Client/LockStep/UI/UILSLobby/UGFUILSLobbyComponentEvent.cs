using Game;
using UnityEngine;

namespace ET.Client
{
    [FriendOf(typeof(UGFUILSLobbyComponent))]
    public static partial class UGFUILSLobbyComponentSystem
    {
        [UGFUIFormEvent(UGFUIFormId.UILSLobby)]
        public class UGFUILSLobbyComponentEvent : AUGFUIFormEvent
        {
            public override void OnInit(UGFUIForm uiForm, object userData)
            {
                base.OnInit(uiForm, userData);
                UGFUILSLobbyComponent uiLSLobby = uiForm.AddComponent<UGFUILSLobbyComponent, Transform>(uiForm.transform);
                uiLSLobby.enterMapButton.SetAsync(uiLSLobby.EnterMap);
                uiLSLobby.replayButton.Set(uiLSLobby.Replay);
            }

            public override void OnOpen(UGFUIForm uiForm, object userData)
            {
                base.OnOpen(uiForm, userData);
            }
        }
    }
}