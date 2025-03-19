using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Architexor.Forms.GeoJSON;
using System;
using System.Linq;
using System.Collections.Generic;
using Architexor.GeoJSON.Base;
using Architexor.Controllers.GeoJSON;

namespace Architexor.Commands.GeoJSON
{
	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class Export : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			Document doc = commandData.Application.ActiveUIDocument.Document;


			XYZ internalOrigin = XYZ.Zero; // Internal Origin is always (0,0,0) in Revit
			XYZ surveyPoint = null;

			ProjectLocation loc = doc.ActiveProjectLocation;loc.GetSiteLocation();
			ProjectPosition position = loc.GetProjectPosition(XYZ.Zero);
			SiteLocation siteLoc = loc.GetSiteLocation();

			double longitude = loc.GetSiteLocation().Longitude * (180 / Math.PI);
			double latitude = loc.GetSiteLocation().Latitude * (180 / Math.PI);
			double angleInDegrees = position.Angle * (180 / Math.PI);


			FilteredElementCollector locations = new FilteredElementCollector(doc).OfClass(typeof(BasePoint));
			/*double basePointLongitude = 0, basePointLatitude = 0, surveyPointLongitude = 0, surveyPointLatitude = 0;
			foreach (var locationPoint in locations)
			{
				BasePoint basePoint = locationPoint as BasePoint;
				if (basePoint.IsShared == true)
				{
					//	this is the survey point
					surveyPointLongitude = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble();
					surveyPointLatitude = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble();
				}
				else
				{
					basePointLongitude = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble();
					basePointLatitude = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble();
				}
			}

#if (REVIT2021 || REVIT2022 || REVIT2023 || REVIT2024 || REVIT2025)
			surveyPointLongitude = UnitUtils.ConvertFromInternalUnits(surveyPointLongitude, UnitTypeId.Meters);
			surveyPointLatitude = UnitUtils.ConvertFromInternalUnits(surveyPointLatitude, UnitTypeId.Meters);
			basePointLongitude = UnitUtils.ConvertFromInternalUnits(basePointLongitude, UnitTypeId.Meters);
			basePointLatitude = UnitUtils.ConvertFromInternalUnits(basePointLatitude, UnitTypeId.Meters);
#else
			surveyPointLongitude = UnitUtils.ConvertFromInternalUnits(surveyPointLongitude, DisplayUnitType.DUT_METERS);
			surveyPointLatitude = UnitUtils.ConvertFromInternalUnits(surveyPointLatitude, DisplayUnitType.DUT_METERS);
			basePointLongitude = UnitUtils.ConvertFromInternalUnits(basePointLongitude, DisplayUnitType.DUT_METERS);
			basePointLatitude = UnitUtils.ConvertFromInternalUnits(basePointLatitude, DisplayUnitType.DUT_METERS);
#endif
			surveyPointLongitude /= 111111;
			surveyPointLatitude /= 111111;
			basePointLongitude /= 111111;
			basePointLatitude /= 111111;

			surveyPointLongitude = 0;
			surveyPointLatitude = 0;
			//basePointLongitude = 0;
			//basePointLatitude = 0;*/
			double basePointX = 0, basePointY = 0;
			foreach (var locationPoint in locations)
			{
				BasePoint basePoint = locationPoint as BasePoint;
				//if (basePoint.IsShared != true)
				if(basePoint.Category.Name == "Survey Point")
				{
					// survey point
					surveyPoint = basePoint.Position;
					basePointX = basePoint.Position.X;
					basePointY = basePoint.Position.Y;
					//TaskDialog.Show("Survey Point", $"X: {basePointX} ft\nY: {basePointY} ft");
				} else
				{
					//basePointX = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble();
					//basePointY = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble();
					basePointX = basePoint.Position.X;
					basePointY = basePoint.Position.Y;
					//TaskDialog.Show("Project Base Point", $"X: {basePointX} ft\nY: {basePointY} ft");
				}
			}

			basePointX = surveyPoint.X;
			basePointY = surveyPoint.Y;

			//	Get All Levels
			List<Level> levels = new FilteredElementCollector(doc)
															.OfClass(typeof(Level))
															.Cast<Level>()
															.ToList();

			FrmExport frm = new FrmExport();
			frm.Longitude = longitude;// - basePointLongitude + surveyPointLongitude;
			frm.Latitude = latitude;// - basePointLatitude + surveyPointLatitude;
			frm.Angle = angleInDegrees;
			frm.Levels = levels;

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
			
			latitude = frm.Latitude;
			longitude = frm.Longitude;
			angleInDegrees = frm.Angle;
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
							, longitude
							, latitude
							, basePointX
							, basePointY
							, angleInDegrees * Math.PI / 180);

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
						, longitude
						, latitude
						, basePointX
						, basePointY
						, angleInDegrees * Math.PI / 180);
					string sPath = frm.Path;
					exporter.Export(sPath + "\\geojson_" + doc.Title + "_" + level.Id + "_" + level.Name + ".json");
				}
			}
			return Result.Succeeded;
		}
	}
}