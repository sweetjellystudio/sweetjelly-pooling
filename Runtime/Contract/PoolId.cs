namespace SWeetJelly.Pooling.Contract
{
    using System;

    public readonly struct PoolId : IEquatable<PoolId>
    {
        public static readonly PoolId Empty = new PoolId(string.Empty);

        public string Value { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public PoolId(string value)
        {
            Value = value ?? string.Empty;
        }

        public bool Equals(PoolId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is PoolId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(PoolId left, PoolId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PoolId left, PoolId right)
        {
            return !left.Equals(right);
        }

        public static implicit operator PoolId(string value)
        {
            return new PoolId(value);
        }
    }
}