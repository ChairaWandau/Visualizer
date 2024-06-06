// Класс, для метода задания точки в ручную
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//
using AstraEngine;
//*********************************************************************
namespace Example
{
    /// <summary>
    /// Класс для отбора точек на плоскости (для построения линий тока) 
    /// </summary>
    internal class TPointsSelection_ManualInput : IPointsSelectionMethods
    {
        /// <summary>
        /// Заданная точка
        /// </summary>
        public Vector3 Point;
        public List<Vector3> PointsSelection()
        {
            List<Vector3> NewPoints = new List<Vector3>();
            NewPoints.Add(Point);
            return NewPoints;
        }
    }
}
