using Architexor.Commands.GeoJSON;
using Architexor.GeoJSON.Base;
using Architexor.GeoJSON.Utils;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using Document = Autodesk.Revit.DB.Document;
using GeoPoint = Architexor.GeoJSON.Base.Point;


namespace Architexor.GeoJSON.Controllers
{
  class DevicePropertyManager
  {
    private Document mDocument;
    private ElementMulticategoryFilter mTargetCategories;
    private double mLongitude;
    private double mLatitude;
    private double mAngle;
    private double mLongitudeOffset;
    private double mLatitudeOffset;
    private bool mShowFeatures;
    public List<GeoJsonFeature> mFeatures;

    public DevicePropertyManager(Document doc, GeoBase geoBase, bool show)
    {
      mDocument = doc;
      mLongitude = geoBase.Longitude;
      mLatitude = geoBase.Latitude;
      mAngle = geoBase.Angle;
      mLongitudeOffset = geoBase.LongitudeOffset;
      mLatitudeOffset = geoBase.LatitudeOffset;

      mTargetCategories = new ElementMulticategoryFilter(new List<BuiltInCategory>() {
        BuiltInCategory.OST_LightingFixtures,
        BuiltInCategory.OST_DataDevices
      });

      mShowFeatures = show;
      if (mShowFeatures)
        mFeatures = new List<GeoJsonFeature>();
    }

    public void UpdateDeviceProperties()
    {
      /// Update lighting fixture elements
      /// 
      UpdateLightingFixtureProperties();
    }

    private void UpdateLightingFixtureProperties()
    {
      using (Transaction trans = new Transaction(mDocument, "fake"))
      {
        trans.Start();
        /// Get all lighting fixtures in the document.
        /// 
        var allFixtures = new FilteredElementCollector(mDocument)
          .WhereElementIsNotElementType()
          .WherePasses(mTargetCategories)
          .ToElements();

        int deviceId = 0;
        foreach (var elem in allFixtures)
        {
          deviceId++;
          elem.LookupParameter(DeviceParameters.DeviceId)?.Set(deviceId);
          UpdateRoomSharedParameters(elem);
          UpdateGeoParameters(elem);
        }
        trans.Commit();
      }
    }
		private void XYZ2GeoLocation(XYZ point, out double lat, out double lon, out double northingRelInMm, out double eastingRelInMm)
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
      //return new double[] { mLongitude + longitudeOffset, mLatitude + latitudeOffset };//, z };
      lat = mLatitude + latitudeOffset;
      lon = mLongitude + longitudeOffset;
      northingRelInMm = adjustedY * 1000;
			eastingRelInMm = adjustedX * 1000;
		}
    private void UpdateGeoParameters(Element elem)
    {
      var devicePoint = (((FamilyInstance)elem).Location as LocationPoint)?.Point;

      XYZ2GeoLocation(devicePoint, out var lat, out var lon, out var northing, out var easting);

      elem.LookupParameter(DeviceParameters.Latitude)?.Set(lat.ToString());
      elem.LookupParameter(DeviceParameters.Longitude)?.Set(lon.ToString());
      elem.LookupParameter(DeviceParameters.EastingRelative)?.Set(easting.ToString());
      elem.LookupParameter(DeviceParameters.NorthingRelative)?.Set(northing.ToString());
      elem.LookupParameter(DeviceParameters.RevitObjId)?.Set(elem.Id.IntegerValue.ToString());
#if REVIT2024 || REVIT2025
      elem.LookupParameter(DeviceParameters.RevitObjId)?.Set(elem.Id.Value.ToString());
#else
      elem.LookupParameter(DeviceParameters.RevitObjId)?.Set(elem.Id.IntegerValue.ToString());
#endif

      if (mShowFeatures)
      {
        GeoPoint geometry = new GeoPoint { coordinates = new double[] { lon, lat } };
        Dictionary<string, object> properties = new Dictionary<string, object>
        {
#if REVIT2024 || REVIT2025
				{ "id", elem.Id.Value },
#else
				{ "id", elem.Id.IntegerValue },
#endif
				{ "name", elem.Name },
        { "category", elem.Category?.Name }
        };
        mFeatures.Add(new GeoJsonFeature
        {
          geometry = geometry,
          properties = properties
        });
      }
    }
		private void UpdateRoomSharedParameters(Element elem)
    {
      var centerPoint = elem.GetElementCenterPoint();
      var room = mDocument.GetRoomAtPoint(centerPoint);
      if (room == null)
      {
        // If no room is returned, the rooms that lays beneath the elements are retrieved,
        // then get the room where the element center point is exactly above.
        var rooms = new FilteredElementCollector(mDocument)
          .OfCategory(BuiltInCategory.OST_Rooms)
          .OfType<Room>()
          .Where(r => r.get_BoundingBox(null)?.Max.Z <= centerPoint.Z)
          .ToList();

        room = centerPoint.GetRoom(rooms);
      }
      if (room == null)
      {
        return;
      }

      // Get room info
      var roomNumber = room.Number;
      var roomName = room.Name.Replace(roomNumber, "");

      // get shared parameters
      var spaceNameParam = elem.LookupParameter(DeviceParameters.SpaceName);
      var spaceNumberParam = elem.LookupParameter(DeviceParameters.SpaceNumber);
      var spaceRevitObjIdParam = elem.LookupParameter(DeviceParameters.SpaceRevitObjId);

      // Set the shared parameter values
      spaceNameParam?.Set(roomName);
      spaceNumberParam?.Set(roomNumber);

#if REVIT2024 || REVIT2025
      spaceRevitObjIdParam?.Set(room.Id.Value.ToString());
#else
      spaceRevitObjIdParam?.Set(room.Id.IntegerValue.ToString());
#endif
    }
  }
}