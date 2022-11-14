using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using ColossalFramework.Globalization;
using nnCitiesShared.Utilities;
using UnityEngine;

namespace nnCitiesShared.Translation;

/// <summary>
/// 
/// </summary>
public class Translator
{
    public static bool UseGameLanguage = true;
    public static string Language = DefaultLanguage;

    private const string DefaultLanguage = "en-EN";
    public static readonly string ResourcePath = Path.Combine(ModUtils.ModPath, "Translations");

    private readonly Dictionary<string, Translation> _translations = new();
    private readonly Dictionary<string, string> _source = new();

    public readonly string[] LanguageCodes;
    public readonly string[] LanguageNativeNames;
    
    private IEnumerable<string> FallbackCodes => LanguageCodes.Where(code => code.Substring(0, 2) == Language.Substring(0, 2));

    public static Translator Instance => _instance ??= new Translator();
    private static Translator? _instance;
    
    private Translator()
    {
        LoadSource();
        LoadTranslations();

        LanguageCodes = new[] { DefaultLanguage }.Concat(_translations.Select(translation => translation.Key)).ToArray();
        LanguageNativeNames = new[] { "English" }.Concat(_translations.Select(translation => translation.Value.NativeName)).ToArray();
    }

    public static void Destruct()
    {
        _instance = null;
    }

    /// <summary>
    /// Get string by current language and given key.
    /// </summary>
    /// <param name="key">Key to translate (in <see cref="KeyStrings"/>)</param>
    public string this[string key] => Language == DefaultLanguage ? Source(key) : Translate(key);

    /// <summary>
    /// Get formatted string. Allows up to five arguments.
    /// Please use a string interpolation($) as input for each argument.
    /// 'expr' arguments are automatically filled by the CallerArgumentExpressionAttribute. don't pass anything on them.
    /// </summary>
    public string this[string key, 
        string arg1, string? arg2 = null, string? arg3 = null, string? arg4 = null, string? arg5 = null,
        [CallerArgumentExpression("arg1")] string expr1 = null!,
        [CallerArgumentExpression("arg2")] string? expr2 = null,
        [CallerArgumentExpression("arg3")] string? expr3 = null,
        [CallerArgumentExpression("arg4")] string? expr4 = null,
        [CallerArgumentExpression("arg5")] string? expr5 = null
    ] 
    {
        get
        {
            // Get unformatted string first.
            string str = this[key];

            for (int i = 1; i <= 5; i++)
            {
                // Iterate manually.
                string? iArg = i switch { 1 => arg1, 2 => arg2, 3 => arg3, 4 => arg4, 5 => arg5, _ => null };
                string? iExpr = i switch { 1 => expr1, 2 => expr2, 3 => expr3, 4 => expr4, 5 => expr5, _ => null };

                if (iArg == null || iExpr == null)
                {
                    break;
                }
                
                // Is it an interpolated string?
                if (iExpr.StartsWith("$"))
                {
                    iExpr = iExpr.TrimStart('$').Trim('"');
                }
                // Although the recommendation is using an interpolated string, we should also deal with cases that are not.
                else
                {
                    // .ToString() must not have been included in the source text. (of course in un-interpolated case)
                    if (iExpr.EndsWith(".ToString()"))
                    {
                        iExpr = iExpr.Replace(".ToString()", String.Empty);
                    }
                
                    // Add braces.
                    iExpr = '{' + iExpr + '}';
                }

                if (str.Contains(iExpr))
                {
                    str = str.Replace(iExpr, iArg);
                }
                // Support composite formatting.
                else if (str.Contains($"{{{i}}}"))
                {
                    str = str.Replace($"{{{i}}}", iArg);
                }
                else
                {
                    Debug.LogError($"can't find matching format '{iExpr}' or '{{{i}}}' in source string. key: {key}, language: {Language}");
                    throw new FormatException();
                }
            }

            return str;
        }
    }
    
    
    
    private void LoadTranslations()
    {
        var languages = Directory.GetDirectories(ResourcePath).Select(dir => new DirectoryInfo(dir).Name);
        foreach (var language in languages)
        {
            _translations[language] = new Translation(language);
        }
    }

    private void LoadSource()
    {
        var allResources = AssemblyUtils.ThisAssembly.GetManifestResourceNames();

        // Assume that all embedded tsv files are source. This should be changed later.
        var sources = allResources.Where(name => name.EndsWith(".tsv"));

        foreach (var source in sources)
        {
            using var stream = AssemblyUtils.ThisAssembly.GetManifestResourceStream(source);
            using var reader = new StreamReader(stream!);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) break;
                _source.Add(line.Split('\t')[0].Trim('"'), line.Split('\t')[1].Trim('"'));
            }
        }
    }
    
    private string Source(string key) => TrySource(key) ?? key;
    private string Translate(string key) => TryTranslate(key) ?? TryFallback(key) ?? TrySource(key) ?? key;
    
    private string? TryTranslate(string key)
    {
        return _translations[Language][key];
    }

    private string? TryFallback(string key)
    {
        foreach (var fallback in FallbackCodes)
        {
            if (_translations[fallback][key] is string translated)
                return translated;
        }

        return null;
    }

    private string? TrySource(string key)
    {
        return _source.TryGetValue(key, out string value) ? value : null;
    }

    public string? TryTranslateBy(string key, string code)
    {
        return _translations[code][key];
    }

    private static void OnGameLocaleChanged()
    {
        if (!UseGameLanguage
            || !LocaleManager.exists 
            || LocaleManager.instance.language == Language.Substring(0, 2))
        {
            return;
        }

        Language = Instance.LanguageCodes
                       .Where(code => LocaleManager.instance.supportedLocaleIDs.Contains(code.Substring(0, 2)))
                       .FirstOrDefault(code => code.Substring(0, 2) == LocaleManager.instance.language)
                   ?? DefaultLanguage;

        UpdateCustomUI();
    }

    private static void Update()
    {
        UpdateSettingsUI();
        UpdateCustomUI();
    }

    public static event Action UpdateCustomUI;
    public static event Action UpdateSettingsUI;

    public static void OnGameLanguageUsed()
    {
        UseGameLanguage = true;
        OnGameLocaleChanged();
        UpdateSettingsUI();
    }

    public static void OnLanguageChanged(int index)
    {
        UseGameLanguage = false;
        Language = Instance.LanguageCodes[index];
        Update();
    }
}


// Quick peek for dropdown use case.
/* 
public class ___
{
    void ____()
    { 
        UIHelper ui = new UIHelper(new UIPanel());
        ui.AddDropdown("Select Language", GetOptions(), GetDefault(), i =>
        {
            if (i == 0)
            {
                Translator.OnGameLanguageUsed();
            }
            else
            {
                Translator.OnLanguageChanged(i - 1);
            }
            // Save();
        });
    }

    int GetDefault()
    {
        return Array.FindIndex(Translator.Instance.LanguageCodes, code => code == Translator.Language) + 1;
    }
    
    string[] GetOptions()
    {
        int length = Translator.Instance.LanguageCodes.Length + 1;
        var result = new string[length];
        result[0] = Translator.Instance["USE_GAME_LANGUAGE"];
        for (int i = 1; i < length; i++)
        {
            result[i] = $"{Translator.Instance.LanguageNativeNames[i - 1]} ({Translator.Instance.LanguageCodes[i - 1]})";
        }
        return result;
    }
}
 */
