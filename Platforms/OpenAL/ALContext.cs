using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foster.Framework;

namespace Foster.OpenAL
{
    public struct ALContext : IEquatable<ALContext>
    {
        public static readonly ALContext Null = new ALContext(IntPtr.Zero);

        public IntPtr Handle;

        public ALContext(IntPtr handle)
        {
            Handle = handle;
        }

        public override bool Equals(object obj)
        {
            return obj is ALContext handle && Equals(handle);
        }

        public bool Equals([AllowNull] ALContext other)
        {
            return Handle.Equals(other.Handle);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Handle);
        }

        public static bool operator ==(ALContext left, ALContext right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ALContext left, ALContext right)
        {
            return !(left == right);
        }

        public static implicit operator IntPtr(ALContext context) => context.Handle;
    }
}
