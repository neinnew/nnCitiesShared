using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace nnCitiesShared.Translation;

public class Translation
{
    public readonly string Language;
    public readonly string NativeName;

    private Dictionary<string, string>? _translation;
    private Dictionary<string, string> Translations => _translation ??= GetTranslation(Language);

    public Translation(string language)
    {
        this.Language = language;
        this.NativeName = GetNativeName;
    }

    private static Dictionary<string, string> GetTranslation(string code)
    {
        try
        {
            var path = Path.Combine(Translator.ResourcePath, code);
            var files = Directory.GetFiles(path, "*.tsv");
            var translation = new Dictionary<string, string>();

            foreach (var file in files)
            {
                var lines = File.ReadAllLines(file);
                var dict = lines.Skip(1).Where(line => line.Split('\t').Length >= 2)
                    .ToDictionary(key => key.Split('\t')[0].Trim('"'), value => value.Split('\t')[1].Trim('"'));
                translation = translation.Union(dict).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            
            return translation;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
            throw;
        }
    }

    private string GetNativeName
    {
        get
        {
            try
            {
                var folder = Path.Combine(Translator.ResourcePath, Language);
                var path =  Path.Combine(folder, "LanguageMeta.tsv");
                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(fileStream);
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line?.Split('\t')[0].Trim('"') == "LANGUAGE_NAME:self")
                    {
                        return line.Split('\t')[1].Trim('"');
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
                throw;
            }
            return String.Empty;
        }
    }
    
    public string? this[string key] => Translations.TryGetValue(key, out string translated) ? translated : null;
    
}