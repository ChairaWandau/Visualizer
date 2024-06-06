// Класс для отбора точек по окружности (для построения линий тока) 
using System;
using System.Collections.Generic;
//
using AstraEngine;
using AstraEngine.Components;
//*****************************************************************
namespace Example
{
    /// <summary>
    /// Класс для отбора точек по окружности (для построения линий тока) 
    /// </summary>
    internal class TPointsSelection_Circle : IPointsSelectionMethods
    {
        /// <summary>
        /// Центр окружности
        /// </summary>
        public Vector3 Center;
        /// <summary>
        /// Радиус окружности
        /// </summary>
        public float Radius;
        /// <summary>
        /// Количество точек
        /// </summary>
        public int PointsAmount;
        //---------------------------------------------------------------
        /// <summary>
        /// Выбор точек по окружности
        /// </summary>
        /// <returns>Лист точек</returns>
        public List<Vector3> PointsSelection()
        {
            try
            {
                List<Vector3> Points = new List<Vector3>(PointsAmount);
                // Находим угол между точками по окружности
                float AngleBetweenPoints = (float)(2f * Math.PI / (float)PointsAmount);
                // Переходим к новому базису с центром в окружности
                //Матрица перехода к новому базису (X -> Z, Y -> Y, Z -> -X)
                Matrix A = new Matrix(0, 0, 1, Center.X, 0, 1, 0, Center.Y, -1, 0, 0, Center.Z, 0, 0, 0, 1);
                // Находим координаты точек путем поворота одной из них на соответствующий угол
                for (int i = 0; i < PointsAmount; i++)
                {
                    Vector3 P = Vector3.Transform(new Vector3(0, Radius, 0), Matrix.CreateRotationZ(AngleBetweenPoints * i));
                    // Возвращаемся к старому базису
                    P = Vector3.Transform(P, A) + Center;
                    Points.Add(P);
                }
                return Points;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0003: Error PointsSelection_Circle:PointsSelection(): " + E.Message);
                return new List<Vector3>();
            }
        }
        //---------------------------------------------------------------
    }
}
