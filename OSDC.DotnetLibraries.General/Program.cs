using System;
using System.Collections.Generic;
using OSDC.DotnetLibraries.Drilling.Surveying;
using OSDC.DotnetLibraries.General.Math;

var start = new SurveyPoint()
{
    X = 0.0,
    Y = 0.0,
    Z = 0.0,
    Abscissa = 0.0,
    Inclination = 1.8894805313071368,
    Azimuth = 2.9623929554282027
};
var computed = new SurveyPoint()
{
    Abscissa = 835.20753115192406,
    Inclination = 1.076558309473163,
    Azimuth = 1.5896388879462602
};
var okForward = start.CompleteCDTSIA(computed, out List<SurveyPoint> forwardSolutions);
Console.WriteLine($"forward ok={okForward}");
Console.WriteLine($"computed xyz=({computed.X:R}, {computed.Y:R}, {computed.Z:R})");
Console.WriteLine($"forward solutions={forwardSolutions.Count}");
for (int idx = 0; idx < forwardSolutions.Count; idx++)
{
    var s = forwardSolutions[idx];
    Console.WriteLine($"fwd[{idx}] s={s.Abscissa:R} i={s.Inclination:R} a={s.Azimuth:R} x={s.X:R} y={s.Y:R} z={s.Z:R}");
}
var recovered = new CurvilinearPoint3D() { X = computed.X, Y = computed.Y, Z = computed.Z };
var okInverse = start.CompleteCDTXYZ(recovered, out List<SurveyPoint> recoveredSolutions);
Console.WriteLine($"inverse ok={okInverse}");
Console.WriteLine($"recovered s={recovered.Abscissa:R} i={recovered.Inclination:R} a={recovered.Azimuth:R} x={recovered.X:R} y={recovered.Y:R} z={recovered.Z:R}");
Console.WriteLine($"recovered solutions={recoveredSolutions.Count}");
for (int idx = 0; idx < recoveredSolutions.Count; idx++)
{
    var s = recoveredSolutions[idx];
    Console.WriteLine($"inv[{idx}] s={s.Abscissa:R} i={s.Inclination:R} a={s.Azimuth:R} x={s.X:R} y={s.Y:R} z={s.Z:R} dls={s.Curvature:R} tf={s.Toolface:R}");
}
