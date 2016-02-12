using Rappelz.Client;
using Rappelz.GameServer.Network;
using System.Net;

namespace XChat {
    public class XClientEmulator {

        #region Declarations
        // Classes
        private XClientAuthEmulator m_cAuthEmulator;
        private XClientGameEmulator m_cGameEmulator;
        private TCPManager m_cManager;
        // If there's a connection running
        #endregion

        public void EmulateClient(string szName, string szPassword, string szIPAddress = "127.0.0.1", int nPort = 4500) {
            m_cManager = new TCPManager( 1, 2048 * 300 );

            Config.ConfigNet config = new Config.ConfigNet();
            config.ListenIp = IPAddress.Parse( szIPAddress );
            config.Port = nPort;

            m_cAuthEmulator = new XClientAuthEmulator( m_cManager, config, szName, szPassword, this );
        }

        public void CreateGameServerSession(PacketOut pOut, Config.ConfigNet config, string pAccount) {

            m_cGameEmulator = new XClientGameEmulator( config, m_cManager, this, pAccount, pOut );
        }

        public void CreateMessagePacket(string szSource, string szMsg, int nType) {
            m_cGameEmulator.CreateMessagePacket( szSource, szMsg, nType );
        }

        public void OnGameLogin(int nCode) {
            m_cGameEmulator.CreateLoginPacket( nCode );
        }

        public void OnServerSelect(int nCode) {
            m_cAuthEmulator.CreateServerSelectPacket( nCode );
        }

        public void StopClient() {
            if ( m_cAuthEmulator != null ) m_cAuthEmulator.Disconnect();
            if ( m_cGameEmulator != null ) m_cGameEmulator.Disconnect();
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
