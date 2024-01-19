# DataManagement

This repository has been created and is maintained by the NORCE Energy Modelling & Simulation team. Its content is donated without any limit or warranty to the Society of Petroleum (SPE) Open Source Drilling 
Community, a sub-committee of the Drilling System Automation Technical Section. Anyone is thus free to use the source code of this repository under its own responsibility.

## MetaInfo

This class is used to decorate data and identify them throughout a HTTP-based REST architecture. The MetaInfo class can be considered as data pointer that helps identify a data through universally unique identifier [uuid](https://en.wikipedia.org/wiki/Universally_unique_identifier), or as is commonly named in the microsoft ecosystem, a globally unique identifier [GUID](https://en.wikipedia.org/wiki/Universally_unique_identifier). In particular, when handling data in a microservice architecture, it is crucial to be able to retrieve the data from provider microservices and to be able to trace their origin. Specific MetaInfo properties such as (host, base path and endpoint) are used to trace the [URI](https://en.wikipedia.org/wiki/Uniform_Resource_Identifier) where these data originate from.
