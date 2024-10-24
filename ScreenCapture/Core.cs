using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCapture
{
    public class CircleFitter
    {
        public bool IsCircle(List<PointF> points, float tolerance = 0.1f)
        {
            if (points.Count < 3)
            {
                throw new ArgumentException("至少需要三個點來擬合圓");
            }

            // 使用最小二乘法擬合圓
            (float centerX, float centerY, float radius) = FitCircle(points);

            // 檢查每個點與擬合圓的距離
            foreach (var point in points)
            {
                float distance = Distance(point.X, point.Y, centerX, centerY);
                if (Math.Abs(distance - radius) > tolerance)
                {
                    return false; // 如果某個點的距離超過容忍範圍，則返回 false
                }
            }

            return true; // 所有點均在圓上
        }

        private (float, float, float) FitCircle(List<PointF> points)
        {
            float sumX = 0, sumY = 0, sumXX = 0, sumYY = 0, sumXY = 0;
            int n = points.Count;

            foreach (var p in points)
            {
                sumX += p.X;
                sumY += p.Y;
                sumXX += p.X * p.X;
                sumYY += p.Y * p.Y;
                sumXY += p.X * p.Y;
            }

            float C = sumXX + sumYY;
            float A = sumX;
            float B = sumY;
            float D = sumXY;

            float denominator = (n * sumXX - sumX * sumX) * (n * sumYY - sumY * sumY) - (n * sumXY - sumX * sumY) * (n * sumXY - sumX * sumY);

            float centerX = ((n * sumYY - sumY * sumY) * (n * sumXX - sumX * sumX) * (sumX * sumXY - sumY * C) + (n * sumXY - sumX * sumY) * (sumX * sumX * (n * sumXY - sumX * sumY) - sumY * (n * sumYY - sumY * sumY)))
                            / denominator;

            float centerY = ((n * sumXX - sumX * sumX) * (n * sumXY - sumX * sumY) * (sumX * sumY - n * C) + (n * sumYY - sumY * sumY) * (sumY * sumY * (n * sumXY - sumX * sumY) - sumX * (n * sumXX - sumX * sumX)))
                            / denominator;

            float radius = (float)Math.Sqrt((sumXX + sumYY) / n - centerX * centerX - centerY * centerY);

            return (centerX, centerY, radius);
        }

        private float Distance(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }
    }



}
