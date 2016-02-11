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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rappelz.GameServer.Network
{
    public class RC4Cipher
    {

        public bool init(String pKey)
        {
            return prepareKey(pKey);
        }

        public void code(ref byte[] pSrc, int off, ref byte[] pDst, int len)
        {
            codeBlock(ref pSrc, off, ref pDst, len);
        }

        public void encode(ref byte[] pSrc, int off, ref byte[] pDst, int len)
        {
            codeBlock(ref pSrc, off, ref pDst, len);
        }

        public void decode(ref byte[] pSrc, int off, ref byte[] pDst, int len)
        {
            codeBlock(ref pSrc, off, ref pDst, len);
        }

        public struct State
        {
            public byte[] m_nBox;
            public int x, y;
        };

        public void saveStateTo(ref State outState)
        {
            outState = m_state;
        }

        public void loadStateFrom(State aState)
        {
            m_state = aState;
        }

        bool prepareKey(String pKey)
        {
            if (String.IsNullOrEmpty(pKey))
                return false;

            // Perform the conversion of the encryption key from unicode to ansi
            //
            byte[] asciiBytes = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, Encoding.Unicode.GetBytes(pKey));

            //
            // Populate m_nBox
            //
            long KeyLen = pKey.Length;

            //
            // First Loop
            //
            m_state.m_nBox = new byte[256];
            for (long count = 0; count < 256; count++)
            {
                m_state.m_nBox[count] = (byte)count;
            }

            //
            // Used to populate m_nBox
            //
            long index2 = 0;

            //
            // Second Loop
            //
            for (long count = 0; count < 256; count++)
            {
                index2 = (index2 + m_state.m_nBox[index2] + asciiBytes[index2 % KeyLen]) % 256;
                byte temp = m_state.m_nBox[count];
                m_state.m_nBox[count] = m_state.m_nBox[index2];
                m_state.m_nBox[index2] = temp;
            }

            m_state.x = m_state.y = 0;

            skipFor(1013);

            return true;
        }

        void skipFor(int len)
        {
            //
            // indexes used below
            //
            long i = m_state.x;
            long j = m_state.y;

            //
            // Run Algorithm
            //
            for (long offset = 0; offset < len; offset++)
            {
                i = (i + 1) % 256;
                j = (j + m_state.m_nBox[i]) % 256;
                byte temp = m_state.m_nBox[i];
                m_state.m_nBox[i] = m_state.m_nBox[j];
                m_state.m_nBox[j] = temp;
            }

            m_state.x = (int)i;
            m_state.y = (int)j;
        }

        void codeBlock(ref byte[] pSrc, int off, ref byte[] pDst, int len)
        {
            //
            // indexes used below
            //
            long i = m_state.x;
            long j = m_state.y;

            //
            // Run Algorithm
            //
            for (long offset = 0; offset < len; offset++)
            {
                i = (i + 1) % 256;
                j = (j + m_state.m_nBox[i]) % 256;
                byte temp = m_state.m_nBox[i];
                m_state.m_nBox[i] = m_state.m_nBox[j];
                m_state.m_nBox[j] = temp;
                byte a = pSrc[offset + off];
                byte b = m_state.m_nBox[(m_state.m_nBox[i] + m_state.m_nBox[j]) % 256];
                pDst[offset] = (byte)((int)a ^ (int)b);
            }

            m_state.x = (int)i;
            m_state.y = (int)j;
        }

        public State m_state;

    }
}
