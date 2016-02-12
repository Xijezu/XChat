using System;
using System.Text;
using Rappelz.GameServer.Network;

namespace Rappelz.Client {
    public class XClientAuthEmulator : INetworkHandler {

        #region Declarations
        // OpenSSL for creating a keypair and Encoding/Decoding for the login
        private OpenSSL.Crypto.RSA m_cRSA;
        // Declarations for later usage, temporary saved
        private byte[] m_pAES_KEY;
        private string m_szName;
        private string m_szPassword;
        private int m_nSelectedServerIdx;

        public bool IsConnected {
            get { return m_cAuthConnection.Socket.Connected; }
        }
        // Connection
        private TCPConnection m_cAuthConnection;
        private XChat.XClientEmulator m_cClientBase;
        // Temporary Packets
        AUTH_PACKETS.SERVER_LIST m_iAuthServerList;
        #endregion

        public XClientAuthEmulator(TCPManager man, Config.ConfigNet conf, string pAccount, string pPassword, XChat.XClientEmulator cBase) {
            m_szName = pAccount;
            m_szPassword = pPassword;
            m_cClientBase = cBase;

            try {
                m_cAuthConnection = new TCPConnection( man, null, this, conf );
                m_cAuthConnection.Start();
                m_cAuthConnection.SendTCP( this.CreateVersionPacket() );
                m_cAuthConnection.SendTCP( this.CreateAESPacket() );
            }
            catch {
                XLog.Log( "Can't connect to Authentication Server!" );
            }
        }

        public void Disconnect() {
            if ( IsConnected ) {
                m_cAuthConnection.Disconnect();
            }
        }

        #region En-/Decryption
        /// <summary>
        /// EncryptAES is being used for encrypting the password sent to the authentication server
        /// </summary>
        /// <param name="key">AES Key received from the server</param>
        /// <param name="hash">Actually the string you want to encrypt</param>
        /// <returns>Encrypted byte[] of hash</returns>
        public static byte[] EncryptAES(byte[] key, string hash) {
            OpenSSL.Crypto.CipherContext d_ctx = new OpenSSL.Crypto.CipherContext( OpenSSL.Crypto.Cipher.AES_128_CBC );
            byte[] var = new byte[16];
            for ( int i = 0; i < 16; i++ ) {
                var[i] = key[i + 16];
            }
            return d_ctx.Encrypt( Encoding.Default.GetBytes( hash ), key, var );
        }

        /// <summary>
        /// Used to decrypt something encrypted in AES
        /// </summary>
        /// <param name="key">AES Key received from the server</param>
        /// <param name="result">Encrypted byte array you want to decrypt</param>
        /// <returns>Decrypted version of "result"</returns>
        public static byte[] DecryptAES(byte[] key, byte[] result) {
            OpenSSL.Crypto.CipherContext d_ctx = new OpenSSL.Crypto.CipherContext( OpenSSL.Crypto.Cipher.AES_128_CBC );
            byte[] var = new byte[16];
            for ( int i = 0; i < 16; i++ ) {
                var[i] = key[i + 16];
            }
            return d_ctx.Decrypt( result, key, var );
        }
        #endregion

        #region Create Packets
        private PacketOut CreateVersionPacket() {
            PacketOut o = new PacketOut( 10001 );
            o.FillString( "200701120", 20 );
            return o;
        }

        private PacketOut CreateAESPacket() {
            PacketOut o = new PacketOut( 71 );
            m_cRSA = new OpenSSL.Crypto.RSA();
            m_cRSA.GenerateKeys( 1024, 65537, null, null );
            o.WriteInt32( m_cRSA.PublicKeyAsPEM.Length );
            o.FillString( m_cRSA.PublicKeyAsPEM, m_cRSA.PublicKeyAsPEM.Length );
            return o;
        }

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
                o.WriteUInt16( m_iAuthServerList.list[nIndex - 1].server_idx );
                m_nSelectedServerIdx = nIndex - 1;
                m_cAuthConnection.SendTCP( o );
            }
            catch ( Exception e ) {
                m_cAuthConnection.Disconnect();
                XLog.Log( "Can't connect to server: {0}", e.Message );
            }
        }
        #endregion

        #region INetworkHandler
        public PacketIn ProcessPacket(TCPConnection con, byte[] buf, int start, int size) {
            PacketIn packet = new PacketIn( buf, start, size );
            switch ( packet.ID ) {

                case 72: // TS_AC_AES_KEY_IV
                    var pAES = new AUTH_PACKETS.AES_KEY_IV( packet );
                    m_pAES_KEY = m_cRSA.PrivateDecrypt( pAES.nKey, OpenSSL.Crypto.RSA.Padding.PKCS1 );
                    GenerateLoginPacket( m_pAES_KEY, con );
                    break;
                case 10000: // TS_AC_RESULT
                    var pResult = new AUTH_PACKETS.RESULT( packet.ReadUInt16(), packet.ReadUInt16(), packet.ReadInt32() );
                    if ( pResult.nLoginFlag == 1 ) {
                        PacketOut o = new PacketOut( 10021 );
                        con.SendTCP( o );
                    }
                    else {
                        m_cAuthConnection.Disconnect();
                        XLog.Log( "Login failed. Result: {0} - Disconnecting...", pResult.nResult );
                    }
                    m_szPassword = string.Empty;
                    break;
                case 10022: // TS_AC_SERVER_LIST
                    m_iAuthServerList = new AUTH_PACKETS.SERVER_LIST( packet );
                    XLog.Log( "Server selection. Please use /select ID to connect to one of the listed servers below." );
                    for ( int i = 0; i < m_iAuthServerList.count; i++ ) {
                        XLog.Log( string.Format( "-> Server {0}: {1}", i + 1, m_iAuthServerList.list[i].server_name ) );
                    }
                    break;
                case 10024: // TS_AC_SELECT_SERVER
                    con.Disconnect();

                    var pSelectServer = new AUTH_PACKETS.SELECT_SERVER( packet, ref m_pAES_KEY );

                    Config.ConfigNet conf = new Config.ConfigNet();
                    conf.Port = m_iAuthServerList.list[m_nSelectedServerIdx].server_port;
                    conf.ListenIp = System.Net.IPAddress.Parse( m_iAuthServerList.list[m_nSelectedServerIdx].server_ip );

                    PacketOut oEncrypt = new PacketOut( 2005 );
                    oEncrypt.FillString( m_szName, 61 );
                    oEncrypt.Write( pSelectServer.encrypted_data, 0, pSelectServer.encrypted_data.Length );
                    oEncrypt.FinalizeLengthAndChecksum();
                    con.Close();
                    m_cClientBase.CreateGameServerSession( oEncrypt, conf, m_szName );
                    break;
                default:
                    break;
            }
            return packet;
        }

        #region Unused
        /// <summary>
        /// Mostly unused method here, getting called when the socket is connected.
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="connection">Socket/TCPConnection</param>
        /// <returns>Nothing</returns>
        public void onConnect(int id, TCPConnection connection) {
            XLog.Log( "Connected to Authentication server." );
        }

        /// <summary>
        /// Mostly unused method here, getting called when the socket is disconnected.
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="connection">Socket/TCPConnection</param>
        /// <returns>Nothing</returns>
        public void onDisconnect(int id, TCPConnection connection) {
            XLog.Log( "Disconnected from Authentication server." );
        }
        #endregion

        #endregion
    }

    #region Auth server packets
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

            public SELECT_SERVER(PacketIn p, ref byte[] pAESKey) {
                result = p.ReadUInt16();
                encrypted_data_size = p.ReadInt32();
                encrypted_data = new byte[encrypted_data_size];
                p.Read( encrypted_data, 0, encrypted_data_size );
                encrypted_data = XClientAuthEmulator.DecryptAES( pAESKey, encrypted_data );
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
                    list[i] = new SERVER_INFO( p.ReadUInt16(), p.ReadString( 21 ), p.ReadByte() == 1 ? true : false, p.ReadString( 256 ), p.ReadString( 16 ), p.ReadInt32(), p.ReadUInt16() );
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
    #endregion
}