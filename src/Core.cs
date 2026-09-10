using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace GrasshopperChinese
{
    public static class NativeTools
    {
        static readonly HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "Grasshopper", "CurveComponents", "FieldComponents", "MathComponents", "SurfaceComponents", "TriangulationComponents", "VectorComponents", "XformComponents", "IOComponents", "ScriptComponents", "RhinoCodePluginGH" };
        public static bool Contains(string assemblyName) { return names.Contains(assemblyName); }
    }
    public sealed class Catalog
    {
        [DataContract]
        public sealed class ExportEntry
        {
            [DataMember] public string Guid, Name, Category, Description, Translation, DescriptionTranslation, Error;
            [DataMember] public List<string> Inputs = new List<string>();
            [DataMember] public List<string> Outputs = new List<string>();
        }
        [DataContract]
        public sealed class Entry
        {
            [DataMember(Name = "name")] public string Name;
            [DataMember(Name = "category")] public string Category;
            [DataMember(Name = "translation")] public string Translation;
        }
        readonly Dictionary<string, string> entries = new Dictionary<string, string>(StringComparer.Ordinal);
        public int Count { get { return entries.Count; } }
        static string Key(string name, string category) { return (category ?? "") + "\u001f" + name; }
        public static Catalog Parse(string json)
        {
            try
            {
                var result = new Catalog();
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    var rows = (Entry[])new DataContractJsonSerializer(typeof(Entry[])).ReadObject(stream);
                    if (rows == null) throw new ArgumentException("Language pack must be an array.");
                    foreach (var row in rows)
                    {
                        if (row == null || string.IsNullOrWhiteSpace(row.Name) || string.IsNullOrWhiteSpace(row.Translation))
                            throw new ArgumentException("Every entry requires a name and translation.");
                        result.entries.Add(Key(row.Name, row.Category), row.Translation);
                    }
                }
                return result;
            }
            catch (Exception e) { throw new ArgumentException("Invalid language pack: " + e.Message, e); }
        }
        public string Find(string name, string category)
        {
            string text;
            if (entries.TryGetValue(Key(name, category), out text) || entries.TryGetValue(Key(name, null), out text)) return text;
            return null;
        }
    }

    public sealed class TextScope : IDisposable
    {
        readonly Action<string> setter;
        readonly string original;
        bool disposed;
        public TextScope(Func<string> get, Action<string> set, string replacement)
        { original = get(); setter = set; setter(replacement); }
        public void Dispose() { if (!disposed) { setter(original); disposed = true; } }
    }

    public static class DisplayText
    {
        public static string Nickname(string current, string original, string translated)
        { return current == original ? translated : current; }
    }
}
