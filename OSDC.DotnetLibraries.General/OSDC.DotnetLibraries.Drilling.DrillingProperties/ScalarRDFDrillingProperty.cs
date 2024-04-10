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
    public class ScalarRDFDrillingProperty
    {
        public Dictionary<string, string>? SparQLQueries { get; set; } = null;

        public AcquiredSignals? Signals { get; set; } = null;

        public ManifestFile? ManifestFile { get; set; } = null;

        public ScalarRDFDrillingProperty() : base()
        {
        }

     }
}
