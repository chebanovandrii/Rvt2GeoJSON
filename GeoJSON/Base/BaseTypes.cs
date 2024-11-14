using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace Architexor.GeoJSON.Base
{
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
	}

	public class Point : Geometry
	{
		public string type => "Point";
		public double[] coordinates { get; set; }
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
		public string type => "MultiLineString";
		public List<List<double[]>> coordinates { get; set; }
	}

	public class Polygon : Geometry
	{
		public string type => "Polygon";
		public List<List<double[]>> coordinates { get; set; }
	}

	public class MultiPolygon : Geometry
	{
		public string type => "MultiPolygon";
		public List<List<List<double[]>>> coordinates { get; set; }
	}
}
