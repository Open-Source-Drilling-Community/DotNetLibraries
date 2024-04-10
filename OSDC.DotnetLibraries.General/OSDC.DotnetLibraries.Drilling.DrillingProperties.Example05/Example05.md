# Semantic Queries for `FluidDensityMeasured`
## Query-DrillingProperties.TestClass-FluidDensityMeasured-000
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured
WHERE {
	?FluidDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?FluidDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured#01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
}

```
## Query-DrillingProperties.TestClass-FluidDensityMeasured-001
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured, ?sigma_FluidDensityMeasured
WHERE {
	?FluidDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?FluidDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured#01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
	?sigma_FluidDensityMeasured rdf:type ddhub:DrillingSignal .
	?sigma_FluidDensityMeasured#01 rdf:type ddhub:DrillingDataPoint .
	?sigma_FluidDensityMeasured#01 ddhub:HasValue ?sigma_FluidDensityMeasured .
	?GaussianUncertainty#01 rdf:type ddhub:GaussianUncertainty .
	?FluidDensityMeasured#01 ddhub:HasUncertainty ?GaussianUncertainty#01 .
	?GaussianUncertainty#01 ddhub:HasUncertaintyStandardDeviation ?sigma_FluidDensityMeasured#01 .
}

```
## Query-DrillingProperties.TestClass-FluidDensityMeasured-002
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured, ?sigma_FluidDensityMeasured
WHERE {
	?FluidDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?FluidDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured#01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
	?sigma_FluidDensityMeasured rdf:type ddhub:DrillingSignal .
	?sigma_FluidDensityMeasured#01 rdf:type ddhub:DrillingDataPoint .
	?sigma_FluidDensityMeasured#01 ddhub:HasValue ?sigma_FluidDensityMeasured .
	?GaussianUncertainty#01 rdf:type ddhub:GaussianUncertainty .
	?FluidDensityMeasured#01 ddhub:HasUncertainty ?GaussianUncertainty#01 .
	?GaussianUncertainty#01 ddhub:HasUncertaintyStandardDeviation ?sigma_FluidDensityMeasured#01 .
	?GaussianUncertainty#01 ddhub:HasUncertaintyMean ?FluidDensityMeasured#01 .
}

```
## Query-DrillingProperties.TestClass-FluidDensityMeasured-003
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured, ?FluidDensityMeasured_prec, ?FluidDensityMeasured_acc
WHERE {
	?FluidDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?FluidDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured#01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
	?FluidDensityMeasured_prec rdf:type ddhub:DrillingSignal .
	?FluidDensityMeasured_prec#01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_prec#01 ddhub:HasValue ?FluidDensityMeasured_prec .
	?FluidDensityMeasured_acc rdf:type ddhub:DrillingSignal .
	?FluidDensityMeasured_acc#01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_acc#01 ddhub:HasValue ?FluidDensityMeasured_acc .
	?SensorUncertainty#01 rdf:type ddhub:SensorUncertainty .
	?SensorUncertainty#01 ddhub:HasUncertaintyPrecision ?FluidDensityMeasured_prec#01 .
	?SensorUncertainty#01 ddhub:HasUncertaintyAccuracy ?FluidDensityMeasured_acc#01 .
	?FluidDensityMeasured#01 ddhub:HasUncertainty ?SensorUncertainty#01 .
}

```
## Query-DrillingProperties.TestClass-FluidDensityMeasured-004
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured, ?FluidDensityMeasured_prec, ?FluidDensityMeasured_acc
WHERE {
	?FluidDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?FluidDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured#01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
	?FluidDensityMeasured_prec rdf:type ddhub:DrillingSignal .
	?FluidDensityMeasured_prec#01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_prec#01 ddhub:HasValue ?FluidDensityMeasured_prec .
	?FluidDensityMeasured_acc rdf:type ddhub:DrillingSignal .
	?FluidDensityMeasured_acc#01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_acc#01 ddhub:HasValue ?FluidDensityMeasured_acc .
	?SensorUncertainty#01 rdf:type ddhub:SensorUncertainty .
	?SensorUncertainty#01 ddhub:HasUncertaintyPrecision ?FluidDensityMeasured_prec#01 .
	?SensorUncertainty#01 ddhub:HasUncertaintyAccuracy ?FluidDensityMeasured_acc#01 .
	?FluidDensityMeasured#01 ddhub:HasUncertainty ?SensorUncertainty#01 .
	?SensorUncertainty#01 ddhub:HasUncertaintyMean ?FluidDensityMeasured#01 .
}

```
## Query-DrillingProperties.TestClass-FluidDensityMeasured-005
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured, ?FluidDensityMeasured_fs, ?FluidDensityMeasured_prop
WHERE {
	?FluidDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?FluidDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured#01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
	?FluidDensityMeasured_fs rdf:type ddhub:DrillingSignal .
	?FluidDensityMeasured_fs#01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_fs#01 ddhub:HasValue ?FluidDensityMeasured_fs#01 .
	?FluidDensityMeasured_prop rdf:type ddhub:DrillingSignal .
	?FluidDensityMeasured_prop#01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_prop#01 ddhub:HasValue ?FluidDensityMeasured_prop#01 .
	?FullScaleUncertainty#01 rdf:type ddhub:FullScaleUncertainty .
	?FullScaleUncertainty#01 ddhub:HasFullScale ?FluidDensityMeasured_fs#01 .
	?FullScaleUncertainty#01 ddhub:HasProportionError ?FluidDensityMeasured_prop#01 .
	?FluidDensityMeasured#01 ddhub:HasUncertainty ?FullScaleUncertainty#01 .
}

```
## Query-DrillingProperties.TestClass-FluidDensityMeasured-006
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured, ?FluidDensityMeasured_fs, ?FluidDensityMeasured_prop
WHERE {
	?FluidDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?FluidDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured#01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
	?FluidDensityMeasured_fs rdf:type ddhub:DrillingSignal .
	?FluidDensityMeasured_fs#01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_fs#01 ddhub:HasValue ?FluidDensityMeasured_fs#01 .
	?FluidDensityMeasured_prop rdf:type ddhub:DrillingSignal .
	?FluidDensityMeasured_prop#01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_prop#01 ddhub:HasValue ?FluidDensityMeasured_prop#01 .
	?FullScaleUncertainty#01 rdf:type ddhub:FullScaleUncertainty .
	?FullScaleUncertainty#01 ddhub:HasFullScale ?FluidDensityMeasured_fs#01 .
	?FullScaleUncertainty#01 ddhub:HasProportionError ?FluidDensityMeasured_prop#01 .
	?FluidDensityMeasured#01 ddhub:HasUncertainty ?FullScaleUncertainty#01 .
	?FullScaleUncertainty#01 ddhub:HasUncertaintyMean ?FluidDensityMeasured#01 .
}

```
# Semantic Queries for `CuttingsDensityMeasured`
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-000
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured
WHERE {
	?CuttingsDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?CuttingsDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured#01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent#01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent#01 rdf:type ddhub:CuttingsComponent .
	?GasComponent#01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?CuttingsComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
  }}

```
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-001
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured, ?sigma_CuttingsDensityMeasured
WHERE {
	?CuttingsDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?CuttingsDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured#01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent#01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent#01 rdf:type ddhub:CuttingsComponent .
	?GasComponent#01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?CuttingsComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
	?sigma_CuttingsDensityMeasured rdf:type ddhub:DrillingSignal .
	?sigma_CuttingsDensityMeasured#01 rdf:type ddhub:DrillingDataPoint .
	?sigma_CuttingsDensityMeasured#01 ddhub:HasValue ?sigma_CuttingsDensityMeasured .
	?GaussianUncertainty#01 rdf:type ddhub:GaussianUncertainty .
	?CuttingsDensityMeasured#01 ddhub:HasUncertainty ?GaussianUncertainty#01 .
	?GaussianUncertainty#01 ddhub:HasUncertaintyStandardDeviation ?sigma_CuttingsDensityMeasured#01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
  }}

```
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-002
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured, ?sigma_CuttingsDensityMeasured
WHERE {
	?CuttingsDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?CuttingsDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured#01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent#01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent#01 rdf:type ddhub:CuttingsComponent .
	?GasComponent#01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?CuttingsComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
	?sigma_CuttingsDensityMeasured rdf:type ddhub:DrillingSignal .
	?sigma_CuttingsDensityMeasured#01 rdf:type ddhub:DrillingDataPoint .
	?sigma_CuttingsDensityMeasured#01 ddhub:HasValue ?sigma_CuttingsDensityMeasured .
	?GaussianUncertainty#01 rdf:type ddhub:GaussianUncertainty .
	?CuttingsDensityMeasured#01 ddhub:HasUncertainty ?GaussianUncertainty#01 .
	?GaussianUncertainty#01 ddhub:HasUncertaintyStandardDeviation ?sigma_CuttingsDensityMeasured#01 .
	?GaussianUncertainty#01 ddhub:HasUncertaintyMean ?CuttingsDensityMeasured#01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
  }}

```
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-003
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured, ?CuttingsDensityMeasured_prec, ?CuttingsDensityMeasured_acc
WHERE {
	?CuttingsDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?CuttingsDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured#01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent#01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent#01 rdf:type ddhub:CuttingsComponent .
	?GasComponent#01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?CuttingsComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
	?CuttingsDensityMeasured_prec rdf:type ddhub:DrillingSignal .
	?CuttingsDensityMeasured_prec#01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_prec#01 ddhub:HasValue ?CuttingsDensityMeasured_prec .
	?CuttingsDensityMeasured_acc rdf:type ddhub:DrillingSignal .
	?CuttingsDensityMeasured_acc#01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_acc#01 ddhub:HasValue ?CuttingsDensityMeasured_acc .
	?SensorUncertainty#01 rdf:type ddhub:SensorUncertainty .
	?SensorUncertainty#01 ddhub:HasUncertaintyPrecision ?CuttingsDensityMeasured_prec#01 .
	?SensorUncertainty#01 ddhub:HasUncertaintyAccuracy ?CuttingsDensityMeasured_acc#01 .
	?CuttingsDensityMeasured#01 ddhub:HasUncertainty ?SensorUncertainty#01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
  }}

```
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-004
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured, ?CuttingsDensityMeasured_prec, ?CuttingsDensityMeasured_acc
WHERE {
	?CuttingsDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?CuttingsDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured#01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent#01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent#01 rdf:type ddhub:CuttingsComponent .
	?GasComponent#01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?CuttingsComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
	?CuttingsDensityMeasured_prec rdf:type ddhub:DrillingSignal .
	?CuttingsDensityMeasured_prec#01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_prec#01 ddhub:HasValue ?CuttingsDensityMeasured_prec .
	?CuttingsDensityMeasured_acc rdf:type ddhub:DrillingSignal .
	?CuttingsDensityMeasured_acc#01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_acc#01 ddhub:HasValue ?CuttingsDensityMeasured_acc .
	?SensorUncertainty#01 rdf:type ddhub:SensorUncertainty .
	?SensorUncertainty#01 ddhub:HasUncertaintyPrecision ?CuttingsDensityMeasured_prec#01 .
	?SensorUncertainty#01 ddhub:HasUncertaintyAccuracy ?CuttingsDensityMeasured_acc#01 .
	?CuttingsDensityMeasured#01 ddhub:HasUncertainty ?SensorUncertainty#01 .
	?SensorUncertainty#01 ddhub:HasUncertaintyMean ?CuttingsDensityMeasured#01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
  }}

```
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-005
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured, ?CuttingsDensityMeasured_fs, ?CuttingsDensityMeasured_prop
WHERE {
	?CuttingsDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?CuttingsDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured#01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent#01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent#01 rdf:type ddhub:CuttingsComponent .
	?GasComponent#01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?CuttingsComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
	?CuttingsDensityMeasured_fs rdf:type ddhub:DrillingSignal .
	?CuttingsDensityMeasured_fs#01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_fs#01 ddhub:HasValue ?CuttingsDensityMeasured_fs#01 .
	?CuttingsDensityMeasured_prop rdf:type ddhub:DrillingSignal .
	?CuttingsDensityMeasured_prop#01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_prop#01 ddhub:HasValue ?CuttingsDensityMeasured_prop#01 .
	?FullScaleUncertainty#01 rdf:type ddhub:FullScaleUncertainty .
	?FullScaleUncertainty#01 ddhub:HasFullScale ?CuttingsDensityMeasured_fs#01 .
	?FullScaleUncertainty#01 ddhub:HasProportionError ?CuttingsDensityMeasured_prop#01 .
	?CuttingsDensityMeasured#01 ddhub:HasUncertainty ?FullScaleUncertainty#01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
  }}

```
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-006
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured, ?CuttingsDensityMeasured_fs, ?CuttingsDensityMeasured_prop
WHERE {
	?CuttingsDensityMeasured rdf:type ddhub:DynamicDrillingSignal .
	?CuttingsDensityMeasured#01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured#01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured#01 ddhub:IsOfMeasurableQuantity quantity:DrillingDensity .
	?tos#01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured#01 ddhub:IsPhysicallyLocatedAt ?tos#01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured#01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent#01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent#01 rdf:type ddhub:CuttingsComponent .
	?GasComponent#01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?CuttingsComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
	?CuttingsDensityMeasured_fs rdf:type ddhub:DrillingSignal .
	?CuttingsDensityMeasured_fs#01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_fs#01 ddhub:HasValue ?CuttingsDensityMeasured_fs#01 .
	?CuttingsDensityMeasured_prop rdf:type ddhub:DrillingSignal .
	?CuttingsDensityMeasured_prop#01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_prop#01 ddhub:HasValue ?CuttingsDensityMeasured_prop#01 .
	?FullScaleUncertainty#01 rdf:type ddhub:FullScaleUncertainty .
	?FullScaleUncertainty#01 ddhub:HasFullScale ?CuttingsDensityMeasured_fs#01 .
	?FullScaleUncertainty#01 ddhub:HasProportionError ?CuttingsDensityMeasured_prop#01 .
	?CuttingsDensityMeasured#01 ddhub:HasUncertainty ?FullScaleUncertainty#01 .
	?FullScaleUncertainty#01 ddhub:HasUncertaintyMean ?CuttingsDensityMeasured#01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?LiquidComponent#01 .
	?CuttingsDensityMeasured#01 ddhub:ConcernsAFluidComponent ?GasComponent#01 .
  }}

```
