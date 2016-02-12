using System;

namespace Rappelz
{
    class XLog
    {
        public static System.IO.TextWriter tw = new System.IO.StreamWriter( System.IO.File.Open( "log.txt", System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read ) );

        static public void Log(string message)
        {
            tw.WriteLine(message);
            Console.WriteLine(message);
            XChatGUI.MainWindow.WindowInstance.AddMessage( null, message, -1 );
        }

        static public void Log(string format, params object[] obj)
        {
            Log(string.Format( format, obj ));
        }

        static public void Debug(string message) {
            tw.WriteLine( message );
            Console.WriteLine( message );
            XChatGUI.MainWindow.WindowInstance.AddMessage( null, message, -3 );
        }

        static public void Debug(string format, params object[] obj) {
            Debug( string.Format( format, obj ) );
        }

        static public void AddMessage(string szSource, string szMsg, int nType) {
            XChatGUI.MainWindow.WindowInstance.AddMessage( szSource, szMsg, nType );
        }
    }
}
