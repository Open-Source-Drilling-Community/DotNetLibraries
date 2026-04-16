# OSDC.DotnetLibraries.Drilling.WebAppUtils

A comprehensive .NET library providing utilities and interfaces for drilling-related web applications. This library offers standardized abstractions for accessing drilling data services through configurable host URLs and includes utility functions for API interactions and data manipulation.

## Overview

OSDC.DotnetLibraries.Drilling.WebAppUtils is part of the OSDC (Open Subsurface Data Collaborative) initiative to create interoperable, reusable .NET libraries for the oil and gas industry. This specific library focuses on providing web application utilities tailored for drilling operations and related subsurface data management.

## Features

- **Standardized Host URL Interfaces**: Abstractions for various drilling-related services:
  - Cartographic Projection hosting
  - Cluster data management
  - Drilling Fluid properties
  - Drill String configuration
  - Field data management
  - Geodetic Datum references
  - Geological Properties
  - Geothermal Properties
  - Rig information
  - Trajectory data
  - Unit Conversion services
  - WellBore Architecture
  - WellBore data
  - Well information

- **Utility Classes**: Helper functions for common operations:
  - `APIUtils`: Utilities for API interactions
  - `DataUtils`: General-purpose data manipulation utilities

## Requirements

- .NET 8.0 or higher
- Visual Studio 2022 or compatible IDE (optional)

## Installation

### NuGet Package

Install the package via NuGet Package Manager:

```bash
dotnet add package OSDC.DotnetLibraries.Drilling.WebAppUtils
```

Or via the Package Manager Console:

```powershell
Install-Package OSDC.DotnetLibraries.Drilling.WebAppUtils
```

### Manual Reference

Alternatively, reference the project directly in your solution or build from source.

## Getting Started

### Basic Usage

To use the host URL interfaces, implement them in your application or inject them as dependencies:

```csharp
using OSDC.DotnetLibraries.Drilling.WebAppUtils;

// Implement an interface for your specific needs
public class MyDrillingService : IWellHostURL
{
    public string GetWellHostURL()
    {
        return "https://api.example.com/wells";
    }
}

// Use utility classes
var apiHelper = new APIUtils();
var dataHelper = new DataUtils();
```

### Configuring Host URLs

Each host URL interface provides an abstraction for accessing specific drilling-related services. Implement the interfaces in your configuration or dependency injection setup to define the actual endpoints for your application.

```csharp
// Example of dependency injection setup
services.AddSingleton<IWellHostURL, MyWellHostURLProvider>();
services.AddSingleton<IDrillStringHostURL, MyDrillStringHostURLProvider>();
// ... configure other interfaces as needed
```

## Project Structure

```
OSDC.DotnetLibraries.Drilling.WebAppUtils/
├── APIUtils.cs                          # API interaction utilities
├── DataUtils.cs                         # Data manipulation utilities
├── I*HostURL.cs                         # Host URL interface definitions
├── OSDC.DotnetLibraries.Drilling.WebAppUtils.csproj
└── Readme.md                            # This file
```

## Contributing

Contributions are welcome! Please ensure that:
- Code follows .NET coding standards and conventions
- Changes include appropriate documentation
- All interfaces maintain backward compatibility where possible

## License

This project is licensed under the terms specified in the LICENSE file. See the LICENSE file for details.

## Support

For issues, questions, or contributions, please refer to the OSDC (Open Subsurface Data Collaborative) project guidelines and repository.

## Additional Resources

- [OSDC Official Website](https://www.opensubsurfacedata.org/)
- [.NET 8.0 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- Open Subsurface Data Collaborative Standards and Specifications
