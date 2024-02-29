# Octree Library

Octree decomposition is a mechanism to encode a 3D space as a tree. Each node of the tree can have 8 branches. The deeper into the tree the smaller space is
associated with the position in the tree.

## Bounds

A `Bounds` is a kind of box that represents a node in the octree decomposition. The `Bounds` at a given level is decomposed in 8 sub-`Bounds` by splitting 
its volumne in half in all 3 directions.

It is easy to test if a `Point3D` is inside or not of a `Bounds` by calling the `Bounds` method `Contains`. It is also easy to test if a `Bounds` intersects 
another `Bounds` by calling `Intersects`.

Here is a short example:

```csharp
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Octree;

class Example
{
    static void Main()
    {
        Bounds boundsA = new Bounds(-1, 1, -1, 1, -1, 1);
        
        Point3D pointA = new Point3D(0,0,0);
        Console.WriteLine("Is the point: (" + pointA.X + ", " + pointA.Y + ", " + pointA.Z +
            ") contained in the bounds: (" + 
            boundsA.MinX + ", " + boundsA.MaxX + "), (" + boundsA.MinY + ", " + boundsA.MaxY + "), (" + boundsA.MinZ + ", " + boundsA.MaxZ + ") ? " +
            boundsA.Contains(pointA));
        Point3D pointB = new Point3D(2, 0, 0);
        Console.WriteLine("Is the point: (" + pointB.X + ", " + pointB.Y + ", " + pointB.Z +
            ") contained in the bounds: (" +
            boundsA.MinX + ", " + boundsA.MaxX + "), (" + boundsA.MinY + ", " + boundsA.MaxY + "), (" + boundsA.MinZ + ", " + boundsA.MaxZ + ") ? " +
            boundsA.Contains(pointB));
        Bounds boundsB = new Bounds(-2, 0, -1, 0, -0.5, 0);
        Console.WriteLine("Does the bounds (" + boundsB.MinX + ", " + boundsB.MaxX + "), (" + boundsB.MinY + ", " + boundsB.MaxY + "), (" + boundsB.MinZ + ", " + boundsB.MaxZ + 
            ") intersects the bounds (" +
            boundsA.MinX + ", " + boundsA.MaxX + "), (" + boundsA.MinY + ", " + boundsA.MaxY + "), (" + boundsA.MinZ + ", " + boundsA.MaxZ + ") ? " +
            boundsB.Intersects(boundsA));
        Bounds boundsC = new Bounds(-3, -2, -3, -2, -3, -2);
        Console.WriteLine("Does the bounds (" + boundsC.MinX + ", " + boundsC.MaxX + "), (" + boundsC.MinY + ", " + boundsC.MaxY + "), (" + boundsC.MinZ + ", " + boundsC.MaxZ +
            ") intersects the bounds (" +
            boundsA.MinX + ", " + boundsA.MaxX + "), (" + boundsA.MinY + ", " + boundsA.MaxY + "), (" + boundsA.MinZ + ", " + boundsA.MaxZ + ") ? " +
            boundsC.Intersects(boundsA));
    }
}
```

The results on the Console are:
```
Is the point: (0, 0, 0) contained in the bounds: (-1, 1), (-1, 1), (-1, 1) ? True
Is the point: (2, 0, 0) contained in the bounds: (-1, 1), (-1, 1), (-1, 1) ? False
Does the bounds (-2, 0), (-1, 0), (-0,5, 0) intersects the bounds (-1, 1), (-1, 1), (-1, 1) ? True
Does the bounds (-3, -2), (-3, -2), (-3, -2) intersects the bounds (-1, 1), (-1, 1), (-1, 1) ? False
```

## Octree Code
An `OctreeCode` is a 64 bits `struct` that represents a node in an octree encoding. it cannot exceed 19 levels in the octree. The first 5 digits are reserved to contain
the depth of the code in the octree encoding. Each node is saved using 3 bits (because $2^3 = 8 $).

An `OctreeCode` can also be represented as a string. In that case each character is supposed to be between 0 and 7. The number of characters
in the string gives the depth of the encoding. The first element of the string is at the top of the octree. The method `Decode` returns the 
string representation of the `Octree`. The method `TryParse` is used to transform a string representation to an `Octree`.

It is also to decompose an `OctreeCode` is a list of bytes. Each of the byte value does not exceed 7. The first element in the list is at the
top of the octree. The method `DecodeToListOfByte` is used to get the list of bytes reprentation of an `Octree`, while the `TryParse` method is
used to get an `OctreeCode` from a List of bytes.

Here is an example:

```csharp
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
```

The results displayed in the Console are:
```
The string 012345676543210 is encoded as 780000A72EFAC688
The same octree code decodes to: 012345676543210
The same code corresponds to the list: 0 1 2 3 4 5 6 7 6 5 4 3 2 1 0
```

## Octree Code Long
An `OctreeCodeLong` works similarly to an `OctreeCode` excepts that it can encode an `Octree` node up to depthe level 32. It is stored on 128 bits.
The first 5 digits are reserved to contain the depth of the code in the octree encoding.

In practice, an `OctreeCodeLong` is stored on two `ulong`, referred as `CodeHigh` and `CodeLow`. The 5-bits used to encode the depth are store on `CodeHigh`.

## Octree
An `Octree` is a data structure that describes a decomposition of a `Bounds` in `OctreeNode`. The `Bounds` are store in `GlobalBounds`. The octree itself
starts at the `Root`, which is an `OctreeNode`.

An `OctreeNode` has possibly an array of `Nodes`. If this array is not empty, it has a length of 8 and contains the next level of `OctreeNode` in the octree 
decomposition. A leaf is marked by having the `Nodes` set an arrary that contains no elements.

An `OctreeNode` can either be connected to an `OctreeCode` or an `OctreeCodeLong`. 

It is possible to add new nodes in the octree, either using `OctreeCode` or `List<byte>` using the method `Add`. Alternatively, `Point3D` or even coordinates
can be added to the `Octree`.

It is possible to retrieve the code, in the form of a list of `byte` and the `Bounds` for a position (given as a `Point3D`) at a desired depth.

It is also possible to obtain all the leaves of the`Octree`, i.e., the volume that is defined by the `Octree`. The results is a list of `OctreeCode` or 
`OctreeCodeLong`.

Here is an example:

```csharp
using OSDC.DotnetLibraries.General.Octree;
using OSDC.DotnetLibraries.General.Math;

class Example
{
    static void Main()
    {
        Bounds bounds = new Bounds(-1, 1, -1, 1, -1, 1);
        Octree<OctreeCodeLong> octree = new Octree<OctreeCodeLong>(bounds);

        Random rnd = new Random();
        for (int i = 0; i < 10; i++)
        {
            Point3D pt = new Point3D(2*rnd.NextDouble()-1, 2 * rnd.NextDouble() - 1, 2 * rnd.NextDouble() - 1);
            Console.WriteLine("Point : (" +  pt.X + ", " + pt.Y + ", " + pt.Z + ")");
            octree.Add(pt);
        }

        List<OctreeCodeLong>? vol = octree.GetLeaves();
        if (vol != null)
        {
            Console.WriteLine("The current leaves of the volumne");
            foreach (var code in vol)
            {
                Bounds? b = octree.GetBounds(code);
                if (b != null)
                {
                    Console.WriteLine("Bound min: (" + b.MinX + ", " + b.MinY + ", " + b.MinZ + ") + max: (" + b.MaxX + ", " + b.MaxY + ", " + b.MaxZ + ")");
                }
            }
        }
    }
}
```

And the results may look something like this:

```
Point : (-0,9468318435989447, -0,06818247896011287, -0,4471816260823409)
Point : (-0,6841114991343233, -0,5217117504032298, 0,8309187817122348)
Point : (0,49908845144004976, 0,7674486069025055, -0,19562912474549243)
Point : (0,1659787257060541, -0,6473628670901717, 0,6640525431818247)
Point : (-0,12831111895344138, -0,04595916380115472, 0,7532857672957207)
Point : (-0,5345687217409911, 0,6278376617021366, 0,2378697334832991)
Point : (-0,9132693687859381, -0,6589644700155146, 0,4360226650336565)
Point : (-0,3849630507417996, -0,45612808872984045, -0,5088883352833877)
Point : (0,8534908522225166, -0,44632196851464734, -0,4396734742005304)
Point : (-0,8244910629884461, 0,0479643960202758, 0,24201406587613583)
The current leaves of the volumne
Bound min: (-0,38496305141597986, -0,45612808875739574, -0,5088883358985186) + max: (-0,3849630504846573, -0,45612808782607317, -0,508888334967196)
Bound min: (-0,9468318438157439, -0,06818247959017754, -0,4471816262230277) + max: (-0,9468318428844213, -0,06818247865885496, -0,44718162529170513)
Bound min: (0,8534908518195152, -0,4463219689205289, -0,43967347498983145) + max: (0,8534908527508378, -0,4463219679892063, -0,4396734740585089)
Bound min: (0,49908845126628876, 0,7674486069008708, -0,19562912546098232) + max: (0,49908845219761133, 0,7674486078321934, -0,19562912452965975)
Bound min: (-0,9132693689316511, -0,6589644700288773, 0,4360226644203067) + max: (-0,9132693680003285, -0,6589644690975547, 0,43602266535162926)
Bound min: (-0,6841114992275834, -0,5217117508873343, 0,8309187814593315) + max: (-0,6841114982962608, -0,5217117499560118, 0,8309187823906541)
Bound min: (-0,12831111904233694, -0,0459591643884778, 0,7532857665792108) + max: (-0,12831111811101437, -0,04595916345715523, 0,7532857675105333)
Bound min: (0,16597872506827116, -0,6473628673702478, 0,6640525422990322) + max: (0,16597872599959373, -0,6473628664389253, 0,6640525432303548)
Bound min: (-0,8244910631328821, 0,047964395955204964, 0,2420140653848648) + max: (-0,8244910622015595, 0,04796439688652754, 0,24201406631618738)
Bound min: (-0,5345687223598361, 0,6278376616537571, 0,23786973301321268) + max: (-0,5345687214285135, 0,6278376625850797, 0,23786973394453526)
```




