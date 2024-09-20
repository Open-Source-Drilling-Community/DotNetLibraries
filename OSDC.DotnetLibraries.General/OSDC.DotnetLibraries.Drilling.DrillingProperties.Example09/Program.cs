using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using DWIS.Vocabulary.Schemas;
using System.Reflection;
using DWIS.API.DTO;
using System.Text.Json;

namespace DrillingProperties
{
    class Example
    {
        static void GenerateMermaidForMD(StreamWriter writer, string? typeName, string? mermaid)
        {
            if (writer != null && !string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(mermaid))
            {
                writer.WriteLine("# Semantic Graph for `" + typeName + "`");
                writer.WriteLine(mermaid);
            }
        }
        static void GenerateDWISForMD(StreamWriter writer, string propertyName, string? semantic)
        {
            if (writer != null && !string.IsNullOrEmpty(propertyName) && !string.IsNullOrEmpty(semantic))
            {
                writer.WriteLine("# Semantic Facts for `" + propertyName + "`");
                writer.WriteLine(semantic);
            }
        }
        static void Main()
        {
            string tempPath = Directory.GetCurrentDirectory();
            DirectoryInfo? dir = new DirectoryInfo(tempPath);
            dir = dir?.Parent?.Parent?.Parent;
            if (dir != null)
            {
                ManifestFile? manifestFile = null;
                string filename = dir.FullName + Path.DirectorySeparatorChar + "simulatorManifest.json";
                if (File.Exists(filename))
                {
                    try
                    {
                        string jsonString = File.ReadAllText(filename);
                        manifestFile = JsonSerializer.Deserialize<ManifestFile>(jsonString);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                if (manifestFile != null)
                {
                    string tempFile = Path.Combine(dir.FullName, "Example09.md");
                    using (StreamWriter writer = new StreamWriter(tempFile))
                    {
                        GenerateDWISForMD(writer, "Drilling Simulator Signals Facts", GeneratorSparQLManifestFile.GetDWIS(manifestFile, true));
                        GenerateMermaidForMD(writer, "Drilling Simulator Signals Graph Representation", GeneratorSparQLManifestFile.GetMermaid(manifestFile));
                    }
                }
            }
        }
    }
}
