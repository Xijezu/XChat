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
    public class XRC4Cipher : ICipher
    {
        public XRC4Cipher()
        {
            rc4 = new RC4Cipher();
            Clear();
        }

        public void SetKey(String pKey)
        {
            rc4.init(pKey);
        }

        public byte[] Peek(byte[] buff)
        {
            byte[] res = new byte[7];
            tryCipher(ref buff, 0, ref res, 7);
            return res;
        }

        public byte[] Peek(byte[] buff, int offset)
        {
            byte[] res = new byte[7];
            tryCipher(ref buff, offset, ref res, 7);
            return res;
        }

        public byte[] Encode(byte[] buff)
        {
            return Encode(buff, 0, buff.Length);
        }

        public byte[] Encode(byte[] buff, int offset, int len)
        {
            byte[] res = new byte[len];

            doCipher(ref buff, offset, ref res, len);
            return res;
        }

        public byte[] Decode(byte[] buff)
        {
            return Decode(buff, 0, buff.Length);
        }

        public byte[] Decode(byte[] buff, int offset, int len)
        {
            byte[] res = new byte[len];
            doCipher(ref buff, offset, ref res, len);
            return res;
        }


        public void Clear()
        {
            rc4.init("Neat & Simple");
        }


        private void tryCipher(ref byte[] pSource, int off, ref byte[] pTarget, int len)
        {
            RC4Cipher.State ss = new RC4Cipher.State();
            ss.m_nBox = new byte[256];
            rc4.m_state.m_nBox.CopyTo(ss.m_nBox, 0);
            ss.x = rc4.m_state.x;
            ss.y = rc4.m_state.y;

            rc4.code(ref pSource, off, ref pTarget, len);

            ss.m_nBox.CopyTo(rc4.m_state.m_nBox, 0);
            rc4.m_state.x = ss.x;
            rc4.m_state.y = ss.y;

        }

        private void doCipher(ref byte[] pSource, int off, ref byte[] pTarget, int len)
        {
            rc4.code(ref pSource, off, ref pTarget, len);
        }

        RC4Cipher rc4;
    }
}
