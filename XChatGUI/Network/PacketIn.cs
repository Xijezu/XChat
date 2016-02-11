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

// adapted from

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

using System.IO;
using System;

namespace Rappelz.GameServer.Network
{
    /// <summary>
    /// Reads primitive data types from an underlying stream.
    /// </summary>
    public class PacketIn : MemoryStream
    {
        private static byte[] fbuff = new byte[8];

        /// <summary>
        /// Header size including checksum at the end of the packet
        /// </summary>
        public const ushort HDR_SIZE = 7;

        /// <summary>
        /// Packet size
        /// </summary>
        protected uint m_psize;

        /// <summary>
        /// Packet ID
        /// </summary>
        protected ushort m_id;

        /// <summary>
        /// Checksum
        /// </summary>
        protected byte m_checksum;

        /// <summary>
        /// Checksum
        /// </summary>
        protected TCPConnection m_socket;

        /// <summary>
        /// Gets the packet size
        /// </summary>
        public ushort PacketSize { get { return (ushort)(m_psize + HDR_SIZE); } }

        /// <summary>
        /// Gets the size of the data portion of the packet
        /// </summary>
        public uint DataSize { get { return m_psize; } }

        /// <summary>
        /// Gets the packet ID
        /// </summary>
        public ushort ID { get { return m_id; } }

        /// <summary>
        /// Gets the packet checksum
        /// </summary>
        public byte Checksum { get { return m_checksum; } }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">Size of the internal buffer</param>
        public PacketIn(int size)
            : base(size)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buf">Buffer containing packet data to read from</param>
        /// <param name="start">Starting index into buf</param>
        /// <param name="size">Number of bytes to read from buf</param>
        public PacketIn(byte[] buf, int start, int size)
            : base(size)
        {
            m_psize = Marshal.ConvertToUInt32(buf, start) - HDR_SIZE;
            m_id = Marshal.ConvertToUInt16(buf, start + 4);
            m_checksum = buf[start + 6];

            Position = 0;
            Write(buf, start + 7, size - HDR_SIZE);
            SetLength(size - HDR_SIZE);
            Position = 0;
        }

        #region IPacket Members

        /// <summary>
        /// Generates a human-readable dump of the packet contents.
        /// </summary>
        /// <returns>a string representing the packet contents in hexadecimal</returns>
        public string ToHumanReadable()
        {
            return BitConverter.ToString(ToArray());
        }

        #endregion

        /// <summary>
        /// Reads in 2 bytes and converts it from network to host byte order
        /// </summary>
        /// <returns>A 2 byte (short) value</returns>
        public virtual ushort ReadUInt16()
        {
            var v1 = (byte)ReadByte();
            var v2 = (byte)ReadByte();

            return Marshal.ConvertToUInt16(v2, v1);
        }

        /// <summary>
        /// Reads in 2 bytes and converts it from network to host byte order
        /// </summary>
        /// <returns>A 2 byte (short) value</returns>
        public virtual short ReadInt16()
        {
            var v1 = (byte)ReadByte();
            var v2 = (byte)ReadByte();

            return Marshal.ConvertToInt16(v2, v1);
        }

        /// <summary>
        /// Reads in 4 bytes and converts it from network to host byte order
        /// </summary>
        /// <returns>A 4 byte int value</returns>
        public virtual int ReadInt32()
        {
            var v1 = (byte)ReadByte();
            var v2 = (byte)ReadByte();
            var v3 = (byte)ReadByte();
            var v4 = (byte)ReadByte();

            return Marshal.ConvertToInt32(v4, v3, v2, v1);
        }

        /// <summary>
        /// Reads in 4 bytes and converts it from network to host byte order
        /// </summary>
        /// <returns>A 4 byte int value</returns>
        public virtual float ReadSingle()
        {
            fbuff[0] = (byte)ReadByte();
            fbuff[1] = (byte)ReadByte();
            fbuff[2] = (byte)ReadByte();
            fbuff[3] = (byte)ReadByte();

            return BitConverter.ToSingle(fbuff,0);
        }

        /// <summary>
        /// Reads in 4 bytes and converts it from network to host byte order
        /// </summary>
        /// <returns>A 4 byte uint value</returns>
        public virtual uint ReadUInt32()
        {
            var v1 = (byte)ReadByte();
            var v2 = (byte)ReadByte();
            var v3 = (byte)ReadByte();
            var v4 = (byte)ReadByte();

            return Marshal.ConvertToUInt32(v4, v3, v2, v1);
        }


        /// <summary>
        /// Reads in 8 bytes and converts it from network to host byte order
        /// </summary>
        /// <returns>A 8 byte value</returns>
        public virtual long ReadInt64()
        {
            var v1 = (byte)ReadByte();
            var v2 = (byte)ReadByte();
            var v3 = (byte)ReadByte();
            var v4 = (byte)ReadByte();
            var v5 = (byte)ReadByte();
            var v6 = (byte)ReadByte();
            var v7 = (byte)ReadByte();
            var v8 = (byte)ReadByte();

            return Marshal.ConvertToLong(v8, v7, v6, v5, v4, v3, v2, v1);
        }

        /// <summary>
        /// Reads in 8 bytes and converts it from network to host byte order
        /// </summary>
        /// <returns>A 8 byte value</returns>
        public virtual ulong ReadUInt64()
        {
            var v1 = (byte)ReadByte();
            var v2 = (byte)ReadByte();
            var v3 = (byte)ReadByte();
            var v4 = (byte)ReadByte();
            var v5 = (byte)ReadByte();
            var v6 = (byte)ReadByte();
            var v7 = (byte)ReadByte();
            var v8 = (byte)ReadByte();

            return Marshal.ConvertToULong(v8, v7, v6, v5, v4, v3, v2, v1);
        }

        /// <summary>
        /// Skips 'num' bytes ahead in the stream
        /// </summary>
        /// <param name="num">Number of bytes to skip ahead</param>
        public void Skip(long num)
        {
            Seek(num, SeekOrigin.Current);
        }

        /// <summary>
        /// Reads a null-terminated string from the stream
        /// </summary>
        /// <param name="maxlen">Maximum number of bytes to read in</param>
        /// <returns>A string of maxlen or less</returns>
        public virtual string ReadString(int maxlen)
        {
            var buf = new byte[maxlen];
            Read(buf, 0, maxlen);

            return Marshal.ConvertToString(buf);
        }

        /// <summary>
        /// Reads in a pascal style string
        /// </summary>
        /// <returns>A string from the stream</returns>
        public virtual string ReadPascalString()
        {
            return ReadString(ReadByte());
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            String pid = String.Format("0x{0:X2} ({1})", m_id, m_id);
            return string.Format("PacketIn: Size={0} ID={1} - ({2})", m_psize, pid, BitConverter.ToString(this.ToArray()));

        }
    }
}