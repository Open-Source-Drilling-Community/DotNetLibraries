using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Triangle3DWithNormal : Triangle3D
    {
        private Vector3D normal_ = null;

        public Vector3D Normal {
            get
            {
                if (normal_ == null)
                {
                    Vector3D v1 = new Vector3D(Vertex1, Vertex2);
                    Vector3D v2 = new Vector3D(Vertex1, Vertex3);
                    normal_ = v1.CrossProduct(v2);
                    normal_.SetLength(1.0);
                }
                return normal_;
            }
            protected set
            {
                normal_ = value;
            }
        }


    }
}
