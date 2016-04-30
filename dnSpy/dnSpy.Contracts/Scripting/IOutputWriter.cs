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

using System;
using dnSpy.Contracts.TextEditor;

namespace dnSpy.Contracts.Scripting {
	/// <summary>
	/// Writes text
	/// </summary>
	public interface IOutputWriter {
		/// <summary>
		/// Writes text
		/// </summary>
		/// <param name="text">Text to write</param>
		/// <param name="color">Color</param>
		void Write(string text, OutputColor color = OutputColor.ReplOutputText);
	}

	/// <summary>
	/// Extension methods
	/// </summary>
	public static class OutputWriterExtensions {
		/// <summary>
		/// Writes text followed by a newline
		/// </summary>
		/// <param name="writer">Writer</param>
		/// <param name="text">Text to write</param>
		/// <param name="color">Color</param>
		public static void WriteLine(this IOutputWriter writer, string text, OutputColor color = OutputColor.ReplOutputText) {
			writer.Write(text, color);
			writer.Write(Environment.NewLine, OutputColor.ReplOutputText);
		}
	}
}