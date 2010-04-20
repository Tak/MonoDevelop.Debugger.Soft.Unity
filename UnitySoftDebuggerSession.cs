// 
// UnityDebuggerSession.cs
//   based on IPhoneDebuggerSession.cs
//  
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
//       Lucas Meijer <lucas@unity3d.com>
//       Levi Bard <levi@unity3d.com>
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
using Mono.Debugger;
using Mono.Debugging;
using Mono.Debugging.Client;
using System.Threading;
using System.Diagnostics;
using System.IO;
using MonoDevelop.Core;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace MonoDevelop.Debugger.Soft.Unity
{
	/// <summary>
	/// Debugger session for Unity scripting code
	/// </summary>
	public class UnitySoftDebuggerSession : RemoteSoftDebuggerSession
	{
		Process unityprocess;
		string unityPath;
		
		public UnitySoftDebuggerSession ()
		{
			// TODO: Make this configurable
			if (PropertyService.IsMac) {
				unityPath = "/Applications/Unity/Unity.app/Contents/MacOS/Unity";
			} else if (PropertyService.IsWindows) {
				unityPath = "";
			}
			
			Adaptor.BusyStateChanged += delegate(object sender, BusyStateEventArgs e) {
				SetBusyState (e);
			};
		}
		
		protected override void OnRun (DebuggerStartInfo startInfo)
		{
			var dsi = (UnityDebuggerStartInfo) startInfo;
			StartUnity(dsi);
			StartListening(dsi);
		}

		/// <summary>
		/// Launch Unity
		/// </summary>
		void StartUnity (UnityDebuggerStartInfo dsi)
		{
			if (unityprocess != null)
				throw new InvalidOperationException ("Unity already started");
			
			var psi = new ProcessStartInfo (unityPath)
			// var psi = new ProcessStartInfo ("/Users/levi/Code/unity/unity-2.7/build/MacEditor/Unity.app/Contents/MacOS/Unity")
			{
				Arguments = string.Empty,
				UseShellExecute = false,
				WorkingDirectory = Path.GetDirectoryName (unityPath)
			};

//			var sdbLog = Environment.GetEnvironmentVariable ("MONODEVELOP_SDB_LOG");
//			if (!String.IsNullOrEmpty (sdbLog)) {
//				options = options ?? new LaunchOptions ();
//				options.AgentArgs = string.Format ("loglevel=1,logfile='{0}'", sdbLog);
//			}
			
			// Pass through environment
			foreach (DictionaryEntry env in Environment.GetEnvironmentVariables ()) {
				Console.WriteLine ("{0} = \"{1}\"", env.Key, env.Value);
				psi.EnvironmentVariables[(string)env.Key] = (string)env.Value;
			}
			foreach (var env in dsi.EnvironmentVariables) {
				Console.WriteLine ("{0} = \"{1}\"", env.Key, env.Value);
				psi.EnvironmentVariables[env.Key] = env.Value;
			}
			
			// Connect back to soft debugger client
			psi.EnvironmentVariables.Add ("MONO_ARGUMENTS","--debugger-agent=transport=dt_socket,address=127.0.0.1:57432");
			psi.EnvironmentVariables.Add ("MONO_LOG_LEVEL","debug");
			
			unityprocess = Process.Start (psi);
			
			unityprocess.EnableRaisingEvents = true;
			unityprocess.Exited += delegate
			{
				EndSession ();
			};
		}
		
		protected override void EndSession ()
		{
			try {
				EndUnityProcess ();
				base.EndSession ();
			} catch (Mono.Debugger.Soft.VMDisconnectedException) {
			}
		}
		
		protected override void OnExit ()
		{
			try {
				EndUnityProcess ();
				base.OnExit ();
			} catch (Mono.Debugger.Soft.VMDisconnectedException) {
			}
		}
		
		void EndUnityProcess ()
		{
			if (unityprocess == null || unityprocess.HasExited)
			{
				unityprocess = null;
				return;
			}

			unityprocess.Kill ();
			unityprocess.WaitForExit (5000);
			unityprocess = null;
		}
	}
}
