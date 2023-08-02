using System.Collections.Generic;

namespace Primer.Shapes
{
    public static class IntegerExtensions
    {
        public static void AddTriangle(this List<int> triangles, int a, int b, int c, bool flip = false)
        {
            if (flip)
            {
                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(b);
                return;
            }
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }
    }
}
