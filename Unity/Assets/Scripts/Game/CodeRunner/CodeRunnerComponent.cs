using System;
using GameFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game
{
    public class CodeRunnerComponent : GameFrameworkComponent
    {
        [Tooltip("编辑器模式下能否加载bytes方式运行代码")]
        [SerializeField, ShowIf("IsHotFix")]
        private bool m_EnableEditorCodeBytesMode = false;

        [ShowInInspector, ReadOnly]
#if UNITY_HOTFIX
        public bool IsHotFix => true;
#else
        public bool IsHotFix => false;
#endif

        [ShowInInspector, ReadOnly]
        public bool IsRunning { get; private set; } = false;

        [ShowInInspector, ReadOnly]
        private Component m_InitComponent;

#if UNITY_EDITOR
        public bool EnableCodeBytesMode => m_EnableEditorCodeBytesMode;
#else
        public bool EnableCodeBytesMode => true;
#endif

        public void StartRun(string startMonoType)
        {
            if (IsRunning)
            {
                throw new GameFrameworkException("CodeRunnerComponent StartRun duplicate!");
            }
            Type initType = Utility.Assembly.GetType(startMonoType);
            if (initType == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Not Found {0}!", startMonoType));
            }
            m_InitComponent = gameObject.AddComponent(initType);
            if (m_InitComponent == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Add {0} Fail!", initType));
            }
            IsRunning = true;
        }

        public void Shutdown()
        {
            if (!IsRunning)
            {
                throw new GameFrameworkException("CodeRunnerComponent can Shutdown only when is running!");
            }
            IsRunning = false;
            DestroyImmediate(this.m_InitComponent);
        }
    }
}
