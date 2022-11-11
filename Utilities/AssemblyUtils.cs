using System.Reflection;

namespace nnCitiesShared.Utilities;

public class AssemblyUtils
{
    public static Assembly ThisAssembly => Assembly.GetExecutingAssembly();
}