using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Architexor.Forms.GeoJSON;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Architexor.GeoJSON.Base;
using Architexor.Controllers.GeoJSON;
using Architexor.GeoJSON.Controllers;
using Autodesk.Revit.ApplicationServices;
using FileDialog = System.Windows.Forms.OpenFileDialog;
using System;
using System.Windows.Forms;
using Architexor.Utils;
using RvtApp = Autodesk.Revit.ApplicationServices.Application;
using System.Security.Cryptography;
using Microsoft.Win32;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace Architexor.Commands.GeoJSON
{
	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class AssignParams : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			Document doc = commandData.Application.ActiveUIDocument.Document;
			RvtApp app = uiApp.Application;

			string paramFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			try
			{
				RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\" + "Architexor" + "\\GeoJSON\\SharedParams");
				if (null != key)
				{
					paramFilePath = key.GetValue("FilePath").ToString();
					key.Close();
				}
			}
			catch (Exception) { }

			FileDialog fileDialog = new FileDialog
			{
				Title = "Select Shared Parameter File",
				Filter = "Shared Parameter Files (*.txt;*.xml)|*.txt;*.xml|All Files (*.*)|*.*",
				InitialDirectory = paramFilePath
			};
			if (fileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return Result.Cancelled;
			}
			string sharedParamFilePath = fileDialog.FileName;

			if (!File.Exists(sharedParamFilePath))
			{
				TaskDialog.Show("Error", "Shared parameters file not found!");
				return Result.Failed;
			}

			// Assign shared parameters file to Revit
			app.SharedParametersFilename = sharedParamFilePath;
			DefinitionFile defFile = app.OpenSharedParameterFile();

			if (defFile == null)
			{
				TaskDialog.Show("Error", "Failed to open shared parameters file!");
				return Result.Failed;
			}

			if (TaskDialogResult.Yes == TaskDialog.Show("Shared Parameters", "Do you want to clear the existing shared parameters from the target Categories?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No, TaskDialogResult.Yes))
			{
				using (Transaction trans = new Transaction(doc, "parameter clear"))
				{
					trans.Start();

					BindingMap clearMap = doc.ParameterBindings;
					DefinitionBindingMapIterator iter = clearMap.ForwardIterator();

					//iter.Reset();
					List<Definition> definitionsToRemove = new List<Definition>();
					while (iter.MoveNext())
					{
						Definition def = iter.Key;
						bool isSharedParameter = false;
						foreach (DefinitionGroup group in defFile.Groups)
						{
							foreach (ExternalDefinition extDef in group.Definitions)
							{
								if (def.Name == extDef.Name)
								{
									isSharedParameter = true;
									break;
								}
							}
							if (isSharedParameter) break;
						}

						if (isSharedParameter)
						{
							definitionsToRemove.Add(def);
						}
					}

					string removed = "";
					foreach (Definition def in definitionsToRemove)
					{
						clearMap.Remove(def);
						removed += (def.Name + "\n");
					}
					TaskDialog.Show("Removed", $"Parameters:\n {removed}");
					trans.Commit();
				}
			}

			using (Transaction trans = new Transaction(doc, "parameter assignment"))
			{
				trans.Start();

				// Define the categories where the parameters will be added
				CategorySet categories = app.Create.NewCategorySet();
				categories.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_LightingFixtures));

				// Get the Parameter Binding Map
				BindingMap bindingMap = doc.ParameterBindings;

				CategorySetIterator setIterator = categories.ForwardIterator();
				while (setIterator.MoveNext())
				{
					Category cat = setIterator.Current as Category;
					if (null == cat)
						continue;
					string targetGroup = "";
#if REVIT2024 || REVIT2025
					switch (cat.Id.Value)
#else
					switch (cat.Id.IntegerValue)
#endif
					{
						case (int)BuiltInCategory.OST_LightingFixtures:
							targetGroup = "Lighting Fixture";
							break;
						case (int)BuiltInCategory.OST_DataDevices:
							targetGroup = "Data Device";
							break;
						default:
							break;
					}

					string existingParams = "";
					// Iterate over shared parameter groups
					foreach (DefinitionGroup group in defFile.Groups)
					{
						if (group.Name != targetGroup)
						{
							continue;
						}
						foreach (ExternalDefinition extDef in group.Definitions)
						{
							// checkif parameter already exists
							if (bindingMap.Contains(extDef))
							{
								existingParams += (extDef.Name + "\n");
								continue;
							}

							// Create an Instance Binding
							InstanceBinding instanceBinding = app.Create.NewInstanceBinding(categories);

							// Bind the parameter
#if REVIT2024 || REVIT2025							
							bool bindingSuccess = bindingMap.Insert(extDef, instanceBinding, GroupTypeId.General);
#else
							bool bindingSuccess = bindingMap.Insert(extDef, instanceBinding, BuiltInParameterGroup.INVALID);
#endif

							if (!bindingSuccess)
							{
								TaskDialog.Show("Error", $"Failed to bind parameter {extDef.Name}");
							}
						}
					}
					if (existingParams.Length > 0)
					{
						TaskDialog.Show("Info", $"Parameters already exist\n {existingParams}");
					}
				}

				trans.Commit();
			}

			TaskDialog.Show("Success", "Shared parameters added successfully.");

			try
			{
				RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
				RegistryKey subkey = key.OpenSubKey("Architexor", true);
				if (null == subkey)
					subkey = key.CreateSubKey("Architexor", true);
				key.Close();
				key = subkey.OpenSubKey("GeoJSON", true);
				if (null == key)
					key = subkey.CreateSubKey("GeoJSON", true);
				subkey.Close();
				subkey = key.OpenSubKey("SharedParams", true);
				if (null == subkey)
					subkey = key.CreateSubKey("SharedParams", true);
				key.Close();
				subkey.SetValue("FilePath", sharedParamFilePath);
				subkey.Close();
			}
			catch (Exception) { }

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class Devices : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			Document doc = commandData.Application.ActiveUIDocument.Document;
			GeoBase geoBase = Architexor.Utils.Util.GetGeoBase(doc);

			TaskDialogResult ret = TaskDialog.Show("Updating Device Parameters", "Do you want to see the GeoJSON features of the updated devices", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No, TaskDialogResult.Yes);
			bool showFeatures = ret == TaskDialogResult.Yes;

			DevicePropertyManager deviceMgr = new DevicePropertyManager(
				doc,
				geoBase,
				showFeatures
				);
			deviceMgr.UpdateDeviceProperties();

			TaskDialog.Show("Success", "Updated Parameters successfully");

			if (showFeatures)
			{
				Util.OpenNewPointsInHtml(deviceMgr.mFeatures);
			}

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class Export : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			Document doc = commandData.Application.ActiveUIDocument.Document;

			#region reserved
			//			XYZ internalOrigin = XYZ.Zero; // Internal Origin is always (0,0,0) in Revit
			//			XYZ surveyPoint = null;

			//			ProjectLocation loc = doc.ActiveProjectLocation;loc.GetSiteLocation();
			//			ProjectPosition position = loc.GetProjectPosition(XYZ.Zero);
			//			SiteLocation siteLoc = loc.GetSiteLocation();

			//			double longitude = loc.GetSiteLocation().Longitude * (180 / Math.PI);
			//			double latitude = loc.GetSiteLocation().Latitude * (180 / Math.PI);
			//			double angleInDegrees = position.Angle * (180 / Math.PI);


			//			FilteredElementCollector locations = new FilteredElementCollector(doc).OfClass(typeof(BasePoint));
			//			/*double basePointLongitude = 0, basePointLatitude = 0, surveyPointLongitude = 0, surveyPointLatitude = 0;
			//			foreach (var locationPoint in locations)
			//			{
			//				BasePoint basePoint = locationPoint as BasePoint;
			//				if (basePoint.IsShared == true)
			//				{
			//					//	this is the survey point
			//					surveyPointLongitude = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble();
			//					surveyPointLatitude = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble();
			//				}
			//				else
			//				{
			//					basePointLongitude = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble();
			//					basePointLatitude = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble();
			//				}
			//			}

			//#if (REVIT2021 || REVIT2022 || REVIT2023 || REVIT2024 || REVIT2025)
			//			surveyPointLongitude = UnitUtils.ConvertFromInternalUnits(surveyPointLongitude, UnitTypeId.Meters);
			//			surveyPointLatitude = UnitUtils.ConvertFromInternalUnits(surveyPointLatitude, UnitTypeId.Meters);
			//			basePointLongitude = UnitUtils.ConvertFromInternalUnits(basePointLongitude, UnitTypeId.Meters);
			//			basePointLatitude = UnitUtils.ConvertFromInternalUnits(basePointLatitude, UnitTypeId.Meters);
			//#else
			//			surveyPointLongitude = UnitUtils.ConvertFromInternalUnits(surveyPointLongitude, DisplayUnitType.DUT_METERS);
			//			surveyPointLatitude = UnitUtils.ConvertFromInternalUnits(surveyPointLatitude, DisplayUnitType.DUT_METERS);
			//			basePointLongitude = UnitUtils.ConvertFromInternalUnits(basePointLongitude, DisplayUnitType.DUT_METERS);
			//			basePointLatitude = UnitUtils.ConvertFromInternalUnits(basePointLatitude, DisplayUnitType.DUT_METERS);
			//#endif
			//			surveyPointLongitude /= 111111;
			//			surveyPointLatitude /= 111111;
			//			basePointLongitude /= 111111;
			//			basePointLatitude /= 111111;

			//			surveyPointLongitude = 0;
			//			surveyPointLatitude = 0;
			//			//basePointLongitude = 0;
			//			//basePointLatitude = 0;*/
			//			double basePointX = 0, basePointY = 0;
			//			foreach (var locationPoint in locations)
			//			{
			//				BasePoint basePoint = locationPoint as BasePoint;
			//				//if (basePoint.IsShared != true)
			//				if(basePoint.Category.Name == "Survey Point")
			//				{
			//					// survey point
			//					surveyPoint = basePoint.Position;
			//					basePointX = basePoint.Position.X;
			//					basePointY = basePoint.Position.Y;
			//					//TaskDialog.Show("Survey Point", $"X: {basePointX} ft\nY: {basePointY} ft");
			//				} else
			//				{
			//					//basePointX = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble();
			//					//basePointY = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble();
			//					basePointX = basePoint.Position.X;
			//					basePointY = basePoint.Position.Y;
			//					//TaskDialog.Show("Project Base Point", $"X: {basePointX} ft\nY: {basePointY} ft");
			//				}
			//			}

			//			basePointX = surveyPoint.X;
			//			basePointY = surveyPoint.Y;
			#endregion

			GeoBase geoBase = Architexor.Utils.Util.GetGeoBase(doc);
			//	Get All Levels
			List<Level> levels = new FilteredElementCollector(doc)
															.OfClass(typeof(Level))
															.Cast<Level>()
															.ToList();

			FrmExport frm = new FrmExport();
			frm.Longitude = geoBase.Longitude;// - basePointLongitude + surveyPointLongitude;
			frm.Latitude = geoBase.Latitude;// - basePointLatitude + surveyPointLatitude;
			frm.Angle = geoBase.AngleToNorth;
			//frm.Levels = levels;

			foreach (Category category in doc.Settings.Categories)
			{
				if (category.CategoryType != CategoryType.Model)
					continue;
				
				ACategory cat = new ACategory()
				{
					Category = category,
					SubCategories = new List<ASubCategory>(),
					Checked = false
				};
				
				foreach(Category subCategory in category.SubCategories)
				{
					if (subCategory.CategoryType != CategoryType.Model)
						continue;

					cat.SubCategories.Add(new ASubCategory()
					{
						Category = subCategory,
						Checked = false
					});
				}
				frm.Categories.Add(cat);
			}

			if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return Result.Cancelled;
			}
			
			geoBase.Latitude = frm.Latitude;
			geoBase.Longitude = frm.Longitude;
			geoBase.AngleToNorth = frm.Angle;
			bool curView = frm.CurView;
			bool isValidView = false;
			if (curView) {
				switch (doc.ActiveView.ViewType)
				{
					case ViewType.Undefined:
						break;
					case ViewType.FloorPlan:
						isValidView = true;
						break;
					case ViewType.EngineeringPlan:
						isValidView = true;
						break;
					case ViewType.AreaPlan:
						isValidView = true;
						break;
					case ViewType.CeilingPlan:
						isValidView = true;
						break;
					case ViewType.Elevation:
						break;
					case ViewType.Section:
						break;
					case ViewType.Detail:
						break;
					case ViewType.ThreeD:
						break;
					case ViewType.Schedule:
						break;
					case ViewType.DraftingView:
						break;
					case ViewType.DrawingSheet:
						break;
					case ViewType.Legend:
						break;
					case ViewType.Report:
						break;
					case ViewType.ProjectBrowser:
						break;
					case ViewType.SystemBrowser:
						break;
					case ViewType.CostReport:
						break;
					case ViewType.LoadsReport:
						break;
					case ViewType.PresureLossReport:
						break;
					case ViewType.PanelSchedule:
						break;
					case ViewType.ColumnSchedule:
						break;
					case ViewType.Walkthrough:
						break;
					case ViewType.Rendering:
						break;
					case ViewType.SystemsAnalysisReport:
						break;
					case ViewType.Internal:
						break;
					default:
						break;
				}
				if (isValidView)
				{
					GeoJSONExporter exporter = new GeoJSONExporter(doc
							, frm.GetSelectedCategories()
							, geoBase
							, frm.ExportType);

					string sPath = frm.Path;
					exporter.Export(sPath + "\\geojson_" + doc.Title + "_" + doc.ActiveView.Name + "_" + doc.ActiveView.ViewType + ".json");
				}
				else
				{
					return Result.Failed;
				}
			}
			else
			{
				foreach (Level level in levels)
				{
					GeoJSONExporter exporter = new GeoJSONExporter(doc
						, level
						, frm.GetSelectedCategories()
						, geoBase
						, frm.ExportType);
					string sPath = frm.Path;
					exporter.Export(sPath + "\\geojson_" + doc.Title + "_" + level.Id + "_" + level.Name + ".json");
				}
			}
			return Result.Succeeded;
		}
	}
}