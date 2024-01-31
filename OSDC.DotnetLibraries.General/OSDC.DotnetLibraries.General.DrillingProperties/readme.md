# Background
This package is developed as part of the Society of Petroleum (SPE) Open Source Drilling Community, a sub-committee of the Drilling System Automation Technical Section.

# Purpose
The purpose of this package is to provide a way to describe drilling properties and their uncertainty. There are also Attributes
that allow to decorate the properties. There is also a function to generate a dictionary of the decorations associated with
the drilling properties that can therefore be serialized in json.

# Principles
A `DrillingProperty` is an abstract class that defines two properties: `Value` and `MetaDataID`. A `Value` is a `ContinuousDistribution` 
and therefore represents any continuous probability distrutions. `ContinuousDistribution`and other probability distributions are defined
in `OSDC.DotnetLibraries.General.Statistics`, which is available as a nuget on nuget.org 
([see here](https://www.nuget.org/packages/OSDC.DotnetLibraries.General.Statistics/)). A `MetaDataID` is a `Guid` used to uniquely identified this particular
property in a class. `DrillingProperty` has a method called `Realize` that is used to draw a value (`double?`) using the
probability distribution defined in `Value`. It may return `null`.

`DrillingProperty`has four sub-classes:
- `ScalarDrillingProperty`: used to represent a scalar value with no uncertainty. The value is maintained as a `DiracDistribution`. 
There is redefinition of the `Value` property which is strongly typed to `DiracDistribution`. It is called `DiracDistributionValue`. 
The `Value` property points to the value contained in `DiracDistrutionValue`. The `Realize` method always return the value of the
`DiracDistribution` or `null` if the value is not defined. So the `ScalarDrillingProperty` is equivalent to a fixed value.
- `GaussianDrillingProperty`: used to represent normal distributions defined by a `Mean` and a `StandardDeviation`. The probability
distribution is defined as a `GaussianDistribution`. In order to benefits from strong typing, a property called `GaussianValue` is defined
of the type `GaussianDistribution`. The `Value` property is redefined to point to the instance managed by `GaussianValue`. The `Realize` method
produces values between -&infin; and +&infin; with a mean value corresponding to the `Mean` of the `GaussianValue` and with a 
standard deviation also defined in the `GaussianValue`. 
- `UniformDrillingProperty`: used to represent a uniform distritution between two values, `Min` and `Max`. The probability
distribution is defined as a `UniformDistribution`. A property of type `UniformDistribution` is defined: `UniformValue`. The 
property `Value` is redefined to point to the instance managed by `UniformValue`. The realize method draws uniformely values
in between `Min` and `Max`.
- `GeneralDistributionDrillingProperty`: used to represent a general probability distribution managed as a histogram. The value
is defined by a `GeneralContinuousDistribution` and accessible using the property `GeneralDistributionValue`. The `Value` property is redefined to point to the instance 
managed by `GeneralDistributionValue`. The `GeneralContinuousDistribution` can be defined either by a data set stored in `Data`.
In that case, an histogram is generated from the data set, which is stored in `Function`. Or directly as a given histogram stored in `Function`.
An histogram is represented as an array of `Tuple<double, double>` for which `Item1` is the value of the bin and `Item2` is the
probability of the bin.

## Example
Here is an example. 
<pre>
```csharp
using OSDC.DotnetLibraries.General.DrillingProperties;
using System.Globalization;

namespace DrillingProperties
{
    class TestClass
    {
        public ScalarDrillingProperty Value1 { get; set; } = new ScalarDrillingProperty();
        public UniformDrillingProperty Value2 { get; set; } = new UniformDrillingProperty();
        public GaussianDrillingProperty Value3 { get; set; } = new GaussianDrillingProperty();
        public GeneralDistributionDrillingProperty Value4 { get; set; } = new GeneralDistributionDrillingProperty();
    }
    class Example
    {
        static void Main()
        {
            TestClass testClass = new TestClass();
            testClass.Value1.DiracDistributionValue.Value = 1.0;
            testClass.Value2.UniformValue.Min = -1.0;
            testClass.Value2.UniformValue.Max = 1.0;
            testClass.Value3.GaussianValue.Mean = 10.0;
            testClass.Value3.GaussianValue.StandardDeviation = 0.5;
            testClass.Value4.GeneralDistributionValue.Function = new Tuple<double, double>[] {
                new Tuple<double, double>(0.0, 0.1),
                new Tuple<double, double>(1.0, 0.2),
                new Tuple<double, double>(2.0, 0.3),
                new Tuple<double, double>(3.0, 4.0),
                new Tuple<double, double>(4.0, 0.1)
            };
            for (int i = 0; i < 10; i++)
            {
                Realize(testClass);
            }
        }

        static void Realize(TestClass testClass)
        {
            double? value1 = testClass.Value1.Realize();
            double? value2 = testClass.Value2.Realize();
            double? value3 = testClass.Value3.Realize();
            double? value4 = testClass.Value4.Realize();
            Console.WriteLine("Realized values:" +
                " value1 = " + value1?.ToString("F", CultureInfo.InvariantCulture) +
                " value2 = " + value2?.ToString("F", CultureInfo.InvariantCulture) +
                " value3 = " + value3?.ToString("F", CultureInfo.InvariantCulture) +
                " value4 = " + value4?.ToString("F", CultureInfo.InvariantCulture));
        }
    }
}
```
</pre>

The execution of the program gives:

<pre>
Realized values: value1 = 1.00 value2 = 0.49 value3 = 9.66 value4 = 2.41
Realized values: value1 = 1.00 value2 = -0.08 value3 = 9.93 value4 = 1.93
Realized values: value1 = 1.00 value2 = -0.81 value3 = 10.01 value4 = 0.11
Realized values: value1 = 1.00 value2 = 0.97 value3 = 9.90 value4 = 1.96
Realized values: value1 = 1.00 value2 = 0.19 value3 = 9.74 value4 = 2.40
Realized values: value1 = 1.00 value2 = 0.64 value3 = 10.14 value4 = 1.24
Realized values: value1 = 1.00 value2 = 0.24 value3 = 10.03 value4 = 1.41
Realized values: value1 = 1.00 value2 = 0.62 value3 = 10.49 value4 = 1.15
Realized values: value1 = 1.00 value2 = -0.21 value3 = 10.45 value4 = 2.60
Realized values: value1 = 1.00 value2 = 0.66 value3 = 9.70 value4 = 1.32
</pre>

# Providing Meta Information
There is the possibility to provide meta information with the declation of a `DrillingProperty`. This is achieved using
specific attributes. The possible attributes are:
- `PhysicalQuantityAttribute`: It takes one argument, which is a value of the `PhysicalQuantity.QuantityEnum` defined in the library
`OSDC.UnitConversion.Conversion`. This attribute is to be used to declare the physical quantity of the property, more precisely
a general quantity that is not further specialized as a `DrillingPhysicalQuantity`. This attribute shall be defined only once for the property.
`PhysicalQuantityAttribute` and `DrillingPhysicalQuantityAttribute` are supposed to be exclusive from eachothers, even there is no
enforcement of that rule by the compiler.
- `DrillingPhysicalQuantityAttribute`: It takes one argument of the type `DrillingPhysicalQuantity.QuantityEnum`, which is defined
in the library `OSDC.UnitConversion.Conversion.DrillingEngineering`. This attribute is intended to be used for properties 
that have a physical quantity that is described in `DrillingPhysicalQuantity`. This attribute shall be defined only once for the property.
`PhysicalQuantityAttribute` and `DrillingPhysicalQuantityAttribute` are supposed to be exclusive from eachothers, even there is no
enforcement of that rule by the compiler.
- `MandatoryAttribute`: It takes one argument of the type `CommonProperty.MandatoryType`. This attribute is used to inform whethere
the property is mandatory and in the affirmative in which context. The value `General` means that it is always mandatory.
The value `None` means that it is always optional. Other values can be combined together using a logical "or", therefore allowing
to state that the property can be mandatory in one or several context. Example contexts are: `Mechanical`, `Hydraulic`, `Directional`, ...
This attribute shall be defined only once for the property.
- `SemanticFactAttribute`: It takes three or more arguments: `Subject`, `Verb` and `Object`. `Subject` and `Object` belongs to either the enumeration
`Nouns.Enum` or a `string`, while `Verb` is a choice from the enum `Verbs.Enum`. Both `Nouns.Enum` and `Verbs.Enum` are defined in the library
`DWIS.Vocabulary.Schemas` which contains the vocabulary defined in the D-WIS project (see [D-WIS.org](https://d-wis.org/)). If there are more
than three arguments, the additional one must come in pair and are strings. They correspond to `attribute` and `value` for the `Object`. This 
attribute is used to defined a true assertion about that property. The use of a `string` for the `Subject` or the `Object` is to
refer to internal variables of the semantic definition. This attribute can be used multiple times therefore allowing
to describe multiple facts about the property, i.e., a semantic network.
- `AbscissaReferenceAttribute`: It takes one argument of type `CommonProperty.AbscissaReferenceType`. It allows to specify
the reference of the property with regards to a curvilinear abscissa coordinate system. For example whether a distance is relative
to the top or the bottom of an element.
- `AzimuthReferenceAttribute`: It takes one argument of type `CommonProperty.AzimuthReferenceType`. It allows to specify the
reference of the property with regards to the origin of azimuth.
- `DepthReferenceAttribute`: It takes one argument of type `CommonProperty.DepthReferenceType`. It allows to specify the reference
of the property with regards to the origin of depth.
- `PositionReferenceAttribute`: It takes one argument of type `CommonProperty.PositionReferenceType`. It allows to specify
the reference of the property with regards to the origin of position.
- `PressureReferenceAttribute`: It takes one argument of type `CommonProperty.PressureReferenceType`. It allows to specify
the reference for pressure of the property.

## Example
Here is an example:

<pre>
```csharp
using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using DWIS.Vocabulary.Schemas;

namespace DrillingProperties
{
    public class TestClass
    {
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [DepthReference(CommonProperty.DepthReferenceType.WGS84)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticFact("BitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.BitDepth)]
        [SemanticFact("BitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.DerivedMeasurement)]
        [SemanticFact("BitDepth#01", Verbs.Enum.IsMechanicallyLocatedAt, "Bit#01")]
        [SemanticFact("Bit#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Bit)]
        public GaussianDrillingProperty MeasuredBitDepth { get; set; } = new GaussianDrillingProperty();

        [PhysicalQuantity(PhysicalQuantity.QuantityEnum.StandardLength)]
        [DepthReference(CommonProperty.DepthReferenceType.DrillFloor)]
        [Mandatory(CommonProperty.MandatoryType.PipeHandling| CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.Hydraulic)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.BelongsToClass, Nouns.Enum.HookPosition)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.BelongsToClass, Nouns.Enum.SetPoint)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.IsMechanicallyLocatedAt, "Elevator#01")]
        [SemanticFact("Elevator#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Elevator)]
        public ScalarDrillingProperty BlockPositionSetPoint { get; set;} = new ScalarDrillingProperty();

        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.BlockVelocity)]
        [Mandatory(CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.Hydraulic | CommonProperty.MandatoryType.MaterialTransport)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.BelongsToClass, Nouns.Enum.HookVelocity)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Limit)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.IsMechanicallyLocatedAt, Nouns.Enum.DrillString)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.IsPhysicallyLocatedAt, Nouns.Enum.TopOfStringReferenceLocation)]
        public UniformDrillingProperty TopOfStringSpeedUpwardLimit { get; set;} = new UniformDrillingProperty();

        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [DepthReference(CommonProperty.DepthReferenceType.WGS84)]
        [Mandatory(CommonProperty.MandatoryType.None)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.BitDepth)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.ComputedData)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.IsMechanicallyLocatedAt, "Bit#01")]
        [SemanticFact("Bit#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Bit)]
        [SemanticFact("TransientT&D#01", Verbs.Enum.BelongsToClass, Nouns.Enum.ComputationUnit)]
        [SemanticFact("TransientT&D#01", Verbs.Enum.BelongsToClass, Nouns.Enum.ModelledDegreeOfFreedom, "DegreeOfFreedom", "4")]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.IsComputationOutput, "TransientT&D#01")]
        public GeneralDistributionDrillingProperty EstimatedBitDepth { get; set; } = new GeneralDistributionDrillingProperty();
    }
    class Example
    {
        static void Main()
        {
            TestClass testClass = new TestClass();
            testClass.MeasuredBitDepth.GaussianValue.Mean = 1000.0;
            testClass.MeasuredBitDepth.GaussianValue.StandardDeviation = 0.1;
            testClass.BlockPositionSetPoint.DiracDistributionValue.Value = 10.0;
            testClass.TopOfStringSpeedUpwardLimit.UniformValue.Min = 0.10;
            testClass.TopOfStringSpeedUpwardLimit.UniformValue.Max = 0.11;
            testClass.EstimatedBitDepth.GeneralDistributionValue.Function = new Tuple<double, double>[]
            {
                new Tuple<double, double>(999.8, 0.05),
                new Tuple<double, double>(999.9, 0.10),
                new Tuple<double, double>(1000.0, 0.25),
                new Tuple<double, double>(1000.1, 0.50),
                new Tuple<double, double>(1000.2, 0.08),
                new Tuple<double, double>(1000.3, 0.02)
            };
        }

    }
}```
</pre>

# Transfer of Meta Information via Json
Json schema does not support the possibility to define attributes (C#), annotations (Java), decoration (Python).
As most data exchanges utilize json formatting for the payload, another way to convey the meta information had to be found.
A static method `GenerateDrillingPropertyMetaData.GetDrillingPropertyMetaData` is available to generate a dictionary of `DrillingProperty` 
described in an `Assembly`. The keys of the dictionary are the `Tuple<string, string>` where the first item is the classname and the second item is the property name of the `DrillingProperty`.
The values are instances of the class `MetaDataDrillingProperty`. A `MetaDataDrillingProperty` has the following properties:
- `Namespace`, a string that contains the namespace of the class where this property is defined
- `ClassName`, a string that contains the class name where this property is defined
- `PropertyName`, a string that contains the name of the property
- `AbscissaReferenceType`, which is of type `CommonProperty.AbscissaReferenceType?`
- `AzimuthReferenceType`, which is of type `CommonProperty.AzimuthReferenceType?`
- `DepthReferenceType`, which is of type `CommonProperty.DepthReferenceType?`
- `MandatoryType`, which is of type `CommonProperty.MandatoryType?`
- `PositionReferenceType`, which is of type `CommonProperty.PositionReferenceType?`
- `PressureReferenceType`, which is of type `CommonProperty.PressureReferenceType?`
- `PhysicalQuantity`, which is of type `PhysicalQuantity.QuantityEnum?`
- `DrillingPhysicalQuantity`, which is of type `DrillingPhysicalQuantity.QuantityEnum?`
- `SemanticFacts`, which is of type `List<SemanticFact>?`

This method can be used to generate the meta information of all the properties defined in an `Assembly`. 

The dictionary can be serialized to json and stored on a file together with the json schema to supplement the data model with the attributes, annotations, decorations 
that could not be saved inside the json schema.

In the context of the generation of code from a json schema, the dictionary can be used to add attributes (C#), annotations (Java) or decoration (Python)
to the generated properties in the classes.

In the context of a micro-service architecture, the generated dictionary can be made available through the `Get` interface of a 
specific end-point of the micro-service.

## Example
Here is an example.

<pre>
```csharp
using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using DWIS.Vocabulary.Schemas;
using System.Reflection;
using System.Text.Json;

namespace DrillingProperties
{
    public class TestClass
    {
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [DepthReference(CommonProperty.DepthReferenceType.WGS84)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticFact("BitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.BitDepth)]
        [SemanticFact("BitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.DerivedMeasurement)]
        [SemanticFact("BitDepth#01", Verbs.Enum.IsMechanicallyLocatedAt, "Bit#01")]
        [SemanticFact("Bit#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Bit)]
        public GaussianDrillingProperty MeasuredBitDepth { get; set; } = new GaussianDrillingProperty();

        [PhysicalQuantity(PhysicalQuantity.QuantityEnum.StandardLength)]
        [DepthReference(CommonProperty.DepthReferenceType.DrillFloor)]
        [Mandatory(CommonProperty.MandatoryType.PipeHandling | CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.Hydraulic)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.BelongsToClass, Nouns.Enum.HookPosition)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.BelongsToClass, Nouns.Enum.SetPoint)]
        [SemanticFact("BlockPositionSP#01", Verbs.Enum.IsMechanicallyLocatedAt, "Elevator#01")]
        [SemanticFact("Elevator#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Elevator)]
        public ScalarDrillingProperty BlockPositionSetPoint { get; set; } = new ScalarDrillingProperty();

        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.BlockVelocity)]
        [Mandatory(CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.Hydraulic | CommonProperty.MandatoryType.MaterialTransport)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.BelongsToClass, Nouns.Enum.HookVelocity)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Limit)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.IsMechanicallyLocatedAt, Nouns.Enum.DrillString)]
        [SemanticFact("TopOfStringVelocityUpward#01", Verbs.Enum.IsPhysicallyLocatedAt, Nouns.Enum.TopOfStringReferenceLocation)]
        public UniformDrillingProperty TopOfStringSpeedUpwardLimit { get; set; } = new UniformDrillingProperty();

        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [DepthReference(CommonProperty.DepthReferenceType.WGS84)]
        [Mandatory(CommonProperty.MandatoryType.None)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.BitDepth)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.BelongsToClass, Nouns.Enum.ComputedData)]
        [SemanticFact("EstimatedBitDepth#01", Verbs.Enum.IsMechanicallyLocatedAt, "Bit#01")]
        [SemanticFact("Bit#01", Verbs.Enum.BelongsToClass, Nouns.Enum.Bit)]
        public GeneralDistributionDrillingProperty EstimatedBitDepth { get; set; } = new GeneralDistributionDrillingProperty();
    }
    class Example
    {
        static void Main()
        {          
            var dict = GenerateDrillingPropertyMetaData.GetDrillingPropertyMetaData(Assembly.GetExecutingAssembly());
            if (dict != null)
            {
                foreach (var keyValue in dict)
                {
                    Console.WriteLine("(" + keyValue.Key.Item1 + ", " + keyValue.Key.Item2 + ") " + "=" + JsonSerializer.Serialize(keyValue.Value));
                }
            }
        }

    }
}
```
</pre>

The output is the following:

<pre>
(TestClass, MeasuredBitDepth) ={"Namespace":"DrillingProperties","ClassName":"TestClass","PropertyName":"MeasuredBitDepth","DepthReferenceType":1,"MandatoryType":65535,"DrillingPhysicalQuantity":3,"SemanticFacts":[{"SubjectName":"BitDepth#01","Verb":59,"Object":79,"ObjectAttributes":[]},{"SubjectName":"BitDepth#01","Verb":59,"Object":138,"ObjectAttributes":[]},{"SubjectName":"BitDepth#01","Verb":95,"ObjectName":"Bit#01","ObjectAttributes":[]},{"SubjectName":"Bit#01","Verb":59,"Object":163,"ObjectAttributes":[]}]}
(TestClass, BlockPositionSetPoint) ={"Namespace":"DrillingProperties","ClassName":"TestClass","PropertyName":"BlockPositionSetPoint","DepthReferenceType":2,"MandatoryType":19,"PhysicalQuantity":65,"SemanticFacts":[{"SubjectName":"BlockPositionSP#01","Verb":59,"Object":91,"ObjectAttributes":[]},{"SubjectName":"BlockPositionSP#01","Verb":59,"Object":124,"ObjectAttributes":[]},{"SubjectName":"BlockPositionSP#01","Verb":95,"ObjectName":"Elevator#01","ObjectAttributes":[]},{"SubjectName":"Elevator#01","Verb":59,"Object":182,"ObjectAttributes":[]}]}
(TestClass, TopOfStringSpeedUpwardLimit) ={"Namespace":"DrillingProperties","ClassName":"TestClass","PropertyName":"TopOfStringSpeedUpwardLimit","MandatoryType":11,"DrillingPhysicalQuantity":0,"SemanticFacts":[{"SubjectName":"TopOfStringVelocityUpward#01","Verb":59,"Object":92,"ObjectAttributes":[]},{"SubjectName":"TopOfStringVelocityUpward#01","Verb":59,"Object":134,"ObjectAttributes":[]},{"SubjectName":"TopOfStringVelocityUpward#01","Verb":95,"Object":176,"ObjectAttributes":[]},{"SubjectName":"TopOfStringVelocityUpward#01","Verb":99,"Object":274,"ObjectAttributes":[]}]}
(TestClass, EstimatedBitDepth) ={"Namespace":"DrillingProperties","ClassName":"TestClass","PropertyName":"EstimatedBitDepth","DepthReferenceType":1,"MandatoryType":0,"DrillingPhysicalQuantity":3,"SemanticFacts":[{"SubjectName":"EstimatedBitDepth#01","Verb":59,"Object":79,"ObjectAttributes":[]},{"SubjectName":"EstimatedBitDepth#01","Verb":59,"Object":140,"ObjectAttributes":[]},{"SubjectName":"EstimatedBitDepth#01","Verb":95,"ObjectName":"Bit#01","ObjectAttributes":[]},{"SubjectName":"Bit#01","Verb":59,"Object":163,"ObjectAttributes":[]}]}
</pre>

# Dependence
This library depends on the following nugets:

- `DWIS.Vocabulary.Schemas`
- `OSDC.DotnetLibraries.General.Statistics`
- `OSDC.UnitConversion.Conversion.DrillingEngineering`


