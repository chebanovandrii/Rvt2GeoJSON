using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Architexor.GeoJSON
{
	// Testing Site
	// https://samples.azuremaps.com/geospatial-files/drag-and-drop-geojson-file-onto-map

	public class Application : IExternalApplication
	{
		//	class instance
		public static Application thisApp = null;//	internal
		internal static UIControlledApplication UIContApp = null;

		public Result OnShutdown(UIControlledApplication application)
		{
		/*	//	Check for update
			if (UpdateHelper.CheckForUpdate(int.Parse(UIContApp.ControlledApplication.VersionNumber)))
			{
				try
				{
					string url = Assembly.GetExecutingAssembly().Location;
					url = url.Substring(0, url.LastIndexOf("\\")) + "\\";
					//	Get the url of the plugin
					Process.Start(url + "AutoUpdater.exe");
				}
				catch (Exception e)
				{
					TaskDialog.Show("Error", e.Message);
				}
			}*/

			return Result.Succeeded;
		}

		public Result OnStartup(UIControlledApplication application)
		{
			thisApp = this;
			UIContApp = application;

			BitmapSource bs;

			//	Create a custom ribbon tab
			string tabName = "Architexor";
			try
			{
				application.CreateRibbonTab(tabName);
			}
			catch (Exception) { }

				
			//	Create push buttons
			string url = Assembly.GetExecutingAssembly().Location;
			//bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
			//	Architexor.GeoJSON.Properties.Resources.btn_export.GetHbitmap(),
			//		IntPtr.Zero,
			//		Int32Rect.Empty,
			//		BitmapSizeOptions.FromEmptyOptions());
			PushButtonData btnExport = new("btnExport", "Export GeoJSON", url, "Architexor.Commands.GeoJSON.Export");
			PushButtonData btnDevices = new("btnDevices", "Update Devices", url, "Architexor.Commands.GeoJSON.Devices");
			PushButtonData btnParams = new("btnParams", "Assign Params", url, "Architexor.Commands.GeoJSON.AssignParams");
			//{
			//	Image = bs,
			//	LargeImage = bs
			//};

			//  Create a ribbon panel
			RibbonPanel panel = application.CreateRibbonPanel(tabName, "GeoJSON");

			panel.AddItem(btnExport);
			panel.AddItem(btnDevices);
			panel.AddItem(btnParams);

			return Result.Succeeded;
		}

		public UIControlledApplication GetUIContApp()
		{
			return UIContApp;
		}

		public static UIApplication GetUiApplication()
		{
			string versionNumber = UIContApp.ControlledApplication.VersionNumber;
			string fieldName = versionNumber switch
			{
				"2017" or "2018" or "2019" or "2020" or "2021" or "2022" or "2023" or "2024" or "2025" => "m_uiapplication",
				_ => "m_uiapplication",
			};
			var fieldInfo = UIContApp.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

			var uiApplication = (UIApplication)fieldInfo?.GetValue(UIContApp);

			return uiApplication;
		}
	}
}
