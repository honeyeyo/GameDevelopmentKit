﻿namespace Game
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        public static CodeRunnerComponent CodeRunner
        {
            get;
            private set;
        }
        
        public static Tables Tables
        {
            get;
            private set;
        }

        private void InitGameComponents()
        {
            CodeRunner = UnityGameFramework.Runtime.GameEntry.GetComponent<CodeRunnerComponent>();
            Tables = UnityGameFramework.Runtime.GameEntry.GetComponent<Tables>();
        }
    }
}
