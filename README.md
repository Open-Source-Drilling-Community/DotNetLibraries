# DotNetLibraries

This repository has been created and is maintained by the NORCE Energy Modelling & Simulation team. This research work has been funded by the [Research Council of Norway](https://www.forskningsradet.no/) and [Industry partners](https://www.digiwells.no/about/board/) in the framework of the center for research-based innovation [SFI Digiwells (2020-2028)](https://www.digiwells.no/) focused on Digitalization, Drilling Engineering and GeoSteering. Part of it is hereby donated **without any limit or warranty** to the Society of Petroleum (SPE) Open Source Drilling Community under the **MIT** license, a sub-committee of the Drilling System Automation Technical Section. Anyone is thus **free to use** the source code of this repository **under its own responsibility**.

## General description
It contains the following standard libraries implemented in .NET, relevant for the development of scientific applications:
- [APIProxy](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.APIProxy): generic http client features to handle REST API requests and responses
- [Common](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.Common): general purpose classes/interfaces to handle data and variables
- [DataManagement](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.DataManagement): utility classes to identify data
- [JsonSD](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.JsonSD): serialization program used to expose data in json format typically used for REST API payload
- [Math (unit tests)](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.Math.UnitTest): unit tests associated with the Math library
- [Math](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.Math): linear algebra utility classes and functions, including enumeration and curvature-aware selection of exact constant-build-and-turn Cartesian target solutions
- [Octree](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.Octree): octree decomposition library
- [Octree (examples)](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.Octree.Example01): console app Octree management examples
- [Statistics](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.Statistics): general purpose statistical classes and functions
- [DrillingProperties](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.Drilling.DrillingProperties): utility classes designed to handle uncertainty associated with scientific data
- [DrillingProperties (unit tests)](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.Drilling.DrillingProperties.UnitTest): unit tests associated with the `DrillingProperty` library
- [DrillingProperties (Example 1)](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.Drilling.DrillingProperties.Example01): An example of the primary use of `DrillingProperty` to describe uncertainty at the level of the properties of a class.
- [DrillingProperties (Example 2)](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.Drilling.DrillingProperties.Example02): An example of use of `DrillingProperty` with a focus on the declation of meta data assocated with drilling properties using `Attribute`.
- [DrillingProperties (Example 3)](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.Drilling.DrillingProperties.Example03): An example of use of `DrillingProperty` with a focus on the generation of the dictionary of descriptions of meta data associated with the drilling properies defined in an assembly.
- [Section](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.Drilling.Section): directional-drilling section and multi-section well-path geometry. Constant-build-and-turn Cartesian targets can be constrained by exact peak curvature without discarding compliant alternative roots. Failed `ComplexPath` calculations report the affected section and constraint, remaining unknowns, and residual or sensitivity information where available.
- [Surveying (unit tests)](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.Drilling.Surveying.UnitTest): unit tests associated with the Surveying library
- [Surveying](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.Drilling.Surveying): utility classes and functions used in wellbore surveying

## Deployment
Most of these libraries have been packaged as .NET NuGets and published to [nuget.org](https://www.nuget.org/packages?q=OSDC.Dotnetlibraries).

## Shared semantic catalogue

[Drilling.SemanticCatalogue](OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.Drilling.SemanticCatalogue/README.md) source 0.12.0 contains 421 concepts (418 Reviewed and three Deprecated legacy names), including the ISCWSA Revision 5-aligned SurveyInstrument increment. It uses UnitConversion 3.4.5 and canonical drilling reference profile 1.1.0, preserving earlier definitions and presentation-reference support.

## Shared resource classification

[General.ResourceClassification](OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.ResourceClassification/README.md)
provides common identity, feature and membership models and pure validation
helpers. Version 0.1.0 targets .NET 8 and references DataManagement 2.2.0.
Field and Cluster are the initial consumer migrations. The package README
documents ownership, contract preservation, validation policies and publication order.
