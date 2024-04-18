using DWIS.API.DTO;
using DWIS.Client.ReferenceImplementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class GaussianRDFDrillingProperty : GaussianDrillingProperty, IRDFDrillingProperty
    {
        public Dictionary<string, Tuple<int, string>>? SparQLQueries { get; set; } = null;

        public AcquiredSignals[]? SubscribedSignals { get; set; } = null;

        public ManifestFile? ManifestFile { get; set; } = null;

        public GaussianRDFDrillingProperty() : base()
        {
        }
    }
}
