﻿using System;
using Crepido.ElmahOfflineViewer.Core.Domain;
using Crepido.ElmahOfflineViewer.Core.Infrastructure.FileSystem;
using Crepido.ElmahOfflineViewer.TestHelpers.Fakes;
using NUnit.Framework;

namespace Crepido.ElmahOfflineViewer.IntegrationTests.Domain
{
	[TestFixture]
	public class FileErrorLogSourceTests : IntegrationTestBase
	{
		[Test]
		public void GetLogs_ParsesAllLogsInDirectory()
		{
			// arrange
			var source = CreateSource();

			// act
			var result = source.GetLogs(TestFilesDirectory);

			// assert
			Assert.That(result.Count, Is.EqualTo(20));
		}
		
		[Test]
		public void GetLogs_MaxNumberOfLogsIsTen_ParsesTenLatestLogsInDirectory()
		{
			// arrange
			var settings = new FakeSettingsManager();
			settings.SetMaxNumberOfLogs(10);

			var source = new FileErrorLogSource(new FileSystemHelper(), new ErrorLogFileParser(new FakeLog()), settings, new FakeLog());

			// act
			var result = source.GetLogs(TestFilesDirectory);

			// assert
			Assert.That(result.Count, Is.EqualTo(10));
		}

		[Test]
		public void GetLogs_DirectoryDoesNotExist_ThrowsApplicationException()
		{
			// arrange
			var source = CreateSource();

			// act
			var result = Assert.Throws<ApplicationException>(() => source.GetLogs(@"x:\invalid\directory"));

			// assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Message, Is.EqualTo(@"The directory: x:\invalid\directory was not found"));
		}

		private static FileErrorLogSource CreateSource()
		{
			return new FileErrorLogSource(new FileSystemHelper(), new ErrorLogFileParser(new FakeLog()), new FakeSettingsManager(), new FakeLog());
		}
	}
}
