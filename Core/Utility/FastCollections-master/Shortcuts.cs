using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace FastCollections {
public static class Shortcuts {

        #region ArrayManipulation
        /// <summary>
        /// Shifts all items in array from index min to max by shiftamount. I.e. the item on index min will be shifted onto index min + shiftamount.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="shiftAmount"></param>
        public static void Shift(Array array, int min, int max, int shiftAmount)
        {
            if (shiftAmount == 0) return;
            Array.Copy(array, min, array, min + shiftAmount, max - min);

        }
        /// <summary>
        /// Clears all items in array.
        /// </summary>
        /// <param name="array"></param>
        public static void ClearArray(Array array)
        {
            System.Array.Clear(array, 0, array.Length);
        }
        #endregion
        #region BitMask Manipulation
        //ulong mask
        /// <summary>
        /// Sets the value at bitIndex of a 64 bit mask to true
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        public static void SetBitTrue(ref ulong mask, int bitIndex)
        {
            mask |= (ulong)1 << bitIndex;
        }
        /// <summary>
        /// Sets the value at bitIndex of a 64 bit mask to false
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        public static void SetBitFalse(ref ulong mask, int bitIndex)
        {
            mask &= ~((ulong)1 << bitIndex);
        }
        /// <summary>
        /// Get the value of the bit at bitIndex
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        /// <returns></returns>
        public static bool GetBit(ulong mask, int bitIndex)
        {
            return (mask & ((ulong)1 << bitIndex)) != 0;
        }


        //uint mask
        /// <summary>
        /// Sets the value at bitIndex of a 32 bit mask to true
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        public static void SetBitTrue(ref uint mask, int bitIndex)
        {
            mask |= (uint)1 << bitIndex;
        }
        /// <summary>
        /// Sets the value at bitIndex of a 32 bit mask to false
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        public static void SetBitFalse(ref uint mask, int bitIndex)
        {
            mask &= ~((uint)1 << bitIndex);
        }
        /// <summary>
        /// Get the value of the bit at bitIndex
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        /// <returns></returns>
        public static bool GetBit(uint mask, int bitIndex)
        {
            return (mask & ((uint)1 << bitIndex)) != 0;
        }

        #endregion
    }
}