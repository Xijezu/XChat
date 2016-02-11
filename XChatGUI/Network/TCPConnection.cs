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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Rappelz.GameServer;
using System.Net;

namespace Rappelz.GameServer.Network
{
    /// <summary>
    /// Base class representing a game client.
    /// </summary>
    public class TCPConnection
    {
        /// <summary>
        /// The network manager for this connection
        /// </summary>
        protected TCPManager m_iocp;

        /// <summary>
        /// The network handler for this listener
        /// </summary>
        protected INetworkHandler m_networkHandler;

        /// <summary>
        /// The socket for the client's connection to the server.
        /// </summary>
        private Socket m_socket;

        private SocketAsyncEventArgs m_readEventArgs;
        private SocketAsyncEventArgs m_writeEventArgs;

        private Config.ConfigNet m_config;

        public ConnectionTag Tag;

        /// <summary>
        /// The current offset into the receive buffer.
        /// </summary>
        protected int m_pBufOffset;

        /// <summary>
        /// Holds the encoding used to encrypt the packets
        /// </summary>
        protected readonly ICipher m_encoding;

        /// <summary>
        /// Holds the encoding used to decrypt the packets
        /// </summary>
        protected readonly ICipher m_decoding;

        /// <summary>
        /// Session for this game server
        /// </summary>
        private object m_session;

        /// <summary>
        /// The client TCP packet send queue
        /// </summary>
        protected readonly Queue<byte[]> m_tcpQueue = new Queue<byte[]>(256);

        /// <summary>
        /// Indicates whether data is currently being sent to the client
        /// </summary>
        protected bool m_sendingTcp;

        /// <summary>
        /// Gets the client's TCP endpoint address, if connected.
        /// </summary>
        public string TcpEndpointAddress
        {
            get
            {
                if (m_socket != null && m_socket.Connected && m_socket.RemoteEndPoint != null)
                    return ((IPEndPoint)m_socket.RemoteEndPoint).Address.ToString();

                return "not connected";
            }
        }

        /// <summary>
        /// Gets the client's TCP endpoint, if connected.
        /// </summary>
        public string TcpEndpoint
        {
            get
            {
                if (m_socket != null && m_socket.Connected && m_socket.RemoteEndPoint != null)
                    return m_socket.RemoteEndPoint.ToString();

                return "not connected";
            }
        }

        /// <summary>
        /// Gets the client's TCP endpoint, if connected.
        /// </summary>
        public string TcpIpAddress
        {
            get
            {
                if (m_socket != null && m_socket.Connected && m_socket.RemoteEndPoint != null)
                {
                    IPEndPoint ip = (IPEndPoint)m_socket.RemoteEndPoint;
                    return ip.Address.ToString();
                }

                return "not connected";
            }
        }

        /// <summary>
        /// Gets the encoding for this processor
        /// </summary>
        public ICipher Encoding
        {
            get { return m_encoding; }
        }

        /// <summary>
        /// Gets the decoding for this processor
        /// </summary>
        public ICipher Decoding
        {
            get { return m_decoding; }
        }

        /// <summary>
        /// Gets the current server session for this server socket
        /// </summary>
        public object Session
        {
            get { return m_session; }
            set { m_session = value; }
        }

        public Socket Socket
        {
            get { return m_socket; }
            set { m_socket = value; }
        }

        /// <summary>
        /// Retrieves the server configuration
        /// </summary>
        public virtual Config.ConfigNet Configuration
        {
            get { return m_config; }
        }


        public TCPConnection(TCPManager iocp, Socket socket, INetworkHandler handler, Config.ConfigNet config)
        {
            m_config = config;
            m_iocp = iocp;
            m_networkHandler = handler;
            m_socket = socket;
            if (m_socket == null)
            {
                InitOutgoingSocket();
            }

            if (config.Encrypted)
            {
                m_encoding = new XRC4Cipher();
                m_decoding = new XRC4Cipher();
            }
            else
            {
                m_encoding = new XCipher();
                m_decoding = new XCipher();
            }
            m_encoding.SetKey("}h79q~B%al;k'y $E");
            m_decoding.SetKey("}h79q~B%al;k'y $E");


            m_readEventArgs = m_iocp.AcquireAsyncEvent();
            m_readEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            m_readEventArgs.UserToken = this;

            m_writeEventArgs = m_iocp.AcquireAsyncEvent();
            m_writeEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            m_writeEventArgs.UserToken = this;

            m_readEventArgs.AcceptSocket = m_socket;
            m_writeEventArgs.AcceptSocket = m_socket;
        }

        private void InitOutgoingSocket()
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            m_socket.Connect(m_config.ListenIp, m_config.Port);
            StartReceive();
        }

        /// <summary>
        /// Starts listening for incoming data.
        /// </summary>
        public void StartReceive()
        {
            if (m_socket != null && m_socket.Connected)
            {
                int bufSize = m_iocp.BufferSize;

                if (m_pBufOffset >= bufSize) //Do we have space to receive?
                {
                    ////Globals.Log.Error(TcpEndpoint + " disconnected because of buffer overflow!");
                    //Globals.Log.Error("m_pBufOffset=" + m_pBufOffset + "; buf size=" + bufSize);
                    ////Globals.Log.Error(BitConverter.ToString(m_tcpRecvBuffer));

                    /*
                                        if (m_srvr != null)
                                            m_srvr.Disconnect(this);
                    */
                }
                else
                {
                    // As soon as the client is connected, post a receive to the connection
                    m_readEventArgs.SetBuffer(m_pBufOffset, bufSize - m_pBufOffset);
                    bool willRaiseEvent = m_socket.ReceiveAsync(m_readEventArgs);
                    if (!willRaiseEvent)
                    {
                        ProcessReceive(m_readEventArgs);
                    }
                }
            }
        }

        // This method is called whenever a receive or send operation is completed on a socket 
        //
        // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

        }

        // This method is invoked when an asynchronous receive operation completes. 
        // If the remote host closed the connection, then the socket is closed.  
        // If data was received then the data is echoed back to the client.
        //
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            TCPConnection token = null;

            try
            {
                token = e.UserToken as TCPConnection;
                int numBytes = e.BytesTransferred;

                if (numBytes > 0 && e.SocketError == SocketError.Success)
                {
                    token.OnReceive(e.Buffer, numBytes);
                    token.StartReceive();
                }
                else
                {
                    //Console.WriteLine(" - Disconnecting client (" + token.TcpEndpoint + "), received bytes=" + numBytes);

                    if (token != null)
                        token.Disconnect();
                    return;
                }
            }
            catch (ObjectDisposedException)
            {
                if (token != null)
                    token.Disconnect();
            }
            catch (SocketException ex)
            {
                if (token != null)
                {
                    Console.WriteLine(  string.Format("{0}  {1}", token.TcpEndpoint, ex.Message));

                    token.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine( "OnReceiveHandler error: {0}", ex);

                if (token != null)
                    token.Disconnect();
            }

        }

        // This method is invoked when an asynchronous send operation completes.  
        // The method issues another receive on the socket to read any additional 
        // data sent from the client
        //
        // <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            TCPConnection token = e.UserToken as TCPConnection;
            if (e.SocketError == SocketError.Success)
            {
                try
                {
                    Queue<byte[]> q = token.m_tcpQueue;

                    int sent = e.BytesTransferred;

                    int count = 0;
                    byte[] data = e.Buffer;

                    if (data == null)
                        return;

                    lock (q)
                    {
                        if (q.Count > 0)
                        {
                            //						Log.WarnFormat("async sent {0} bytes, sending queued packets count: {1}", sent, q.Count);
                            count = CombinePackets(data, q, data.Length, token);
                        }
                        if (count <= 0)
                        {
                            //						Log.WarnFormat("async sent {0} bytes", sent);
                            token.m_sendingTcp = false;
                            return;
                        }
                    }

                    int start = Environment.TickCount;
                    token.m_writeEventArgs.SetBuffer(0, count);
                    token.Socket.SendAsync(token.m_writeEventArgs);

                    int took = Environment.TickCount - start;
                    if (took > 100)
                        Console.WriteLine( "{0} - ProcessSend took {0}ms! (TCP to client: {1})", "lol", took, token.ToString());
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine( "Packet processor: ObjectDisposedException" );
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    Console.WriteLine( "Packet processor: SocketException: {0}", ex.SocketErrorCode);
                    //GameServer.Instance.Disconnect(client);
                }
                catch (Exception ex)
                {
                    // assure that no exception is thrown into the upper layers and interrupt game loops!
                    Console.WriteLine( "AsyncSendCallback. client: " + token + ", Exception: {0}", ex);
                    //GameServer.Instance.Disconnect(client);
                }
            }
            else
            {
                token.Disconnect();
            }

        }

        /// <summary>
        /// Called after the client connection has been accepted.
        /// </summary>
        public void OnConnect()
        {
            m_networkHandler.onConnect(0, this);
        }

        /// <summary>
        /// Called right after the client has been disconnected.
        /// </summary>
        public void OnDisconnect()
        {
            m_networkHandler.onDisconnect(0, this);
        }

        /// <summary>
        /// Called when the client has received data.
        /// </summary>
        /// <param name="numBytes">number of bytes received in _pBuf</param>
        protected void OnReceive(byte[] buffer, int numBytes)
        {
            lock (this)
            {
                //End Offset of buffer
                int bufferSize = m_pBufOffset + numBytes;

                //Size < minimum
                if (bufferSize < PacketIn.HDR_SIZE)
                {
                    m_pBufOffset = bufferSize; // undo buffer read
                    return;
                }

                //Reset the offset
                m_pBufOffset = 0;

                //Current offset into the buffer
                int curOffset = 0;

                do
                {
                    byte[] hdr = m_decoding.Peek(buffer, curOffset);

                    int packetLength = Marshal.ConvertToInt32(hdr, 0);
                    int dataLeft = bufferSize - curOffset;

                    if (dataLeft < packetLength)
                    {
                        Buffer.BlockCopy(buffer, curOffset, buffer, 0, dataLeft);
                        m_pBufOffset = dataLeft;
                        break;
                    }

                    int packetEnd = curOffset + packetLength;


                    byte[] d = m_decoding.Decode(buffer, curOffset, packetLength);

                    //Console.WriteLine( Marshal.ToHexDump(
                                    //string.Format("{0} <=== <{3}> Packet 0x{1:X2} ({2}) length: {4}", m_config.PacketInTxt, Marshal.ConvertToInt16(d, 4),
                                    //Marshal.ConvertToInt16(d, 4), TcpEndpoint, Marshal.ConvertToInt32(d, 0),
                                    //d.Length),
                                    //d));


                    long start = Environment.TickCount;
                    try
                    {
                        PacketIn packet = m_networkHandler.ProcessPacket(this, d, 0, packetLength);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine( "ProcessPacket error: {0}", e.ToString());
                    }
                    long timeUsed = Environment.TickCount - start;

                    curOffset += packetLength;
                } while (bufferSize - 1 > curOffset);

                if (bufferSize - 1 == curOffset)
                {
                    buffer[0] = buffer[curOffset];
                    m_pBufOffset = 1;
                }
            }

        }


        private void Disconnect(SocketAsyncEventArgs e)
        {
            TCPConnection token = e.UserToken as TCPConnection;
            token.Disconnect();
        }

        public void Disconnect()
        {
            // close the socket associated with the client
            OnDisconnect();
            Close();
        }

        public void Close()
        {
            // close the socket associated with the client
            try
            {
                m_socket.Shutdown(SocketShutdown.Both);
            }
            // throws if client process has already closed
            catch (Exception ex)
            {
                //Globals.Log.Error("Exception: {0}", ex);
            }
            m_socket.Close();

            // decrement the counter keeping track of the total number of clients connected to the server
            // Free the SocketAsyncEventArg so they can be reused by another client
            m_readEventArgs.Completed -= IO_Completed;
            m_iocp.ReleaseAsyncEvent(m_readEventArgs);
            m_writeEventArgs.Completed -= IO_Completed;
            m_iocp.ReleaseAsyncEvent(m_writeEventArgs);
        }

        #region TCP

        /// <summary>
        /// Sends a packet via TCP
        /// </summary>
        /// <param name="packet">The packet to be sent</param>
        public void SendTCP(PacketOut packet)
        {
            //Fix the packet size
            packet.FinalizeLengthAndChecksum();

            //SavePacket(packet);

            //Get the packet buffer
            byte[] buf = packet.GetBuffer(); //packet.WritePacketLength sets the Capacity

            //Send the buffer
            SendTCP(buf);
        }

        /// <summary>
        /// Sends a packet via TCP
        /// </summary>
        /// <param name="buf">Buffer containing the data to be sent</param>
        public void SendTCP(byte[] buf)
        {
            //Check if client is connected
            if (m_socket.Connected)
            {
                //Globals.Log.Debug(Marshal.ToHexDump(
                                /*string.Format("{0} <=== <{3}> Packet 0x{1:X2} ({2}) length: {4}", m_config.PacketOutText, Marshal.ConvertToInt16(buf, 4),
                                Marshal.ConvertToInt16(buf, 4), TcpEndpoint, Marshal.ConvertToInt32(buf, 0),
                                buf.Length),
                                buf));*/


                try
                {
                    if (buf.Length > 4092)
                    {
                        // we need to split this up
                        byte[] nb1 = new byte[4092];
                        int nl = buf.Length - 4092;
                        byte[] nb2 = new byte[nl];
                        Buffer.BlockCopy(buf, 0, nb1, 0, 4092);
                        Buffer.BlockCopy(buf, 4092, nb2, 0, nl);
                        SendTCPOF(nb1);
                        SendTCPOF(nb2);
                        return;
                    }
                    lock (m_tcpQueue)
                    {
                        if (m_sendingTcp)
                        {
                            m_tcpQueue.Enqueue(buf);
                            return;
                        }

                        m_sendingTcp = true;
                    }

                    Buffer.BlockCopy(m_encoding.Encode(buf), 0, m_writeEventArgs.Buffer, 0, buf.Length);

                    int start = Environment.TickCount;
                    m_writeEventArgs.SetBuffer(0, buf.Length);
                    m_socket.SendAsync(m_writeEventArgs);

                    int took = Environment.TickCount - start;
                    //if (took > 100)
                        //Globals.Log.Warning("SendTCP.SendAsync took {0}ms! (TCP to client: {1})", took, this.ToString());
                }
                catch (Exception ex)
                {
                    // assure that no exception is thrown into the upper layers and interrupt game loops!
                    Disconnect();
                    Console.WriteLine( "Exception: {0}", ex.ToString());
                    /*
                                        log.Warning("It seems <" + ((m_client.Account != null) ? m_client.Account.Name : "???") +
                                                     "> went linkdead. Closing connection. (SendTCP, " + e.GetType() + ": " + e.Message + ")");
                                        GameServer.Instance.Disconnect(m_client);
                    */
                }
            }
        }

        /// <summary>
        /// Sends a packet via TCP
        /// </summary>
        /// <param name="buf">Buffer containing the data to be sent</param>
        public void SendTCPOF(byte[] buf)
        {
            //Check if client is connected
            if (m_socket.Connected)
            {
                try
                {
                    if (buf.Length > 4092)
                    {
                        // we need to split this up
                        byte[] nb1 = new byte[4092];
                        int nl = buf.Length - 4092;
                        byte[] nb2 = new byte[nl];
                        Buffer.BlockCopy(buf, 0, nb1, 0, 4092);
                        Buffer.BlockCopy(buf, 4092, nb2, 0, nl);
                        SendTCPOF(nb1);
                        SendTCPOF(nb2);
                        return;
                    }
                    lock (m_tcpQueue)
                    {
                        if (m_sendingTcp)
                        {
                            m_tcpQueue.Enqueue(buf);
                            return;
                        }

                        m_sendingTcp = true;
                    }

                    Buffer.BlockCopy(m_encoding.Encode(buf), 0, m_writeEventArgs.Buffer, 0, buf.Length);

                    int start = Environment.TickCount;
                    m_writeEventArgs.SetBuffer(0, buf.Length);
                    m_socket.SendAsync(m_writeEventArgs);

                    int took = Environment.TickCount - start;
                    //if (took > 100)
                        //Globals.Log.Warning("SendTCP.SendAsync took {0}ms! (TCP to client: {1})", took, this.ToString());
                }
                catch (Exception ex)
                {
                    // assure that no exception is thrown into the upper layers and interrupt game loops!
                    Disconnect();
                    //Globals.Log.Error("Exception: {0}", ex);
                }
            }
        }

        /// <summary>
        /// Combines queued packets in one stream.
        /// </summary>
        /// <param name="buf">The target buffer.</param>
        /// <param name="q">The queued packets.</param>
        /// <param name="length">The max stream len.</param>
        /// <param name="client">The client.</param>
        /// <returns>The count of bytes writen.</returns>
        private int CombinePackets(byte[] buf, Queue<byte[]> q, int length, TCPConnection client)
        {
            int i = 0;
            do
            {
                var pak = q.Peek();
                if (i + pak.Length > buf.Length)
                {
                    if (i == 0)
                    {
                        //Globals.Log.Warning("packet size {0} > buf size {1}, ignored; client: {2}\n{3}", pak.Length, buf.Length, client,
                                       //Marshal.ToHexDump("packet data:", pak));
                        q.Dequeue();
                        continue;
                    }
                    break;
                }

                Buffer.BlockCopy(m_encoding.Encode(pak), 0, buf, i, pak.Length);
                i += pak.Length;

                q.Dequeue();
            } while (q.Count > 0);

            return i;
        }

        /// <summary>
        /// Send the packet via TCP without changing any portion of the packet
        /// </summary>
        /// <param name="packet">Packet to send</param>
        public void SendTCPRaw(PacketOut packet)
        {
            SendTCP((byte[])packet.GetBuffer().Clone());
        }

        #endregion

    }

}
