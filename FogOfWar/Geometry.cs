using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FogOfWar {
    public class GameGeometry {
        public List<Vector2> Geometry { get; set; }
        public List<float> XPoints { get; set; }
        public List<float> YPoints { get; set; }
        public List<float> PointConstants { get; set; }
        public List<float> PointMultiples { get; set; }
        public List<float> LineSlopes { get; set; }
        public List<float> LineConstants { get; set; }

        public GameGeometry(List<Vector2> geometry) {
            this.Geometry = geometry;
            XPoints = new List<float>();
            YPoints = new List<float>();
            PointConstants = new List<float>();
            PointMultiples = new List<float>();
            LineSlopes = new List<float>();
            LineConstants = new List<float>();
            foreach (Vector2 point in geometry) {
                XPoints.Add(point.X);
                YPoints.Add(point.Y);
                PointConstants.Add(0);
                PointMultiples.Add(0);
                LineSlopes.Add(0);
                LineConstants.Add(0);
            }

            preprocessGeometry();
        }

        /**
         * Generic helper function which precomputes commonly used variables over a set of
         * line segments defined by an ordered set of points. Stores the computed data
         * in the set of provided arrays.
         */
        private void preprocessGeometry() {
            int j = Geometry.Count - 1;
            for (int i = 0; i < Geometry.Count; i++) {
                if (YPoints[j] == YPoints[i]) {
                    PointConstants[i] = XPoints[i];
                    PointMultiples[i] = 0;
                } else {
                    PointConstants[i] = XPoints[i] - (YPoints[i] * XPoints[j]) / (YPoints[j] - YPoints[i]) + (YPoints[i] * XPoints[i]) / (YPoints[j] - YPoints[i]);
                    PointMultiples[i] = (XPoints[j] - XPoints[i]) / (YPoints[j] - YPoints[i]);
                }

                LineSlopes[i] = getLineSlope(XPoints[i], YPoints[i], XPoints[j], YPoints[j]);
                LineConstants[i] = getLineConstant(XPoints[i], YPoints[i], XPoints[j], YPoints[j]);

                j = i;
            }
        }

        private static float getLineSlope(float xi, float yi, float xj, float yj) {
            if (xi == xj) {
                return float.NaN;
            }

            return (yj - yi) / (xj - xi);
        }

        private static float getLineConstant(float xi, float yi, float xj, float yj) {
            float slope = getLineSlope(xi, yi, xj, yj);
            if (float.IsNaN(slope)) {
                return float.NaN;
            }

            return yi - (xi * slope);
        }

        private static bool isPointOnLineSegment(float xi, float yi, float xj, float yj, float xTest, float yTest) {
            float slope = getLineSlope(xi, yi, xj, yj);
            float intercept = getLineConstant(xi, yi, xj, yj);
            float xMax = Math.Max(xi, xj);
            float xMin = Math.Min(xi, xj);
            float yMax = Math.Max(yi, yj);
            float yMin = Math.Min(yi, yj);

            if (xTest < xMin || xTest > xMax || yTest < yMin || yTest > yMax) {
                return false;
            }

            return float.IsNaN(slope) || slope * xTest + intercept == yTest;
        }
    }
}
