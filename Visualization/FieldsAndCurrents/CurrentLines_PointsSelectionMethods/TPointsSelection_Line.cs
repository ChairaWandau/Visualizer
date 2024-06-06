// Класс для отбора точек по отрезку для линий тока
using System;
using System.Collections.Generic;
//
using AstraEngine;
using AstraEngine.Components;
//*****************************************************************
namespace Example
{
    /// <summary>
    /// Отбор точек по отрезку
    /// </summary>
    internal class TPointsSelection_Line : IPointsSelectionMethods
    {
        /// <summary>
        /// Первая точка отрезка
        /// </summary>
        public Vector3 FirstPoint;
        /// <summary>
        /// Вторая точка отрезка
        /// </summary>
        public Vector3 SecondPoint;
        /// <summary>
        /// Шаг
        /// </summary>
        public float Step;
        /// <summary>
        /// Отбор точек
        /// </summary>
        /// <returns></returns>
        public List<Vector3> PointsSelection()
        {
            try
            {
                List<Vector3> Points = new List<Vector3>();
                Vector3 NextPoint = new Vector3();
                // Вектор направления
                Vector3 Direction = SecondPoint - FirstPoint;
                Direction.Normalize();
                Points.Add(FirstPoint);
                // Длина отрезка
                float LengthLine = Vector3.Distance(FirstPoint, SecondPoint);
                while (true)
                {
                    NextPoint = Points[Points.Count - 1] + Direction * Step;
                    if (Vector3.Distance(FirstPoint, NextPoint) <= LengthLine)
                    {
                        Points.Add(NextPoint);
                    }
                    else
                    {
                        break;
                    }
                }
                Points.Add(SecondPoint);
                return Points;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0003: Error TPointsSelection_Line:PointsSelection(): " + E.Message);
                return null;
            }
            
        }
    }
}
