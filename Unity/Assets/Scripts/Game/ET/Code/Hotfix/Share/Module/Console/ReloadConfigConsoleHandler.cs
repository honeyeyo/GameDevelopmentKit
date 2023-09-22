using System;
using Cysharp.Threading.Tasks;

namespace ET
{
    [ConsoleHandler(ConsoleMode.ReloadConfig)]
    public class ReloadConfigConsoleHandler: IConsoleHandler
    {
        public async UniTask Run(Fiber fiber, ModeContex contex, string content)
        {
            switch (content)
            {
                case ConsoleMode.ReloadConfig:
                    contex.Parent.RemoveComponent<ModeContex>();
                    Log.Console("C must have config name, like: C UnitConfig");
                    break;
                default:
                    string[] ss = content.Split(" ");
                    string configName = ss[1];
                    string category = $"{configName}Category";
                    Type type = CodeTypes.Instance.GetType($"ET.{category}");
                    if (type == null)
                    {
                        Log.Console($"reload config but not find {category}");
                        return;
                    }
<<<<<<< HEAD
                    await CodeLoaderComponent.Instance.ReloadAsync();
                    fiber.Console($"reload config {configName} finish!");
=======
                    await ConfigLoader.Instance.Reload(type);
                    Log.Console($"reload config {configName} finish!");
>>>>>>> 7d37d33dfbf69d664e224d4387156fcf2fda4f70
                    break;
            }
        }
    }
}