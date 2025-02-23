
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;

namespace ET
{
public partial class Tables
{
    public DTStartMachineConfig DTStartMachineConfig { private set; get; }
    public DTStartProcessConfig DTStartProcessConfig { private set; get; }
    public DTStartSceneConfig DTStartSceneConfig { private set; get; }
    public DTStartZoneConfig DTStartZoneConfig { private set; get; }
    public DTOneConfig DTOneConfig { private set; get; }
    public DTAIConfig DTAIConfig { private set; get; }
    public DTUnitConfig DTUnitConfig { private set; get; }
    public DTDemo DTDemo { private set; get; }
    private System.Collections.Generic.Dictionary<string, IDataTable> _tables;
    public System.Collections.Generic.IEnumerable<IDataTable> DataTables => _tables.Values;
    public IDataTable GetDataTable(string tableName) => _tables.TryGetValue(tableName, out var v) ? v : null;

    public async Cysharp.Threading.Tasks.UniTask LoadAsync(System.Func<string, Cysharp.Threading.Tasks.UniTask<ByteBuf>> loader)
    {
        TablesMemory.BeginRecord();

        _tables = new System.Collections.Generic.Dictionary<string, IDataTable>();
        var loadTasks = new System.Collections.Generic.List<Cysharp.Threading.Tasks.UniTask>();

        DTStartMachineConfig = new DTStartMachineConfig(() => loader("dtstartmachineconfig"));
        loadTasks.Add(DTStartMachineConfig.LoadAsync());
        _tables.Add("DTStartMachineConfig", DTStartMachineConfig);
        DTStartProcessConfig = new DTStartProcessConfig(() => loader("dtstartprocessconfig"));
        loadTasks.Add(DTStartProcessConfig.LoadAsync());
        _tables.Add("DTStartProcessConfig", DTStartProcessConfig);
        DTStartSceneConfig = new DTStartSceneConfig(() => loader("dtstartsceneconfig"));
        loadTasks.Add(DTStartSceneConfig.LoadAsync());
        _tables.Add("DTStartSceneConfig", DTStartSceneConfig);
        DTStartZoneConfig = new DTStartZoneConfig(() => loader("dtstartzoneconfig"));
        loadTasks.Add(DTStartZoneConfig.LoadAsync());
        _tables.Add("DTStartZoneConfig", DTStartZoneConfig);
        DTOneConfig = new DTOneConfig(() => loader("dtoneconfig"));
        loadTasks.Add(DTOneConfig.LoadAsync());
        _tables.Add("DTOneConfig", DTOneConfig);
        DTAIConfig = new DTAIConfig(() => loader("dtaiconfig"));
        loadTasks.Add(DTAIConfig.LoadAsync());
        _tables.Add("DTAIConfig", DTAIConfig);
        DTUnitConfig = new DTUnitConfig(() => loader("dtunitconfig"));
        loadTasks.Add(DTUnitConfig.LoadAsync());
        _tables.Add("DTUnitConfig", DTUnitConfig);
        DTDemo = new DTDemo(() => loader("dtdemo"));
        loadTasks.Add(DTDemo.LoadAsync());
        _tables.Add("DTDemo", DTDemo);

        await Cysharp.Threading.Tasks.UniTask.WhenAll(loadTasks);

        Refresh();

        TablesMemory.EndRecord();
    }

    private void ResolveRef()
    {
        DTStartMachineConfig.ResolveRef(this);
        DTStartProcessConfig.ResolveRef(this);
        DTStartSceneConfig.ResolveRef(this);
        DTStartZoneConfig.ResolveRef(this);
        DTOneConfig.ResolveRef(this);
        DTAIConfig.ResolveRef(this);
        DTUnitConfig.ResolveRef(this);
        DTDemo.ResolveRef(this);
        PostResolveRef();
    }

    public void Refresh()
    {
        PostLoad();
        ResolveRef();
    }

    partial void PostLoad();
    partial void PostResolveRef();
}
}
