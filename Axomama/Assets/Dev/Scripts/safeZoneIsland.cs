using UnityEngine;

namespace Dev.Scripts
{
    public class SafeZoneIsland
    {
        public SafeZoneIsland(int x, bool y, Vector2 z)
        {
            size = x;
            vertical = y;
            originPoint = z;
            expanded = false;
        }
        
        public int size { get;}
        public bool vertical { get;}
        public Vector2 originPoint { get;}
        
        public bool expanded { get;set;}
    }
}