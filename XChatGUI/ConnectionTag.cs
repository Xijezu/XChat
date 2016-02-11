using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rappelz.GameServer
{
    public class ConnectionTag
    {
        public string szAccountName;                    // Data           :   this+0x0, Member, Type: 
	    public List<string> vCharacterNameList = new List<string>();         // Data           :   this+0x40, Member, Type: class 
	    public object player;                           // Data           :   this+0x50, Member, Type: struct 
	    public int nAccountID;                          // Data           :   this+0x54, Member, Type: 
	    public int nVersion;                            // Data           :   this+0x58, Member, Type: 
	    public uint nLastReadTime;                      // Data           :   this+0x5C, Member, Type: 
	    public bool bAuthByAuthServer;                  // Data           :   this+0x60, Member, Type: 
	    public byte nPCBangMode;                        // Data           :   this+0x61, Member, Type: 
	    public int nEventCode;                          // Data           :   this+0x64, Member, Type: 
	    public int nAge;                                // Data           :   this+0x68, Member, Type: 
	    public uint nContinuousPlayTime;                // Data           :   this+0x6C, Member, Type: 
	    public uint nContinuousLogoutTime;              // Data           :   this+0x70, Member, Type: 
	    public uint nLastContinuousPlayTimeProcTime;    // Data           :   this+0x74, Member, Type: 
	    public int nConnId;                             // Data           :   this+0x78, Member, Type: 
	    public string strNameToDelete;                  // Data           :   this+0x7C, Member, Type: class 
	    public bool bStorageSecurityCheck;              // Data           :   this+0x98, Member, Type: 
	    public byte[] pSecuritySolutionBuffer;          // Data           :   this+0x9C, Member, Type: 

        public ConnectionTag(string _szAccountName)
        {
            nVersion = -1;
            nAccountID = 0;
            nLastReadTime = 0;
            bAuthByAuthServer = false;
            nPCBangMode = 0;
            nEventCode = 0;
            nAge = 0;
            nContinuousPlayTime = 0;
            nContinuousLogoutTime = 0;
            nLastContinuousPlayTimeProcTime = nLastReadTime;
            nConnId = 0;
            bStorageSecurityCheck = false;
            szAccountName = _szAccountName;
        }

        public ConnectionTag()
        {
            nVersion = -1;
            nAccountID = 0;
            nLastReadTime = 0;
            bAuthByAuthServer = false;
            nPCBangMode = 0;
            nEventCode = 0;
            nAge = 0;
            nContinuousPlayTime = 0;
            nContinuousLogoutTime = 0;
            nLastContinuousPlayTimeProcTime = nLastReadTime;
            nConnId = 0;
            bStorageSecurityCheck = false;
            szAccountName = "";
        }
    }
}
