// 
// UnityProjectServiceExtension.cs 
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
using System.Linq;
using System.Collections.Generic;

using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.Core.Execution;

namespace MonoDevelop.Debugger.Soft.Unity
{
	/// <summary>
	/// ProjectServiceExtension to allow Unity projects to be executed under the soft debugger
	/// </summary>
	public class UnityProjectServiceExtension: ProjectServiceExtension
	{
		/// <summary>
		/// Detects whether any of the given projects reference UnityEngine
		/// </summary>
		private static bool ReferencesUnity (IEnumerable<Project> projects)
		{
			return null != projects.FirstOrDefault (project => 
				(project is DotNetProject) && 
				null != ((DotNetProject)project).References.FirstOrDefault (reference =>
				       reference.Reference.Contains ("UnityEngine")
				)
			);
		}
		
		#region ProjectServiceExtension overrides
		
		/// <summary>
		/// Flags Unity projects for debugging with this addin
		/// </summary>
		public override bool CanExecute (IBuildTarget item, ExecutionContext context, ConfigurationSelector configuration)
		{
			if (item is WorkspaceItem) {
				return CanExecute ((WorkspaceItem)item, context, configuration);
			}
			if (item is Project &&
			    !string.IsNullOrEmpty (Util.UnityLocation) &&
			    File.Exists (Util.UnityLocation) &&
			    ReferencesUnity (new Project[]{ (Project)item })) {
				return context.ExecutionHandler.CanExecute (new UnityExecutionCommand (item.BaseDirectory.FullPath));
			}
			return false;
		}
		
		/// <summary>
		/// Launch Unity project
		/// </summary>
		public override void Execute (MonoDevelop.Core.IProgressMonitor monitor, IBuildTarget item, ExecutionContext context, ConfigurationSelector configuration)
		{
			IdeApp.ProjectOperations.CurrentRunOperation = context.ExecutionHandler.Execute (new UnityExecutionCommand (item.BaseDirectory.FullPath), context.ConsoleFactory.CreateConsole (true));
		}
		
		#endregion
	}
	
	/// <summary>
	/// Unity execution command
	/// </summary>
	/// <remarks>
	/// This is necessary to fake out the execution handler for an assembly project
	/// </remarks>
	public class UnityExecutionCommand: ExecutionCommand
	{
		public UnityExecutionCommand (string baseDirectory)
		{
			ProjectPath = baseDirectory;
		}
		
		#region implemented abstract members of MonoDevelop.Core.Execution.ExecutionCommand
		
		public string ProjectPath {
			get; private set;
		}
		
		public override string CommandString {
			get{ return string.Empty; }
		}
		
		#endregion
	}
}

