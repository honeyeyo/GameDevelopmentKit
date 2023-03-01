//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Bright.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ET
{
   
public sealed partial class DTOneConfig : IDataTable
{

    private DROneConfig _data;

    private readonly Task<ByteBuf> _loadFunc;

    public DTOneConfig(Task<ByteBuf> loadFunc)
    {
        _loadFunc = loadFunc;
    }

    public async Task LoadAsync()
    {
        ByteBuf _buf = await _loadFunc;
        int n = _buf.ReadSize();
        if (n != 1) throw new SerializationException("table mode=one, but size != 1");
        _data = DROneConfig.DeserializeDROneConfig(_buf);
        PostInit();
    }


    /// <summary>
    /// 匹配最大时间
    /// </summary>
     public long MaxMatchTime => _data.MaxMatchTime;
    /// <summary>
    /// 匹配最大时间
    /// </summary>
     public int Test => _data.Test;

    public void Resolve(Dictionary<string, IDataTable> _tables)
    {
        _data.Resolve(_tables);
        PostResolve();
    }

    public void TranslateText(System.Func<string, string, string> translator)
    {
        _data.TranslateText(translator);
    }

    
    partial void PostInit();
    partial void PostResolve();
}

}