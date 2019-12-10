using System;

namespace TK.CustomMap
{
    public struct Area
    {
        public Area(double area, AreaUnit unitAbbreviation)
        {
            UnitAbbreviation = unitAbbreviation;
            Value = area;
        }
        public double Value { get; }
        public AreaUnit UnitAbbreviation { get; }

        public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (obj is Area other && Equals(other));
        public static bool operator ==(Area left, Area right) => Equals(left, right);
        public static bool operator !=(Area left, Area right) => !Equals(left, right);
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) UnitAbbreviation;
                hashCode = (hashCode * 397) ^ Value.GetHashCode();
                return hashCode;
            }
        }
        
        private bool Equals(Area other) => 
            Math.Abs(Value - other.Value) <= 0 && UnitAbbreviation == other.UnitAbbreviation;
    }
}