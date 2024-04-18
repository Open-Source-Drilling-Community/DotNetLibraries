using DWIS.API.DTO;
using DWIS.Client.ReferenceImplementation;
using System.Reflection;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public interface IRDFDrillingProperty : IDrillingProperty
    {
        public Dictionary<string, Tuple<int, string>>? SparQLQueries { get; set; }

        public AcquiredSignals[]? SubscribedSignals { get; set; }

        public ManifestFile? ManifestFile { get; set; }

        public void CreateSparQLQueries(Assembly? assembly, string? typeName, string? propName)
        {
            if (assembly != null && !string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(propName))
            {
                SparQLQueries = GetSparQLQueries(assembly, typeName, propName);
            }
        }

        public void SubscribeSparQLQueries(IOPCUADWISClient? ddhubClient)
        {
            if (ddhubClient != null && ddhubClient.Connected && SparQLQueries != null)
            {
                foreach (var kvp in SparQLQueries)
                {
                    if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null && kvp.Value.Item1 > 0 && !string.IsNullOrEmpty(kvp.Value.Item2))
                    {
                        var result = ddhubClient.GetQueryResult(kvp.Value.Item2);
                        if (result != null && result.Results != null && result.Results.Count > 0)
                        {
                            SubscribedSignals = new AcquiredSignals[kvp.Value.Item1];
                            for (int i = 0; i < kvp.Value.Item1; i++)
                            {
                                SubscribedSignals[i] = AcquiredSignals.CreateWithSubscription([kvp.Value.Item2], [kvp.Key], i, ddhubClient);
                            }
                        }
                    }
                }
            }
        }

        public void CreateManifestFile(Assembly? assembly, string? typeName, string? propName)
        {
            if (assembly != null && !string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(propName))
            {
                ManifestFile = GetManifestFile(assembly, typeName, propName);
            }
        }

        public void PublishManifest(IOPCUADWISClient? ddhubClient)
        {
            if (ddhubClient != null && ddhubClient.Connected && ManifestFile != null && SparQLQueries != null && SparQLQueries.Count > 0)
            {
                // check if the semantic has not already been injected
                bool found = false;
                foreach (var kvp in SparQLQueries)
                {
                    if (kvp.Value != null && kvp.Value.Item1 > 0 && !string.IsNullOrEmpty(kvp.Value.Item2))
                    {
                        var result = ddhubClient.GetQueryResult(kvp.Value.Item2);
                        if (result != null && result.Results != null && result.Results.Count > 0)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                // if not, inject
                if (!found)
                {
                    var result = ddhubClient.Inject(ManifestFile);
                    if (result == null || !result.Success)
                    {
                        throw new Exception("Error in manifest file");
                    }
                }
            }
        }
    }
}
