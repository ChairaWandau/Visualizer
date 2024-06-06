// Класс для отбора точек на плоскости (для построения линий тока) 
using System;
using System.Collections.Generic;
//
using AstraEngine;
using AstraEngine.Components;
using AstraEngine.Components.Importers.YAML.Core.Tokens;
using MPT707.Aerodynamics;
//*****************************************************************
namespace Example
{
    /// <summary>
    /// Класс для отбора точек на плоскости (для построения линий тока) 
    /// </summary>
    internal class TPointsSelection_OnPlane: IPointsSelectionMethods
    {
        /// <summary>
        /// Точка, определяющая плоскость, параллельную YZ
        /// </summary>
        public float Point { get; set; }
        /// <summary>
        /// Шаг между точками
        /// </summary>
        public float Step; 
        /// <summary>
        /// Объект класса для использования оттуда методов
        /// </summary>
        TViewerAero_Helper Helper=new TViewerAero_Helper();
        /// <summary>
        /// Параллелепипед расчетной области
        /// </summary>
        public BoundingBox BB;
        /// <summary>
        /// Вершины расчетной области
        /// </summary>
        List<Vector3[]> Edges;
        //---------------------------------------------------------------
        public TPointsSelection_OnPlane(BoundingBox boundingBox, List<Vector3[]> DomainEdges)
        {
            BB = boundingBox;
            Edges = DomainEdges;
        }
        /// <summary>
        /// Выбор точек по плоскости
        /// </summary>
        /// <returns>Лист точек</returns>
        public List<Vector3> PointsSelection()
        {
            try
            {
                Plane Plane= new Plane(Vector3.UnitX, Point);
                // Поиск точек пересечений плоскости с гранями сетки
                var R = Helper.LineWithPlaneIntersection(Plane, Edges);
                if(R.Count==0) TJournalLog.WriteLog("There is no intersection of the plane with the domain.");
                // Составляем список точек в новом базисе
                List<Vector3> Points = new List<Vector3>();
                float Vertical = 0f;
                float Horizontal = 0f;
                while (Vertical<= BB.Max.Y - BB.Min.Y)
                {
                    while (Horizontal<= BB.Max.Z - BB.Min.Z)
                    {
                        Points.Add(new Vector3(Point, BB.Min.Y + Vertical, BB.Min.Z + Horizontal));
                        Horizontal += Step;
                    }
                    Vertical += Step;
                    Horizontal = 0f;
                }
                return Points;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0001: Error PointsSelection_OnPlane:PointsSelection(): " + E.Message);
                return new List<Vector3>();
            }
        }
        //---------------------------------------------------------------
    }
}
