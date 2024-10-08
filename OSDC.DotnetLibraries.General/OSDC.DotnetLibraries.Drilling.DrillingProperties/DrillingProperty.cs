﻿using DWIS.API.DTO;
using DWIS.Client.ReferenceImplementation;
using DWIS.Vocabulary.Schemas;
using System.Collections.Generic;
using System.Reflection;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public abstract class DrillingProperty 
    {
        public abstract bool FuseData(List<AcquiredSignals>? signals);
        public abstract bool Equals(DrillingProperty? other);
        public abstract void CopyTo(DrillingProperty? dest);
    }
}