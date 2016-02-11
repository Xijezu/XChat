/*
 * Rappelz: Endless Odyssey - The first free open source Rappelz server emulator
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

// Adapter from:

/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System.Text;
using System;
using System.Security.Cryptography;

namespace Rappelz.GameServer {
    /// <summary>
    /// Provides basic functionality to convert data types.
    /// </summary>
    public static class Marshal {
        /// <summary>
        /// Reads a null-terminated string from a byte array.
        /// </summary>
        /// <param name="cstyle">the bytes</param>
        /// <returns>the string</returns>
        public static string ConvertToString(byte[] cstyle) {
            if ( cstyle == null )
                return null;

            for ( int i = 0; i < cstyle.Length; i++ ) {
                if ( cstyle[i] == 0 )
                    return Encoding.Default.GetString( cstyle, 0, i );
            }
            return Encoding.Default.GetString( cstyle );
        }

        /// <summary>
        /// Converts 4 bytes to an integer value
        /// in high to low order
        /// </summary>
        /// <param name="val">the bytes</param>
        /// <returns>the integer value</returns>
        public static int ConvertToInt32(byte[] val) {
            return ConvertToInt32( val, 0 );
        }

        /// <summary>
        /// Converts 4 bytes to an integer value
        /// in high to low order
        /// </summary>
        /// <param name="val">the bytes</param>
        /// <param name="startIndex">where to read the values from</param>
        /// <returns>the integer value</returns>
        public static int ConvertToInt32(byte[] val, int startIndex) {
            return ConvertToInt32( val[startIndex + 3], val[startIndex + 2], val[startIndex + 1], val[startIndex + 0] );
        }

        /// <summary>
        /// Converts 4 bytes to an integer value
        /// in high to low order
        /// </summary>
        /// <param name="v1">the first bytes</param>
        /// <param name="v2">the second bytes</param>
        /// <param name="v3">the third bytes</param>
        /// <param name="v4">the fourth bytes</param>
        /// <returns>the integer value</returns>
        public static int ConvertToInt32(int v1, int v2, int v3, int v4) {
            return ( ( v1 << 24 ) | ( v2 << 16 ) | ( v3 << 8 ) | v4 );
        }

        /// <summary>
        /// Converts 8 bytes to an integer value
        /// in high to low order
        /// </summary>
        /// <param name="v1">the first bytes</param>
        /// <param name="v2">the second bytes</param>
        /// <param name="v3">the third bytes</param>
        /// <param name="v4">the fourth bytes</param>
        /// <returns>the integer value</returns>
        public static long ConvertToLong(long v1, long v2, long v3, long v4, long v5, long v6, long v7, long v8) {
            return ( ( v1 << 56 ) | ( v2 << 48 ) | ( v3 << 40 ) | ( v4 << 32 ) | ( v5 << 24 ) | ( v6 << 16 ) | ( v7 << 8 ) | v8 );
        }

        /// <summary>
        /// Converts 8 bytes to an integer value
        /// in high to low order
        /// </summary>
        /// <param name="v1">the first bytes</param>
        /// <param name="v2">the second bytes</param>
        /// <param name="v3">the third bytes</param>
        /// <param name="v4">the fourth bytes</param>
        /// <returns>the integer value</returns>
        public static ulong ConvertToULong(long v1, long v2, long v3, long v4, long v5, long v6, long v7, long v8) {
            return (ulong)( ( v1 << 56 ) | ( v2 << 48 ) | ( v3 << 40 ) | ( v4 << 32 ) | ( v5 << 24 ) | ( v6 << 16 ) | ( v7 << 8 ) | v8 );
        }

        /// <summary>
        /// Converts 4 bytes to an unsigned integer value
        /// in high to low order
        /// </summary>
        /// <param name="val">the bytes</param>
        /// <returns>the integer value</returns>
        public static uint ConvertToUInt32(byte[] val) {
            return ConvertToUInt32( val, 0 );
        }

        /// <summary>
        /// Converts 4 bytes to an unsigned integer value
        /// in high to low order
        /// </summary>
        /// <param name="val">the bytes</param>
        /// <param name="startIndex">where to read the values from</param>
        /// <returns>the integer value</returns>
        public static uint ConvertToUInt32(byte[] val, int startIndex) {
            return ConvertToUInt32( val[startIndex + 3], val[startIndex + 2], val[startIndex + 1], val[startIndex + 0] );
        }

        /// <summary>
        /// Converts 4 bytes to an unsigned integer value
        /// in high to low order
        /// </summary>
        /// <param name="v1">the first bytes</param>
        /// <param name="v2">the second bytes</param>
        /// <param name="v3">the third bytes</param>
        /// <param name="v4">the fourth bytes</param>
        /// <returns>the integer value</returns>
        public static uint ConvertToUInt32(uint v1, uint v2, uint v3, uint v4) {
            return (uint)( ( v1 << 24 ) | ( v2 << 16 ) | ( v3 << 8 ) | v4 );
        }

        /// <summary>
        /// Converts 2 bytes to an short value
        /// in high to low order
        /// </summary>
        /// <param name="val">the bytes</param>
        /// <returns>the integer value</returns>
        public static short ConvertToInt16(byte[] val) {
            return ConvertToInt16( val, 0 );
        }

        /// <summary>
        /// Converts 2 bytes to an short value
        /// in high to low order
        /// </summary>
        /// <param name="val">the bytes</param>
        /// <param name="startIndex">where to read the values from</param>
        /// <returns>the integer value</returns>
        public static short ConvertToInt16(byte[] val, int startIndex) {
            return ConvertToInt16( val[startIndex + 1], val[startIndex + 0] );
        }

        /// <summary>
        /// Converts 2 bytes to an short value
        /// in high to low order
        /// </summary>
        /// <param name="v1">the first bytes</param>
        /// <param name="v2">the second bytes</param>
        /// <returns>the integer value</returns>
        public static short ConvertToInt16(short v1, short v2) {
            return (short)( ( v1 << 8 ) | v2 );
        }

        /// <summary>
        /// Converts 2 bytes to an unsigned short value
        /// in high to low order
        /// </summary>
        /// <param name="val">the bytes</param>
        /// <returns>the integer value</returns>
        public static ushort ConvertToUInt16(byte[] val) {
            return ConvertToUInt16( val, 0 );
        }

        /// <summary>
        /// Converts 2 bytes to an unsigned short value
        /// in high to low order
        /// </summary>
        /// <param name="val">the bytes</param>
        /// <param name="startIndex">where to read the values from</param>
        /// <returns>the integer value</returns>
        public static ushort ConvertToUInt16(byte[] val, int startIndex) {
            return ConvertToUInt16( val[startIndex + 1], val[startIndex + 0] );
        }

        /// <summary>
        /// Converts 2 bytes to an integer value
        /// in high to low order
        /// </summary>
        /// <param name="v1">the first bytes</param>
        /// <param name="v2">the second bytes</param>
        /// <returns>the integer value</returns>
        public static ushort ConvertToUInt16(ushort v1, ushort v2) {
            return (ushort)( v2 | ( v1 << 8 ) );
        }

        /// <summary>
        /// Converts a byte array into a hex dump
        /// </summary>
        /// <param name="description">Dump description</param>
        /// <param name="dump">byte array</param>
        /// <returns>the converted hex dump</returns>
        public static string ToHexDump(string description, byte[] dump) {
            return ToHexDump( description, dump, 0, dump.Length );
        }

        /// <summary>
        /// Converts a byte array into a hex dump
        /// </summary>
        /// <param name="description">Dump description</param>
        /// <param name="dump">byte array</param>
        /// <param name="start">dump start offset</param>
        /// <param name="count">dump bytes count</param>
        /// <returns>the converted hex dump</returns>
        public static string ToHexDump(string description, byte[] dump, int start, int count) {
            var hexDump = new StringBuilder();
            if ( description != null ) {
                hexDump.Append( description ).Append( "\n" );
            }
            int end = start + count;
            for ( int i = start; i < end; i += 16 ) {
                var text = new StringBuilder();
                var hex = new StringBuilder();
                hex.Append( i.ToString( "X4" ) );
                hex.Append( ": " );

                for ( int j = 0; j < 16; j++ ) {
                    if ( j + i < end ) {
                        byte val = dump[j + i];
                        hex.Append( dump[j + i].ToString( "X2" ) );
                        hex.Append( " " );
                        if ( val >= 32 && val <= 127 ) {
                            text.Append( (char)val );
                        }
                        else {
                            text.Append( "." );
                        }
                    }
                    else {
                        hex.Append( "   " );
                        text.Append( " " );
                    }
                }
                hex.Append( "  " );
                hex.Append( text.ToString() );
                hex.Append( '\n' );
                hexDump.Append( hex.ToString() );
            }
            return hexDump.ToString();
        }

        public static byte[] StringToBytes(String StringToConvert) {

            char[] CharArray = StringToConvert.ToCharArray();
            return StringToBytes( StringToConvert, CharArray.Length + 1 );
        }

        public static byte[] StringToBytesNoZero(String StringToConvert) {

            char[] CharArray = StringToConvert.ToCharArray();
            return StringToBytes( StringToConvert, CharArray.Length );
        }

        public static byte[] StringToBytes(String StringToConvert, int ts) {

            char[] CharArray = StringToConvert.ToCharArray();

            byte[] ByteArray = new byte[ts];

            for ( int i = 0; i < ts; i++ ) {
                if ( i < CharArray.Length ) {
                    ByteArray[i] = Convert.ToByte( CharArray[i] );
                }
                else {
                    ByteArray[i] = 0;
                }

            }
            return ByteArray;
        }

        public static string CalculateMD5Hash(string input) {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes( input );
            byte[] hash = md5.ComputeHash( inputBytes );

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for ( int i = 0; i < hash.Length; i++ ) {
                sb.Append( hash[i].ToString( "x2" ) );
            }
            return sb.ToString();
        }

        public static byte ToByte(object Expression) {
            bool isNum;
            byte retNum;
            isNum = Byte.TryParse( Convert.ToString( Expression ), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );
            if ( isNum ) return retNum;
            return 0;
        }

        public static byte ToByte(object Expression, byte def) {
            bool isNum;
            byte retNum;
            isNum = Byte.TryParse( Convert.ToString( Expression ), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );
            if ( isNum ) return retNum;
            return def;
        }

        public static UInt32 ToUInt32(object Expression) {
            bool isNum;
            UInt32 retNum;
            isNum = UInt32.TryParse( Convert.ToString( Expression ), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );
            if ( isNum ) return retNum;
            return 0;
        }

        public static UInt32 ToUInt32(object Expression, UInt32 def) {
            bool isNum;
            UInt32 retNum;
            isNum = UInt32.TryParse( Convert.ToString( Expression ), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );
            if ( isNum ) return retNum;
            return def;
        }

        public static Int32 ToInt32(object Expression) {
            bool isNum;
            Int32 retNum;
            isNum = Int32.TryParse( Convert.ToString( Expression ), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );
            if ( isNum ) return retNum;
            return 0;
        }

        public static Int32 ToInt32(object Expression, Int32 def) {
            bool isNum;
            Int32 retNum;
            isNum = Int32.TryParse( Convert.ToString( Expression ), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );
            if ( isNum ) return retNum;
            return def;
        }

        public static Int64 ToInt64(object Expression) {
            bool isNum;
            long retNum;
            isNum = Int64.TryParse( Convert.ToString( Expression ), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );
            if ( isNum ) return retNum;
            return 0;
        }

        public static Int64 ToInt64(object Expression, Int64 def) {
            bool isNum;
            long retNum;
            isNum = Int64.TryParse( Convert.ToString( Expression ), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );
            if ( isNum ) return retNum;
            return def;
        }

        public static double ToDouble(object Expression, double def) {
            bool isNum;
            double retNum;
            isNum = Double.TryParse( Convert.ToString( Expression ), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );
            if ( isNum ) return retNum;
            return def;
        }

        public static float ToSingle(object Expression) {
            bool isNum;
            float retNum;
            isNum = Single.TryParse( Convert.ToString( Expression ), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );
            if ( isNum ) return retNum;
            return 0.0f;
        }

        public static float ToSingle(object Expression, float def) {
            bool isNum;
            float retNum;
            isNum = Single.TryParse( Convert.ToString( Expression ), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );
            if ( isNum ) return retNum;
            return def;
        }

        /// <summary>
        /// The return value is the high-order word of the specified value.
        /// </summary>
        /// <param name="pDWord"></param>
        /// <returns></returns>
        public static short HiWord(int pDWord) {
            return ( (short)( ( ( pDWord ) >> 16 ) & 0xFFFF ) );
        }


        /// <summary>
        /// The return value is the low-order word of the specified value.
        /// </summary>
        /// <param name="pDWord">The value</param>
        /// <returns></returns>
        public static short LoWord(int pDWord) {
            return ( (short)( pDWord & 0xffff ) );
        }

    }
}