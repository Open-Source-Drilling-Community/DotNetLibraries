using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using DWIS.Vocabulary.Schemas;
using System.Reflection;
using DWIS.API.DTO;

namespace DrillingProperties
{

    [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
    [SemanticTypeVariable("ComputedData")]
    [SemanticFact("ComputedData", Nouns.Enum.DynamicDrillingSignal)]
    [SemanticFact("ComputedData#01", Nouns.Enum.ComputedData)]
    [SemanticFact("ComputedData#01", Verbs.Enum.HasDynamicValue, "ComputedData")]
    [SemanticFact("ProcessState", Nouns.Enum.ProcessState)]
    [SemanticFact("ProcessState", Nouns.Enum.DeterministicModel)]
    [SemanticFact("ComputedData#01", Verbs.Enum.IsGeneratedBy, "ProcessState")]
    [SemanticFact("DrillingProcessStateInterpreter", Nouns.Enum.DWISDrillingProcessStateInterpreter)]
    [SemanticFact("ProcessState", Verbs.Enum.IsProvidedBy, "DrillingProcessStateInterpreter")]
    public class TestClass
    {
        /// <summary>
        /// the time stamp in UTC when the state has been updated
        /// </summary>
        public DateTime TimeStampUTC { get; set; }
        /// <summary>
        /// Part1: microstates from 0 to 15
        /// </summary>
        public int Part1 { get; set; }
        /// <summary>
        /// Part2: microstates from 16 to 31
        /// </summary>
        public int Part2 { get; set; }
        /// <summary>
        /// Part3: microstates from 32 to 47
        /// </summary>
        public int Part3 { get; set; }
        /// <summary>
        /// Part4: microstates from 48 to 63
        /// </summary>
        public int Part4 { get; set; }
        /// <summary>
        /// Part5: microstates from 64 to 79
        /// </summary>
        public int Part5 { get; set; }
    }
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
        static void GenerateSparQLForMD(StreamWriter writer, string? typeName, Dictionary<string, Tuple<int, string>>? queries)
        {
            if (writer != null && !string.IsNullOrEmpty(typeName) && queries != null)
            {
                writer.WriteLine("# Semantic Queries for `" + typeName + "`");
                foreach (var query in queries)
                {
                    if (query.Value != null)
                    {
                        writer.WriteLine("## " + query.Key);
                        writer.WriteLine("```sparql");
                        writer.WriteLine(query.Value.Item2);
                        writer.WriteLine("```");
                    }
                }
            }
        }
        static void Main()
        {
            TestClass testClass = new TestClass();
            Assembly? assembly = Assembly.GetAssembly(typeof(TestClass));
            if (assembly != null)
            {
                string tempPath = Directory.GetCurrentDirectory();
                DirectoryInfo? dir = new DirectoryInfo(tempPath);
                dir = dir?.Parent?.Parent?.Parent;
                if (dir != null)
                {
                    string tempFile = Path.Combine(dir.FullName, "Example07.md");
                    using (StreamWriter writer = new StreamWriter(tempFile))
                    {
                        var queries = GeneratorSparQLManifestFile.GetSparQLQueries(assembly, typeof(TestClass).FullName);
                        if (queries != null)
                        {
                            GenerateSparQLForMD(writer, typeof(TestClass).FullName, queries);
                        }
                        var manifestFile = GeneratorSparQLManifestFile.GetManifestFile(assembly, typeof(TestClass).FullName, "SampleManifest", "ExampleCompany", "Test:");
                        if (manifestFile != null)
                        {
                            GenerateMermaidForMD(writer, typeof(TestClass).FullName, GeneratorSparQLManifestFile.GetMermaid(manifestFile));
                        }

                    }
                }
            }
        }
    }
}
