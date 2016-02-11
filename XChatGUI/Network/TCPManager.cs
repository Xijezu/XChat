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

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Rappelz.GameServer.Network
{
    public class TCPManager
    {
        /// <summary>
        /// The maximum number of connections
        /// </summary>
        private int m_numConnections;

        /// <summary>
        /// The buffer size to use for each socket I/O operation 
        /// </summary>
        private int m_BufferSize;

        /// <summary>
        /// Returns the buffer size to use for each socket I/O operation 
        /// </summary>
        public int BufferSize
        {
            get { return m_BufferSize; }
        }

        #region Packet buffer pool

        /// <summary>
        /// Holds all packet buffers.
        /// </summary>
        private Stack<byte[]> m_packetBufPool;

        /// <summary>
        /// Gets the count of packet buffers in the pool.
        /// </summary>
        public int PacketPoolSize
        {
            get { return m_packetBufPool.Count; }
        }

        /// <summary>
        /// Allocates all packet buffers.
        /// </summary>
        /// <returns>success</returns>
        private bool AllocatePacketBuffers()
        {
            int count = m_numConnections * 2;

            m_packetBufPool = new Stack<byte[]>(count);
            for (int i = 0; i < count; i++)
            {
                m_packetBufPool.Push(new byte[m_BufferSize]);
            }
            //Globals.Log.Debug("allocated packet buffers: {0}", count.ToString());

            return true;
        }

        /// <summary>
        /// Gets packet buffer from the pool.
        /// </summary>
        /// <returns>byte array that will be used as packet buffer.</returns>
        private byte[] AcquirePacketBuffer()
        {
            lock (m_packetBufPool)
            {
                if (m_packetBufPool.Count > 0)
                    return m_packetBufPool.Pop();
            }

            //Globals.Log.Warning("packet buffer pool is empty!");

            return new byte[m_BufferSize];
        }

        /// <summary>
        /// Releases previously acquired packet buffer.
        /// </summary>
        /// <param name="buf">The released buf</param>
        public void ReleasePacketBuffer(byte[] buf)
        {
            if (buf == null)
                return;

            lock (m_packetBufPool)
            {
                m_packetBufPool.Push(buf);
            }
        }

        #endregion

        #region SocketAsyncEventArgs pool

        /// <summary>
        /// Holds all packet buffers.
        /// </summary>
        private Stack<SocketAsyncEventArgs> m_socketAsyncEventArgsPool;

        /// <summary>
        /// Gets the count of packet buffers in the pool.
        /// </summary>
        public int SocketAsyncEventArgsPoolSize
        {
            get { return m_socketAsyncEventArgsPool.Count; }
        }

        /// <summary>
        /// Allocates all packet buffers.
        /// </summary>
        /// <returns>success</returns>
        private bool AllocateSocketAsyncEventArgs()
        {
            int count = m_numConnections * 2;

            m_socketAsyncEventArgsPool = new Stack<SocketAsyncEventArgs>(count);
            for (int i = 0; i < count; i++)
            {
                SocketAsyncEventArgs readWriteEventArg = new SocketAsyncEventArgs();
                //readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                readWriteEventArg.UserToken = null;

                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                byte[] buf = AcquirePacketBuffer();
                readWriteEventArg.SetBuffer(buf, 0, buf.Length);

                // add SocketAsyncEventArg to the pool
                m_socketAsyncEventArgsPool.Push(readWriteEventArg);
            }
            //Globals.Log.Debug("allocated packet buffers: {0}", count.ToString());

            return true;
        }

        /// <summary>
        /// Gets packet buffer from the pool.
        /// </summary>
        /// <returns>byte array that will be used as packet buffer.</returns>
        public SocketAsyncEventArgs AcquireAsyncEvent()
        {
            lock (m_socketAsyncEventArgsPool)
            {
                if (m_socketAsyncEventArgsPool.Count > 0)
                    return m_socketAsyncEventArgsPool.Pop();
            }

            //Globals.Log.Warning("SocketAsyncEventArgs pool is empty!");

            SocketAsyncEventArgs readWriteEventArg = new SocketAsyncEventArgs();
            //readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            readWriteEventArg.UserToken = null;

            // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
            byte[] buf = AcquirePacketBuffer();
            readWriteEventArg.SetBuffer(buf, 0, buf.Length);

            // add SocketAsyncEventArg to the pool
            m_socketAsyncEventArgsPool.Push(readWriteEventArg);

            return readWriteEventArg;
        }

        /// <summary>
        /// Releases previously acquired packet buffer.
        /// </summary>
        /// <param name="buf">The released buf</param>
        public void ReleaseAsyncEvent(SocketAsyncEventArgs saea)
        {
            try
            {
                if (saea == null)
                    return;
                saea.SetBuffer(0, m_BufferSize);
                saea.AcceptSocket = null;

                lock (m_socketAsyncEventArgsPool)
                {
                    m_socketAsyncEventArgsPool.Push(saea);
                }
            }
            catch
            {

            }
        }

        #endregion

        /// <summary>
        /// Constructor that takes a server configuration as parameter
        /// </summary>
        /// <param name="config">The configuraion for the server</param>
        public TCPManager(int numConnections, int receiveBufferSize)
        {
            m_numConnections = numConnections;
            m_BufferSize = receiveBufferSize;

            AllocatePacketBuffers();
            AllocateSocketAsyncEventArgs();
        }
    }
}
