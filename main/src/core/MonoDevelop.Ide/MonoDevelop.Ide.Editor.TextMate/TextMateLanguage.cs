﻿//
// TextMateLanguage.cs
//
// Author:
//       Mike Krüger <mikkrg@microsoft.com>
//
// Copyright (c) 2016 Microsoft Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using MonoDevelop.Ide.Editor.Extension;
using MonoDevelop.Ide.Editor.Highlighting;
using MonoDevelop.Ide.Editor.Highlighting.RegexEngine;
using System.Collections.Generic;
using System.Linq;

namespace MonoDevelop.Ide.Editor.TextMate
{
	public class TextMateLanguage
	{
		readonly System.Collections.Immutable.ImmutableStack<string> scope;

		Dictionary<string, string> shellVariables;
		Dictionary<string, string> ShellVariables {
			get {
				if (shellVariables != null)
					return shellVariables;
				shellVariables = new Dictionary<string, string> ();
				foreach (var setting in SyntaxHighlightingService.GetSettings (scope).Where (s => s.Settings.ContainsKey ("shellVariables"))) {
					var vars = (PArray)setting.Settings ["shellVariables"];
					foreach (var d in vars.OfType<PDictionary> ()) {
						var name = d.Get<PString> ("name").Value;
						shellVariables [name] = d.Get<PString> ("value").Value;
					}
				}
				return shellVariables;
			}
		}


		internal IEnumerable<TmSnippet> Snippets {
			get {
				return SyntaxHighlightingService.GetSnippets (scope);
			}
		}

		string GetCommentStartString (int num)
		{
			if (num > 1)
				return "TM_COMMENT_START_" + (num + 1);
			return "TM_COMMENT_START";
		}

		string GetCommentEndString (int num)
		{
			if (num > 1)
				return "TM_COMMENT_END_" + (num + 1);
			return "TM_COMMENT_END";
		}

		List<string> lineComments;
		public IReadOnlyList<string> LineComments {
			get {
				if (lineComments != null)
					return lineComments;
				ExtractComments ();
				return lineComments;
			}
		}

		List<Tuple<string, string>> blockComments;
		public IReadOnlyList<Tuple<string, string>> BlockComments {
			get {
				if (blockComments != null)
					return blockComments;
				ExtractComments ();
				return blockComments;
			}
		}

		void ExtractComments ()
		{
			lineComments = new List<string> ();
			blockComments = new List<Tuple<string, string>> ();
			int i = 0;
			while (true) {
				string start, end;
				if (!ShellVariables.TryGetValue (GetCommentStartString (i), out start))
					break;
				if (ShellVariables.TryGetValue (GetCommentEndString (i), out end)) {
					blockComments.Add (Tuple.Create (start, end));
				} else {
					lineComments.Add (start);
				}
			}
		}

		TextMateLanguage (System.Collections.Immutable.ImmutableStack<string> scope)
		{
			this.scope = scope;
		}

		public static TextMateLanguage Create (System.Collections.Immutable.ImmutableStack<string> scope) => new TextMateLanguage (scope);
	}
}