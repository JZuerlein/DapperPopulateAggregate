using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public readonly struct CustomerId : IComparable<CustomerId>, IEquatable<CustomerId>
    {
        public int Value { get; }
        public CustomerId(int value) { Value = value; }
        public bool Equals(CustomerId other) => Value == other.Value;
        public int CompareTo(CustomerId other) => Value.CompareTo(other.Value);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CustomerId other && Equals((CustomerId)other);
        }

        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
        public static bool operator ==(CustomerId a, CustomerId b) => a.CompareTo(b) == 0;
        public static bool operator !=(CustomerId a, CustomerId b) => !(a == b);
    }
}
