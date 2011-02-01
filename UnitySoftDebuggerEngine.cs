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
using System.IO;
using System.Net;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

using MonoDevelop.Debugger;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using Mono.Debugging.Client;
using MonoDevelop.Debugger.Soft;

namespace MonoDevelop.Debugger.Soft.Unity
{
	public class UnitySoftDebuggerEngine: IDebuggerEngine
	{
		UnitySoftDebuggerSession session;
		static PlayerConnection unityPlayerConnection;
		
		internal static Dictionary<uint, PlayerConnection.PlayerInfo> UnityPlayers {
			get;
			private set;
		}
		
		static UnitySoftDebuggerEngine ()
		{
			UnityPlayers = new Dictionary<uint, PlayerConnection.PlayerInfo> ();
			
			try {
			// HACK: Poll Unity players
			unityPlayerConnection = new PlayerConnection ();
			ThreadPool.QueueUserWorkItem (delegate {
				while (true) {
					lock (unityPlayerConnection) {
						unityPlayerConnection.Poll ();
					}
					Thread.Sleep (1000);
				}
			});
			} catch (Exception e)
			{
				LoggingService.LogError ("Error launching player connection discovery service: Unity player discovery will be unavailable", e);
			}
		}
		
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
			int index = 1;
			List<ProcessInfo> processes = new List<ProcessInfo> ();
			bool foundEditor = false;
			Process[] systemProcesses = Process.GetProcesses ();
			
			if (null != unityPlayerConnection) {
				lock (unityPlayerConnection) {
					foreach (string player in unityPlayerConnection.AvailablePlayers) {
						try {
							PlayerConnection.PlayerInfo info = PlayerConnection.PlayerInfo.Parse (player);
							if (info.m_AllowDebugging) {
								UnityPlayers[info.m_Guid] = info;
								processes.Add (new ProcessInfo (info.m_Guid, info.m_Id));
								++index;
							}
						} catch {
							// Don't care; continue
						}
					}
				}
			}
			
			if (null != systemProcesses) {
				foreach (Process p in systemProcesses) {
					if (p.ProcessName.StartsWith ("unity", StringComparison.OrdinalIgnoreCase) ||
						p.ProcessName.Contains ("Unity.app")) {
						processes.Add (new ProcessInfo (p.Id, string.Format ("{0} ({1})", "Unity Editor", p.ProcessName)));
						foundEditor = true;
					}
				}
			}
			
			if (!foundEditor && PropertyService.IsMac) {
				processes.Add (new ProcessInfo (56432, "Unity Editor (placeholder)"));
			}
			
			return processes.ToArray ();
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
