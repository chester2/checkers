using System;
using System.Collections.Generic;
using System.Linq;

namespace Lib
{
    public static class Util
    {
        public static IEnumerable<T> EnumerateEnum<T>()
            => Enum.GetValues(typeof(T)).Cast<T>();

        public static int CountEnum<T>()
            => Enum.GetValues(typeof(T)).Length;
    }
}
