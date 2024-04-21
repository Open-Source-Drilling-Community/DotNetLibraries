# Semantic Queries for `DrillingProperties.TestClass`
## Query-DrillingProperties.TestClass-000
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>
PREFIX quantity: <http://ddhub.no/UnitAndQuantity>

SELECT ?ComputedData
WHERE {
	?ComputedData rdf:type ddhub:DynamicDrillingSignal .
	?ComputedData#01 rdf:type ddhub:ComputedData .
	?ComputedData#01 ddhub:HasDynamicValue ?ComputedData .
	?ProcessState rdf:type ddhub:ProcessState .
	?ProcessState ddhub:BelongsToClass ddhub:DeterministicModel .
	?ComputedData#01 ddhub:IsGeneratedBy ?ProcessState .
	?DrillingProcessStateInterpreter rdf:type ddhub:DWISDrillingProcessStateInterpreter .
	?ProcessState ddhub:IsProvidedBy ?DrillingProcessStateInterpreter .
}

```
# Semantic Graph for `DrillingProperties.TestClass`
```mermaid
flowchart TD
	 classDef typeClass fill:#f96;
	 classDef classClass fill:#9dd0ff;
	 classDef opcClass fill:#ff9dd0;
	 classDef quantityClass fill:#d0ff9d;
	Test:ComputedData([Test:ComputedData]) --> opc:string([opc:string]):::opcClass
	Test:ComputedData_01([Test:ComputedData_01]) --> ComputedData([ComputedData]):::typeClass
	Test:ProcessState([Test:ProcessState]) --> ProcessState([ProcessState]):::typeClass
	Test:DrillingProcessStateInterpreter([Test:DrillingProcessStateInterpreter]) --> DWISDrillingProcessStateInterpreter([DWISDrillingProcessStateInterpreter]):::typeClass
	Test:ProcessState([Test:ProcessState]) -- http://ddhub.no/BelongsToClass --> http://ddhub.no/DeterministicModel([http://ddhub.no/DeterministicModel]):::classClass
	Test:ComputedData_01([Test:ComputedData_01]) -- http://ddhub.no/HasDynamicValue --> Test:ComputedData([Test:ComputedData]):::classClass
	Test:ComputedData_01([Test:ComputedData_01]) -- http://ddhub.no/IsGeneratedBy --> Test:ProcessState([Test:ProcessState]):::classClass
	Test:ProcessState([Test:ProcessState]) -- http://ddhub.no/IsProvidedBy --> Test:DrillingProcessStateInterpreter([Test:DrillingProcessStateInterpreter]):::classClass
```

