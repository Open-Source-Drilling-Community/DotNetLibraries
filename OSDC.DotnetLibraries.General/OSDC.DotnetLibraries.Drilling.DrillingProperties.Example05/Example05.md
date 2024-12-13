# Semantic Queries for `FluidDensitySetPoint`
## Query-DrillingProperties.TestClass-FluidDensitySetPoint-000
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensitySetPoint
WHERE {
	?FluidDensitySetPoint_01 rdf:type ddhub:SetPoint .
	?FluidDensitySetPoint_01 ddhub:HasDynamicValue ?FluidDensitySetPoint .
	?FluidDensitySetPoint_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
}

```
# Semantic Queries for `FluidDensityMargin`
## Query-DrillingProperties.TestClass-FluidDensityMargin-000
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMin ?FluidDensityMax
WHERE {
	?FluidDensityUniform_01 rdf:type ddhub:ComputedData .
	?FluidDensityUniform_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?FDEUncertainty_01 rdf:type ddhub:MinMaxUncertainty .
	?FluidDensityUniform_01 ddhub:HasUncertainty ?FDEUncertainty_01 .
	?FluidDensityMin_01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMax_01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMin_01 ddhub:HasDynamicValue ?FluidDensityMin .
	?FluidDensityMax_01 ddhub:HasDynamicValue ?FluidDensityMax .
	?FDEUncertainty_01 ddhub:HasUncertaintyMin ?FluidDensityMin_01 .
	?FDEUncertainty_01 ddhub:HasUncertaintyMax ?FluidDensityMax_01 .
}

```
# Semantic Queries for `FluidDensityEstimated`
## Query-DrillingProperties.TestClass-FluidDensityEstimated-000
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityEstimated ?FluidDensityEstimatedStdDev
WHERE {
	?FluidDensityEstimated_01 rdf:type ddhub:ComputedData .
	?FluidDensityEstimated_01 ddhub:HasDynamicValue ?FluidDensityEstimated .
	?FluidDensityEstimated_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?FDEUncertainty_01 rdf:type ddhub:GaussianUncertainty .
	?FluidDensityEstimated_01 ddhub:HasUncertainty ?FDEUncertainty_01 .
	?FluidDensityEstimatedStdDev_01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityEstimatedStdDev_01 ddhub:HasStaticValue ?FluidDensityEstimatedStdDev .
	?FDEUncertainty_01 ddhub:HasUncertaintyStandardDeviation ?FluidDensityEstimatedStdDev_01 .
}

```
## Query-DrillingProperties.TestClass-FluidDensityEstimated-001
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityEstimated ?FluidDensityEstimatedStdDev ?factOptionSet
WHERE {
	?FluidDensityEstimated_01 rdf:type ddhub:ComputedData .
	?FluidDensityEstimated_01 ddhub:HasDynamicValue ?FluidDensityEstimated .
	?FluidDensityEstimated_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?FDEUncertainty_01 rdf:type ddhub:GaussianUncertainty .
	?FluidDensityEstimated_01 ddhub:HasUncertainty ?FDEUncertainty_01 .
	?FluidDensityEstimatedStdDev_01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityEstimatedStdDev_01 ddhub:HasStaticValue ?FluidDensityEstimatedStdDev .
	?FDEUncertainty_01 ddhub:HasUncertaintyStandardDeviation ?FluidDensityEstimatedStdDev_01 .
	?FDEUncertainty_01 ddhub:HasUncertaintyMean ?FluidDensityEstimated_01 .
  BIND (' 1' as ?factOptionSet)
}

```
# Semantic Queries for `FluidDensityMeasured`
## Query-DrillingProperties.TestClass-FluidDensityMeasured-000
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured
WHERE {
	?FluidDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured_01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
}

```
## Query-DrillingProperties.TestClass-FluidDensityMeasured-001
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured ?sigma_FluidDensityMeasured ?factOptionSet
WHERE {
	?FluidDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured_01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
	?sigma_FluidDensityMeasured_01 rdf:type ddhub:DrillingDataPoint .
	?sigma_FluidDensityMeasured_01 ddhub:HasValue ?sigma_FluidDensityMeasured .
	?GaussianUncertainty_01 rdf:type ddhub:GaussianUncertainty .
	?FluidDensityMeasured_01 ddhub:HasUncertainty ?GaussianUncertainty_01 .
	?GaussianUncertainty_01 ddhub:HasUncertaintyStandardDeviation ?sigma_FluidDensityMeasured_01 .
  BIND (' 1' as ?factOptionSet)
}

```
## Query-DrillingProperties.TestClass-FluidDensityMeasured-002
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured ?sigma_FluidDensityMeasured ?factOptionSet
WHERE {
	?FluidDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured_01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
	?sigma_FluidDensityMeasured_01 rdf:type ddhub:DrillingDataPoint .
	?sigma_FluidDensityMeasured_01 ddhub:HasValue ?sigma_FluidDensityMeasured .
	?GaussianUncertainty_01 rdf:type ddhub:GaussianUncertainty .
	?FluidDensityMeasured_01 ddhub:HasUncertainty ?GaussianUncertainty_01 .
	?GaussianUncertainty_01 ddhub:HasUncertaintyStandardDeviation ?sigma_FluidDensityMeasured_01 .
	?GaussianUncertainty_01 ddhub:HasUncertaintyMean ?FluidDensityMeasured_01 .
  BIND (' 1 11' as ?factOptionSet)
}

```
## Query-DrillingProperties.TestClass-FluidDensityMeasured-003
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured ?FluidDensityMeasured_prec ?FluidDensityMeasured_acc ?factOptionSet
WHERE {
	?FluidDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured_01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
	?FluidDensityMeasured_prec_01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_prec_01 ddhub:HasValue ?FluidDensityMeasured_prec .
	?FluidDensityMeasured_acc_01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_acc_01 ddhub:HasValue ?FluidDensityMeasured_acc .
	?SensorUncertainty_01 rdf:type ddhub:SensorUncertainty .
	?SensorUncertainty_01 ddhub:HasUncertaintyPrecision ?FluidDensityMeasured_prec_01 .
	?SensorUncertainty_01 ddhub:HasUncertaintyAccuracy ?FluidDensityMeasured_acc_01 .
	?FluidDensityMeasured_01 ddhub:HasUncertainty ?SensorUncertainty_01 .
  BIND (' 2' as ?factOptionSet)
}

```
## Query-DrillingProperties.TestClass-FluidDensityMeasured-004
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured ?FluidDensityMeasured_prec ?FluidDensityMeasured_acc ?factOptionSet
WHERE {
	?FluidDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured_01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
	?FluidDensityMeasured_prec_01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_prec_01 ddhub:HasValue ?FluidDensityMeasured_prec .
	?FluidDensityMeasured_acc_01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_acc_01 ddhub:HasValue ?FluidDensityMeasured_acc .
	?SensorUncertainty_01 rdf:type ddhub:SensorUncertainty .
	?SensorUncertainty_01 ddhub:HasUncertaintyPrecision ?FluidDensityMeasured_prec_01 .
	?SensorUncertainty_01 ddhub:HasUncertaintyAccuracy ?FluidDensityMeasured_acc_01 .
	?FluidDensityMeasured_01 ddhub:HasUncertainty ?SensorUncertainty_01 .
	?SensorUncertainty_01 ddhub:HasUncertaintyMean ?FluidDensityMeasured_01 .
  BIND (' 2 21' as ?factOptionSet)
}

```
## Query-DrillingProperties.TestClass-FluidDensityMeasured-005
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured ?FluidDensityMeasured_fs ?FluidDensityMeasured_prop ?factOptionSet
WHERE {
	?FluidDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured_01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
	?FluidDensityMeasured_fs_01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_fs_01 ddhub:HasValue ?FluidDensityMeasured_fs .
	?FluidDensityMeasured_prop_01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_prop_01 ddhub:HasValue ?FluidDensityMeasured_prop .
	?FullScaleUncertainty_01 rdf:type ddhub:FullScaleUncertainty .
	?FullScaleUncertainty_01 ddhub:HasFullScale ?FluidDensityMeasured_fs_01 .
	?FullScaleUncertainty_01 ddhub:HasProportionError ?FluidDensityMeasured_prop_01 .
	?FluidDensityMeasured_01 ddhub:HasUncertainty ?FullScaleUncertainty_01 .
  BIND (' 3' as ?factOptionSet)
}

```
## Query-DrillingProperties.TestClass-FluidDensityMeasured-006
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?FluidDensityMeasured ?FluidDensityMeasured_fs ?FluidDensityMeasured_prop ?factOptionSet
WHERE {
	?FluidDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?FluidDensityMeasured_01 ddhub:HasDynamicValue ?FluidDensityMeasured .
	?FluidDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?FluidDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?FluidDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
	?FluidDensityMeasured_fs_01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_fs_01 ddhub:HasValue ?FluidDensityMeasured_fs .
	?FluidDensityMeasured_prop_01 rdf:type ddhub:DrillingDataPoint .
	?FluidDensityMeasured_prop_01 ddhub:HasValue ?FluidDensityMeasured_prop .
	?FullScaleUncertainty_01 rdf:type ddhub:FullScaleUncertainty .
	?FullScaleUncertainty_01 ddhub:HasFullScale ?FluidDensityMeasured_fs_01 .
	?FullScaleUncertainty_01 ddhub:HasProportionError ?FluidDensityMeasured_prop_01 .
	?FluidDensityMeasured_01 ddhub:HasUncertainty ?FullScaleUncertainty_01 .
	?FullScaleUncertainty_01 ddhub:HasUncertaintyMean ?FluidDensityMeasured_01 .
  BIND (' 3 31' as ?factOptionSet)
}

```
# Semantic Queries for `CuttingsDensityMeasured`
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-000
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured
WHERE {
	?CuttingsDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured_01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent_01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent_01 rdf:type ddhub:CuttingsComponent .
	?GasComponent_01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?CuttingsComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
  }
}

```
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-001
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured ?sigma_CuttingsDensityMeasured ?factOptionSet
WHERE {
	?CuttingsDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured_01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent_01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent_01 rdf:type ddhub:CuttingsComponent .
	?GasComponent_01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?CuttingsComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
	?sigma_CuttingsDensityMeasured_01 rdf:type ddhub:DrillingDataPoint .
	?sigma_CuttingsDensityMeasured_01 ddhub:HasValue ?sigma_CuttingsDensityMeasured .
	?GaussianUncertainty_01 rdf:type ddhub:GaussianUncertainty .
	?CuttingsDensityMeasured_01 ddhub:HasUncertainty ?GaussianUncertainty_01 .
	?GaussianUncertainty_01 ddhub:HasUncertaintyStandardDeviation ?sigma_CuttingsDensityMeasured_01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
  }
  BIND (' 1' as ?factOptionSet)
}

```
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-002
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured ?sigma_CuttingsDensityMeasured ?factOptionSet
WHERE {
	?CuttingsDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured_01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent_01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent_01 rdf:type ddhub:CuttingsComponent .
	?GasComponent_01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?CuttingsComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
	?sigma_CuttingsDensityMeasured_01 rdf:type ddhub:DrillingDataPoint .
	?sigma_CuttingsDensityMeasured_01 ddhub:HasValue ?sigma_CuttingsDensityMeasured .
	?GaussianUncertainty_01 rdf:type ddhub:GaussianUncertainty .
	?CuttingsDensityMeasured_01 ddhub:HasUncertainty ?GaussianUncertainty_01 .
	?GaussianUncertainty_01 ddhub:HasUncertaintyStandardDeviation ?sigma_CuttingsDensityMeasured_01 .
	?GaussianUncertainty_01 ddhub:HasUncertaintyMean ?CuttingsDensityMeasured_01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
  }
  BIND (' 1 11' as ?factOptionSet)
}

```
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-003
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured ?CuttingsDensityMeasured_prec ?CuttingsDensityMeasured_acc ?factOptionSet
WHERE {
	?CuttingsDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured_01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent_01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent_01 rdf:type ddhub:CuttingsComponent .
	?GasComponent_01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?CuttingsComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
	?CuttingsDensityMeasured_prec_01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_prec_01 ddhub:HasValue ?CuttingsDensityMeasured_prec .
	?CuttingsDensityMeasured_acc_01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_acc_01 ddhub:HasValue ?CuttingsDensityMeasured_acc .
	?SensorUncertainty_01 rdf:type ddhub:SensorUncertainty .
	?SensorUncertainty_01 ddhub:HasUncertaintyPrecision ?CuttingsDensityMeasured_prec_01 .
	?SensorUncertainty_01 ddhub:HasUncertaintyAccuracy ?CuttingsDensityMeasured_acc_01 .
	?CuttingsDensityMeasured_01 ddhub:HasUncertainty ?SensorUncertainty_01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
  }
  BIND (' 2' as ?factOptionSet)
}

```
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-004
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured ?CuttingsDensityMeasured_prec ?CuttingsDensityMeasured_acc ?factOptionSet
WHERE {
	?CuttingsDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured_01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent_01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent_01 rdf:type ddhub:CuttingsComponent .
	?GasComponent_01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?CuttingsComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
	?CuttingsDensityMeasured_prec_01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_prec_01 ddhub:HasValue ?CuttingsDensityMeasured_prec .
	?CuttingsDensityMeasured_acc_01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_acc_01 ddhub:HasValue ?CuttingsDensityMeasured_acc .
	?SensorUncertainty_01 rdf:type ddhub:SensorUncertainty .
	?SensorUncertainty_01 ddhub:HasUncertaintyPrecision ?CuttingsDensityMeasured_prec_01 .
	?SensorUncertainty_01 ddhub:HasUncertaintyAccuracy ?CuttingsDensityMeasured_acc_01 .
	?CuttingsDensityMeasured_01 ddhub:HasUncertainty ?SensorUncertainty_01 .
	?SensorUncertainty_01 ddhub:HasUncertaintyMean ?CuttingsDensityMeasured_01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
  }
  BIND (' 2 21' as ?factOptionSet)
}

```
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-005
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured ?CuttingsDensityMeasured_fs ?CuttingsDensityMeasured_prop ?factOptionSet
WHERE {
	?CuttingsDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured_01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent_01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent_01 rdf:type ddhub:CuttingsComponent .
	?GasComponent_01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?CuttingsComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
	?CuttingsDensityMeasured_fs_01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_fs_01 ddhub:HasValue ?CuttingsDensityMeasured_fs .
	?CuttingsDensityMeasured_prop_01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_prop_01 ddhub:HasValue ?CuttingsDensityMeasured_prop .
	?FullScaleUncertainty_01 rdf:type ddhub:FullScaleUncertainty .
	?FullScaleUncertainty_01 ddhub:HasFullScale ?CuttingsDensityMeasured_fs_01 .
	?FullScaleUncertainty_01 ddhub:HasProportionError ?CuttingsDensityMeasured_prop_01 .
	?CuttingsDensityMeasured_01 ddhub:HasUncertainty ?FullScaleUncertainty_01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
  }
  BIND (' 3' as ?factOptionSet)
}

```
## Query-DrillingProperties.TestClass-CuttingsDensityMeasured-006
```sparql
PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub:<http://ddhub.no/>
PREFIX quantity:<http://ddhub.no/UnitAndQuantity>

SELECT ?CuttingsDensityMeasured ?CuttingsDensityMeasured_fs ?CuttingsDensityMeasured_prop ?factOptionSet
WHERE {
	?CuttingsDensityMeasured_01 rdf:type ddhub:PhysicalData .
	?CuttingsDensityMeasured_01 ddhub:HasDynamicValue ?CuttingsDensityMeasured .
	?CuttingsDensityMeasured_01 ddhub:IsOfMeasurableQuantity quantity:MassDensityDrilling .
	?tos_01 rdf:type ddhub:TopOfStringReferenceLocation .
	?CuttingsDensityMeasured_01 ddhub:IsPhysicallyLocatedAt ?tos_01 .
	?MovingAverage rdf:type ddhub:MovingAverage .
	?CuttingsDensityMeasured_01 ddhub:IsTransformationOutput ?MovingAverage .
	?LiquidComponent_01 rdf:type ddhub:LiquidComponent .
	?CuttingsComponent_01 rdf:type ddhub:CuttingsComponent .
	?GasComponent_01 rdf:type ddhub:GasComponent .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?CuttingsComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
	?CuttingsDensityMeasured_fs_01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_fs_01 ddhub:HasValue ?CuttingsDensityMeasured_fs .
	?CuttingsDensityMeasured_prop_01 rdf:type ddhub:DrillingDataPoint .
	?CuttingsDensityMeasured_prop_01 ddhub:HasValue ?CuttingsDensityMeasured_prop .
	?FullScaleUncertainty_01 rdf:type ddhub:FullScaleUncertainty .
	?FullScaleUncertainty_01 ddhub:HasFullScale ?CuttingsDensityMeasured_fs_01 .
	?FullScaleUncertainty_01 ddhub:HasProportionError ?CuttingsDensityMeasured_prop_01 .
	?CuttingsDensityMeasured_01 ddhub:HasUncertainty ?FullScaleUncertainty_01 .
	?FullScaleUncertainty_01 ddhub:HasUncertaintyMean ?CuttingsDensityMeasured_01 .
  FILTER NOT EXISTS {
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?LiquidComponent_01 .
	?CuttingsDensityMeasured_01 ddhub:ConcernsAFluidComponent ?GasComponent_01 .
  }
  BIND (' 3 31' as ?factOptionSet)
}

```
