using System.Linq;
using ColossalFramework.Plugins;

namespace nnCitiesShared.Utilities;

public static class ModUtils
{
    private static PluginManager.PluginInfo ThisMod => _thisMod 
        ??= PluginManager.instance.FindPluginInfo(AssemblyUtils.ThisAssembly)
            ?? PluginManager.instance.GetPluginsInfo()
                .First(plugin => Equals(plugin.GetAssemblies()
                    .First(assembly => Equals(assembly, AssemblyUtils.ThisAssembly)), AssemblyUtils.ThisAssembly));
    
    private static PluginManager.PluginInfo? _thisMod;

    public static string ModPath => ThisMod.modPath;
}