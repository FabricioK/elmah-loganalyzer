﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ElmahLogAnalyzer.Core.Infrastructure.FileSystem;
using ElmahLogAnalyzer.Core.Infrastructure.Settings;
using ElmahLogAnalyzer.Core.Infrastructure.Web;

namespace ElmahLogAnalyzer.Core.Domain
{
	public class ErrorLogDownloader : IErrorLogDownloader
	{
		private readonly IWebRequestHelper _webRequst;
		private readonly IFileSystemHelper _fileSystemsHelper;
		private readonly ICsvParser _csvParser;
		private readonly ISettingsManager _settingsManager;
		
		public ErrorLogDownloader(NetworkConnection connection, IWebRequestHelper webRequst, IFileSystemHelper fileSystemHelper, ICsvParser csvParser, ISettingsManager settingsManager)
		{
			Connection = connection;

			_webRequst = webRequst;
			_fileSystemsHelper = fileSystemHelper;
			_csvParser = csvParser;
			_settingsManager = settingsManager;

			ResolveDownloadDirectory();
		}

		public NetworkConnection Connection { get; private set; }

		public string DownloadDirectory { get; private set; }

		public IEnumerable<KeyValuePair<Uri, DateTime>> CsvContent { get; private set; }

		public void Download()
		{
			CreateDownloadDirectory();
			ResolveLogsAvailableForDownload();

			var entries = ResolveLogsToDownload();

			var webRequst = _webRequst;
			var downloadDirectory = DownloadDirectory;

			var errors = // ...
				from entry in entries 
				let downloadUrl = ResolveErrorLogDownloadUrl(Connection, entry)
				let fileName = ResolveErrorLogFileName(downloadUrl.Uri, entry.Value)
				let path = Path.Combine(downloadDirectory, fileName)
				where !ErrorlogAlreadyDownloaded(path)
				select new
				{
					FilePath = path,
					Xml = webRequst.Uri(downloadUrl),
				};

			Parallel.ForEach(errors, error => _fileSystemsHelper.CreateTextFile(error.FilePath, error.Xml));
		}

		private static NetworkConnection ResolveErrorLogDownloadUrl(NetworkConnection connection, KeyValuePair<Uri, DateTime> entry)
		{
			var url = entry.Key.AbsoluteUri.Replace("/detail?", "/xml?");
			
			if (connection.IsHttps)
			{
				url = url.Replace("http:", "https:");
			}

			return connection.CopyWithCredentials(url);
		}

		private static string ResolveErrorLogFileName(Uri detailsUrl, DateTime time)
		{
			const string template = "error-{0:yyyy'-'MM'-'dd}T{0:HHmmss}Z-{1}.xml";

			var startIndex = detailsUrl.AbsoluteUri.LastIndexOf('=');
			var id = detailsUrl.AbsoluteUri.Substring(startIndex + 1);
			
			return string.Format(CultureInfo.InvariantCulture, template, time.ToUniversalTime(), id);
		}

	    private void ResolveDownloadDirectory()
	    {
	    	var folder = DownloadDirectoryResolver.Resolve(Connection.Uri);
			DownloadDirectory = Path.Combine(_fileSystemsHelper.GetCurrentDirectory(), folder);
		}

		private void CreateDownloadDirectory()
		{
			if (!_fileSystemsHelper.DirectoryExists(DownloadDirectory))
			{
				_fileSystemsHelper.CreateDirectory(DownloadDirectory);
			}
		}
		
		private void ResolveLogsAvailableForDownload()
		{
			var downloadUrl = new ElmahUrlHelper().ResolveCsvDownloadUrl(Connection.Uri);
			var connection = Connection.CopyWithCredentials(downloadUrl.AbsoluteUri);

			var csvContent = _webRequst.Uri(connection);
			CsvContent = _csvParser.Parse(csvContent).ToList(/* materialize */);
		}

		private IEnumerable<KeyValuePair<Uri, DateTime>> ResolveLogsToDownload()
		{
			return _settingsManager.ShouldGetAllLogs ? CsvContent : CsvContent.Take(_settingsManager.GetMaxNumberOfLogs());
		}
		
		private bool ErrorlogAlreadyDownloaded(string path)
		{
			return _fileSystemsHelper.FileExists(path);
		}
	}
}
