namespace System.Runtime.CompilerServices;

#region Caller Attributes

public sealed class CallerMemberNameAttribute : Attribute
{
}

public sealed class CallerFilePathAttribute : Attribute
{
}

public sealed class CallerLineNumberAttribute : Attribute
{
}

public sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute(string parameterName) { }
}
    
#endregion