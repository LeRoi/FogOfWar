using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FogOfWar {
    public class LightMap {
        private const double epsilon = 0.00001;
        private const double removalThreshold = 1;

        /**
         * Returns a list of points which, when connected sequentially, forms a polygon whose
         * interior should be revealed given a light source at the provided coordinates.
         * Thanks to http://ncase.me/sight-and-light/ for inspiration for this approach.
         */
        public static List<Vector2> getLightMap(Vector2 source, List<FogOfWar.Wall> walls) {
            HashSet<Vector2> points = uniquePoints(walls);
            HashSet<Vector4> segments = uniqueSegments(walls);
            HashSet<double> angles = getAngles(source, points);
            List<Vector4> intersections = new List<Vector4>();

            foreach (double angle in angles) {
                double dx = Math.Cos(angle);
                double dy = Math.Sin(angle);
                Vector4 ray = new Vector4(source.X, source.Y, (float) dx, (float) dy);

                Vector3 closest = Vector3.Zero;
                bool assigned = false;
                foreach (Vector4 segment in segments) {
                    Vector3? intersection = getIntersection(ray,
                        new Vector4(segment.X, segment.Y,
                        segment.Z - segment.X, segment.W - segment.Y));
                    if (intersection == null) continue;
                    Vector3 intersect = intersection.GetValueOrDefault();
                    if (!assigned || intersect.Z < closest.Z) {
                        closest = intersect;
                        assigned = true;
                    }
                }

                if (assigned) intersections.Add(new Vector4(closest, (float) angle));
            }

            // Sort points by angle, and remove clustered points to ease processing load.
            intersections.Sort(compareIntersects);
            List<Vector2> polygonPoints = new List<Vector2>();
            foreach (Vector4 point in intersections) {
                Vector2 point2 = new Vector2(point.X, point.Y);
                bool same = false;
                foreach (Vector2 v in polygonPoints) {
                    same |= distance(v, point2) < removalThreshold;
                }
                if (!same) {
                    polygonPoints.Add(point2);
                }
            }
            return polygonPoints;
        }

        public static List<Vector2> getLightMap(Point source, List<FogOfWar.Wall> walls) {
            return getLightMap(new Vector2(source.X, source.Y), walls);
        }

        /**
         * Returns a list of all unique wall points.
         */
        private static HashSet<Vector2> uniquePoints(List<FogOfWar.Wall> walls) {
            HashSet<Vector2> points = new HashSet<Vector2>();
            foreach (FogOfWar.Wall wall in walls) {
                Rectangle sides = wall.getRect();
                points.Add(new Vector2(sides.Left, sides.Top));
                points.Add(new Vector2(sides.Right, sides.Top));
                points.Add(new Vector2(sides.Left, sides.Bottom));
                points.Add(new Vector2(sides.Right, sides.Bottom));
            }

            // Add corners of window.
            points.Add(new Vector2(0, 0));
            points.Add(new Vector2(0, FogOfWar.height));
            points.Add(new Vector2(FogOfWar.width, 0));
            points.Add(new Vector2(FogOfWar.width, FogOfWar.height));
            return points;
        }

        /**
         * Returns a list of all unique wall segments.
         */
        private static HashSet<Vector4> uniqueSegments(List<FogOfWar.Wall> walls) {
            HashSet<Vector4> segments = new HashSet<Vector4>();
            foreach (FogOfWar.Wall wall in walls) {
                Rectangle sides = wall.getRect();
                segments.Add(new Vector4(sides.Left, sides.Top, sides.Right, sides.Top));
                segments.Add(new Vector4(sides.Left, sides.Top, sides.Left, sides.Bottom));
                segments.Add(new Vector4(sides.Left, sides.Bottom, sides.Right, sides.Bottom));
                segments.Add(new Vector4(sides.Right, sides.Top, sides.Right, sides.Bottom));
            }

            // Add walls for window.
            segments.Add(new Vector4(0, 0, FogOfWar.width, 0));
            segments.Add(new Vector4(0, 0, 0, FogOfWar.height));
            segments.Add(new Vector4(0, FogOfWar.height, FogOfWar.width, FogOfWar.height));
            segments.Add(new Vector4(FogOfWar.width, 0, FogOfWar.width, FogOfWar.height));
            return segments;
        }

        /**
         * Returns a list of angles from the source to each provided point.
         */
        private static HashSet<double> getAngles(Vector2 source, HashSet<Vector2> points) {
            HashSet<double> angles = new HashSet<double>();
            foreach (Vector2 point in points) {
                double angle = Math.Atan2(point.Y - source.Y, point.X - source.X);
                angles.Add(angle);
                angles.Add(angle - epsilon);
                angles.Add(angle + epsilon);
            }
            return angles;
        }

        /**
         * Returns the intersection of a ray and a segment.
         * Both should be in parametric form.
         * Returns (x, y, T1) solution.
         * Returns null if ray and segment are parallel.
         */
        private static Vector3? getIntersection(Vector4 ray, Vector4 seg) {
            double rayMagnitude = Math.Sqrt(ray.Z * ray.Z + ray.W * ray.W);
            double segmentMagnitude = Math.Sqrt(seg.Z * seg.Z + seg.W * seg.W);
            if (ray.Z / rayMagnitude == seg.Z / segmentMagnitude &&
                ray.W / rayMagnitude == seg.W / segmentMagnitude) {
                return null;
            }

            double t2 = (ray.Z * (seg.Y - ray.Y) + ray.W * (ray.X - seg.X)) /
                (seg.Z * ray.W - seg.W * ray.Z);
            double t1 = (seg.X + seg.Z * t2 - ray.X) / ray.Z;

            if (t1 < 0) return null;
            if (t2 < 0 || t2 > 1) return null;

            return new Vector3((float) (ray.X + ray.Z * t1),
                (float) (ray.Y + ray.W * t1),
                (float) t1);
        }

        private static double distance(Vector2 left, Vector2 right) {
            return Math.Sqrt(Math.Pow(left.X - right.X, 2) + Math.Pow(left.Y - right.Y, 2));
        }

        private static int compareIntersects(Vector4 left, Vector4 right) {
            return left.W.CompareTo(right.W);
        }
    }
}
