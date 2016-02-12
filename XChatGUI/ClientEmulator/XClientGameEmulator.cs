using System;
using System.Collections.Generic;
using Rappelz.GameServer.Network;

namespace Rappelz.Client {
    public class XClientGameEmulator : INetworkHandler{

        #region Declarations
        // Thread for sending the Ping Packet
        public System.Threading.Thread m_tPingThread;
        // TCPConnection to Game Server
        private TCPConnection m_cGameConnection;
        // Temporary packet for the CharacterList packet
        GAME_PACKETS.CharacterList m_iGameCharacterList;
        // Saves the handle of a player with their actual name
        Dictionary<int, string> m_dHandles = new Dictionary<int, string>();
        // Other declarations
        private string m_szAccount;
        public bool IsConnected {
            get { return m_cGameConnection.Socket.Connected; }
        }
        #endregion

        public XClientGameEmulator(Config.ConfigNet conf, TCPManager man, XChat.XClientEmulator pBase, string pAccount, PacketOut pOut) {
            m_szAccount = pAccount;
            m_cGameConnection = new TCPConnection( man, null, this, conf );
            try {
                m_cGameConnection.Start();
                m_cGameConnection.SendTCP(CreateVersionPacket());
                m_cGameConnection.SendTCP( pOut );

            }
            catch {
                XLog.Log( "Can't connect to Game Server!" );
            }
        }

        public void Disconnect() {
            CreateLogoutPacket();
        }

        #region INetworkHandler
        #region Unused
        /// <summary>
        /// Mostly unused method here, getting called when the socket is connected.
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="connection">Socket/TCPConnection</param>
        /// <returns>Nothing</returns>
        public void onConnect(int id, TCPConnection connection) {
            XLog.Log( "Connected to Game Server" );
        }

        /// <summary>
        /// Mostly unused method here, getting called when the socket is disconnected.
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="connection">Socket/TCPConnection</param>
        /// <returns>Nothing</returns>
        public void onDisconnect(int id, TCPConnection connection) {
            XLog.Log( "Disconnected from Game Server");
        }
        #endregion

        /// <summary>
        /// The function which is used to proceed an incoming packet to the active TCPConnection
        /// </summary>
        /// <param name="con">The connection which received the packet</param>
        /// <param name="buf">byte[] array containing the raw content of the received packet</param>
        /// <param name="start">Start of the content in buf, usually 0</param>
        /// <param name="size">Size of the packet (minus start)</param>
        /// <returns>PacketIn -> Converted raw package to MemoryStream based PacketIn</returns>
        public PacketIn ProcessPacket(TCPConnection con, byte[] buf, int start, int size) {
            PacketIn packet = new PacketIn( buf, start, size );

            switch ( packet.ID ) {

                case 0: // ResultMsg
                    var res = new AUTH_PACKETS.RESULT( packet.ReadUInt16(), packet.ReadUInt16(), packet.ReadInt32() );
                    if ( res.nRequestPacket == 2005 ) {
                        if ( res.nResult == 0 ) {
                            con.SendTCP( CreateReportPacket() );
                            con.SendTCP( CreateCharacterListPacket() );
                        }
                        else {
                            con.Disconnect();
                            XLog.Log( "Can't connect to game server. Result: {0} - disconnecting...", res.nResult );
                        }
                    }
                    break;
                case 2004: // CharacterList
                    m_iGameCharacterList = new GAME_PACKETS.CharacterList( packet );
                    XLog.Log( "Character selection. Please use /use ID to select a character." );
                    for ( int i = 0; i < m_iGameCharacterList.nCount; i++ ) {
                        XLog.Log( "-> Character {0}: {1}", i + 1, m_iGameCharacterList.nList[i].szName );
                    }
                    break;
                case 21: // ChatLocal
                    var tmp = packet.ReadInt32();
                    string szSource = m_dHandles.ContainsKey( tmp ) ? m_dHandles[tmp] : "INVALID-HANDLE:" + tmp;
                    int nLen = packet.ReadByte();
                    int nType = packet.ReadByte();
                    XLog.AddMessage( szSource, packet.ReadString( nLen ), nType );
                    break;
                case 22: // ChatMsg
                    var pMessage = new GAME_PACKETS.ChatMessage( packet );
                    XLog.AddMessage( pMessage.szName, pMessage.szMessage, pMessage.nType );
                    break;
                case 3: // Enter: Handle -> Name, small hack so we don't have to read the full packet (which is fucking large)
                    if ( packet.ReadByte() == 0 ) {
                        int key = packet.ReadInt32();
                        if ( m_dHandles.ContainsKey( key ) ) {
                            m_dHandles.Remove( key );
                        }
                        packet.Seek( 77, System.IO.SeekOrigin.Current );
                        string value = packet.ReadString( 19 );
                        m_dHandles.Add( key, value );
                    }
                    break;
                case 507: // Property: Own Name -> Handle
                    if ( m_dHandles.ContainsKey( 0 ) && m_tPingThread == null) {
                        var szName = m_dHandles[0];
                        m_dHandles.Remove( 0 );
                        m_dHandles.Add( packet.ReadInt32(), szName );
                        m_tPingThread = new System.Threading.Thread( new System.Threading.ThreadStart( SendPingPacket ) );
                        m_tPingThread.IsBackground = true;
                        m_tPingThread.Start();
                    }
                    break;
                default:
                    break;
            }
            return packet;
        }
        #endregion

        #region Create Packets
        private PacketOut CreateVersionPacket() {
            PacketOut o = new PacketOut( 51 );
            o.FillString( "200701120", 20 );
            o.FinalizeLengthAndChecksum();
            return o;
        }

        /// <summary>
        /// Lets the server know that you want the character list
        /// </summary>
        /// <returns>Nothing</returns>
        private PacketOut CreateCharacterListPacket() {
            PacketOut o = new PacketOut( 2001 );
            o.FillString( m_szAccount, 61 );
            o.FinalizeLengthAndChecksum();
            return o;
        }

        /// <summary>
        /// Not sure about the content - but it actually sends your GPU with its driver version.
        /// </summary>
        /// <returns></returns>
        private PacketOut CreateReportPacket() {
            PacketOut o = new PacketOut( 8000 );
            o.WriteShort( 8704 );
            o.WriteShort( 18197 );
            o.WriteByte( 81 );
            o.WriteByte( 251 );
            o.WriteString( "Windows  (6.2.9200)|ATI Radeon HD 3600 SeriesDrv Version : 8.17.10.1129", 71 ); // Only for testing. And yes, my GPU sucks. Problem?
            o.FinalizeLengthAndChecksum();
            return o;
        }

        /// <summary>
        /// Logs your character into the game
        /// </summary>
        /// <param name="nIndex">Selected Character Index</param>
        /// <returns></returns>
        public void CreateLoginPacket(int nIndex) {
            try {
                PacketOut oLogin = new PacketOut( 1 );
                oLogin.FillString( m_iGameCharacterList.nList[nIndex - 1].szName, 61 );
                oLogin.WriteByte( (byte)m_iGameCharacterList.nList[nIndex - 1].nRace );
                oLogin.FinalizeLengthAndChecksum();
                m_dHandles.Add( 0, m_iGameCharacterList.nList[nIndex - 1].szName );
                m_cGameConnection.SendTCP( oLogin );
                XLog.AddMessage( "", "", -5 ); // Clear box
            }
            catch ( Exception e ) {
                XLog.Log( "Error while logging in to the server: {0}", e.Message );
            }
        }

        /// <summary>
        /// Sends a Message in game
        /// </summary>
        /// <param name="szSource">Receiver</param>
        /// <param name="szMsg">Message</param>
        /// <param name="nType">Message Type (whisper, global, advertisment, [...]</param>
        /// <returns></returns>
        public void CreateMessagePacket(string szSource, string szMsg, int nType) {
            PacketOut o = new PacketOut( 20 );
            o.FillString( szSource, 21 );
            o.WriteByte( 0 );
            o.WriteByte( (byte)szMsg.Length );
            o.WriteByte( (byte)nType );
            o.WriteString( szMsg, szMsg.Length );
            o.FinalizeLengthAndChecksum();
            m_cGameConnection.SendTCP( o );
        }

        /// <summary>
        /// Safe method of logging you out
        /// </summary>
        /// <returns></returns>
        private void CreateLogoutPacket() {
            PacketOut o = new PacketOut( 27 );
            o.FinalizeLengthAndChecksum();
            m_cGameConnection.SendTCP( o );
            m_cGameConnection.Disconnect();
            if ( m_tPingThread != null && m_tPingThread.ThreadState == System.Threading.ThreadState.Running ) {
                m_tPingThread.Abort();
            }
            if( IsConnected ) {
                m_cGameConnection.Disconnect();
            }
            
        }
        #endregion

        #region PingPacket
        /// <summary>
        /// Sends a ping packet to the server, keeping the connection alive.
        /// </summary>
        /// <returns>Nothing</returns>
        private void SendPingPacket() {
            // As long as there is a connection to the game server (sorry for the checks xP)
            while ( m_cGameConnection != null && m_cGameConnection.Socket != null && m_cGameConnection.Socket.Connected ) {
                PacketOut o = new PacketOut( 9999 );
                o.FinalizeLengthAndChecksum();
                m_cGameConnection.SendTCP( o );

                XLog.Debug( "Ping packet sent." );
                System.Threading.Thread.Sleep( 1000 * 60 );
            }
            m_tPingThread.Abort();
        }
        #endregion
    }

    #region Game Server Packets
    public class GAME_PACKETS {

        public class ChatMessage {
            public string szName { get; set; }
            public int nSize { get; set; }
            public byte nType { get; set; }
            public string szMessage { get; set; }

            public ChatMessage(PacketIn p) {
                szName = p.ReadString( 21 );
                nSize = p.ReadInt16();
                nType = (byte)p.ReadByte();
                szMessage = p.ReadString( nSize );
            }
            public override string ToString() {
                return string.Format( "Message by {0} (Type {1}): {2}", szName, nType, szMessage );
            }
        }

        public class CharacterList {
            public int nCount { get; set; }
            public CList[] nList { get; set; }

            public CharacterList(PacketIn packet) {
                packet.Seek( 6, System.IO.SeekOrigin.Current );
                nCount = packet.ReadInt16();
                nList = new CList[nCount];
                for ( int i = 0; i < nCount; i++ ) {
                    nList[i] = new CList( packet );
                }
            }

            public class CList {
                public int nRace { get; set; }
                public string szName { get; set; }

                public CList(PacketIn packet) {
                    packet.Seek( 4, System.IO.SeekOrigin.Current );
                    nRace = packet.ReadInt32();
                    packet.Seek( 161, System.IO.SeekOrigin.Current );
                    szName = packet.ReadString( 19 );
                    packet.Seek( 376, System.IO.SeekOrigin.Current );
                }
            }


        }
    }
    #endregion
}
