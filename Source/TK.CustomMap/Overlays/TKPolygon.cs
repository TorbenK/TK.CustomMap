using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace TK.CustomMap.Overlays
{
    /// <summary>
    ///     A polygon to display on the map
    /// </summary>
    public class TKPolygon : TKOverlay
    {
        // ReSharper disable once InconsistentNaming
        private const int wgs84RADIUS = 6378137;
        private List<Position> _coordinates;
        private Color _strokeColor;
        private float _strokeWidth;

        /// <summary>
        ///     Creates a new instance of <c>TKPolygon</c>
        /// </summary>
        public TKPolygon()
        {
            _coordinates = new List<Position>();
        }

        /// <summary>
        ///     List of positions of the polygon
        /// </summary>
        public List<Position> Coordinates
        {
            get => _coordinates;
            set => SetField(ref _coordinates, value);
        }

        /// <summary>
        ///     Gets/Sets the stroke color of the polygon
        /// </summary>
        public Color StrokeColor
        {
            get => _strokeColor;
            set => SetField(ref _strokeColor, value);
        }

        /// <summary>
        ///     Gets/Sets the width of the stroke
        /// </summary>
        public float StrokeWidth
        {
            get => _strokeWidth;
            set => SetField(ref _strokeWidth, value);
        }

        /// <summary>
        ///     Calculate the area of this <see cref="TKPolygon" /> in m^2
        /// </summary>
        public double GetArea()
        {
            var coords = EnsureCorrectWinding(Coordinates).ToList();
            var area = CalculateAreaInSquareMeters(coords);
            return area;
        }

        private static double CalculateAreaInSquareMeters(IReadOnlyList<Position> path, double radius = wgs84RADIUS)
        {
            var size = path.Count;
            if (size < 3) return 0;
            var total = 0d;
            var prev = path[size - 1];
            var prevTanLat = Math.Tan((Math.PI / 2 - ToRadian(prev.Latitude)) / 2);
            var prevLng = ToRadian(prev.Longitude);
            // For each edge, accumulate the signed area of the triangle formed by the North Pole
            // and that edge ("polar triangle").
            foreach (var point in path)
            {
                var tanLat = Math.Tan((Math.PI / 2 - ToRadian(point.Latitude)) / 2);
                var lng = ToRadian(point.Longitude);
                total += PolarTriangleArea(tanLat, lng, prevTanLat, prevLng);
                prevTanLat = tanLat;
                prevLng = lng;
            }

            var result = total * (radius * radius);
            return Math.Abs(result);

            double PolarTriangleArea(double tan1, double lng1, double tan2, double lng2)
            {
                var deltaLng = lng1 - lng2;
                var t = tan1 * tan2;
                return 2 * Math.Atan2(t * Math.Sin(deltaLng), 1 + t * Math.Cos(deltaLng));
            }
        }

        private static IEnumerable<Position> EnsureCorrectWinding(IEnumerable<Position> positions, bool isHole = false)
        {
            var vertices = positions.ToList();

            var clockWiseCount = 0;
            var counterClockWiseCount = 0;
            var p1 = vertices[0];

            for (var i = 1; i < vertices.Count; i++)
            {
                var p2 = vertices[i];
                var p3 = vertices[(i + 1) % vertices.Count];

                var e1 = new Position(p1.Longitude - p2.Longitude, p1.Latitude - p2.Latitude);
                var e2 = new Position(p3.Longitude - p2.Longitude, p3.Latitude - p2.Latitude);

                if (e1.Longitude * e2.Latitude - e1.Latitude * e2.Longitude >= 0)
                    clockWiseCount++;
                else
                    counterClockWiseCount++;

                p1 = p2;
            }

            var isClockwise = clockWiseCount > counterClockWiseCount;

            return isClockwise
                ? isHole
                    ? vertices
                    : vertices.Reverse<Position>()
                : isHole
                    ? vertices.Reverse<Position>()
                    : vertices;
        }

        private static double ToRadian(double val) => val * (Math.PI / 180);
    }
}