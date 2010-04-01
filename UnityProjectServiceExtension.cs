using System;
using System.Linq;
using System.Collections.Generic;

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
		
		protected override bool CanExecute (WorkspaceItem item, ExecutionContext context, ConfigurationSelector configuration)
		{
			Console.WriteLine ("CanExecute0: {0}", item.Name);
			if (ReferencesUnity (item.GetAllProjects ())) {
				return context.ExecutionHandler.CanExecute (new UnityExecutionCommand ());
			}
			return false;
		}
		
		protected override bool CanExecute (SolutionEntityItem item, ExecutionContext context, ConfigurationSelector configuration)
		{
			Console.WriteLine ("CanExecute1: {0}", item.Name);
			if (item is Project && ReferencesUnity (new Project[]{ (Project)item })) {
				return context.ExecutionHandler.CanExecute (new UnityExecutionCommand ());
			}
			return false;
		}
		
		public override bool CanExecute (IBuildTarget item, ExecutionContext context, ConfigurationSelector configuration)
		{
			Console.WriteLine ("CanExecute2: {0}", item.Name);
			if (item is WorkspaceItem) {
				return CanExecute ((WorkspaceItem)item, context, configuration);
			}
			if (item is Project && ReferencesUnity (new Project[]{ (Project)item })) {
				return context.ExecutionHandler.CanExecute (new UnityExecutionCommand ());
			}
			return false;
		}
		
		protected override bool CanExecute (Solution solution, ExecutionContext context, ConfigurationSelector configuration)
		{
			Console.WriteLine ("CanExecute3: {0}", solution.Name);
			if (ReferencesUnity (solution.GetAllProjects ())) {
				return context.ExecutionHandler.CanExecute (new UnityExecutionCommand ());
			}
			return false;
		}
		
		protected override void Execute (MonoDevelop.Core.IProgressMonitor monitor, WorkspaceItem item, ExecutionContext context, ConfigurationSelector configuration)
		{
			base.Execute(monitor, item, context, configuration);
		}
		
		protected override void Execute (MonoDevelop.Core.IProgressMonitor monitor, SolutionEntityItem item, ExecutionContext context, ConfigurationSelector configuration)
		{
			base.Execute(monitor, item, context, configuration);
		}
		
		public override void Execute (MonoDevelop.Core.IProgressMonitor monitor, IBuildTarget item, ExecutionContext context, ConfigurationSelector configuration)
		{
			base.Execute(monitor, item, context, configuration);
		}
		
		protected override void Execute (MonoDevelop.Core.IProgressMonitor monitor, Solution solution, ExecutionContext context, ConfigurationSelector configuration)
		{
			base.Execute(monitor, solution, context, configuration);
		}
		
		#endregion
	}
	
	/// <summary>
	/// Unity execution command
	/// </summary>
	public class UnityExecutionCommand: ExecutionCommand
	{
		public UnityExecutionCommand ()
		{
		}
		
		#region implemented abstract members of MonoDevelop.Core.Execution.ExecutionCommand
		
		public override string CommandString {
			get {
				return string.Empty;
			}
		}
		
		#endregion
	}
}

