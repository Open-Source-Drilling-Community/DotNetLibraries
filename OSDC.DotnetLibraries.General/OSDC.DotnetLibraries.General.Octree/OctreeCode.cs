
namespace OSDC.DotnetLibraries.General.Octree
{
    public struct OctreeCode : IOctreeCode
    {
        /// <summary>
        /// Contains the depth of the octree in the first 5 digits. The node indexes are saved using 3 bits for each index in the last 3 * #indexes bits of the Code. 
        /// The index to the left corresponds to the first node, while the last index is the leaf
        /// </summary>
        public ulong Code { get; private set; }

        /// <summary>
        /// The depth (number of digits in the stringCode) is saved in the first 5 digits of the Code. We can go up to depth 19 using one long (5 + 3 * 19 = 62, which is less than 64)
        /// </summary>
        private static readonly byte reservedForDepth_ = 5;

        /// <summary>
        /// Parse from stringCode to OctreeCode, where stringCode shall be in octal (e.g base 8)
        /// </summary>
        /// <param name="stringCode"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool TryParse(string stringCode, out OctreeCode code)
        {
            code = new OctreeCode();
            if (string.IsNullOrEmpty(stringCode) || stringCode.Length > 19)
            {
                return false;
            }
            ulong depth = (ulong)stringCode.Length;

            // We use all 64 bits of the Code, so we have to fill it with 59 zeroes
            code.Code = depth << (64 - reservedForDepth_);
            // Then we add each of the integers (0-7) from the stringCode to the Code
            for (int i = 0; i < stringCode.Length; i++)
            {
                // Fetch integers one-by-one from the string
                if (int.TryParse(stringCode.Substring(i, 1), out int index))
                {
                    if (index < 8)
                    {
                        // Convert each integer to a longIndex using 3 bits for each integer (the last index should use the last 3 bits) - add this integer to the Code using bitwise '|'
                        ulong longIndex = (ulong)index << (3 * (stringCode.Length - 1 - i));
                        code.Code |= longIndex;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringCode"></param>
        /// <returns></returns>
        public bool TryParse(string stringCode)
        {
            OctreeCode code;
            if (OctreeCode.TryParse(stringCode, out code))
            {
                Code = code.Code;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Parse from stringCode to List of byte's, where stringCode shall be in octal (e.g base 8)
        /// </summary>
        /// <param name="stringCode"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool TryParse(string stringCode, out List<byte> list)
        {
            OctreeCode oc = new OctreeCode();
            bool result = TryParse(stringCode, out oc);
            list = oc.DecodeToListOfByte();
            return result;
        }

        /// <summary>
        /// Parse from list of byte's to OctreeCode
        /// </summary>
        /// <param name="list"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool TryParse(List<byte> list, out OctreeCode code)
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
            OctreeCode code;
            if (TryParse(list, out code))
            {
                Code = code.Code;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Decode the Code to a list of byte's
        /// </summary>
        /// <returns></returns>
        public List<byte> DecodeToListOfByte()
        {
            string code = Decode();
            List<byte> result = new List<byte>();
            foreach (char c in code.ToCharArray())
            {
                result.Add(Convert.ToByte(Char.GetNumericValue(c)));
            }
            return result;
        }

        /// <summary>
        /// Decode the Code to a list of int's
        /// </summary>
        /// <returns></returns>
        public List<int> DecodeToListOfInt()
        {
            string code = Decode();
            List<int> result = new List<int>();
            foreach (char c in code.ToCharArray())
            {
                result.Add((int)Char.GetNumericValue(c));
            }
            return result;
        }

        /// <summary>
        /// Returns a string which is decoded from the Code containing indexes (0-7)
        /// </summary>
        /// <returns></returns>
        public string Decode()
        {
            // Calculate the maximum number for 5 bits - this will correspond to '11111'
            ulong maxDepth = (ulong)(1 << reservedForDepth_) - 1;
            // Insert enough zeros after the maxDepth such that it corresponds to '1111100000...' (64 digits). Use this maxDepth to mask the Code using bitwise '&'
            ulong lDepth = Code & (maxDepth << 64 - reservedForDepth_);
            // Remove the zeros and avoid filling with one's
            ulong depth = maxDepth & (lDepth >> (64 - reservedForDepth_));
            string result = "";
            if (depth > 19)
                return result;

            // Calculate the maxIndex for a 3-bits value
            ulong maxIndex = (1 << 3) - 1;
            // Insert enough zeros to match the first index we want to extract
            maxIndex = maxIndex << (3 * ((int)depth - 1));
            for (int i = 0; i < (int)depth; i++)
            {
                ulong lIndex = Code & maxIndex;
                ulong index = lIndex >> (3 * ((int)depth - 1 - i));
                result += index.ToString();
                // Prepare for the next index
                maxIndex = maxIndex >> 3;
            }
            return result;
        }
    }
}
