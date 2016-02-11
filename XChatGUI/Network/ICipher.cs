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
    public interface ICipher
    {
        void SetKey(String pKey);
        byte[] Peek(byte[] buff);
        byte[] Peek(byte[] buff, int offset);
        byte[] Encode(byte[] buff);
        byte[] Encode(byte[] buff, int offset, int len);
        byte[] Decode(byte[] buff);
        byte[] Decode(byte[] buff, int offset, int len);
        void Clear();

    }
}
