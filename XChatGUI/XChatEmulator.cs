using Rappelz.GameServer.Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace XChat {

    class XClientEmulator : INetworkHandler {
        private OpenSSL.Crypto.RSA rsa;
        public byte[] m_pAES_KEY;
        private string m_szName;
        private string m_szPassword;
        private int m_nSelectedServerIdx;
        private TCPConnection m_szConnection;
        public System.Threading.Thread m_tPingThread;
        private bool m_bIsConnected;
        public bool IsConnected { get { return m_bIsConnected; } }
        // Temp Packets
        AUTH_PACKETS.SERVER_LIST m_iAuthServerList;
        GAME_PACKETS.CharacterList m_iGameCharacterList;
        // HANDLE -> Name 
        Dictionary<int, string> m_dHandles = new Dictionary<int, string>();


        public void EmulateClient(string szName, string szPassword) {
            TCPManager man = new TCPManager( 1, 2048 * 300 );
            Config.ConfigNet config = new Config.ConfigNet();
            config.ListenIp = IPAddress.Parse( "127.0.0.1" );
            config.Port = 4500;

            m_dHandles.Clear();

            m_szName = szName;
            m_szPassword = szPassword;
            m_szConnection = new TCPConnection( man, null, this, config );
            m_szConnection.Start();
            m_bIsConnected = true;

            m_szConnection.SendTCP( this.CreateVersionPacket( 10001 ) );
            m_szConnection.SendTCP( this.CreateAESPacket() );
        }

        private TCPConnection CreateGameServerSession(Config.ConfigNet config) {
            if(m_szConnection.Socket.Connected) {
                m_szConnection.Disconnect();
            }
            TCPManager man = new TCPManager( 1, 2048 * 300 );
            m_szConnection = new TCPConnection( man, null, this, config );
            m_szConnection.Start();
            return m_szConnection;
        }

        #region Unused
        public void onConnect(int id, TCPConnection connection) {
            XLog.Log("Connected to server...");
        }
        public void onDisconnect(int id, TCPConnection connection) {
            XLog.Log( "Disconnected from server." );
        }
        #endregion

        private void SendPingPacket() {
            while ( m_bIsConnected ) {
                PacketOut o = new PacketOut( 9999 );
                o.FinalizeLengthAndChecksum();
                m_szConnection.SendTCP( o );
                System.Threading.Thread.Sleep( 1000 * 60 );
            }
            m_tPingThread.Abort();
        }

        public PacketIn ProcessPacket(TCPConnection con, byte[] buf, int start, int size) {
            PacketIn packet = new PacketIn( buf, start, size );
            switch ( packet.ID ) {
                #region Auth Server Packet handling
                case 72: // TS_AC_AES_KEY_IV
                    var pAES = new AUTH_PACKETS.AES_KEY_IV( packet );
                    m_pAES_KEY = rsa.PrivateDecrypt( pAES.nKey, OpenSSL.Crypto.RSA.Padding.PKCS1 );
                    GenerateLoginPacket( m_pAES_KEY, con );
                    break;
                case 10000: // TS_AC_RESULT
                    var pResult = new AUTH_PACKETS.RESULT( packet.ReadUInt16(), packet.ReadUInt16(), packet.ReadInt32() );
                    if ( pResult.nLoginFlag == 1 ) {
                        PacketOut o = new PacketOut( 10021 );
                        con.SendTCP( o );
                    }
                    else {
                        m_szConnection.Disconnect();
                        m_bIsConnected = false;
                        XLog.Log( "Login failed. Result: {0} - Disconnecting...", pResult.nResult );
                    }
                    break;
                case 10022: // TS_AC_SERVER_LIST
                    m_iAuthServerList = new AUTH_PACKETS.SERVER_LIST( packet );
                    XLog.Log( "Server selection. Please use /select ID to connect to one of the listed servers below." );
                    for ( int i = 0; i < m_iAuthServerList.count; i++ ) {
                        XLog.Log( string.Format( "-> Server {0}: {1}", i + 1, m_iAuthServerList.list[i].server_name ) );
                    }
                    break;
                case 10024: // TS_AC_SELECT_SERVER
                    var pSelectServer = new AUTH_PACKETS.SELECT_SERVER( packet, this );
                    con.Disconnect();

                    Config.ConfigNet conf = new Config.ConfigNet();
                    conf.Port = m_iAuthServerList.list[m_nSelectedServerIdx].server_port;
                    conf.ListenIp = System.Net.IPAddress.Parse( m_iAuthServerList.list[m_nSelectedServerIdx].server_ip );

                    m_szConnection = CreateGameServerSession( conf );
                    m_szConnection.SendTCP( CreateVersionPacket( 51 ) );

                    PacketOut oEncrypt = new PacketOut( 2005 );
                    oEncrypt.FillString( m_szName, 61 );
                    oEncrypt.Write( pSelectServer.encrypted_data, 0, pSelectServer.encrypted_data.Length );
                    oEncrypt.FinalizeLengthAndChecksum();
                    m_szConnection.SendTCP( oEncrypt );
                    break;
                #endregion

                #region Game Server Packet handling
                case 0: // ResultMsg
                    var res = new AUTH_PACKETS.RESULT( packet.ReadUInt16(), packet.ReadUInt16(), packet.ReadInt32() );
                    if ( res.nRequestPacket == 2005 ) {
                        if ( res.nResult == 0 ) {
                            con.SendTCP( CreateReportPacket() );
                            con.SendTCP( CreateCharacterListPacket() );
                        }
                        else {
                            m_szConnection.Disconnect();
                            m_bIsConnected = false;
                            XLog.Log( "Can't connect to game server. Result: {0} - disconnecting...", res.nResult );
                        }
                    }
                    break;
                case 2004: // CharacterList
                    m_iGameCharacterList = new GAME_PACKETS.CharacterList( packet );
                    XLog.Log( "Character selection. Please use /use ID to select a character." );
                    for(int i = 0; i < m_iGameCharacterList.nCount; i++ ) {
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
                case 3: // Enter: Handle -> Name
                    if ( packet.ReadByte() == 0 ) {
                        int key = packet.ReadInt32();
                        if ( m_dHandles.ContainsKey( key ) ) {
                            m_dHandles.Remove( key );
                        }
                        packet.Seek( 77, System.IO.SeekOrigin.Current );
                        string value = packet.ReadString( 19 );
                        m_dHandles.Add( key, value );
                        m_tPingThread = new System.Threading.Thread( new System.Threading.ThreadStart( SendPingPacket ) );
                        m_tPingThread.IsBackground = true;
                        m_tPingThread.Start();
                    }
                    break;
                case 507: // Property: Own Name -> Handle
                    if ( m_dHandles.ContainsKey( 0 ) ) {
                        var szName = m_dHandles[0];
                        m_dHandles.Remove( 0 );
                        m_dHandles.Add( packet.ReadInt32(), szName );
                    }
                    break;
                #endregion
                default:
                    break;
            }
            return packet;
        }

        #region En-/Decryption
        public byte[] EncryptAES(byte[] key, string hash) {
            OpenSSL.Crypto.CipherContext d_ctx = new OpenSSL.Crypto.CipherContext( OpenSSL.Crypto.Cipher.AES_128_CBC );
            byte[] var = new byte[16];
            for ( int i = 0; i < 16; i++ ) {
                var[i] = key[i + 16];
            }
            return d_ctx.Encrypt( Encoding.Default.GetBytes( hash ), key, var );
        }

        public byte[] DecryptAES(byte[] key, byte[] result) {
            OpenSSL.Crypto.CipherContext d_ctx = new OpenSSL.Crypto.CipherContext( OpenSSL.Crypto.Cipher.AES_128_CBC );
            byte[] var = new byte[16];
            for ( int i = 0; i < 16; i++ ) {
                var[i] = key[i + 16];
            }
            return d_ctx.Decrypt( result, key, var );
        }

        private PacketOut CreateAESPacket() {
            PacketOut o = new PacketOut( 71 );
            rsa = new OpenSSL.Crypto.RSA();
            rsa.GenerateKeys( 1024, 65537, null, null );
            int size = rsa.PublicKeyAsPEM.Length;
            o.WriteInt32( size );
            o.FillString( rsa.PublicKeyAsPEM, size );
            return o;
        }
        #endregion

        #region Auth Server Packets
        private void GenerateLoginPacket(byte[] key, TCPConnection con) {

            PacketOut o = new PacketOut( 10010 );
            var a = EncryptAES( key, m_szPassword );
            o.FillString( m_szName, 61 );
            o.WriteInt32( a.Length );
            o.Write( a, 0, a.Length );
            o.Fill( 0, 61 - a.Length );
            con.SendTCP( o );
        }

        public void CreateServerSelectPacket(int nIndex) {
            try {
                PacketOut o = new PacketOut( 10023 );
                o.WriteUInt16( m_iAuthServerList.list[nIndex -1].server_idx );
                m_nSelectedServerIdx = nIndex - 1;
                m_szConnection.SendTCP( o );
            }catch(Exception e ) {
                m_bIsConnected = false;
                m_szConnection.Disconnect();
                XLog.Log( "Can't connect to server: {0}", e.Message );
            }
        }
        #endregion

        #region Game Server Packets 
        private PacketOut CreateVersionPacket(int nCode) {
            PacketOut o = new PacketOut( (ushort)nCode ); // 10001
            o.FillString( "200701120", 20 );
            if ( nCode == 51 ) {
                o.FinalizeLengthAndChecksum();
            }
            return o;
        }

        private PacketOut CreateCharacterListPacket() {
            PacketOut o = new PacketOut( 2001 );
            o.FillString( m_szName, 61 );
            o.FinalizeLengthAndChecksum();
            return o;
        }

        private PacketOut CreateReportPacket() {
            PacketOut o = new PacketOut( 8000 );
            o.WriteShort( 8704 );
            o.WriteShort( 18197 );
            o.WriteByte( 81 );
            o.WriteByte( 251 );
            o.WriteString( "Windows  (6.2.9200)|ATI Radeon HD 3600 SeriesDrv Version : 8.17.10.1129", 71 );
            o.FinalizeLengthAndChecksum();
            return o;
        }

        public void CreateLoginPacket(int nIndex) {
            try {
                PacketOut oLogin = new PacketOut( 1 );
                oLogin.FillString( m_iGameCharacterList.nList[nIndex - 1].szName, 61 );
                oLogin.WriteByte( (byte)m_iGameCharacterList.nList[nIndex - 1].nRace );
                oLogin.FinalizeLengthAndChecksum();
                m_dHandles.Add( 0, m_iGameCharacterList.nList[nIndex - 1].szName );
                m_szConnection.SendTCP( oLogin );
                XLog.AddMessage("", "", -5);
            }catch(Exception e ) {
                XLog.Log( "Error while logging in to the server: {0}", e.Message );
            }
        }

        public void CreateMessagePacket(string szSource, string szMsg, int nType) {
            PacketOut o = new PacketOut( 20 );
            o.FillString( szSource, 21 );
            o.WriteByte( 0 );
            o.WriteByte( (byte)szMsg.Length );
            o.WriteByte( (byte)nType );
            o.WriteString( szMsg, szMsg.Length );
            o.FinalizeLengthAndChecksum();
            m_szConnection.SendTCP( o );
        }

        public void CreateLogoutPacket() {
            PacketOut o = new PacketOut( 27 );
            o.FinalizeLengthAndChecksum();
            m_szConnection.SendTCP( o );
            m_szConnection.Disconnect();
            m_bIsConnected = false;
            if(m_tPingThread != null && m_tPingThread.ThreadState == System.Threading.ThreadState.Running)
                m_tPingThread.Abort();
        }
        #endregion

        public class AUTH_PACKETS {

            public class AES_KEY_IV {
                public int nSize { get; set; }
                public byte[] nKey { get; set; }

                public AES_KEY_IV(PacketIn packet) {
                    nSize = packet.ReadInt32();
                    nKey = new byte[nSize];
                    packet.Read( nKey, 0, nSize );
                }
                public override string ToString() {
                    return string.Format( "AES_KEY_IV Packet:\nSize: {0}\nKey{1}", nSize, Encoding.Default.GetString( nKey ) );
                }
            }

            public class RESULT {
                public UInt16 nRequestPacket { get; set; }
                public UInt16 nResult { get; set; }
                public Int32 nLoginFlag { get; set; }
                public RESULT(UInt16 request, UInt16 result, int flag) {
                    nRequestPacket = request;
                    nResult = result;
                    nLoginFlag = flag;
                }
                public override string ToString() {
                    return string.Format( "RESULT Packet:\nRequest Packet ID:{0}\nResult: {1}\nLogin Flag: {2}", nRequestPacket, nResult, nLoginFlag );
                }
            }

            public class SELECT_SERVER {
                public UInt16 result { get; set; }
                public int encrypted_data_size { get; set; }
                public byte[] encrypted_data { get; set; }
                public uint pending_time { get; set; }
                public uint unknown { get; set; }
                public uint unknown2 { get; set; }
                public SELECT_SERVER(PacketIn p, XClientEmulator emu) {
                    result = p.ReadUInt16();
                    encrypted_data_size = p.ReadInt32();
                    encrypted_data = new byte[encrypted_data_size];
                    p.Read( encrypted_data, 0, encrypted_data_size );
                    encrypted_data = emu.DecryptAES( emu.m_pAES_KEY, encrypted_data );
                    pending_time = p.ReadUInt32();
                    unknown = p.ReadUInt32();
                    unknown2 = p.ReadUInt32();
                }
                public override string ToString() {
                    return string.Format( "SELECT_SERVER Packet:\nResult:{0}\nSize: {1}\nData: {2}\nPending Time: {3}", result, encrypted_data_size, encrypted_data, pending_time );
                }
            }

            public class SERVER_LIST {
                public UInt16 last_login_idx { get; set; }
                public UInt16 count { get; set; }
                public SERVER_INFO[] list { get; set; }
                public SERVER_LIST(PacketIn p) {
                    last_login_idx = p.ReadUInt16();
                    count = p.ReadUInt16();
                    list = new SERVER_INFO[count];
                    for ( int i = 0; i < count; i++ ) {
                        list[0] = new SERVER_INFO( p.ReadUInt16(), p.ReadString( 21 ), p.ReadByte() == 1 ? true : false, p.ReadString( 256 ), p.ReadString( 16 ), p.ReadInt32(), p.ReadUInt16() );
                    }
                }

                public class SERVER_INFO {
                    public UInt16 server_idx { get; set; }
                    public string server_name { get; set; }
                    public bool is_adult_server { get; set; }
                    public string server_screenshot { get; set; }
                    public string server_ip { get; set; }
                    public int server_port { get; set; }
                    public UInt16 user_ration { get; set; }
                    public SERVER_INFO(UInt16 idx, string name, bool adult, string sc, string ip, int port, UInt16 ratio) {
                        server_idx = idx;
                        server_name = name;
                        is_adult_server = adult;
                        server_screenshot = sc;
                        server_ip = ip;
                        server_port = port;
                        user_ration = ratio;
                    }
                }
            }

        }

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
    }
}

public static class Config {
    public class ConfigNet {
        public IPAddress ListenIp = IPAddress.Parse( "0.0.0.0" );
        public IPAddress SentIp = IPAddress.Parse( "0.0.0.0" );
        public int Port;
        public bool Encrypted = true;
    }
}
