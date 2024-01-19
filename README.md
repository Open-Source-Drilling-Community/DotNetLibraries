# DotNetLibraries

This repository has been created and is maintained by the NORCE Energy Modelling & Simulation team. This research work has been funded by the [Research Council of Norway](https://www.forskningsradet.no/) and [Industry partners](https://www.digiwells.no/about/board/) in the framework of the center for research-based innovation [SFI Digiwells (2020-2028)](https://www.digiwells.no/) focused on Digitalization, Drilling Engineering and GeoSteering. Part of it is hereby donated **without any limit or warranty** to the Society of Petroleum (SPE) Open Source Drilling Community, a sub-committee of the Drilling System Automation Technical Section. Anyone is thus **free to use** the source code of this repository **under its own responsibility**.

## General description
It contains the following standard libraries implemented in .NET, relevant for the development of scientific applications:
- [APIProxy](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.APIProxy): generic http client features to handle REST API requests and responses
- [Common](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.Common): general purpose classes/interfaces to handle data and variables
- [DataManagement](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.DataManagement): utility classes to identify data
- [DrillingProperties](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.DrillingProperties): utility classes designed to handle uncertainty associated with scientific data
- [JsonSD](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.JsonSD): serialization program used to expose data in json format typically used for REST API payload
- [Math (unit tests)](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.Math.UnitTest): unit tests associated with the Math library
- [Math](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.Math): linear algebra utility classes and functions
- [Statistics](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.Statistics): general purpose statistical classes and functions
- [Surveying (unit tests)](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.Surveying.UnitTest): unit tests associated with the Surveying library
- [Surveying](https://github.com/Open-Source-Drilling-Community/DotNetLibraries/tree/main/OSDC.DotnetLibraries.General/OSDC.DotnetLibraries.General.Surveying): utility classes and functions used in wellbore surveying

## Deployment
Most of these libraries have been packaged as .NET NuGets and published to [nuget.org](https://www.nuget.org/packages?q=OSDC.Dotnetlibraries).
