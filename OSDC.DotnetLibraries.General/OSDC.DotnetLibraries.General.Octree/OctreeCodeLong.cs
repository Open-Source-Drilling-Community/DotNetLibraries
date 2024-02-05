
namespace OSDC.DotnetLibraries.General.Octree
{
    /// <summary>
    /// A class to represent octrees up to depth 31
    /// </summary>
    public struct OctreeCodeLong : IOctreeCode
    {

        private const byte reservedForDepth_ = 5;
        private const byte depthPivot_ = (byte)((sizeof(ulong) * 8 - reservedForDepth_) / 3);

        private ulong Code1 { get; set; }
        private ulong Code2 { get; set; }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public OctreeCodeLong(OctreeCodeLong src)
        {
            Code1 = src.Code1;
            Code2 = src.Code2;
        }
        /// <summary>
        /// code1 is expected to contain the highest significant values (below depthPivot_) and the depth of the octree.
        /// code2 contains the lowest significant values (from depth level depthPivot_ and above)
        /// </summary>
        /// <param name="code1"></param>
        /// <param name="code2"></param>
        public OctreeCodeLong(ulong code1, ulong code2)
        {
            Code1 = code1;
            Code2 = code2;
        }
        /// <summary>
        /// depth is the depth of the octree.
        /// codeHigh is the highest significant values (below depthPivot_).
        /// codeLow is the lowest significant values (above depthPivot_).
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="codeHigh"></param>
        /// <param name="codeLow"></param>
        public OctreeCodeLong(byte depth, ulong codeHigh, ulong codeLow)
        {
            Code1 = codeHigh;
            Code2 = codeLow;
            Code1 &= ~(31UL << (64 - reservedForDepth_));
            Code1 |= (ulong)depth << (64 - reservedForDepth_);
        }

        /// <summary>
        /// create an OctreeCodeLong based on an array of octree indices.
        /// The length of the array defines the depth of the octree.
        /// The highest significant value is at index 0.
        /// The lowest significant value is at the last index of the array.
        /// </summary>
        /// <param name="bytes"></param>
        public OctreeCodeLong(byte[] bytes)
        {
            Code1 = 0L;
            Code2 = 0L;
            Initialize(bytes, bytes.Length);
        }

        /// <summary>
        /// create an OctreeCodeLong based on a string of octree indices.
        /// The length of the string defines the depth of the octree.
        /// The highest significant value is at index 0.
        /// The lowest significant value is at the last index of the array.
        /// </summary>
        public OctreeCodeLong(string stringCode)
        {
            Code1 = 0L;
            Code2 = 0L;
            Initialize(stringCode, stringCode.ToCharArray().Length);
        }

        public static bool TryParse(string stringCode, out OctreeCodeLong code)
        {
            if (!string.IsNullOrEmpty(stringCode))
            {
                code = new OctreeCodeLong(stringCode);
                return true;
            }
            else
            {
                code = new OctreeCodeLong();
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringCode"></param>
        /// <returns></returns>
        public bool TryParse(string stringCode)
        {
            OctreeCodeLong code;
            if (TryParse(stringCode, out code))
            {
                Code1 = code.Code1;
                Code2 = code.Code2;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryParse(List<byte> list, out OctreeCodeLong code)
        {
            string stringCode = "";
            foreach (byte b in list)
                stringCode += Convert.ToString(b);
            return TryParse(stringCode, out code);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool TryParse(List<byte> list)
        {
            OctreeCodeLong code;
            if (TryParse(list, out code))
            {
                Code1 = code.Code1;
                Code2 = code.Code2;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// return a byte array that contains the octree index at each level. 
        /// The highest significant index starts at 0.
        /// The lowest significant index is at the end of the array.
        /// The length of the array is the depth of the octree
        /// </summary>
        /// <returns></returns>
        public byte[] Decode()
        {
            int depth = (int)(Code1 >> (64 - reservedForDepth_));
            byte[] results = new byte[depth];
            int dpt = System.Math.Min(depth, depthPivot_);
            for (int i = 0; i < dpt; i++)
            {
                results[i] = (byte)((Code1 >> ((dpt - 1) * 3 - 3 * i)) & 7UL);
            }
            for (int i = 0; i < depth - depthPivot_; i++)
            {
                results[i + depthPivot_] = (byte)((Code2 >> ((depth - depthPivot_ - 1) * 3 - 3 * i)) & 7UL);
            }
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<byte> DecodeToListOfByte()
        {
            int depth = (int)(Code1 >> (64 - reservedForDepth_));
            List<byte> results = new List<byte>();
            int dpt = System.Math.Min(depth, depthPivot_);
            for (int i = 0; i < dpt; i++)
            {
                results.Add((byte)((Code1 >> ((dpt - 1) * 3 - 3 * i)) & 7UL));
            }
            for (int i = 0; i < depth - depthPivot_; i++)
            {
                results.Add((byte)((Code2 >> ((depth - depthPivot_ - 1) * 3 - 3 * i)) & 7UL));
            }
            return results;
        }

        /// <summary>
        /// return the depth of the octree code
        /// </summary>
        public byte Depth
        {
            get
            {
                return (byte)(Code1 >> (64 - reservedForDepth_));
            }
            set
            {

                Code1 = ((ulong)value) << (64 - reservedForDepth_) | (Code1 & ~(31UL << (64 - reservedForDepth_)));
            }
        }

        /// <summary>
        /// return the highest significant part of the octress, i.e., from levels lesser than depthPivot_ (excluded)
        /// </summary>
        public ulong CodeHigh
        {
            get
            {
                return Code1 & ~(31UL << (64 - reservedForDepth_));
            }
            set
            {
                ulong depth = Code1 >> (64 - reservedForDepth_);
                Code1 = value | (depth << (64 - reservedForDepth_));
            }
        }
        /// <summary>
        /// return the lowest significant part of the octree, i.e., from levels depthPivot_ and above
        /// </summary>
        public ulong CodeLow
        {
            get
            {
                return Code2;
            }
            set
            {
                Code2 = value;
            }
        }
        /// <summary>
        /// truncate the octree code to the given depth
        /// </summary>
        /// <param name="depth"></param>
        public void Truncate(byte depth)
        {
            byte currentDepth = Depth;
            ulong codeHigh = CodeHigh;
            ulong codeLow = CodeLow;
            if (currentDepth > depth)
            {
                byte[] bytes = Decode();
                Initialize(bytes, depth);
            }
        }

        /// <summary>
        /// Initialize an OctreeCodeLong based on an array of octree indices.
        /// The length of the array defines the depth of the octree.
        /// The highest significant value is at index 0.
        /// The lowest significant value is at the last index of the array.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="depth"></param>
        public void Initialize(byte[] bytes, int depth)
        {
            Code1 = 0L;
            Code2 = 0L;
            if (bytes != null)
            {
                if (depth < 32)
                {
                    Code1 = ((ulong)depth << (64 - reservedForDepth_));
                    int dpt = System.Math.Min(depth, depthPivot_);
                    for (int i = 0; i < dpt; i++)
                    {
                        Code1 |= ((ulong)bytes[i] << ((dpt - 1) * 3 - 3 * i));
                    }
                    for (int i = 0; i < depth - depthPivot_; i++)
                    {
                        Code2 |= ((ulong)bytes[i + depthPivot_] << ((depth - depthPivot_ - 1) * 3 - 3 * i));
                    }
                }
            }
        }

        public void Initialize(string stringCode, int depth)
        {
            Code1 = 0L;
            Code2 = 0L;
            if (!string.IsNullOrEmpty(stringCode) && stringCode.Length < 32)
            {
                Code1 = ((ulong)depth << (64 - reservedForDepth_));
                int dpt = System.Math.Min(depth, depthPivot_);
                for (int i = 0; i < dpt; i++)
                {
                    if (ulong.TryParse(stringCode.Substring(i, 1), out ulong ci))
                    {
                        Code1 |= (ci << ((dpt - 1) * 3 - 3 * i));
                    }
                }
                for (int i = 0; i < depth - depthPivot_; i++)
                {
                    if (ulong.TryParse(stringCode.Substring(i + depthPivot_, 1), out ulong ci))
                    {
                        Code2 |= (ci << ((depth - depthPivot_ - 1) * 3 - 3 * i));
                    }
                }
            }
        }

        /// <summary>
        /// "pretty print" of an OctreeCodeLong
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Depth.ToString() + ":" + Convert.ToString((long)CodeHigh, 2) + ":" + Convert.ToString((long)CodeLow, 2);
        }

        /// <summary>
        /// Convert from string to OctreeCodeLong
        /// </summary>
        /// <returns></returns>
        public static OctreeCodeLong FromString(string stringCode)
        {
            string[] split = stringCode.Split(':');
            if (split.Length == 3)
            {
                byte depth = Convert.ToByte(split[0]);
                ulong codeHigh = Convert.ToUInt64(split[1], 2);
                ulong codeLow = Convert.ToUInt64(split[2], 2);
                return new OctreeCodeLong(depth, codeHigh, codeLow);
            }
            return new OctreeCodeLong();
        }

        /// <summary>
        /// check if two codes intersect each other's
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool Intersect(OctreeCodeLong code)
        {
            byte[] bytes1 = Decode();
            byte[] bytes2 = code.Decode();
            bool intersect = true;
            for (int i = 0; i < System.Math.Min(bytes1.Length, bytes2.Length); i++)
            {
                if (bytes1[i] != bytes2[i])
                {
                    intersect = false;
                    break;
                }
            }
            return intersect;
        }
    }
}
