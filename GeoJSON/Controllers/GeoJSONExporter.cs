using Architexor.Forms.GeoJSON;
using Architexor.GeoJSON.Base;
using Architexor.Utils;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using View = Autodesk.Revit.DB.View;
using GeoPoint = Architexor.GeoJSON.Base.Point;

namespace Architexor.Controllers.GeoJSON
{
	public class GeoJSONExporter
	{
		private Document mDocument;
		private Level mLevel;
		private double mLongitude;
		private double mLatitude;
		private double mAngle;
		private List<string> mCategories = null;
		private double mLongitudeOffset;
		private double mLatitudeOffset;
		private bool mCurViewOnly = false;
		private ExportType mType = ExportType.All;

		public GeoJSONExporter(Document document, Level level, List<string> categories, GeoBase geoBase, ExportType type)
		{
			mDocument = document;
			mLevel = level;
			mCategories = categories;
			mLongitude = geoBase.Longitude;
			mLatitude = geoBase.Latitude;
			mAngle = geoBase.Angle;
			mLongitudeOffset = geoBase.LongitudeOffset;
			mLatitudeOffset = geoBase.LatitudeOffset;
			mCurViewOnly = false;
			mType = type;
		}

		public GeoJSONExporter(Document document, List<string> categories, GeoBase geoBase, ExportType type)
		{
			mDocument = document;
			mLevel = document.ActiveView.GenLevel;
			mCategories = categories;
			mLongitude = geoBase.Longitude;
			mLatitude = geoBase.Latitude;
			mAngle = geoBase.Angle;
			mLongitudeOffset = geoBase.LongitudeOffset;
			mLatitudeOffset = geoBase.LatitudeOffset;
			mCurViewOnly = true;
			mType = type;
		}

		#region Standard elements
		private List<Element> GetElementsByCategory(int category, bool currentView)
		{
			if (currentView)
			{
				View curView = mDocument.ActiveView; // Get the active view
				if (!(curView is ViewPlan curPlan))
					return null;

				// Collect all elements visible in the current view
				List<Element> elements = new FilteredElementCollector(mDocument, curView.Id)
					.WhereElementIsNotElementType()
					.Where(e => (e.Category != null)
#if REVIT2024 || REVIT2025
				&& (category == e.Category.Id.Value)
#else
					&& (category == e.Category.Id.IntegerValue)
#endif
					).ToList();

				return elements;
			}
			else
			{
				//	Collect elements in the specified level
				List<Element> elements = new FilteredElementCollector(mDocument)
																.WherePasses(new ElementLevelFilter(mLevel.Id))
																.WhereElementIsNotElementType()
																.Where(e => (e.Category != null)
#if REVIT2024 || REVIT2025
				&& (category == e.Category.Id.Value)
#else
					&& (category == e.Category.Id.IntegerValue)
#endif
				).ToList();

				return elements;
			}
		}
		private List<Element> GetLightingFixturesByLevel()
		{
			//	Collect elements in the specified level
			List<Element> elements = new FilteredElementCollector(mDocument)
															.WherePasses(new ElementLevelFilter(mLevel.Id))
															.WhereElementIsNotElementType()
															.Where(e => (e.Category != null)
#if REVIT2024 || REVIT2025
				&& (e.Category.Id.Value == (int)BuiltInCategory.OST_LightingFixtures)
#else
				&& (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingFixtures)
#endif
			).ToList();

			return elements;
		}
		private List<Element> GetElementsInCurrentView()
		{
			View curView = mDocument.ActiveView; // Get the active view
			if (!(curView is ViewPlan curPlan))
				return null;

			// Collect all elements visible in the current view
			List<Element> elements = new FilteredElementCollector(mDocument, curView.Id)
				.WhereElementIsNotElementType()
#if REVIT2024 || REVIT2025
				.Where(e => (e.Category != null)
				&& (e.Category.Id.Value != (int)BuiltInCategory.OST_RoomSeparationLines)
				&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name)))
#else
				.Where(e => (e.Category != null)
				&& (e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_RoomSeparationLines)
				&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name)))
#endif
				.ToList();

			List<Level> levels = new FilteredElementCollector(mDocument)
															.OfClass(typeof(Level))
															.Cast<Level>()
															.ToList();

			levels.Sort((level1, level2) => level1.Elevation.CompareTo(level2.Elevation));
			int nIndex = levels.FindIndex(x => x.Id == mLevel.Id);
			double elevation = mLevel.Elevation;

			List<Element> etc;
			if (nIndex == 0) //	First Level
			{
				etc = new FilteredElementCollector(mDocument)
					.OfClass(typeof(FamilyInstance))
					.WhereElementIsNotElementType()
#if REVIT2024 || REVIT2025
					.Where(e => e.Category.Id.Value != (int)BuiltInCategory.OST_RoomSeparationLines
						&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name))
						&& e.LevelId.Value == -1)
#else
					.Where(e => e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_RoomSeparationLines
						&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name))
						&& e.LevelId.IntegerValue == -1)
#endif
					.Where(e => (e.get_BoundingBox(null)?.Max.Z + e.get_BoundingBox(null)?.Min.Z) / 2 < elevation)
					.ToList();
			}
			else if (nIndex == levels.Count - 1)  //	Last Level
			{
				etc = new FilteredElementCollector(mDocument)
					.OfClass(typeof(FamilyInstance))
					.WhereElementIsNotElementType()
#if REVIT2024 || REVIT2025
					.Where(e => e.Category.Id.Value != (int)BuiltInCategory.OST_RoomSeparationLines
						&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name))
						&& e.LevelId.Value == -1)
#else
					.Where(e => e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_RoomSeparationLines
						&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name))
						&& e.LevelId.IntegerValue == -1)
#endif
					.Where(e => (e.get_BoundingBox(null)?.Max.Z + e.get_BoundingBox(null)?.Min.Z) / 2 > elevation)
					.ToList();
			}
			else
			{
				etc = new FilteredElementCollector(mDocument)
					.OfClass(typeof(FamilyInstance))
					.WhereElementIsNotElementType()
#if REVIT2024 || REVIT2025
					.Where(e => e.Category.Id.Value != (int)BuiltInCategory.OST_RoomSeparationLines
						&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name))
						&& e.LevelId.Value == -1)
#else
					.Where(e => e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_RoomSeparationLines
						&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name))
						&& e.LevelId.IntegerValue == -1)
#endif
					.Where(e => (e.get_BoundingBox(null)?.Max.Z + e.get_BoundingBox(null)?.Min.Z) / 2 > elevation
												&& (e.get_BoundingBox(null)?.Max.Z + e.get_BoundingBox(null)?.Min.Z) / 2 < levels[nIndex + 1].Elevation)
					.ToList();
			}

			elements.AddRange(etc);

			return elements;
		}
		private List<Element> GetElementsByLevel()
		{
			//	Collect elements in the specified level
			List<Element> elements = new FilteredElementCollector(mDocument)
															.WherePasses(new ElementLevelFilter(mLevel.Id))
															.WhereElementIsNotElementType()
#if REVIT2024 || REVIT2025
															.Where(e => (e.Category != null)
				&& (e.Category.Id.Value != (int)BuiltInCategory.OST_RoomSeparationLines)
				&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name)))
#else
															.Where(e => (e.Category != null)
				&& (e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_RoomSeparationLines)
				&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name)))
#endif
															.ToList();

			List<Level> levels = new FilteredElementCollector(mDocument)
															.OfClass(typeof(Level))
															.Cast<Level>()
															.ToList();

			levels.Sort((level1, level2) => level1.Elevation.CompareTo(level2.Elevation));
			int nIndex = levels.FindIndex(x => x.Id == mLevel.Id);

			double elevation = mLevel.Elevation;

			List<Element> etc;
			if (nIndex == 0) //	First Level
			{
				etc = new FilteredElementCollector(mDocument)
					.OfClass(typeof(FamilyInstance))
					.WhereElementIsNotElementType()
#if REVIT2024 || REVIT2025
					.Where(e => e.Category.Id.Value != (int)BuiltInCategory.OST_RoomSeparationLines
						&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name))
						&& e.LevelId.Value == -1)
#else
					.Where(e => e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_RoomSeparationLines
						&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name))
						&& e.LevelId.IntegerValue == -1)
#endif
					.Where(e => (e.get_BoundingBox(null)?.Max.Z + e.get_BoundingBox(null)?.Min.Z) / 2 < elevation)
					.ToList();
			}
			else if (nIndex == levels.Count - 1)  //	Last Level
			{
				etc = new FilteredElementCollector(mDocument)
					.OfClass(typeof(FamilyInstance))
					.WhereElementIsNotElementType()
#if REVIT2024 || REVIT2025
					.Where(e => e.Category.Id.Value != (int)BuiltInCategory.OST_RoomSeparationLines
						&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name))
						&& e.LevelId.Value == -1)
#else
					.Where(e => e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_RoomSeparationLines
						&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name))
						&& e.LevelId.IntegerValue == -1)
#endif
					.Where(e => (e.get_BoundingBox(null)?.Max.Z + e.get_BoundingBox(null)?.Min.Z) / 2 > elevation)
					.ToList();
			}
			else
			{
				etc = new FilteredElementCollector(mDocument)
					.OfClass(typeof(FamilyInstance))
					.WhereElementIsNotElementType()
#if REVIT2024 || REVIT2025
					.Where(e => e.Category.Id.Value != (int)BuiltInCategory.OST_RoomSeparationLines
						&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name))
						&& e.LevelId.Value == -1)
#else
					.Where(e => e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_RoomSeparationLines
						&& (mCategories.Count == 0 || mCategories.Contains(e.Category.Name))
						&& e.LevelId.IntegerValue == -1)
#endif
					.Where(e => (e.get_BoundingBox(null)?.Max.Z + e.get_BoundingBox(null)?.Min.Z) / 2 > elevation
												&& (e.get_BoundingBox(null)?.Max.Z + e.get_BoundingBox(null)?.Min.Z) / 2 < levels[nIndex + 1].Elevation)
					.ToList();
			}

			elements.AddRange(etc); 

			return elements;
		}

		private List<List<XYZ>> GetElementPoints(Element element)
		{
			List<List<XYZ>> lines = new List<List<XYZ>>();

			//	Get the floor plan view for the specified level
			View floorPlanView = GetFloorPlanView();
			if (floorPlanView == null) return lines;

			Options geomOptions = new Options
			{
				ComputeReferences = true,
				IncludeNonVisibleObjects = false,
				View = floorPlanView, //	Use the target floor plan view
			};

			GeometryElement geomElement = element.get_Geometry(geomOptions);
			if (geomElement == null) return lines;

			//	Traverse the geomtry and collect only floor-plan points
			foreach (GeometryObject geomObj in geomElement)
			{
				if (geomObj is GeometryInstance geomInstance)
				{
					foreach (GeometryObject instObj in geomInstance.GetInstanceGeometry())
					{
						ProcessGeometryObject(instObj, lines);
					}
				}
				else
				{
					ProcessGeometryObject(geomObj, lines);
				}
			}
			return lines;
		}
		#endregion

		#region Stairs functions
		private List<Element> GetStairs()
		{
			List<Level> levels = new FilteredElementCollector(mDocument)
															.OfClass(typeof(Level))
															.Cast<Level>()
															.ToList();

			levels.Sort((level1, level2) => level1.Elevation.CompareTo(level2.Elevation));
			int nIndex = levels.FindIndex(x => x.Id == mLevel.Id);

			double elevation = mLevel.Elevation;

			List<Element> stairs;
			if(nIndex == 0) //	First Level
			{
				stairs = new FilteredElementCollector(mDocument)
															.OfCategory(BuiltInCategory.OST_Stairs)
															.WhereElementIsNotElementType()
															.Where(stair => (stair.get_BoundingBox(null).Max.Z + stair.get_BoundingBox(null).Min.Z) / 2 < elevation)
															.ToList();
			}
			else if(nIndex == levels.Count - 1)	//	Last Level
			{
				stairs = new FilteredElementCollector(mDocument)
															.OfCategory(BuiltInCategory.OST_Stairs)
															.WhereElementIsNotElementType()
															.Where(stair => (stair.get_BoundingBox(null).Max.Z + stair.get_BoundingBox(null).Min.Z) / 2 > elevation)
															.ToList();
			} else
			{
				stairs = new FilteredElementCollector(mDocument)
															.OfCategory(BuiltInCategory.OST_Stairs)
															.WhereElementIsNotElementType()
															.Where(stair => (stair.get_BoundingBox(null).Max.Z + stair.get_BoundingBox(null).Min.Z) / 2 < elevation
																						&& (stair.get_BoundingBox(null).Max.Z + stair.get_BoundingBox(null).Min.Z) / 2 > levels[nIndex - 1].Elevation)
															.ToList();
			}

			return stairs;
		}
		#endregion

		#region Common functions
		private View GetFloorPlanView()
		{
			if (mCurViewOnly)
			{
				return mDocument.ActiveView;
			}

			//	Filter views to get floor plan views only
			FilteredElementCollector viewCollector = new FilteredElementCollector(mDocument)
				.OfClass(typeof(ViewPlan))
				.WhereElementIsNotElementType();

			foreach (ViewPlan view in viewCollector)
			{
				//	Ensure it's a floor plan view and matches the target level
				if (view.ViewType == ViewType.FloorPlan && view.GenLevel?.Id == mLevel.Id)
				{
					return view;
				}
			}

			return null;
		}

		private List<XYZ> SampleArcPoints(Arc arc, int segmentCount = 10)
		{
			List<XYZ> arcPoints = new List<XYZ>();

			//	Calculate points along the arc
			for (int i = 0; i <= segmentCount; i++)
			{
				if (arc.IsBound)
				{
					double parameter = arc.GetEndParameter(0) + (arc.GetEndParameter(1) - arc.GetEndParameter(0)) * i / segmentCount;
					XYZ point = arc.Evaluate(parameter, false);
					arcPoints.Add(point);
				}
			}

			return arcPoints;
		}

		private bool IsPlanar(Curve curve)
		{
			//	Check if a curve is planar by verifying Z-coordinates (floor plan is assumed horizontal)
			if (null != curve && curve.IsBound)
			{
				return Math.Abs(curve.GetEndPoint(0).Z - curve.GetEndPoint(1).Z) < 0.001;
			}
			return false;
			//&& Math.Abs(curve.GetEndPoint(0).Z - level.Elevation) < 0.001;
			//return true;
		}

		private bool IsPlanarFaceOnLevel(PlanarFace face)
		{
			return Math.Abs(face.Origin.Z - mLevel.Elevation) < 0.001;
		}

		private void ProcessGeometryObject(GeometryObject geomObj, List<List<XYZ>> lines)
		{
			//	Check for planar edges or curves that represent 2D floor-plan lines
			if (geomObj is Arc arc && IsPlanar(arc))
			{
				//	Sample points along the arc
				List<XYZ> points = new List<XYZ>();
				points.AddRange(SampleArcPoints(arc));
				lines.Add(points);
			}
			else if (geomObj is Curve curve && IsPlanar(curve))
			{
				//	Add curve endpoints for floor-plan representation
				List<XYZ> points = new List<XYZ>();
				points.Add(curve.GetEndPoint(0));
				points.Add(curve.GetEndPoint(1));
				lines.Add(points);
			}
			else if (geomObj is Solid solid)
			{
				//	Process faces and edges of solids to get only the 2D outline points
				foreach (Face face in solid.Faces)
				{
					if (face is PlanarFace)  //	Only process planar faces
					{
						List<XYZ> points = new List<XYZ>();
						EdgeArray edgeArray = face.EdgeLoops.get_Item(0);
						foreach (Edge edge in edgeArray)
						{
							Curve edgeCurve = edge.AsCurve();
							if (edgeCurve != null && IsPlanar(edgeCurve))
							{
								List<XYZ> localPoints = new List<XYZ>();
								if (edgeCurve is Arc edgeArc)
								{
									localPoints = SampleArcPoints(edgeArc);
								}
								else
								{
									localPoints.Add(edge.AsCurve().GetEndPoint(0));
									localPoints.Add(edge.AsCurve().GetEndPoint(1));
								}

								if (points.Count > 0 && Util.IsEqual(points[points.Count - 1], localPoints[0]))
								{
									for (int i = 1; i < localPoints.Count; i++)
										points.Add(localPoints[i]);
								}
								else
								{
									if (points.Count > 0)
									{
										lines.Add(points);
										points = new List<XYZ>();
									}
									for (int i = 0; i < localPoints.Count; i++)
										points.Add(localPoints[i]);
								}
							}
						}
						if (points.Count > 0)
						{
							lines.Add(points);
						}
					}
				}
			}
		}

		public void Export(string filePath)
		{
			List<GeoJsonFeature> features = CreateGeoJsonFeatures();

			var featureCollection = new
			{
				type = "FeatureCollection",
				features = features
			};

			string json = JsonConvert.SerializeObject(featureCollection, Formatting.Indented);
			File.WriteAllText(filePath, json);
		}

		//	Convert XYZ point to [longitude, latitude, elevation]
		private double[] ConvertToGeoJsonCoordinates(XYZ point)
		{
			double relativeX = point.X - mLongitudeOffset;
			double relativeY = point.Y - mLatitudeOffset;

			//	Offset by model's location if needed, convert to latitude/longitude if necessary
#if (REVIT2021 || REVIT2022 || REVIT2023 || REVIT2024 || REVIT2025)
			double x = UnitUtils.ConvertFromInternalUnits(point.X, UnitTypeId.Meters);
			double y = UnitUtils.ConvertFromInternalUnits(point.Y, UnitTypeId.Meters);
			double z = UnitUtils.ConvertFromInternalUnits(point.Z, UnitTypeId.Meters);
			double xMeters = UnitUtils.ConvertFromInternalUnits(relativeX, UnitTypeId.Meters);
			double yMeters = UnitUtils.ConvertFromInternalUnits(relativeY, UnitTypeId.Meters);
#else
			double x = UnitUtils.ConvertFromInternalUnits(point.X, DisplayUnitType.DUT_METERS);
			double y = UnitUtils.ConvertFromInternalUnits(point.Y, DisplayUnitType.DUT_METERS);
			double z = UnitUtils.ConvertFromInternalUnits(point.Z, DisplayUnitType.DUT_METERS);
			double xMeters = UnitUtils.ConvertFromInternalUnits(relativeX, DisplayUnitType.DUT_METERS);
			double yMeters = UnitUtils.ConvertFromInternalUnits(relativeY, DisplayUnitType.DUT_METERS);
#endif

			//	Apply rotatio for True North alignment
			double adjustedX = xMeters * System.Math.Cos(mAngle) - yMeters * System.Math.Sin(mAngle);
			double adjustedY = xMeters * System.Math.Sin(mAngle) + yMeters * System.Math.Cos(mAngle);

			//	Calculate latitude and longitude offset
			double longitudeOffset = adjustedX / (111111 * Math.Cos(mLatitude * Math.PI / 180));
			double latitudeOffset = adjustedY / 111111;

			//	Longitude and Latitude could be adjusted based on model's base point if required
			return new double[] { mLongitude + longitudeOffset, mLatitude + latitudeOffset };//, z };
		}

		//	Create GeoJSON feature from Revit element points
		private GeoJsonFeature CreateGeoJsonFeature(Element element, List<List<XYZ>> lines)
		{
			//	Convert XYZ points to GeoJSON 2D/3D coordinates
			List<List<double[]>> geoJsonCoordinates = new List<List<double[]>>();
			foreach (var line in lines)
			{
				List<double[]> coordinates = new List<double[]>();
				foreach (var point in line)
				{
					coordinates.Add(ConvertToGeoJsonCoordinates(point));
				}
				if (coordinates.Count > 0)
					geoJsonCoordinates.Add(coordinates);
			}
			//	Add the first point
			//geoJsonCoordinates.Add(ConvertToGeoJsonCoordinates(points[0], longitude, latitude, rotationAngle));

			//	Define geometry type (Polygon here for example)
			MultiLineString geometry = new MultiLineString
			{
				coordinates = geoJsonCoordinates//new List<List<double[]>> { geoJsonCoordinates }
			};

			//	Define properties (name, category, etc.)
			Dictionary<string, object> properties = new Dictionary<string, object>
			{
#if REVIT2024 || REVIT2025
				{ "id", element.Id.Value },
#else
				{ "id", element.Id.IntegerValue },
#endif
				{ "name", element.Name },
				{ "category", element.Category?.Name }
			};

			//	Create and return the feature
			return new GeoJsonFeature
			{
				geometry = geometry,
				properties = properties
			};
		}

		private GeoJsonFeature CreateGeoJsonPoint(Element element)
		{
			var loc = (((FamilyInstance)element).Location as LocationPoint)?.Point;
			double[] coordinate = ConvertToGeoJsonCoordinates(loc);
			GeoPoint geometry = new GeoPoint { coordinates = coordinate };

			Dictionary<string, object> properties = new Dictionary<string, object>
			{
#if REVIT2024 || REVIT2025
				{ "id", element.Id.Value },
#else
				{ "id", element.Id.IntegerValue },
#endif
				{ "name", element.Name },
				{ "category", element.Category?.Name }
			};

			//	Create and return the feature
			return new GeoJsonFeature
			{
				geometry = geometry,
				properties = properties
			};
		}

		public List<GeoJsonFeature> CreateGeoJsonFeatures()
		{
			List<GeoJsonFeature> features = new List<GeoJsonFeature>();

			if (mCurViewOnly)
			{
				List<Element> elements = GetElementsInCurrentView();

				foreach (var element in elements)
				{
					List<List<XYZ>> lines = GetElementPoints(element);
					if (lines.Count > 0)
					{
						GeoJsonFeature feature = CreateGeoJsonFeature(element, lines);
						features.Add(feature);
					}
				}
			}
			else
			{
				List<Element> elements = GetElementsByLevel();

				foreach (var element in elements)
				{
					List<List<XYZ>> lines = GetElementPoints(element);
					if (lines.Count > 0)
					{
						GeoJsonFeature feature = CreateGeoJsonFeature(element, lines);
						features.Add(feature);
					}
				}
			}

			List<Element> devices = new List<Element>();
			if (mType == ExportType.LightingFixturePoints || mType == ExportType.All)
			{
				// Lighting Fixtures
				List<Element> elems = GetElementsByCategory((int)BuiltInCategory.OST_LightingFixtures, mCurViewOnly);
				if (null != elems && elems.Count > 0)
				{
					devices.AddRange(elems);
				}
			}
			if (mType == ExportType.DataDevicePoints || mType == ExportType.All)
			{
				// Data Devices
				List<Element> elems = GetElementsByCategory((int)BuiltInCategory.OST_DataDevices, mCurViewOnly);
				if (null != elems && elems.Count > 0)
				{
					devices.AddRange(elems);
				}
			}
			foreach (var device in devices)
			{
				features.Add(CreateGeoJsonPoint(device));
			}

			// Stairs
			List<Element> stairsCollection = GetStairs();
			foreach (var stairs in stairsCollection)
			{
				//List<XYZ> points = GetBoundaryPoints(stairs, level);
				//GeoJsonFeature feature = CreateGeoJsonFeatureForStairs(doc, stairs as Stairs, level, longitude, latitude, rotationAngle);
				//GeoJsonFeature feature = CreateGeoJsonFeature(stairs, longitude, latitude, rotationAngle);
				//features.Add(feature);
				List<List<XYZ>> lines = GetElementPoints(stairs);
				if (lines.Count > 0)
				{
					GeoJsonFeature feature = CreateGeoJsonFeature(stairs, lines);
					features.Add(feature);
				}
			}

			// Rooms
			List<GeoJsonFeature> rooms = GetRoomPolygons();
			if (null != rooms)
			{
				features.AddRange(rooms);
			}

			return features;
		}
#endregion

		#region Room Related functions
		List<GeoJsonFeature> GetRoomPolygons()
		{
			List<GeoJsonFeature> roomPolygons = new();

			//	Set up boundary options to control the boundary retrieval
			SpatialElementBoundaryOptions boundaryOptions = new SpatialElementBoundaryOptions
			{
				SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish
			};

			//	Filter for rooms on the specified level
			List<Element> rooms = new FilteredElementCollector(mDocument)
				.OfCategory(BuiltInCategory.OST_Rooms)
				.WhereElementIsNotElementType()
				.Where(element => element.LevelId == mLevel.Id)
				.ToList();

			foreach(Room room in rooms)
			{
				//	Get the boundaries of the room
				IList<IList<BoundarySegment>> boundaryLoops = room.GetBoundarySegments(boundaryOptions);

				if(boundaryLoops.Count < 1)
				{
					return null;
				}
				else if(boundaryLoops.Count < 2)
				{

				}
				else
				{

				}
					foreach (IList<BoundarySegment> loop in boundaryLoops)
					{
						List<XYZ> polygon = new List<XYZ>();

						//	Extract points from each segment to form a polygon
						foreach (BoundarySegment segment in loop)
						{
						
							Curve curve = segment.GetCurve();

							//	Add start point of the curve to the polygon list
							polygon.Add(curve.GetEndPoint(0));

							//	Optionally handle curves (like arcs) for smoother polygons
							if (curve is Arc arc)
							{
								//	Divide the arc into points if needed to approximate curve as a polygon
								int divisions = 10; //	Higher for smoother arc approximation
								for (int i = 0; i <= divisions; i++)
								{
									double t = i / (double)divisions;
									XYZ pointOnArc = arc.Evaluate(t, true);
									polygon.Add(pointOnArc);
								}
							}
						}

						//	Close the polygon by adding the starting point at the end
						if (polygon.Count > 0 && !polygon.First().IsAlmostEqualTo(polygon.Last()))
						{
							polygon.Add(polygon.First());
						}

						List<double[]> geoJsonCoordinates = new List<double[]>();
						foreach (XYZ point in polygon)
						{
							geoJsonCoordinates.Add(ConvertToGeoJsonCoordinates(point));
						}
						//	Define geometry type (Polygon)
						Polygon geometry = new Polygon
						{
							coordinates = new List<List<double[]>> { geoJsonCoordinates }
						};
						//MultiLineString geometry = new MultiLineString
						//{
						//	coordinates = new List<List<double[]>> { geoJsonCoordinates }
						//};

						//	Define properties (name, category, etc.)
						Dictionary<string, object> properties = new Dictionary<string, object>
					{
#if REVIT2024 || REVIT2025
						{ "id", room.Id.Value },
#else
						{ "id", room.Id.IntegerValue },
#endif
						{ "name", room.Name },
						{ "category", room.Category?.Name }
					};

						//	Create and return the feature
						roomPolygons.Add(new GeoJsonFeature
						{
							geometry = geometry,
							properties = properties
						});
					}
			}

			return roomPolygons;
		}
		#endregion
	}
}
