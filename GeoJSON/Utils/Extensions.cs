using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Architexor.GeoJSON.Utils
{
	public static class Extensions
	{
		public static XYZ GetElementCenterPoint(this Element element)
		{
			var bb = element.get_BoundingBox(null);
			return (bb.Max + bb.Min) / 2;
		}
		public static Room GetRoom(this XYZ point, IEnumerable<Room> rooms)
		{
			// TODO
			// Update the room retrival 
			return rooms.Where(r =>
			{
				// Check if the 2D projection point is inside the rectangle represents
				//the 2D projection of the room bounding box.
				var bb = r.get_BoundingBox(null);
				return bb?.Max.X >= point.X && bb.Max.Y >= point.Y && bb.Min.X <= point.X && bb.Min.Y <= point.Y;
			}).OrderByDescending(r => r.get_BoundingBox(null)?.Max.Z).FirstOrDefault();
		}
	}
}
