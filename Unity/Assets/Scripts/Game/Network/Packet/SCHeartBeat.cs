﻿//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using ProtoBuf;
using System;

namespace Game
{
    [Serializable, ProtoContract(Name = @"SCHeartBeat")]
    public class SCHeartBeat : SCPacketBase
    {
        public override int Id
        {
            get
            {
                return 2;
            }
        }

        public override void Clear()
        {
        }
    }
}
