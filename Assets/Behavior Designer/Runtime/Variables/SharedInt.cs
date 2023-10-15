using Unity.Mathematics;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedInt : SharedVariable<int>
    {
        public SharedInt()
        {
            Value = int.MinValue;
        }
        public bool IsNull()
        {
            return Value == int.MinValue;
        }
        public static implicit operator SharedInt(int value) { return new SharedInt { mValue = value }; }
    }
}