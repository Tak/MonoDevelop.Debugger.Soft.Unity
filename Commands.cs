// 
// Commands.cs 
//   
// Author:
//       Levi Bard <levi@unity3d.com>
// 
// Copyright (c) 2010 Unity Technologies
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
// 
// 
using System;
using System.IO;

using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Core;
using MonoDevelop.Components.Commands;

namespace MonoDevelop.Debugger.Soft.Unity
{
	public enum Commands
	{
		SearchReference
	}
	
	class SearchReferenceCommandHandler: MonoDevelop.Components.Commands.CommandHandler
	{
		string apiBase;
		
		static string onlineApiBase = "http://unity3d.com/support/documentation/ScriptReference";
		static string classReferencePage = "20_class_hierarchy.html";
		static string searchPage = "30_search.html";
		
		public SearchReferenceCommandHandler()
		{
			string[] paths = new string[]{};
			
			
			if (PropertyService.IsMac) {
				paths = new string[]{ "/Applications/Unity/Documentation/ScriptReference" };
			} else if (PropertyService.IsWindows) {
				paths = new string[]{ "C:/Program Files/Unity/Documentation/ScriptReference", "C:/Program Files (x86)/Unity/Documentation/Script Reference" };
			}// Initialize script reference base path
			
			
			foreach (string path in paths) {
				if (Directory.Exists (path)) {
					apiBase = path;
					break;
				}
			}
			
			if (string.IsNullOrEmpty (apiBase)) {
				apiBase = onlineApiBase;
			}// Fall back to online docs if local script reference isn't found
		}
		
		// Always enable API Reference Search
		protected override void Update (CommandInfo item)
		{
			item.Visible = item.Enabled = true;
		}
		
		protected override void Run ()
		{
			string token = GetCurrentToken ();
			
			// Fallback to class reference root if no token found
			string url = string.Format ("{0}/{1}", apiBase, classReferencePage); 
			
			if (!string.IsNullOrEmpty (token)) {
				url = string.Format("{0}/{1}.html", apiBase, token);
				if (!File.Exists (url)) {
					url = string.Format ("{0}/{1}.html", apiBase, token.Replace ('.', '-'));
					if (!File.Exists (url)) {
						url = string.Format ("{0}/{1}?q={2}", onlineApiBase, searchPage, token);
					}// If changing Base.member to Base-member doesn't help, fall back to online search
				}// If a local literal path isn't found for the token
			}// If a token is found
			
			if (!url.StartsWith ("http://", StringComparison.OrdinalIgnoreCase)) {
				url = string.Format ("file://{0}", url);
			}// Prepend file:// for local lookups
			
			DesktopService.ShowUrl (url);
		}
		
		// Characters that signify the beginning or end of a searchable token
		static char[] tokenBreakers = { ' ', '\t', '(', ')', '[', ']', '{', '}', ';', ':' };
		
		// Get the currently highlighted token from the active document
		static string GetCurrentToken() {
			Document doc = IdeApp.Workbench.ActiveDocument;
			
			if (null != doc) {
				if (doc.TextEditorData.IsSomethingSelected) return doc.TextEditorData.SelectedText;
				int line = doc.TextEditor.CursorLine;
				int column = Math.Max (1, doc.TextEditor.CursorColumn-1);
				string lineText = doc.TextEditor.GetLineText (line);
				
				if (3 < lineText.Length) {
					int start = lineText.LastIndexOfAny (tokenBreakers, column-1);
					int end = lineText.IndexOfAny (tokenBreakers, column);
					
					if (0 > end) end = lineText.Length;
					
					int tokenLength = end - start - 1;
					
					if (0 < tokenLength && lineText.Length >= start + 1 + tokenLength) {
						return lineText.Substring (start+1, tokenLength).Trim ();
					}// If we found a valid token
				}// If we have a searchable lineText
			}// If a document is open
			
			return string.Empty;
		}
	}

}

