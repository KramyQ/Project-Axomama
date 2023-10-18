using UnityEngine;

namespace Items
{
    public class Item : MonoBehaviour
    {
        public enum EAttachType { None, Left, Right }
        public EAttachType attachType;
        
        public virtual bool StartUse()
        {
            return true;
        }

        public virtual bool StopUse()
        {
            return true;
        }
    }
}