﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using dnSpy.Contracts.Hex;

namespace dnSpy.AsmEditor.Hex.PE {
	sealed class MethodBodyReader {
		readonly HexBuffer buffer;
		readonly IList<uint> rids;
		readonly HexPosition methodBodyPosition;
		readonly HexPosition maxMethodBodyEndPosition;
		HexPosition currentPosition;

		byte headerSize;
		ushort flags;
		ushort maxStack;
		uint codeSize;
		uint localVarSigTok;

		public MethodBodyReader(HexBuffer buffer, IList<uint> rids, HexPosition methodBodyPosition, HexPosition maxMethodBodyEndPosition) {
			this.buffer = buffer;
			this.rids = rids;
			this.methodBodyPosition = methodBodyPosition;
			this.maxMethodBodyEndPosition = maxMethodBodyEndPosition;
		}

		public MethodBodyInfo? Read() {
			currentPosition = methodBodyPosition;
			if (!ReadHeader())
				return null;

			var headerSpan = HexSpan.FromBounds(methodBodyPosition, currentPosition);
			var instructionsSpan = new HexSpan(currentPosition, codeSize);
			currentPosition += codeSize;
			bool isSmallExceptionClauses;
			var exceptionsSpan = ReadExceptionHandlers(out isSmallExceptionClauses);

			return new MethodBodyInfo(rids, headerSpan, instructionsSpan, exceptionsSpan, isSmallExceptionClauses ? MethodBodyInfoFlags.SmallExceptionClauses : MethodBodyInfoFlags.None);
		}

		byte ReadByte() => buffer.ReadByte(currentPosition++);

		ushort ReadUInt16() {
			var res = buffer.ReadUInt16(currentPosition);
			currentPosition += 2;
			return res;
		}

		uint ReadUInt32() {
			var res = buffer.ReadUInt32(currentPosition);
			currentPosition += 4;
			return res;
		}

		// From dnlib's MethodBodyReader
		bool ReadHeader() {
			byte b = ReadByte();
			switch (b & 7) {
			case 2:
			case 6:
				flags = 2;
				maxStack = 8;
				codeSize = (uint)(b >> 2);
				localVarSigTok = 0;
				headerSize = 1;
				break;

			case 3:
				flags = (ushort)((ReadByte() << 8) | b);
				headerSize = (byte)(flags >> 12);
				maxStack = ReadUInt16();
				codeSize = ReadUInt32();
				localVarSigTok = ReadUInt32();

				currentPosition += -12 + headerSize * 4;
				if (headerSize < 3)
					flags &= 0xFFF7;
				headerSize *= 4;
				break;

			default:
				return false;
			}

			if (currentPosition + codeSize > maxMethodBodyEndPosition)
				return false;

			return true;
		}

		// From dnlib's MethodBodyReader
		HexSpan ReadExceptionHandlers(out bool isSmallExceptionClauses) {
			isSmallExceptionClauses = false;
			if ((flags & 8) == 0)
				return default(HexSpan);

			currentPosition = (currentPosition.ToUInt64() + 3) & ~3UL;
			// Only read the first one. Any others aren't used.
			byte b = ReadByte();
			if ((b & 0x3F) != 1)
				return default(HexSpan); // Not exception handler clauses
			if ((b & 0x40) != 0) {
				isSmallExceptionClauses = false;
				return ReadFatExceptionHandlers();
			}
			else {
				isSmallExceptionClauses = true;
				return ReadSmallExceptionHandlers();
			}
		}

		// From dnlib's MethodBodyReader
		static ushort GetNumberOfExceptionHandlers(uint num) => (ushort)num;

		// From dnlib's MethodBodyReader
		HexSpan ReadFatExceptionHandlers() {
			currentPosition--;
			int num = GetNumberOfExceptionHandlers((ReadUInt32() >> 8) / 24);
			return new HexSpan(currentPosition - 4, (ulong)num * 24 + 4);
		}

		// From dnlib's MethodBodyReader
		HexSpan ReadSmallExceptionHandlers() {
			int num = GetNumberOfExceptionHandlers((uint)ReadByte() / 12);
			return new HexSpan(currentPosition - 2, (ulong)num * 12 + 4);
		}
	}
}
