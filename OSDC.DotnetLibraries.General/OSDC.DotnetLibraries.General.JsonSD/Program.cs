using NJsonSchema;
using OSDC.DotnetLibraries.General.DataManagement;

namespace OSDC.DotnetLibraries.General.JsonSD
{
    class Program
    {
        static void Main(string[] args)
        {
            string schemaVersion = "1.0.2";

            //Generating json schema on OSDC central data model repository
            string rootDir = Environment.GetEnvironmentVariable("OSDCDrillingContextDataRoot");
            GenerateCentralizedJsonSchemas(rootDir, schemaVersion);
        }

        static void GenerateCentralizedJsonSchemas(string repoRootDir, string schemaVersion)
        {
            if (!string.IsNullOrEmpty(repoRootDir))
            {
                string rootDir = repoRootDir + "\\json-schemas";
                if (Directory.Exists(rootDir))
                {
                    rootDir += "\\MetaInfo";
                    try
                    {
                        if (!Directory.Exists(rootDir))
                        {
                            Directory.CreateDirectory(rootDir);
                        }
                        rootDir += "\\" + schemaVersion;
                        if (!Directory.Exists(rootDir))
                        {
                            Directory.CreateDirectory(rootDir);
                            var schema = JsonSchema.FromType<MetaInfo>();
                            var schema2Json = schema.ToJson();
                            using (StreamWriter writer = new StreamWriter(rootDir + "\\MetaInfo.json"))
                            {
                                writer.WriteLine(schema2Json);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Directory {rootDir} already exists: modify the program if you intend to overide the existing schema");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }
    }
}
