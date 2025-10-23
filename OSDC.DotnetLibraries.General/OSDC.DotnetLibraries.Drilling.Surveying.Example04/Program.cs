// See https://aka.ms/new-console-template for more information
using OSDC.DotnetLibraries.Drilling.Surveying;
double length = 100.0;
SurveyPoint sv1 = new SurveyPoint() { Abscissa = 0, Inclination = 1.0 * System.Math.PI / 180.0, Azimuth = 0, X = 0, Y = 0, Z = 0 };
SurveyPoint sv2 = new SurveyPoint() { Abscissa = sv1.Abscissa + length, Inclination = 10.0 * System.Math.PI / 180.0, Azimuth = 20.0 * System.Math.PI / 180.0 };
bool ok = sv1.CompleteSIA(sv2);
SurveyPoint sv4 = new SurveyPoint();
ok = sv1.InterpolateAtAbscissa(sv2, sv1.Abscissa.Value + 2.0 * SurveyPoint.InterpolationDeltaAbscissa, sv4);
SurveyPoint sv3 = new SurveyPoint() { Abscissa = sv1.Abscissa + length };

if (sv2.Curvature != null && sv4.Toolface != null)
{
    using (StreamWriter writer = new StreamWriter("c:\\temp\\CTC.txt"))
    {
        int n = 1;
        do
        {
            writer.WriteLine("Number of intermediate points: " + n);
            SurveyPoint.CompleteCTCSDT1Step = length / n;
            List<SurveyPoint> inters1 = new List<SurveyPoint>();
            inters1.Add(new SurveyPoint(sv1));
            ok = sv1.CompleteCDTSDT1(sv3, sv2.Curvature.Value, sv4.Toolface.Value, inters1);
            List<SurveyPoint> inters2 = new List<SurveyPoint>();
            foreach (var sv in inters1)
            {
                inters2.Add(new SurveyPoint(sv));
            }
            for (int i = 0; i < inters2.Count-1; i++)
            {
                inters2[i].CompleteSIA(inters1[i + 1]);
            }
            SurveyPoint inter1 = new SurveyPoint();
            SurveyPoint inter2 = new SurveyPoint();
            for (double md = sv1.Abscissa.Value; md <= sv3.Abscissa.Value; md += 1.0)
            {
                bool found1 = false;
                for (int j = 0; j < inters1.Count - 1; j++)
                {
                    if (md >= inters1[j].Abscissa && md <= inters1[j+1].Abscissa)
                    {
                        found1 = true;
                        ok = inters1[j].InterpolateAtAbscissa(inters1[j + 1], md, inter1);
                        break;
                    }
                }
                bool found2 = false;
                for (int j = 0; j < inters2.Count - 1; j++)
                {
                    if (md >= inters2[j].Abscissa && md <= inters2[j + 1].Abscissa)
                    {
                        found2 = true;
                        ok = inters2[j].InterpolateAtAbscissa(inters2[j + 1], md, inter2);
                        break;
                    }
                }
                if (found1 && found2)
                {
                    writer.WriteLine(
                        inter1.Abscissa + "\t" + 
                        (inter1.Inclination * 180.0 / Math.PI).ToString() + "\t" + 
                        (inter1.Azimuth * 180.0 / Math.PI).ToString() + "\t" +
                        inter1.Z + "\t" + inter1.X + "\t" + inter1.Y + "\t" + 
                        (inter1.Curvature * 180.0 * 30.0 / Math.PI).ToString() + "\t" +
                        (inter1.BUR * 180.0 * 30.0 / Math.PI).ToString() + "\t" +
                        (inter1.TUR * 180.0 * 30.0 / Math.PI).ToString() + "\t" + 
                        (inter1.Toolface * 180.0 / Math.PI).ToString() + "\t" +
                        inter2.Abscissa + "\t" +
                        (inter2.Inclination * 180.0 / Math.PI).ToString() + "\t" +
                        (inter2.Azimuth * 180.0 / Math.PI).ToString() + "\t" +
                        inter2.Z + "\t" + inter2.X + "\t" + inter2.Y + "\t" +
                        (inter2.Curvature * 180.0 * 30.0 / Math.PI).ToString() + "\t" +
                        (inter2.BUR * 180.0 * 30.0 / Math.PI).ToString() + "\t" +
                        (inter2.TUR * 180.0 * 30.0 / Math.PI).ToString() + "\t" +
                        (inter2.Toolface * 180.0 / Math.PI).ToString());
                }
            }
            n *= 2;
        } while (n < 2*65536);
        writer.WriteLine();
        for (n = 10; n < 10000; n *= 2)
        {
            SurveyPoint simp = new SurveyPoint() { Abscissa = sv3.Abscissa };
            SurveyPoint.CompleteCTCSDT2Count = n;
            ok = sv1.CompleteCDTSDT2(simp, sv2.Curvature.Value, sv4.Toolface.Value);
            double d = Math.Sqrt((sv3.Z.Value - simp.Z.Value) * (sv3.Z.Value - simp.Z.Value) +
                                      (sv3.X.Value - simp.X.Value) * (sv3.X.Value - simp.X.Value) +
                                      (sv3.Y.Value - simp.Y.Value) * (sv3.Y.Value - simp.Y.Value));
            writer.WriteLine(n + "\t" +
                            simp.Abscissa + "\t" +
                            (simp.Inclination * 180.0 / Math.PI).ToString() + "\t" +
                            (simp.Azimuth * 180.0 / Math.PI).ToString() + "\t" +
                            simp.Z + "\t" +
                            simp.X + "\t" +
                            simp.Y + "\t" +
                            (simp.Curvature * 180.0 * 30.0 / Math.PI).ToString() + "\t" +
                            (simp.BUR * 180.0 * 30.0 / Math.PI).ToString() + "\t" +
                            (simp.TUR * 180.0 * 30.0 / Math.PI).ToString() + "\t" +
                            (simp.Toolface * 180.0 / Math.PI).ToString() + "\t" +d);
        }
    }
}

