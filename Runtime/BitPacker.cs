using UnityEngine;

namespace SimplePathfinding
{
    public static class BitPacker
    {
        const int G_BITS = 16;
        const int G_MASK = 0xFFFF;
        const int H_MASK = 0xFFFF;

        public static int Pack(int g, int h) => (h << G_BITS) | g;


        public static int UnpackG(int p) => p & G_MASK;
        

        public static int UnpackH(int p) => (p >> G_BITS) & H_MASK;

        public static int UnpackF(int p) => UnpackG(p) + UnpackH(p);
        

        public static int CompareF(int a, int b) => UnpackF(a) - UnpackF(b);

    }
}