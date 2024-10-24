using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tradegame.Models
{
    public class PointInfo
    {
       public Point point { get; set; }
       
       public List<PointInfo> NextPoints { get; set; }

       public PointInfo(Point _point)
        {
            point = _point;
            NextPoints = new List<PointInfo>();
        }
       
       public bool HasnextPoint(PointInfo pointInfo, Point _point)
        {
            foreach(var pointinfo in pointInfo.NextPoints)
            {
                if(pointinfo.point == _point)
                {
                    return true;
                }
            }

            return false;
        }
        public bool IsExists(List<PointInfo> PointsInfo, Point point)
        {
            foreach(var pointInfo in PointsInfo)
            {
                if (pointInfo.point == point) return true;
            }
            return false;
        }
        public void InsertPoint(PointInfo pointInfo,Point _point)
        {
            PointInfo nextpoint = new PointInfo(_point);
            pointInfo.NextPoints.Add(nextpoint);
        }

        public int NumberOfSquares(PointInfo pointInfo, Point _endpoint)
        {
            //bfs
            int ans = 0;
            List<KeyValuePair<double, double>> VisitedPoints = new List<KeyValuePair<double, double>>();
            Point _startpoint = pointInfo.point;
            VisitedPoints.Add(new KeyValuePair<double, double>(_endpoint.X, _endpoint.Y));
            List<PointInfo> bfspoints = new List<PointInfo>();
            bfspoints.Add(pointInfo);
            while (bfspoints.Count != 0)
            {
                PointInfo curentpointInfo = bfspoints.First();
                Point currentpoint = bfspoints.First().point;
                VisitedPoints.Add(new KeyValuePair<double, double>(currentpoint.X, currentpoint.Y));
                List<PointInfo> nextPointsInfo = bfspoints.First().NextPoints;
                int level = 1;
                bfspoints.RemoveAt(0);
                foreach(var nextpointInfo in nextPointsInfo)
                {
                    Point nextpoint = nextpointInfo.point;
                    if (nextpoint == _endpoint)
                    {
                        if (level == 3)
                        {
                            ans++;
                        }
                    }
                    if(VisitedPoints.Contains(new KeyValuePair<double, double>(nextpoint.X, nextpoint.Y))){
                        continue;
                    }
                    else
                    {
                        VisitedPoints.Add(new KeyValuePair<double, double>(nextpoint.X, nextpoint.Y));
                    }
                }
                level++;
                if (level == 4) break;
            }
            return 0;
        }
    }
}
