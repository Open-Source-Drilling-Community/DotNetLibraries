using System.Globalization;

namespace OSDC.DotnetLibraries.General.OpenFoamRead
{
    public class OpenFoamGeometry
    {
        public BoundarySurface[] BoundarySurfaces { get; set; }
        public Point3D[] Vertices { get; set; }
        public Face[] Faces { get; set; }
        public int[] Owners { get; set; }
        public int[] Neighbours { get; set; }
        /// <summary>
        /// default constructor
        /// </summary>
        public OpenFoamGeometry() { }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public OpenFoamGeometry(OpenFoamGeometry src)
        {
            if (src != null)
            {
                if (src.BoundarySurfaces != null)
                {
                    src.BoundarySurfaces = new BoundarySurface[src.BoundarySurfaces.Length];
                    src.BoundarySurfaces.CopyTo(src.BoundarySurfaces, 0);
                }
                if (src.Vertices != null)
                {
                    src.Vertices = new Point3D[src.Vertices.Length];
                    src.Vertices.CopyTo(src.Vertices, 0);
                }
                if (src.Faces != null)
                {
                    src.Faces = new Face[src.Faces.Length];
                    src.Faces.CopyTo(src.Faces, 0);
                }
                if (src.Owners != null)
                {
                    src.Owners = new int[src.Owners.Length];
                    src.Owners.CopyTo(src.Owners, 0);
                }
                if (src.Neighbours != null)
                {
                    src.Neighbours = new int[src.Neighbours.Length];
                    src.Neighbours.CopyTo(src.Neighbours, 0);
                }
            }
        }

        public void Read(string folder)
        {
            if (Directory.Exists(folder))
            {
                folder += "\\" + "constant";
                if (Directory.Exists(folder))
                {
                    folder += "\\" + "polyMesh";
                    if (Directory.Exists(folder))
                    {
                        string pointFileName = folder + "\\" + "points";
                        string facesFileName = folder + "\\" + "faces";
                        string boundaryName = folder + "\\" + "boundary";
                        string ownerFileName = folder + "\\" + "owner";
                        string neighbourName = folder + "\\" + "neighbour";
                        if (File.Exists(pointFileName) &&
                            File.Exists(facesFileName) &&
                            File.Exists(boundaryName) &&
                            File.Exists(ownerFileName) &&
                            File.Exists(neighbourName))
                        {
                            using (StreamReader reader = new StreamReader(pointFileName))
                            {
                                bool multilineComment = false;
                                int count = -1;
                                bool start = false;
                                int index = -1;
                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();
                                    if (!string.IsNullOrEmpty(line))
                                    {
                                        line = line.TrimStart(' ');
                                        line = line.TrimEnd(' ');
                                        line = line.TrimStart('\t');
                                        line = line.TrimEnd('\t');
                                        if (line.Length >= 2 && line[0] == '/' && line[1] == '*')
                                        {
                                            multilineComment = true;
                                        }
                                        else if (line.Length >= 2 && line[line.Length - 1] == '/' && line[line.Length - 2] == '*')
                                        {
                                            multilineComment = false;
                                        }
                                        else if (!multilineComment)
                                        {
                                            if (line.Length >= 2 && line[0] == '/' && line[1] == '/')
                                            {
                                                // skip the line
                                            }
                                            else
                                            {
                                                string[] tokens = line.Split(' ', '\t');
                                                if (tokens != null && tokens.Length == 1)
                                                {
                                                    int c;
                                                    if (int.TryParse(tokens[0], out c))
                                                    {
                                                        if (c > 0)
                                                        {
                                                            count = c;
                                                            Vertices = new Point3D[count];
                                                            start = true;
                                                        }
                                                    }
                                                    else if (start && tokens[0] == "(")
                                                    {
                                                        index = 0;
                                                    }
                                                    else if (start && index >= 0 && tokens[0] == ")")
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (index >= 0 && tokens != null && tokens.Length == 3)
                                                {
                                                    if (tokens[0].Length > 0 && tokens[0][0] == '(')
                                                    {
                                                        tokens[0] = tokens[0].Substring(1);
                                                    }
                                                    if (tokens[2].Length > 0 && tokens[2][tokens[2].Length-1] == ')')
                                                    {
                                                        tokens[2] = tokens[2].Substring(0, tokens[2].Length-1); 
                                                    }
                                                    double x, y, z;
                                                    if (index < Vertices.Length &&
                                                        double.TryParse(tokens[0], NumberStyles.Any, CultureInfo.InvariantCulture, out x) &&
                                                        double.TryParse(tokens[1], NumberStyles.Any, CultureInfo.InvariantCulture, out y) &&
                                                        double.TryParse(tokens[2], NumberStyles.Any, CultureInfo.InvariantCulture, out z))
                                                    {
                                                        Vertices[index++] = new Point3D(x, y, z);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            using (StreamReader reader = new StreamReader(facesFileName))
                            {
                                bool multilineComment = false;
                                int count = -1;
                                bool start = false;
                                int index = -1;
                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();
                                    if (!string.IsNullOrEmpty(line))
                                    {
                                        line = line.TrimStart(' ');
                                        line = line.TrimEnd(' ');
                                        line = line.TrimStart('\t');
                                        line = line.TrimEnd('\t');
                                        if (line.Length >= 2 && line[0] == '/' && line[1] == '*')
                                        {
                                            multilineComment = true;
                                        }
                                        else if (line.Length >= 2 && line[line.Length - 1] == '/' && line[line.Length - 2] == '*')
                                        {
                                            multilineComment = false;
                                        }
                                        else if (!multilineComment)
                                        {
                                            if (line.Length >= 2 && line[0] == '/' && line[1] == '/')
                                            {
                                                // skip the line
                                            }
                                            else
                                            {
                                                string[] tokens = line.Split(' ', '\t');
                                                if (tokens != null && tokens.Length == 1)
                                                {
                                                    int c;
                                                    if (int.TryParse(tokens[0], out c))
                                                    {
                                                        if (c > 0)
                                                        {
                                                            count = c;
                                                            Faces = new Face[count];
                                                            start = true;
                                                        }
                                                    }
                                                    else if (start && tokens[0] == "(")
                                                    {
                                                        index = 0;
                                                    }
                                                    else if (start && index >= 0 && tokens[0] == ")")
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (index >= 0 && tokens != null && tokens.Length >= 1)
                                                {
                                                    int idx = tokens[0].IndexOf('(');
                                                    if (idx >= 0 && idx < tokens[0].Length)
                                                    {
                                                        string lead = tokens[0].Substring(0, idx);
                                                        int vertexCount;
                                                        if (!string.IsNullOrEmpty(lead) &&
                                                            int.TryParse(lead, out vertexCount) &&
                                                            vertexCount == tokens.Length)
                                                        {
                                                            Face face = new Face();
                                                            face.Vertices = new int[vertexCount];
                                                            tokens[0] = tokens[0].Substring(idx + 1);
                                                            if (tokens[tokens.Length-1].Length > 0)
                                                            {
                                                                int last = vertexCount- 1;
                                                                if (tokens[last][tokens[last].Length-1] == ')')
                                                                {
                                                                    tokens[last] = tokens[last].Substring(0, tokens[last].Length - 1);
                                                                }
                                                            }
                                                            for (int i = 0; i < vertexCount; i++)
                                                            {
                                                                int ic;
                                                                if (int.TryParse(tokens[i], out ic) &&
                                                                    ic <= Vertices.Length)
                                                                {
                                                                    face.Vertices[i] = ic;
                                                                }
                                                            }
                                                            Faces[index++] = face;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            using (StreamReader reader = new StreamReader(ownerFileName))
                            {
                                bool multilineComment = false;
                                int count = -1;
                                bool start = false;
                                int index = -1;
                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();
                                    if (!string.IsNullOrEmpty(line))
                                    {
                                        line = line.TrimStart(' ');
                                        line = line.TrimEnd(' ');
                                        line = line.TrimStart('\t');
                                        line = line.TrimEnd('\t');
                                        if (line.Length >= 2 && line[0] == '/' && line[1] == '*')
                                        {
                                            multilineComment = true;
                                        }
                                        else if (line.Length >= 2 && line[line.Length - 1] == '/' && line[line.Length - 2] == '*')
                                        {
                                            multilineComment = false;
                                        }
                                        else if (!multilineComment)
                                        {
                                            if (line.Length >= 2 && line[0] == '/' && line[1] == '/')
                                            {
                                                // skip the line
                                            }
                                            else
                                            {
                                                string[] tokens = line.Split(' ', '\t');
                                                if (index < 0 && tokens != null && tokens.Length == 1)
                                                {
                                                    int c;
                                                    if (int.TryParse(tokens[0], out c))
                                                    {
                                                        if (c > 0)
                                                        {
                                                            count = c;
                                                            Owners = new int[count];
                                                            start = true;
                                                        }
                                                    }
                                                    else if (start && tokens[0] == "(")
                                                    {
                                                        index = 0;
                                                    }
                                                }
                                                else if (start && index >= 0 && tokens != null && tokens.Length == 1)
                                                {
                                                    if (tokens[0] == ")")
                                                    {
                                                        break;
                                                    }
                                                    int c;
                                                    if (int.TryParse(tokens[0], out c))
                                                    {
                                                        Owners[index++] = c;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            using (StreamReader reader = new StreamReader(neighbourName))
                            {
                                bool multilineComment = false;
                                int count = -1;
                                bool start = false;
                                int index = -1;
                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();
                                    if (!string.IsNullOrEmpty(line))
                                    {
                                        line = line.TrimStart(' ');
                                        line = line.TrimEnd(' ');
                                        line = line.TrimStart('\t');
                                        line = line.TrimEnd('\t');
                                        if (line.Length >= 2 && line[0] == '/' && line[1] == '*')
                                        {
                                            multilineComment = true;
                                        }
                                        else if (line.Length >= 2 && line[line.Length - 1] == '/' && line[line.Length - 2] == '*')
                                        {
                                            multilineComment = false;
                                        }
                                        else if (!multilineComment)
                                        {
                                            if (line.Length >= 2 && line[0] == '/' && line[1] == '/')
                                            {
                                                // skip the line
                                            }
                                            else
                                            {
                                                string[] tokens = line.Split(' ', '\t');
                                                if (index < 0 && tokens != null && tokens.Length == 1)
                                                {
                                                    int c;
                                                    if (int.TryParse(tokens[0], out c))
                                                    {
                                                        if (c > 0)
                                                        {
                                                            count = c;
                                                            Neighbours = new int[count];
                                                            start = true;
                                                        }
                                                    }
                                                    else if (start && tokens[0] == "(")
                                                    {
                                                        index = 0;
                                                    }
                                                }
                                                else if (start && index >= 0 && tokens != null && tokens.Length == 1)
                                                {
                                                    if (tokens[0] == ")")
                                                    {
                                                        break;
                                                    }
                                                    int c;
                                                    if (int.TryParse(tokens[0], out c))
                                                    {
                                                        Neighbours[index++] = c;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }
    }
}