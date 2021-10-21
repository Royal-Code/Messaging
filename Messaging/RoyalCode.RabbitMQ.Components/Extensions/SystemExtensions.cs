using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    internal static class SystemExtensions
    {
        /// <summary>
        /// Perform a foreach in the array elements executing the action.
        /// </summary>
        /// <typeparam name="T">The array values type.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="action">The action.</param>
        public static void Each<T>(this T[] array, Action<T> action)
        {
            for (int i = 0; i < array.Length; i++)
            {
                action(array[i]);
            }
        }
    }
}
