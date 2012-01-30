﻿using System;
using System.Collections.Generic;
using ElmahLogAnalyzer.Core.Common;
using ElmahLogAnalyzer.Core.Infrastructure.Settings;
using ElmahLogAnalyzer.Core.Presentation;
using Moq;
using NUnit.Framework;

namespace ElmahLogAnalyzer.UnitTests.Presentation
{
	[TestFixture]
	public class SettingsPresenterTests : UnitTestBase
	{
		private Mock<ISettingsView> _view;
		private Mock<ISettingsManager> _userSettingsManager;

		[SetUp]
		public void Setup()
		{
			_view = new Mock<ISettingsView>();
			_userSettingsManager = new Mock<ISettingsManager>();

			_userSettingsManager.Setup(x => x.GetMaxNumberOfLogs()).Returns(500);
			_userSettingsManager.Setup(x => x.GetDefaultExportLogsDirectory()).Returns(@"c:\exportedlogs");
		}

		[Test]
		public void Ctor_SetsView()
		{
			// act
			var presenter = new SettingsPresenter(_view.Object, _userSettingsManager.Object);
			
			// assert
			Assert.That(presenter.View, Is.EqualTo(_view.Object));
		}

		[Test]
		public void ViewOnLoaded_DisplaysMaxNumberOfLogsOptionsAndUsersSelectedValue()
		{
			// arrange
			var presenter = new SettingsPresenter(_view.Object, _userSettingsManager.Object);
			
			// act
			_view.Raise(x => x.OnLoaded += null, new EventArgs());

			// assert
			_view.Verify(x => x.LoadMaxNumberOfLogOptions(It.IsAny<List<NameValuePair>>(), "500"), Times.Once());
		}

		[Test]
		public void ViewOnLoaded_DisplaysDefaultExportLogsDirectory()
		{
			// arrange
			var presenter = new SettingsPresenter(_view.Object, _userSettingsManager.Object);

			// act
			_view.Raise(x => x.OnLoaded += null, new EventArgs());

			// assert
			_view.VerifySet(x => x.DefaultExportLogsDirectory = @"c:\exportedlogs", Times.Once());
		}

		[Test]
		public void ViewOnSave_SavesMaxNumberOfLogsOption()
		{
			// arrange
			var presenter = new SettingsPresenter(_view.Object, _userSettingsManager.Object);

			_view.Setup(x => x.MaxNumberOfLogs).Returns(200);

			// act
			_view.Raise(x => x.OnSave += null, new EventArgs());

			// assert
			_userSettingsManager.Verify(x => x.SetMaxNumberOfLogs(200), Times.Once());
		}
		
		[Test]
		public void ViewOnSave_SavesDefaultExportLogsDirectory()
		{
			// arrange
			var presenter = new SettingsPresenter(_view.Object, _userSettingsManager.Object);

			_view.Setup(x => x.DefaultExportLogsDirectory).Returns(@"c:\exportedlogs");

			// act
			_view.Raise(x => x.OnSave += null, new EventArgs());

			// assert
			_userSettingsManager.Verify(x => x.SetDefaultExportLogsDirectory(@"c:\exportedlogs"), Times.Once());
		}
	}
}
