// 
// PlayerConnection.cs 
//   
// Authors:
//       Kim Steen Riber <kim@unity3d.com>
//       Mantas Puida <mantas@unity3d.com>
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
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Text.RegularExpressions;

namespace MonoDevelop.Debugger.Soft.Unity
{
	/// <summary>
	/// Discovery subset of native PlayerConnection class.
	/// </summary>
	public class PlayerConnection
	{
		public const int PLAYER_MULTICAST_PORT = 54998;
		public const string PLAYER_MULTICAST_GROUP = "225.0.0.222";
		
		private Socket m_MulticastSocket = null;
		private HashSet<string> m_AvailablePlayers = new HashSet<string>();
		
		public HashSet<string> AvailablePlayers {
			get {
				return new HashSet<string>(m_AvailablePlayers);
			}
		}
		
		public struct PlayerInfo
		{
			public IPEndPoint m_IPEndPoint;
			public UInt32 m_Flags;
			public UInt32 m_Guid;
			public UInt32 m_EditorGuid;
			public string m_Id;
			
			public override string ToString ()
			{
				return string.Format ("PlayerInfo {0} {1} {2} {3} {4} {5}", m_IPEndPoint.Address, m_IPEndPoint.Port,
									  m_Flags, m_Guid, m_EditorGuid, m_Id);
			}
			
			public static PlayerInfo Parse(string playerString)
			{
				PlayerInfo res = new PlayerInfo();
				
				try {
					// "[IP] %s [Port] %u [Flags] %u [Guid] %u [EditorId] %u [id] %s"
					Regex r = new Regex("\\[IP\\] (?<g1>.*) \\[Port\\] (?<g2>.*) \\[Flags\\] (?<g3>.*)" +
										" \\[Guid\\] (?<g4>.*) \\[EditorId\\] (?<g5>.*) \\[id\\] (?<g6>.*)");
					
					MatchCollection matches = r.Matches(playerString);
					
					if (matches.Count != 1)
					{
						throw new Exception(string.Format("Player string not recognised {0}", playerString));
					}
					
					string ip = matches[0].Groups["g1"].Value;
					
					res.m_IPEndPoint = new IPEndPoint(IPAddress.Parse(ip), UInt16.Parse (matches[0].Groups["g2"].Value));
					res.m_Flags = UInt32.Parse(matches[0].Groups["g3"].Value);
					res.m_Guid = UInt32.Parse(matches[0].Groups["g4"].Value);
					res.m_EditorGuid = UInt32.Parse(matches[0].Groups["g5"].Value);
					res.m_Id = matches[0].Groups["g6"].Value;
					
					System.Console.WriteLine(res.ToString());
				} catch (Exception e) {
					throw new ArgumentException ("Unable to parse player string", e);
				}
				
				return res;
			}
		}
		
		public PlayerConnection ()
		{
			try
			{
				m_MulticastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				m_MulticastSocket.ExclusiveAddressUse = false;
				m_MulticastSocket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				IPEndPoint ipep = new IPEndPoint(IPAddress.Any, PLAYER_MULTICAST_PORT);
				m_MulticastSocket.Bind(ipep);
				
				IPAddress ip=IPAddress.Parse(PLAYER_MULTICAST_GROUP);
				m_MulticastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, 
									new MulticastOption(ip,IPAddress.Any));
			}
			catch
			{
				m_MulticastSocket = null;
				throw;
			}
		}
		
		public void Poll ()
		{
			if (m_MulticastSocket != null && m_MulticastSocket.Available > 0)
			{ 
				byte[] buffer = new byte[1024];
				
				int num = m_MulticastSocket.Receive(buffer);
				string str = System.Text.Encoding.ASCII.GetString(buffer, 0, num);
				
				RegisterPlayer(str);
			}
		}
		
		protected void RegisterPlayer(string playerString)
		{
			m_AvailablePlayers.Add(playerString);
		}
	}
}

