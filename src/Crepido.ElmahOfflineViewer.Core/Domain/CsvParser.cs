﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Crepido.ElmahOfflineViewer.Core.Common;
using Microsoft.VisualBasic.FileIO;

namespace Crepido.ElmahOfflineViewer.Core.Domain
{
	public class CsvParser
	{
		public IEnumerable<KeyValuePair<Uri, DateTime>> Parse(string content)
		{
			var bytes = Encoding.Unicode.GetBytes(content);
			using (var stream = new MemoryStream(bytes))
			using (var parser = new TextFieldParser(stream, Encoding.Unicode) { TextFieldType = FieldType.Delimited })
			{
				parser.SetDelimiters(",");
				foreach (var currentRow in parser.Read().Skip(/* headers */ 1))
				{
					var date = DateTime.Parse(currentRow[2], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces);
					var detailsUrl = new Uri(currentRow[9]);
					yield return new KeyValuePair<Uri, DateTime>(detailsUrl, date);
				}
			}
		}
	}
}
