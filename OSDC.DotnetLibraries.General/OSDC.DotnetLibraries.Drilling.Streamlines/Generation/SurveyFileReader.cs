using System.Globalization;

namespace OSDC.DotnetLibraries.Drilling.Streamlines.Generation
{
    /// <summary>
    /// Reads survey files, with or without the ellipse of uncertainty.
    /// <para>
    /// The format is one station per line: measured depth, inclination and azimuth in degrees, and where
    /// present the semi-major axis, the semi-minor axis and the angle of the semi-major axis from the high
    /// side in degrees. Fields are separated by whitespace, semicolons or commas used as separators, the
    /// decimal mark may be a point or a comma, blank lines and lines beginning with a hash or a double
    /// slash are ignored, and a header line is recognised by its first field not being a number.
    /// </para>
    /// </summary>
    public static class SurveyFileReader
    {
        private const double Degree = System.Math.PI / 180.0;

        /// <summary>
        /// reads a file carrying the ellipse of uncertainty at every station
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<WellboreUncertaintyStation> ReadWithUncertainty(string path)
        {
            List<WellboreUncertaintyStation> stations = new List<WellboreUncertaintyStation>();
            foreach (double[] fields in ReadRows(path, 6))
            {
                stations.Add(new WellboreUncertaintyStation(fields[0], fields[1] * Degree, fields[2] * Degree,
                                                            fields[3], fields[4], fields[5] * Degree));
            }
            return stations;
        }

        /// <summary>
        /// reads a file carrying only measured depth, inclination and azimuth, and gives every station the
        /// ellipse the given model says it should have
        /// </summary>
        /// <param name="path"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<WellboreUncertaintyStation> ReadWithNominalUncertainty(
            string path, NominalUncertaintyModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            List<WellboreUncertaintyStation> stations = new List<WellboreUncertaintyStation>();
            foreach (double[] fields in ReadRows(path, 3))
            {
                double md = fields[0];
                stations.Add(new WellboreUncertaintyStation(md, fields[1] * Degree, fields[2] * Degree,
                                                            model.GetSemiMajorAxis(md),
                                                            model.GetSemiMinorAxis(md),
                                                            model.SemiMajorAngle));
            }
            return stations;
        }

        /// <summary>
        /// the numeric rows of a file, each with at least the given number of fields
        /// </summary>
        private static IEnumerable<double[]> ReadRows(string path, int wanted)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            foreach (string raw in File.ReadLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)
                    || line.StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }
                string[] parts = Split(line);
                if (parts.Length < wanted)
                {
                    continue;
                }
                double[] fields = new double[wanted];
                bool usable = true;
                for (int f = 0; f < wanted; f++)
                {
                    if (!TryParse(parts[f], out fields[f]))
                    {
                        usable = false;
                        break;
                    }
                }
                if (usable)
                {
                    yield return fields;
                }
            }
        }

        /// <summary>
        /// Splits a line into fields.
        /// <para>
        /// A comma is a separator only when it is not being used as a decimal mark, which is decided by
        /// whether splitting on whitespace alone already yields enough fields. Files written on a machine
        /// with a comma decimal mark are common and would otherwise be read as twice as many fields, each
        /// half a number.
        /// </para>
        /// </summary>
        private static string[] Split(string line)
        {
            string[] byWhitespace = line.Split(new[] { ' ', '\t', ';' },
                                               StringSplitOptions.RemoveEmptyEntries);
            if (byWhitespace.Length >= 3)
            {
                return byWhitespace;
            }
            return line.Split(new[] { ' ', '\t', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static bool TryParse(string text, out double value)
        {
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
            return double.TryParse(text.Replace(',', '.'), NumberStyles.Float,
                                   CultureInfo.InvariantCulture, out value);
        }
    }
}
