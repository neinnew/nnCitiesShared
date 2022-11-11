namespace nnCitiesShared.Translation;

/// <summary>
/// Use this class as static using for concise code.
/// 
/// ------------------- Use case --------------------------
/// <code>
///     using static nnCitiesShared.Translation.Usage;
///     using k = nnCitiesShared.Translation.KeyStrings;
///     ...
///     string text = T[k.KEY_123_ABC];
/// </code>
/// -------------------------------------------------------
/// </summary>
public static class Usage
{
    public static Translator T => Translator.Instance;
}