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
    public class XCipher : ICipher
    {
        public void SetKey(String pKey)
        {
        }

        public byte[] Peek(byte[] buff)
        {
            byte[] res = new byte[7];

            Buffer.BlockCopy(buff, 0, res, 0, 7);
            return res;
        }

        public byte[] Peek(byte[] buff, int offset)
        {
            byte[] res = new byte[7];

            Buffer.BlockCopy(buff, offset, res, 0, 7);
            return res;
        }

        public byte[] Encode(byte[] buff)
        {
            return Encode(buff, 0, buff.Length);
        }

        public byte[] Encode(byte[] buff, int offset, int len)
        {
            byte[] res = new byte[len];

            Buffer.BlockCopy(buff, offset, res, 0, len);
            return res;
        }

        public byte[] Decode(byte[] buff)
        {
            return Decode(buff, 0, buff.Length);
        }

        public byte[] Decode(byte[] buff, int offset, int len)
        {
            byte[] res = new byte[len];

            Buffer.BlockCopy(buff, offset, res, 0, len);
            return res;
        }

        public void Clear()
        {
        }
    }
}
