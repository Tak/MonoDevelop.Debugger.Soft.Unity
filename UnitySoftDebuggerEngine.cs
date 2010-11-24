// 
// UnitySoftDebuggerEngine.cs
//   based on MoonlightSoftDebuggerEngine.cs
//  
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
//       Lucas Meijer <lucas@unity3d.com>
// 
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
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
using System.Collections.Generic;
using System.IO;
using MonoDevelop.Debugger;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using Mono.Debugging.Client;
using MonoDevelop.Debugger.Soft;
using System.Net;
using System.Reflection;

namespace MonoDevelop.Debugger.Soft.Unity
{
	public class UnitySoftDebuggerEngine: IDebuggerEngine
	{
		UnitySoftDebuggerSession session;
		
		public string Id {
			get {
				return "Mono.Debugger.Soft.Unity";
			}
		}
		
		static readonly List<string> UserAssemblies = new List<string>{
		};

		public bool CanDebugCommand (ExecutionCommand command)
		{			return (command is UnityExecutionCommand && null == session);
		}
		
		public DebuggerStartInfo CreateDebuggerStartInfo (ExecutionCommand command)
		{
			var cmd = command as UnityExecutionCommand;
			if (null == cmd){ return null; }
			var msi = new UnityDebuggerStartInfo ("Unity");
			msi.SetUserAssemblies (null);
			msi.Arguments = string.Format ("-projectPath \"{0}\"", cmd.ProjectPath);
			return msi;
		}

		public DebuggerFeatures SupportedFeatures {
			get {
				return DebuggerFeatures.Breakpoints | 
					   DebuggerFeatures.Pause | 
					   DebuggerFeatures.Stepping | 
					   DebuggerFeatures.DebugFile |
					   DebuggerFeatures.ConditionalBreakpoints |
					   DebuggerFeatures.Tracepoints |
					   DebuggerFeatures.Catchpoints |
					   DebuggerFeatures.Attaching;
			}
		}
		
		public DebuggerSession CreateSession ()
		{
			session = new UnitySoftDebuggerSession ();
			session.TargetExited += delegate{ session = null; };
			return session;
		}
		
		public ProcessInfo[] GetAttachableProcesses ()
		{
			return new ProcessInfo[0];
		}
		
		public string Name {
			get {
				return "Mono Soft Debugger for Unity";
			}
		}
	}
	
	class UnityDebuggerStartInfo : RemoteDebuggerStartInfo
	{
		public UnityDebuggerStartInfo (string appName)
			: base (appName, IPAddress.Loopback, 57432)
		{
		}
	}
}
