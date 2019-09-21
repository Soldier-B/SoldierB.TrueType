using System;

namespace SoldierB.TrueType
{
    public sealed class InvalidFontException : Exception
    {
        internal InvalidFontException() : base() { }
        internal InvalidFontException(string message) : base(message) { }
    }
}
