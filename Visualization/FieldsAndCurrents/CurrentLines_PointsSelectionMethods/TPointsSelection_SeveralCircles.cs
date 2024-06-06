// Класс для отбора точек по нескольким окружностям для линий тока
using System;
using System.Collections.Generic;
//
using AstraEngine;
using AstraEngine.Components;
//*****************************************************************
namespace Example
{
    /// <summary>
    /// Отбор точек по нескольким окружностям
    /// </summary>
    internal class TPointsSelection_SeveralCircles : IPointsSelectionMethods
    {
        /// <summary>
        /// Центр окружности
        /// </summary>
        public Vector3 Center=new Vector3();
        /// <summary>
        /// Радиус окружности
        /// </summary>
        public float RadiusMin;
        /// <summary>
        /// Количество окружностей
        /// </summary>
        public int CountCircle;
        /// <summary>
        /// Угол между точками
        /// </summary>
        public float Angle;
        /// <summary>
        /// Коэффициент количества точек на кругах
        /// </summary>
        public float Ratio;
        //---------------------------------------------------------------
        /// <summary>
        /// Выбор точек по окружности
        /// </summary>
        /// <returns>Лист точек</returns>
        public List<Vector3> PointsSelection()
        {
            try
            {
                List<Vector3> Points = new List<Vector3>();
                // Переходим к новому базису с центром в окружности
                //Матрица перехода к новому базису (X -> Z, Y -> Y, Z -> -X)
                Matrix A = new Matrix(0, 0, 1, Center.X, 0, 1, 0, Center.Y, -1, 0, 0, Center.Z, 0, 0, 0, 1);
                // Находим угол между точками по окружности
                float AngleBetweenPoints = (float)(Angle * Math.PI / 180f);
                int CountPoints = (int)(360f / Angle);
                float Radius = RadiusMin;
                // Находим координаты точек путем поворота одной из них на соответствующий угол
                for (int i=0; i<CountCircle; i++)
                {
                    if (i>0)
                    {
                        CountPoints = (int)(Ratio * CountPoints);
                        Radius = RadiusMin + 2f * RadiusMin * (float)i;
                        AngleBetweenPoints = (float)(2f * Math.PI / (float)CountPoints);
                    }
                    for (int j=0; j<CountPoints; j++)
                    {
                        Vector3 P = Vector3.Transform(new Vector3(0, Radius, 0), Matrix.CreateRotationZ(AngleBetweenPoints * j));
                        P = Vector3.Transform(P, A) + Center;
                        Points.Add(P);
                    }
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
