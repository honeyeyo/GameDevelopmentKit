//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Bright.Serialization;
using System.Collections.Generic;


namespace Game.Hot
{
public sealed partial class Vector3 :  Bright.Config.BeanBase 
{
    public Vector3(ByteBuf _buf) 
    {
        X = _buf.ReadFloat();
        Y = _buf.ReadFloat();
        Z = _buf.ReadFloat();
        PostInit();
    }

    public static Vector3 DeserializeVector3(ByteBuf _buf)
    {
        return new Vector3(_buf);
    }

    public float X { get; private set; }
    public float Y { get; private set; }
    public float Z { get; private set; }

    public const int __ID__ = 2002444080;
    public override int GetTypeId() => __ID__;

    public  void Resolve(Dictionary<string, IDataTable> _tables)
    {
        PostResolve();
    }

    public  void TranslateText(System.Func<string, string, string> translator)
    {
    }

    public override string ToString()
    {
        return "{ "
        + "X:" + X + ","
        + "Y:" + Y + ","
        + "Z:" + Z + ","
        + "}";
    }
    
    partial void PostInit();
    partial void PostResolve();
}

}