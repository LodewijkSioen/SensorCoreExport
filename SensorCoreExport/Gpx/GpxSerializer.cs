﻿using Lumia.Sense;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SensorCoreExport.Gpx
{
    public class GpxSerializer
    {
        readonly XNamespace _sce = "http://www.SensorCoreExport.com";

        public void Serialize(IEnumerable<TrackPoint> trackpoints, Stream destination)
        {
            var waypointsPerDay = trackpoints.GroupBy(w => w.Timestamp.Date);
            var gpx = new gpxType
            {
                creator = "SensorCore Export",
                trk = GetTracks(waypointsPerDay).ToArray()
            };
            var ns = new XmlSerializerNamespaces();
            ns.Add("sce", _sce.NamespaceName);
            var ser = new XmlSerializer(typeof(gpxType));
            ser.Serialize(destination, gpx, ns);
        }

        public void Serialize(IEnumerable<Place> places, Stream destination)
        {
            var gpx = new gpxType()
            {
                creator = "SensorCore Export",
                wpt = GetWaypoints(places).ToArray()
            };
            var ns = new XmlSerializerNamespaces();
            ns.Add("sce", _sce.NamespaceName);
            var ser = new XmlSerializer(typeof(gpxType));
            ser.Serialize(destination, gpx, ns);
        }

        private IEnumerable<wptType> GetWaypoints(IEnumerable<Place> places)
        {
            foreach (var place in places)
            {
                yield return new wptType
                {
                    lat = (decimal)place.Position.Latitude,
                    lon = (decimal)place.Position.Longitude,
                    ele = (decimal)place.Position.Altitude,
                    eleSpecified = true,
                    time = place.Timestamp.UtcDateTime,
                    timeSpecified = true,
                    src = "Lumia SensorCore",
                    extensions = new extensionsType
                    {
                        Any = new[]
                    {
                        new XElement(_sce + "Id", place.Id),
                        new XElement(_sce + "Radius", place.Radius),
                        new XElement(_sce + "LengthOfStay", place.LengthOfStay.ToString("g"))
                    }
                    }
                };
            }
        }

        private IEnumerable<trkType> GetTracks(IEnumerable<IGrouping<DateTime, TrackPoint>> tracksPerDay)
        {
            foreach (var tracksOfDay in tracksPerDay)
            {
                yield return new trkType
                {
                    name = $"Tracks recorded for {tracksOfDay.Key:d}",
                    src = "Lumia SensorCore",
                    trkseg = new[] { new trksegType { trkpt = GetWaypoints(tracksOfDay).ToArray() } }
                };
            }
        }

        private IEnumerable<wptType> GetWaypoints(IEnumerable<TrackPoint> source)
        {
            foreach (var point in source)
            {
                yield return GetWayPoint(point, point.Timestamp);

                if (point.LengthOfStay > TimeSpan.FromMinutes(10) && (point.Timestamp + point.LengthOfStay).Date == point.Timestamp.Date)
                {
                    yield return GetWayPoint(point, point.Timestamp.Add(point.LengthOfStay));
                }
            }
        }

        private wptType GetWayPoint(TrackPoint point, DateTimeOffset time)
        {
            return new wptType
            {
                lat = (decimal)point.Position.Latitude,
                lon = (decimal)point.Position.Longitude,
                ele = (decimal)point.Position.Altitude,
                eleSpecified = true,
                time = time.UtcDateTime,
                timeSpecified = true,
                src = "Lumia SensorCore",
                extensions = new extensionsType
                {
                    Any = new[]
                    {
                        new XElement(_sce + "Id", point.Id),
                        new XElement(_sce + "Radius", point.Radius),
                        new XElement(_sce + "LengthOfStay", point.LengthOfStay.ToString("g"))
                    }
                }
            };
        }
    }
}
