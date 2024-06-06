// Класс, в котором содержатся методы для помощи создания линий тока
using System;
using System.Collections.Generic;
//
using AstraEngine;
using AstraEngine.Engine.GraphicCore;
//***************************************************************
namespace Example
{
    // Помощник для линий тока
    internal class TViewerAero_HelperCurrentLines
    {
        /// <summary>
        /// Визуализатор для раскрашивания линий тока
        /// </summary>
        private TViewerAero_Visualizer Visualizer;
        /// <summary>
        /// объект визуализатора
        /// </summary>
        public TViewerAero_HelperCurrentLines(TViewerAero_Visualizer Visualizer)
        {
            this.Visualizer = Visualizer;
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Создание трианглконтейнеров
        /// </summary>
        /// <param name="Radius">Радиус цилиндра</param>
        /// <param name="Points">Массив точек</param>
        /// <param name="NumberFaces">Количество граней в сечении</param>
        /// <param name="Min">Минимальное значение величины</param>
        /// <param name="Max">Максимальное значение величины</param>
        /// <returns></returns>
        public List<TTriangleContainer> CreateCurrentLines (TFemElement_Visual[] Points, float Radius, int NumberFaces, float Min, float Max)
        {
            List<TTriangleContainer> Cyllinder = new List<TTriangleContainer>();
            List<TTriangle> Triangles = new List<TTriangle>();
            for (int i=0; i<Points.Length-1; i++)
            {
                Triangles.AddRange(CreateTrianglesForCyllinder(Radius, Points[i], Points[i + 1], NumberFaces, Min, Max));
            }
            TTriangleContainer CurrentLines = new TTriangleContainer(Triangles);
            Cyllinder.Add(CurrentLines);
            return Cyllinder;
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Создание треугольников для одного цилиндра
        /// </summary>
        /// <param name="Radius">Радиус цилиндра</param>
        /// <param name="VertexStart">Вершина старта</param>
        /// <param name="VertexEnd">Вершина конца</param>
        /// <param name="NumberFaces">Количество граней в сечении цилиндра</param>
        /// <param name="Min">Минимальное значение величины</param>
        /// <param name="Max">Максимальное значение величины</param>
        /// <returns>Список треугольников</returns>
        private List<TTriangle> CreateTrianglesForCyllinder (float Radius, TFemElement_Visual VertexStart, TFemElement_Visual VertexEnd, int NumberFaces, float Min, float Max)
        {
            // Вектор нормали
            Vector3 Normal = VertexEnd.Position - VertexStart.Position;
            // Расчет коэффциента D
            float D = -(VertexStart.Position.X * Normal.X + VertexStart.Position.Y * Normal.Y + VertexStart.Position.Z * Normal.Z);
            Normal.Normalize();
            // Вектор направляющей для Х
            Vector3 NewX;
            // Вектор для точки на плоскости
            Vector3 PointsInPlane=new Vector3();
            if (Normal.Z != 0) PointsInPlane = new Vector3(0f, 0f, -(D / Normal.Z));
            else if (Normal.X != 0) PointsInPlane = new Vector3(-(D / Normal.X), 0f, 0f);
            else PointsInPlane = new Vector3(0f, -(D / Normal.Y), 0f);
            NewX=PointsInPlane-VertexStart.Position;
            NewX.Normalize();
            // Вектор направляющей для Y
            Vector3 NewY = Vector3.Cross(NewX, Normal);
            NewY.Normalize();
            //Матрица перехода к новому базису
            Matrix A = new Matrix(NewX.X, NewX.Y, NewX.Z, VertexStart.Position.X, NewY.X, NewY.Y, NewY.Z, VertexStart.Position.Y, -Normal.X, -Normal.Y, -Normal.Z, VertexStart.Position.Z, 0, 0, 0, 1);
            // Центр координат нового базиса в глобальной системе координат
            Vector3 OldCenter = Vector3.Transform(VertexStart.Position, Matrix.Invert(A * (-1)));
            // Точка конца цилиндра в новой системе координат 
            Vector3 End = Vector3.Transform(VertexEnd.Position, Matrix.Invert(A)) + OldCenter;
            // Создание точек в новом базисе
            TFemElement_Visual[] NewBasisPoints = GetPointsInNewBasis(NumberFaces, End, Radius);
            TFemElement_Visual[] OldBasisPoints = new TFemElement_Visual[NewBasisPoints.Length];
            // Переход из нового базиса в старый
            for (int i=0; i<OldBasisPoints.Length; i++)
            {
                OldBasisPoints[i] = new TFemElement_Visual
                {
                    Position = Vector3.Transform(NewBasisPoints[i].Position, A) + VertexStart.Position
                };
            }
            // Объединение точек в треугольники
            List<TTriangle> Triangles = new List<TTriangle>();
            for (int i = 0; i < OldBasisPoints.Length; i += 2)
            {
                TTriangle triangle = new TTriangle();
                TTriangle triangle2 = new TTriangle();
                if (i > OldBasisPoints.Length - 3)
                {
                    triangle.P0 = OldBasisPoints[i].Position;
                    triangle.P1 = OldBasisPoints[i + 1].Position;
                    triangle.P2 = OldBasisPoints[1].Position;
                    triangle.UV0 = Visualizer.GetUVCoordinate(VertexStart.VelocityModule, Min, Max);
                    triangle.UV1 = Visualizer.GetUVCoordinate(VertexEnd.VelocityModule, Min, Max); 
                    triangle.UV2 = Visualizer.GetUVCoordinate(VertexEnd.VelocityModule, Min, Max); 
                    Triangles.Add(triangle);

                    triangle2.P0 = OldBasisPoints[i].Position;
                    triangle2.P1 = OldBasisPoints[1].Position;
                    triangle2.P2 = OldBasisPoints[0].Position;
                    triangle2.UV0 = Visualizer.GetUVCoordinate(VertexStart.VelocityModule, Min, Max);
                    triangle2.UV1 = Visualizer.GetUVCoordinate(VertexEnd.VelocityModule, Min, Max);
                    triangle2.UV2 = Visualizer.GetUVCoordinate(VertexStart.VelocityModule, Min, Max);
                    Triangles.Add(triangle2);
                }
                else
                {
                    triangle.P0 = OldBasisPoints[i].Position;
                    triangle.P1 = OldBasisPoints[i + 1].Position;
                    triangle.P2 = OldBasisPoints[i + 3].Position;
                    triangle.UV0 = Visualizer.GetUVCoordinate(VertexStart.VelocityModule, Min, Max);
                    triangle.UV1 = Visualizer.GetUVCoordinate(VertexEnd.VelocityModule, Min, Max);
                    triangle.UV2 = Visualizer.GetUVCoordinate(VertexEnd.VelocityModule, Min, Max);
                    Triangles.Add(triangle);

                    triangle2.P0 = OldBasisPoints[i].Position;
                    triangle2.P1 = OldBasisPoints[i + 3].Position;
                    triangle2.P2 = OldBasisPoints[i + 2].Position;
                    triangle2.UV0 = Visualizer.GetUVCoordinate(VertexStart.VelocityModule, Min, Max);
                    triangle2.UV1 = Visualizer.GetUVCoordinate(VertexEnd.VelocityModule, Min, Max);
                    triangle2.UV2 = Visualizer.GetUVCoordinate(VertexStart.VelocityModule, Min, Max);
                    Triangles.Add(triangle2);
                }
            }
            return Triangles;
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Получение точек цилиндра в новом базисе
        /// </summary>
        /// <param name="NumberFaces">Количество граней в сечении</param>
        /// <param name="H">Высота цилиндра</param>
        /// <param name="Radius">Радиус цилиндра</param>
        /// <returns></returns>
        private TFemElement_Visual[] GetPointsInNewBasis (int NumberFaces, Vector3 H, float Radius)
        {
            Vector3 zero = Vector3.Zero;
            TFemElement_Visual[] array = new TFemElement_Visual[NumberFaces*2];
            float num =-1f;
            for (int i = 0; i < NumberFaces * 2; i += 2)
            {
                num++;
                float num3 = 2*(float)(Math.PI)*num / NumberFaces;
                float num4 = (float)Math.Sin(num3);
                float num5 = (float)Math.Cos(num3);
                array[i] = new TFemElement_Visual();
                array[i+1] = new TFemElement_Visual();
                array[i].Position = new Vector3(zero.X + (num4 * Radius), zero.Y + (num5 * Radius), zero.Z);
                array[i + 1].Position = new Vector3(zero.X + (num4 * Radius), zero.Y + (num5 * Radius), zero.Z) + H;
            }
            return array;
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
    }
}
