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
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Rappelz.GameServer;

namespace Rappelz.GameServer.Network
{
    public class TCPListener
    {
        protected TCPManager m_iocp;

        /// <summary>
        /// The configuration of this server
        /// </summary>
        protected Config.ConfigNet m_config;

        /// <summary>
        /// The network handler for this listener
        /// </summary>
        protected INetworkHandler m_networkHandler;

        /// <summary>
        /// Socket that receives connections
        /// </summary>
        protected Socket m_listen;

        /// <summary>
        /// Holds the startSystemTick when server is up.
        /// </summary>
        protected int m_startTick;

        /// <summary>
        /// Holds the startSystemTick when server is up.
        /// </summary>
        protected SocketAsyncEventArgs m_eventArgs;

        /// <summary>
        /// Gets the current network handler that is servicing this client.
        /// </summary>
        public INetworkHandler NetworkHandler
        {
            get { return m_networkHandler; }
        }

        public TCPListener(TCPManager iocp, INetworkHandler handler, Config.ConfigNet config)
        {
            m_iocp = iocp;
            m_config = config;
            m_networkHandler = handler;
        }

        /// <summary>
        /// Initializes and binds the socket, doesn't listen yet!
        /// </summary>
        /// <returns>true if bound</returns>
        protected bool InitSocket()
        {
            try
            {
                m_listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_listen.Bind(new IPEndPoint(m_config.ListenIp, m_config.Port));
            }
            catch (Exception e)
            {
                //Globals.Log.Error("InitSocket", e);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts the Listener
        /// </summary>
        /// <returns>True if the server was successfully started</returns>
        public virtual bool Start()
        {
            //Test if we have a valid port yet
            //if not try  binding.
            //---------------------------------------------------------------
            //Try to init the server port
            if (m_listen == null)
                InitSocket();

            //---------------------------------------------------------------
            //Packet buffers
            //---------------------------------------------------------------
            //Set the GameServer StartTick
            m_startTick = Environment.TickCount;

            System.GC.Collect(System.GC.MaxGeneration, GCCollectionMode.Forced);

            ////Globals.Log.Info("{0} Listener is now open for connections!", m_config.Name);

            try
            {
                m_listen.Listen(100);
                StartAccept(null);

                ////Globals.Log.Debug("{0} Listener is now listening to incoming connections!", m_config.Name);
            }
            catch (Exception e)
            {
                ////Globals.Log.Error("Start", e);

                if (m_listen != null)
                    m_listen.Close();

                return false;
            }

            return true;
        }

        // Begins an operation to accept a connection request from the client 
        //
        // <param name="acceptEventArg">The context object to use when issuing 
        // the accept operation on the server's listening socket</param>
        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                // socket must be cleared since the context object is being reused
                acceptEventArg.AcceptSocket = null;
            }

//            m_maxNumberAcceptedClients.WaitOne();
            bool willRaiseEvent = m_listen.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
            }
        }

        // This method is the callback method associated with Socket.AcceptAsync 
        // operations and is invoked when an accept operation is complete
        //
        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
//             Interlocked.Increment(ref m_numConnectedSockets);
//             Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
//                 m_numConnectedSockets);

            // create a new connection object for this incoming connection

            Socket sock = null;

            try
            {
                if (m_listen == null)
                    return;

                sock = e.AcceptSocket;

//                sock.SendBufferSize = m_iocp.BufferSize;
//                sock.ReceiveBufferSize = m_iocp.BufferSize;
//                sock.NoDelay = Constants.UseNoDelay;

                TCPConnection baseSocket = null;

                try
                {
                    string ip = sock.Connected ? sock.RemoteEndPoint.ToString() : "socket disconnected";
                    //Globals.Log.Debug("{0} - Incoming connection from {1}", m_config.Name, ip);

                    baseSocket = new TCPConnection(m_iocp, sock, m_networkHandler, m_config);
                    baseSocket.Tag = new ConnectionTag();

//                     lock (_clients)
//                         _clients.Add(baseSocket);

                    baseSocket.OnConnect();
                    baseSocket.StartReceive();
                }
                catch (SocketException)
                {
                    //Globals.Log.Error("BaseServer SocketException");
                    if (baseSocket != null)
                        baseSocket.Disconnect();
                }
                catch (Exception ex)
                {
                    //Globals.Log.Error("Client creation", ex);

                    if (baseSocket != null)
                        baseSocket.Disconnect();
                }
            }
            catch
            {
                //Globals.Log.Error("AcceptCallback: Catch");

                if (sock != null) // don't leave the socket open on exception
                {
                    try
                    {
                        sock.Close();
                    }
                    catch
                    {
                    }
                }
            }
            finally
            {
                if (m_listen != null)
                {
//                    e.AcceptSocket = null;
                    StartAccept(e);
                }
            }
        }
    }
}
