using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace XChatGUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        static public MainWindow WindowInstance;
        private XChat.XClientEmulator m_iClientEmulator;

        public MainWindow() {
            InitializeComponent();
            richTextBox.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
            WindowInstance = this;
        }

        private void button_Click(object sender, RoutedEventArgs e) {
            string szMsg = textBox.Text;
            if ( szMsg.StartsWith( "/connect" ) ) {
                if(m_iClientEmulator == null || !m_iClientEmulator.IsConnected) {
                    var pLogin = szMsg.Split( ' ' );
                    if ( pLogin.Length != 3 )
                        return;
                    m_iClientEmulator = new XChat.XClientEmulator();
                    m_iClientEmulator.EmulateClient( pLogin[1], pLogin[2] );
                }
            }else if ( szMsg.Equals( "/disconnect" ) ) {
                m_iClientEmulator.CreateLogoutPacket();
            }else if ( szMsg.StartsWith( "/select" ) ) {
                string[] pSelection;
                if((pSelection = szMsg.Split(' ')).Length == 2 ) {
                    int nCode = 0;
                    if(!int.TryParse(pSelection[1], out nCode)) {
                        return;
                    }
                    m_iClientEmulator.CreateServerSelectPacket( nCode );
                    System.Threading.Thread.Sleep(200);
                    XChat.XLog.Log("Connecting to game server...");
                }
            }else if ( szMsg.StartsWith( "/use" ) ) {
                string[] pSelection;
                if ( ( pSelection = szMsg.Split( ' ' ) ).Length == 2 ) {
                    int nCode = 0;
                    if ( !int.TryParse( pSelection[1], out nCode ) ) {
                        return;
                    }
                    m_iClientEmulator.CreateLoginPacket( nCode );
                }
            }
            else {
                onSendMessage( szMsg );
            }
            textBox.Text = string.Empty;
        }

        delegate void ParametizedMethodInvoker(string szSource, string szMsg, int nType);
        public void AddMessage(string szSource, string szMsg, int nType) {
            if ( !Dispatcher.CheckAccess() ) {
                Dispatcher.Invoke( new ParametizedMethodInvoker( AddMessage ), szSource, szMsg, nType );
                return;
            }

            if(nType == -5) // Clear
            {
                richTextBox.Document.Blocks.Clear();
                return;
            }

            if ( szSource == "@PARTY" || szSource == "@GUILD" || szSource == "@FRIEND" ) {
                return;
            }

            switch ( nType ) {
                case (int)ChatType.CHAT_WHISPER_SENT:
                    AppendColor( string.Format( "[To: " ), "#FFDE00" );
                    AppendColor( string.Format( "{0}]: {1}\r", szSource, szMsg ), "#FFFFFF" );
                    break;
                case (int)ChatType.CHAT_NORMAL:
                    AppendColor( string.Format( "{0}: ", szSource ), "#3BC5FE" );
                    AppendColor( string.Format( "{0}\r", szMsg ), "#FFFFFF" );
                    break;
                case (int)ChatType.CHAT_ADV:
                    AppendColor( string.Format( "{0}: {1}\r", szSource, szMsg ), "#00FFFC" );
                    break;
                case (int)ChatType.CHAT_WHISPER:
                case (int)ChatType.CHAT_GM_WHISPER:
                    AppendColor( string.Format( "[From: " ), "#FFDE00" );
                    AppendColor( string.Format( "{0}]: {1}\r", szSource, szMsg ), "#FFFFFF" );
                    break;
                case (int)ChatType.CHAT_GLOBAL:
                    AppendColor( string.Format( "{0}: ", szSource ), "#3BC5FE" );
                    AppendColor( string.Format( "{0}\r", szMsg ), "#A9E9FE" );
                    break;
                case (int)ChatType.CHAT_GM:
                    AppendColor( string.Format( "{0}: {1}\r", szSource, szMsg ), "#FF9900" );
                    break;
                case (int)ChatType.CHAT_PARTY:
                    AppendColor( string.Format( "{0}: ", szSource ), "#97FF00" );
                    AppendColor( string.Format( "{0}\r", szMsg ), "#D7FD8D" );
                    break;
                case (int)ChatType.CHAT_GUILD:
                    AppendColor( string.Format( "{0}: ", szSource ), "#D26EFC" );
                    AppendColor( string.Format( "{0}\r", szMsg ), "#F3B9FD" );
                    break;
                case (int)ChatType.CHAT_NOTICE:
                    AppendColor( string.Format( "System: {1}\r", szSource, szMsg ), "#FFDE00" );
                    break;
                case (int)ChatType.CHAT_ANNOUNCE:
                    AppendColor( string.Format( "Announce: {1}\r", szSource, szMsg ), "#00FF00" );
                    break;
                default:
                    AppendColor( string.Format( "[XCHAT] {1}\r", szSource, szMsg ), "#FFFFFF" );
                    break;
            }
            richTextBox.ScrollToEnd();
        }

        private void onSendMessage(string szMsg) {
            string szSource = "";
            if(string.IsNullOrEmpty(szMsg) ) {
                return;
            }
            switch ( szMsg[0] ) {
                case '%':
                    m_iClientEmulator.CreateMessagePacket( szSource, szMsg.Remove( 0, 1 ), (int)ChatType.CHAT_GUILD );
                    break;
                case '#':
                    m_iClientEmulator.CreateMessagePacket( szSource, szMsg.Remove( 0, 1 ), (int)ChatType.CHAT_PARTY );
                    break;
                case '!':
                    m_iClientEmulator.CreateMessagePacket( szSource, szMsg.Remove( 0, 1 ), (int)ChatType.CHAT_GLOBAL );
                    break;
                case '$':
                    m_iClientEmulator.CreateMessagePacket( szSource, szMsg.Remove( 0, 1 ), (int)ChatType.CHAT_ADV );
                    break;
                case '"':
                    string[] pMsg;
                    if((pMsg = szMsg.Split(' ')).Length < 2 ) {
                        return;
                    }
                    szSource = pMsg[0].Remove( 0, 1 );
                    szMsg = szMsg.Remove( 0, szMsg.IndexOf( ' ' ) + 1 );
                    m_iClientEmulator.CreateMessagePacket( szSource, szMsg, (int)ChatType.CHAT_WHISPER );
                    AddMessage( szSource, szMsg, (int)ChatType.CHAT_WHISPER_SENT );
                    break;
                case '/':
                default:
                    m_iClientEmulator.CreateMessagePacket( szSource, szMsg, (int)ChatType.CHAT_NORMAL );
                    break;
            }
        }

        private void AppendColor(string szMsg, string szColor) {
            BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange( richTextBox.Document.ContentEnd, richTextBox.Document.ContentEnd );
            tr.Text = szMsg;
            try {
                tr.ApplyPropertyValue( TextElement.ForegroundProperty,
                    bc.ConvertFromString( szColor ) );
            }
            catch ( FormatException ) { }
        }

        enum ChatType : int {
            CHAT_WHISPER_SENT = -2,
            CHAT_NORMAL = 0, 
            CHAT_YELL = 1,
            CHAT_ADV = 2,
            CHAT_WHISPER = 3,
            CHAT_GLOBAL = 4,
            CHAT_GM = 6,
            CHAT_GM_WHISPER = 7,
            CHAT_PARTY = 0xA,
            CHAT_GUILD = 0xB,
            CHAT_NOTICE = 20,
            CHAT_ANNOUNCE = 21,
        }

        private void textBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            if(e.Key == System.Windows.Input.Key.Enter ) {
                button_Click( textBox, null );
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_iClientEmulator.CreateLogoutPacket();
        }
    }
}
