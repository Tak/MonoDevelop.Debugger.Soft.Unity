// 
// Util.cs 
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
using MonoDevelop.Core;

namespace MonoDevelop.Debugger.Soft.Unity
{
	public static class Util
	{
		static readonly string unityOSX = "/Applications/Unity/Unity.app/Contents/MacOS/Unity"; 
		static readonly string unityWinX86 = @"C:\Program Files (x86)\Unity\Editor\Unity.exe";
		static readonly string unityWin = @"C:\Program Files\Unity\Editor\Unity.exe";
		public static readonly string UnityLocationProperty = "MonoDevelop.Debugger.Soft.Unity.UnityLocation";
		public static readonly string UnityLaunchProperty = "MonoDevelop.Debugger.Soft.Unity.LaunchUnity";
		
		public static string FindUnity ()
		{
			string unityLocation = string.Empty;
			
			if (PropertyService.IsMac) {
				unityLocation = unityOSX;
			} else if (PropertyService.IsWindows) {
				unityLocation = (File.Exists (unityWinX86)? unityWinX86: unityWin);
			} else {
				LoggingService.LogError ("Unity is not supported on this platform.");
			}
			
			return unityLocation;
		}
	}
}

