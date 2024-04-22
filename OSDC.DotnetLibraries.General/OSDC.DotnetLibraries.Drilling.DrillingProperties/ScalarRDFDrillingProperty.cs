using DWIS.API.DTO;
using DWIS.Client.ReferenceImplementation;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class ScalarRDFDrillingProperty : ScalarDrillingProperty, IRDFDrillingProperty
    {
        public Dictionary<string, QuerySpecification>? SparQLQueries { get; set; } = null;

        public AcquiredSignals[]? SubscribedSignals { get; set; } = null;

        public ManifestFile? ManifestFile { get; set; } = null;

        public ScalarRDFDrillingProperty() : base()
        {
        }

     }
}
