﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using Elastic.Installer.Domain.Configuration.Wix.Session;

namespace Elastic.InstallerHosts.Elasticsearch.Tasks.Immediate
{
	public class ValidateArgumentsTask : ElasticsearchInstallationTaskBase
	{
		public ValidateArgumentsTask(string[] args, ISession session) : base(args, session) { }

		protected override bool ExecuteTask()
		{
			this.Session.Log($"Session Installing: {this.Session.IsInstalling}");
			this.Session.Log($"Session Uninstalling: {this.Session.IsUninstalling}");
			this.Session.Log($"Session Rollback: {this.Session.IsRollback}");
			this.Session.Log($"Session Upgrading: {this.Session.IsUpgrading}");
			this.Session.Log("Passed Args:\r\n" + string.Join(", ", this.SanitizedArgs));
			this.Session.Log("ViewModelState:\r\n" + this.InstallationModel);
			if (!this.InstallationModel.IsValid || this.InstallationModel.Steps.Any(s => !s.IsValid))
			{
				var failures = this.InstallationModel.ValidationFailures
					.Concat(this.InstallationModel.Steps.SelectMany(s => s.ValidationFailures))
					.ToList();

				var validationFailures = ValidationFailures(failures);
				throw new Exception("Cannot continue installation because of the following errors" + Environment.NewLine + validationFailures);
			}

			var installDir = this.Session.Get<string>("INSTALLDIR");
			var version = this.InstallationModel.NoticeModel.CurrentVersion.ToString();
			if (!new Regex(version + @"\\?$").IsMatch(installDir))
				throw new Exception($"INSTALLDIR does not end in current version number: {version} ({installDir})");
			return true;
		}
	}
}