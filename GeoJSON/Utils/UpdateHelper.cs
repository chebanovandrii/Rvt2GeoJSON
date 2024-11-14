using Architexor.Core;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PanelTool.Utils
{
	public static class UpdateHelper
	{
		//	Related to check update feature
		public struct AddInFile
		{
			public string Name;
			public string Version;
			public string Checksum;
		}

		public static List<AddInFile> GetFileList()
		{
			List<AddInFile> files = new();

			try
			{
				string sRes = ApiService.GetResponse(Constants.API_ENDPOINT + "file");

				JArray jArr = JArray.Parse(sRes);
				foreach (JObject obj in jArr)
				{
					AddInFile aif = new AddInFile()
					{
						Name = obj.GetValue("name").ToString(),
						Version = obj.ContainsKey("version") ? obj.GetValue("version").ToString() : "",
					};
					for (int version = 2019; version < 2030; version++)
					{
						if (obj.ContainsKey("checksum_" + version))
						{
							aif.Checksum = obj.GetValue("checksum_" + version).ToString();
							break;
						}
					}

					files.Add(aif);
				}
			}
			catch (Exception)
			{
				//MessageBox.Show("");
			}

			return files;
		}

		public static bool CheckForUpdate(string sRevitVersion)
		{
			string url = Assembly.GetExecutingAssembly().Location;
			url = url.Substring(0, url.LastIndexOf("\\")) + "\\";
			bool bNeed = false;

			try
			{
				List<AddInFile> files = GetFileList();

				foreach (AddInFile aif in files)
				{
					//	Check if needs
					if (!File.Exists(url + aif.Name))
					{
						MessageBox.Show(aif.Name + " needs to be downloaded.");
						bNeed = true;
					}
					else
					{
						//	Check file version
						if (aif.Version != "")
						{
							FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(url + aif.Name);
							_ = new Version(fvi.FileVersion).CompareTo(new Version(aif.Version)) < 0 ? bNeed = true : bNeed = false;
							if (bNeed)
								MessageBox.Show(aif.Name + " " + fvi.FileVersion + " needs to be updated to " + aif.Version + ".");
						}
						else
						{
							string sChecksum;
							using (var md5 = MD5.Create())
							{
								using (var stream = File.OpenRead(url + aif.Name))
								{
									byte[] hash = md5.ComputeHash(stream);
									sChecksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
								}
							}
							if (aif.Checksum != sChecksum && aif.Checksum != "")
								bNeed = true;

							if (bNeed)
								MessageBox.Show(aif.Name + " needs to be updated.");
						}
					}
					if (bNeed)
					{
						//	Need to update
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			if (bNeed)
			{
				
			}
			return bNeed;
		}
	}
}
