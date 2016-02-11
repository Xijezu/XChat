using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rappelz.GameServer.Network
{
    public class ServerLog
    {
        public static void LogChat(int nSenderAccountID, int nSenderCharacterID, byte nChatType, int nSenderPosX, int nSenderPosY, int nReceiverAccountID, int nReceiverCharacterID, string pszSenderAccount, string pszSenderCharacter, string pszReceiverAccount, string pszReceiverCharacter, string pszChat)
        {
/*
            const char *v17; // edi@0
            XSyncStreamConnection *v18; // edi@2
            int v19; // eax@3
            char v20; // al@3
            char v21; // al@5
            char v22; // al@7
            char v23; // al@9
            const unsigned __int16 v24; // ax@11
            char *v25; // ebx@13
            void *v26; // ebx@13
            void *v27; // ebx@13
            int v28; // eax@13
            const char *v29; // [sp-Ch] [bp-4024h]@2
            char szBuffer[16384]; // [sp+14h] [bp-4004h]@3
            unsigned int v31; // [sp+4014h] [bp-4h]@1
            int v32; // [sp+4018h] [bp+0h]@1

            v31 = (unsigned int)&v32 ^ __security_cookie;
            if ( g_bWriteChatLog )
            {
                v29 = v17;
                v18 = GetChatLogServerConnection();
                if ( v18 )
                {
                    memset(szBuffer, 0, 0x4000u);
                    v19 = *(_DWORD *)(*(_DWORD *)(__readfsdword(44) + 4 * _tls_index) + 664);
                    *(_WORD *)szBuffer = 0;
                    *(_DWORD *)&szBuffer[5] = v19;
                    *(_DWORD *)&szBuffer[9] = nSenderAccountID;
                    *(_DWORD *)&szBuffer[13] = nSenderCharacterID;
                    szBuffer[17] = nChatType;
                    *(_DWORD *)&szBuffer[18] = nSenderPosX;
                    *(_DWORD *)&szBuffer[22] = nSenderPosY;
                    *(_DWORD *)&szBuffer[26] = nReceiverAccountID;
                    *(_DWORD *)&szBuffer[30] = nReceiverCharacterID;
                    v20 = nSenderAccountLength;
                    *(_WORD *)&szBuffer[2] = 40;
                    szBuffer[4] = 2;
                    if ( nSenderAccountLength == -1 )
                        strlen((char *)pszSenderAccount);
                    szBuffer[34] = v20;
                    v21 = nSenderCharacterLength;
                    if ( nSenderCharacterLength == -1 )
                        strlen((char *)pszSenderCharacter);
                    szBuffer[35] = v21;
                    v22 = nReceiverAccountLength;
                    if ( nReceiverAccountLength == -1 )
                        strlen((char *)pszReceiverAccount);
                    szBuffer[36] = v22;
                    v23 = nReceiverCharacterLength;
                    if ( nReceiverCharacterLength == -1 )
                        strlen((char *)pszReceiverCharacter);
                    szBuffer[37] = v23;
                    v24 = nChatLength;
                    if ( nChatLength == -1 )
                        strlen((char *)pszChat);
                    *(_WORD *)&szBuffer[38] = v24;
                    *(_WORD *)&szBuffer[2] += v24
                                            + (unsigned __int8)szBuffer[34]
                                            + (unsigned __int8)szBuffer[35]
                                            + (unsigned __int8)szBuffer[36]
                                            + (unsigned __int8)szBuffer[37];
                    memcpy_s(&szBuffer[40], 0x3FD8u, pszSenderAccount, (unsigned __int8)szBuffer[34]);
                    v25 = &szBuffer[(unsigned __int8)szBuffer[34] + 40];
                    memcpy_s(
                        &szBuffer[(unsigned __int8)szBuffer[34] + 40],
                        16344 - (unsigned __int8)szBuffer[34],
                        pszSenderCharacter,
                        (unsigned __int8)szBuffer[35]);
                    v26 = &v25[(unsigned __int8)szBuffer[35]];
                    memcpy_s(
                        v26,
                        16344 - (unsigned __int8)szBuffer[34] - (unsigned __int8)szBuffer[35],
                        pszReceiverAccount,
                        (unsigned __int8)szBuffer[36]);
                    v27 = (char *)v26 + (unsigned __int8)szBuffer[36];
                    memcpy_s(
                        v27,
                        16344 - (unsigned __int8)szBuffer[35] - (unsigned __int8)szBuffer[34] - (unsigned __int8)szBuffer[36],
                        pszReceiverCharacter,
                        (unsigned __int8)szBuffer[37]);
                    memcpy_s(
                        (char *)v27 + (unsigned __int8)szBuffer[37],
                        16344
                      - (unsigned __int8)szBuffer[36]
                      - (unsigned __int8)szBuffer[35]
                      - (unsigned __int8)szBuffer[34]
                      - (unsigned __int8)szBuffer[37],
                        pszChat,
                        *(unsigned __int16 *)&szBuffer[38]);
                    v28 = ((int (__stdcall *)(_DWORD, _DWORD))v18->baseclass_0.baseclass_18.vfptr->Write)(
                              szBuffer,
                              *(unsigned __int16 *)&szBuffer[2]);
                    if ( v28 != *(unsigned __int16 *)&szBuffer[2] )
                        chat_log_error(v18, v29);
                }
            }
*/
        }

        public static void Log(ushort id, long n1, long n2, long n3, long n4, long n5, long n6, long n7, long n8, long n9, long n10, long n11, string szStr1, string szStr2, string szStr3, string szStr4)
        {
/*
            int v20; // ebx@0
            int v21; // edi@0
            int v22; // esi@0
            XSyncStreamConnection *v23; // edi@2
            int v24; // ebx@2
            unsigned int v25; // ecx@3
            unsigned int v26; // eax@3
            int v27; // eax@5
            int v28; // eax@9
            int v29; // eax@13
            int v30; // eax@17
            int v31; // esi@18
            int v32; // [sp-Ch] [bp-78h]@3
            int v33; // [sp-8h] [bp-74h]@2
            int v34; // [sp-4h] [bp-70h]@2
            LS_11N4S header; // [sp+0h] [bp-6Ch]@3

            if (g_bWriteLog)
            {
                v34 = v20;
                v33 = v21;
                v23 = GetLogServerConnection();
                v24 = 0;
                if ( v23 )
                {
                    header.baseclass_0.thread_id = *(_DWORD *)(*(_DWORD *)(__readfsdword(44) + 4 * _tls_index) + 668);
                    header.baseclass_0.id = id;
                    header.n1 = n1;
                    header.n2 = n2;
                    header.n3 = n3;
                    header.n4 = n4;
                    header.n5 = n5;
                    header.n6 = n6;
                    header.n7 = n7;
                    header.n8 = n8;
                    header.n9 = n9;
                    header.n10 = n10;
                    v25 = *(_DWORD *)n11;
                    v26 = *((_DWORD *)n11 + 1);
                    v32 = v22;
                    header.baseclass_0.size = 105;
                    header.baseclass_0.type = 1;
                    header.n11 = __PAIR__(v26, v25);
                    if ( szStr1 )
                    {
                        if ( len1 == -1 )
                        {
                            strlen((char *)szStr1);
                            len1 = v27;
                        }
                    }
                    else
                    {
                        len1 = 0;
                    }
                    if ( szStr2 )
                    {
                        if ( len2 == -1 )
                        {
                            strlen((char *)szStr2);
                            len2 = v28;
                        }
                    }
                    else
                    {
                        len2 = 0;
                    }
                    if ( szStr3 )
                    {
                        if ( len3 == -1 )
                        {
                            strlen((char *)szStr3);
                            len3 = v29;
                        }
                    }
                    else
                    {
                        len3 = 0;
                    }
                    if ( szStr4 )
                    {
                        v24 = len4;
                        if ( len4 == -1 )
                        {
                            strlen((char *)szStr4);
                            v24 = v30;
                        }
                    }
                    header.string_length_2 = len2;
                    header.baseclass_0.size += len1 + (_WORD)len2 + v24 + (_WORD)len3;
                    v31 = (int)&v23->baseclass_0.baseclass_18;
                    header.string_length_1 = len1;
                    header.string_length_3 = len3;
                    header.string_length_4 = v24;
                    if ( ((int (__thiscall *)(IStreamIntf *, LS_11N4S *, signed int, int, int, int))v23->baseclass_0.baseclass_18.vfptr->Write)(
                     &v23->baseclass_0.baseclass_18,
                     &header,
                     105,
                     v32,
                     v33,
                     v34) != 105 )
                    log_error(v23, *(const char **)&header);
                    if ( (*(int (__thiscall **)(IStreamIntf *, const char *, _DWORD))(*(_DWORD *)v31 + 8))(
                     &v23->baseclass_0.baseclass_18,
                     szStr1,
                     (unsigned __int16)len1) != len1 )
                    log_error(v23, *(const char **)&header);
                    if ( (*(int (__thiscall **)(IStreamIntf *, const char *, _DWORD))(*(_DWORD *)v31 + 8))(
                     &v23->baseclass_0.baseclass_18,
                     szStr2,
                     (unsigned __int16)len2) != len2 )
                    log_error(v23, *(const char **)&header);
                    if ( (*(int (__thiscall **)(IStreamIntf *, const char *))(*(_DWORD *)v31 + 8))(
                     &v23->baseclass_0.baseclass_18,
                     szStr3) != len3 )
                    log_error(v23, (const char *)(unsigned __int16)len3);
                    if ( (*(int (__thiscall **)(IStreamIntf *))(*(_DWORD *)v31 + 8))(&v23->baseclass_0.baseclass_18) != v24 )
                    log_error(v23, (const char *)(unsigned __int16)v24);
                }
            }
*/
        }
    }
}
