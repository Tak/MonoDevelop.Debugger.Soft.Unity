using System;

using MonoDevelop.Projects;

namespace MonoDevelop.Debugger.Soft.Unity
{
	public class UnityProjectServiceExtension: ProjectServiceExtension
	{
		protected override bool CanExecute (WorkspaceItem item, ExecutionContext context, ConfigurationSelector configuration)
		{
			return base.CanExecute(item, context, configuration);
		}
		
		protected override void Execute (MonoDevelop.Core.IProgressMonitor monitor, WorkspaceItem item, ExecutionContext context, ConfigurationSelector configuration)
		{
			base.Execute(monitor, item, context, configuration);
		}
		
		
		
	}
}

