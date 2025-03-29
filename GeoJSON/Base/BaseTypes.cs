using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Architexor.GeoJSON.Base
{
	public class GeoBase
	{
		/// <summary>
		/// Base point in Longitude - degree
		/// </summary>
		public double Longitude { get; set; }
		/// <summary>
		/// Base point in Latitude - degree
		/// </summary>
		public double Latitude { get; set; }
		/// <summary>
		/// Angle in Degrees
		/// </summary>
		public double AngleToNorth { get; set; }
		/// <summary>
		/// Angle to north in Radian
		/// </summary>
		public double Angle { get { return AngleToNorth * Math.PI / 180; } }
		/// <summary>
		/// Offset to Base point in Longitude - degree
		/// </summary>
		public double LongitudeOffset { get; set; }
		/// <summary>
		/// Offset to Base point in Latitude - degree
		/// </summary>
		public double LatitudeOffset { get; set; }
	}

	public class DeviceParameters
	{
		public static readonly string DeviceId = "Device IDs";

		public static readonly string Latitude = "Latitude";
		public static readonly string Longitude = "Longitude";
		public static readonly string Easting = "Easting";
		public static readonly string Northing = "Northing";
		public static readonly string EastingRelative = "Easting - Relative";
		public static readonly string NorthingRelative = "Northing - Relative";

		public static readonly string StartLatitude = "Start Latitude";
		public static readonly string StartLongitude = "Start Longitude";
		public static readonly string EndLatitude = "End Latitude";
		public static readonly string EndLongitude = "End Longitude";
		public static readonly string SpaceName = "Space Name";
		public static readonly string SpaceNumber = "Space Number";
		public static readonly string SpaceRevitObjId = "Space RevitObjId";
		public static readonly string RevitObjId = "revitObjId";

		public static readonly string StartEastingRelative = "Start Easting - Relative";
		public static readonly string StartNorthingRelative = "Start Northing - Relative";
		public static readonly string EndEastingRelative = "End Easting - Relative";
		public static readonly string EndNorthingRelative = "End Northing - Relative";
	}
	public class ASubCategory
	{
		public Category Category { get; set; }
		public bool Checked { get; set; } = false;
	}

	public class ACategory
	{
		public Category Category { get; set; }
		public List<ASubCategory> SubCategories { get; set; }
		public bool Checked { get; set; } = false;
	}

	public class GeoJsonFeature
	{
		public string type => "Feature";
		public Geometry geometry { get; set; }
		public Dictionary<string, object> properties { get; set; }
	}

	public class GeoJsonFeatureCollection
	{
		public string type { get; set; }
		public int LevelCount { get; set; }
		public int LevelIndex { get; set; }
		public List<GeoJsonFeature> features { get; set; }
	}

	public class Geometry
	{
		/***********
		 * Point
		 * LineString
		 * Polygon
		 * MultiPoint
		 * MultiLineString
		 * MultiPolygon
		 */
		public string type;
		public List<List<double[]>> coordinates { get; set; }
		public virtual double CenterLon
		{
			get
			{
				// get average of double[0] through coordinates
				if (coordinates == null || coordinates.Count == 0)
					return 0;

				double totalLon = 0;
				int count = 0;

				foreach (var coordinateList in coordinates)
				{
					foreach (var coordinate in coordinateList)
					{
						totalLon += coordinate[0];
						count++;
					}
				}

				return totalLon / count;
			}
		}
		public virtual double CenterLat
		{
			get
			{
				// get average of double[0] through coordinates
				if (coordinates == null || coordinates.Count == 0)
					return 0;

				double totalLon = 0;
				int count = 0;

				foreach (var coordinateList in coordinates)
				{
					foreach (var coordinate in coordinateList)
					{
						totalLon += coordinate[1];
						count++;
					}
				}

				return totalLon / count;
			}
		}
	}

	public class Point : Geometry
	{
		public new string type => "Point";
		public new double[] coordinates { get; set; }
		public override double CenterLon {
			get
			{
				return coordinates[0];
			}
		}
		public override double CenterLat
		{
			get
			{
				return coordinates[1];
			}
		}
	}

	public class MultiPoint : Geometry
	{
		public string type => "MultiPoint";
		public List<double[]> coordinates { get; set; }
	}

	public class LineString : Geometry
	{
		public string type => "LineString";
		public List<double[]> coordinates { get; set; }
	}

	public class MultiLineString : Geometry
	{
		public new string type => "MultiLineString";
		public new List<List<double[]>> coordinates { get; set; }
	}

	public class Polygon : Geometry
	{
		public new string type => "Polygon";
		public new List<List<double[]>> coordinates { get; set; }
	}

	public class MultiPolygon : Geometry
	{
		public string type => "MultiPolygon";
		public List<List<List<double[]>>> coordinates { get; set; }
	}
}
