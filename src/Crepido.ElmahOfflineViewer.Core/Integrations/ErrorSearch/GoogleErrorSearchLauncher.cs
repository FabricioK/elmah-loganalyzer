﻿using Crepido.ElmahOfflineViewer.Core.Infrastructure;

namespace Crepido.ElmahOfflineViewer.Core.Integrations.ErrorSearch
{
	public class GoogleErrorSearchLauncher : ErrorSearchLauncherBase
	{
		private const string UrlTemplate = "http://www.google.com/search?q={0}+{1}";

		public GoogleErrorSearchLauncher(IProcessStarter processStarter) : base(processStarter)
		{
		}

		public override string GetUrlTemplate()
		{
			return UrlTemplate;
		}
	}
}
