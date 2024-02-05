using OSDC.DotnetLibraries.General.Octree;

class Example
{
    static void Main()
    {
        string str1 = "012345676543210";
        OctreeCode code;
        if (OctreeCode.TryParse(str1, out code))
        {
            Console.WriteLine("The string " + str1 + " is encoded as " + code.Code.ToString("X"));
            string str2 = code.Decode();
            Console.WriteLine("The same octree code decodes to: " + str2);
            List<byte> list = code.DecodeToListOfByte();
            if (list != null)
            {
                Console.Write("The same code corresponds to the list: ");
                foreach (byte b in list)
                {
                    Console.Write(b + " ");
                }
                Console.WriteLine();
            }
        }
    }
}