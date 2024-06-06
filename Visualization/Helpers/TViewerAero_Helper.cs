// Вспомогательный класс, хранящий методы, потребные для TViewerAero и TViewerAero_Plane
using System;
using System.Collections.Generic;
using AstraEngine;
using AstraEngine.Components;
using AstraEngine.Components.MathHelper;
//***************************************************************
namespace Example
{
    /// <summary>
    /// Вспомогательный класс, хранящий методы, потребные для TViewerAero и TViewerAero_Plane
    /// </summary>
    public class TViewerAero_Helper
    {
        /// <summary>
        /// Получить грани расчетной области
        /// </summary>
        /// <param name="BB">Параллелепипед расчетной области</param>
        /// <returns>Лист граней расчетной области</returns>
        public List<Vector3[]> DefineDomainEdges(BoundingBox BB)
        {
            Vector3 Point0 = new Vector3(BB.Min.X, BB.Min.Y, BB.Min.Z);
            Vector3 Point1 = new Vector3(BB.Min.X, BB.Min.Y, BB.Max.Z);
            Vector3 Point2 = new Vector3(BB.Min.X, BB.Max.Y, BB.Min.Z);
            Vector3 Point3 = new Vector3(BB.Min.X, BB.Max.Y, BB.Max.Z);
            Vector3 Point4 = new Vector3(BB.Max.X, BB.Min.Y, BB.Min.Z);
            Vector3 Point5 = new Vector3(BB.Max.X, BB.Min.Y, BB.Max.Z);
            Vector3 Point6 = new Vector3(BB.Max.X, BB.Max.Y, BB.Min.Z);
            Vector3 Point7 = new Vector3(BB.Max.X, BB.Max.Y, BB.Max.Z);
            List<Vector3[]> DomainEdges = new List<Vector3[]>
            {
                new Vector3[2] { Point0, Point1 },
                new Vector3[2] { Point1, Point3 },
                new Vector3[2] { Point3, Point2 },
                new Vector3[2] { Point2, Point0 },
                new Vector3[2] { Point0, Point4 },
                new Vector3[2] { Point4, Point6 },
                new Vector3[2] { Point6, Point2 },
                new Vector3[2] { Point6, Point7 },
                new Vector3[2] { Point7, Point5 },
                new Vector3[2] { Point5, Point4 },
                new Vector3[2] { Point5, Point1 },
                new Vector3[2] { Point3, Point7 }
            };
            return DomainEdges;
        }
//---------------------------------------------------------------
        /// <summary>
        /// Получить координаты проекции точки на плоскость
        /// </summary>
        /// <param name="Point">Исходная точка</param>
        /// <param name="Value">Плоскость, на которую проецируются точки</param>
        public OpenTK.Vector3d ProjectedPointCoordinate(OpenTK.Vector3d Point, Plane Value)
        {
            Vector3 T = ((float)(Value.A * Point.X + Value.B * Point.Y + Value.C * Point.Z) + Value.D) / (Value.A * Value.A + Value.B * Value.B + Value.C * Value.C) * Value.Normal;
            return Point - new OpenTK.Vector3d(T.X, T.Y, T.Z);
        }
//---------------------------------------------------------------
        /// <summary>
        /// Получить углы поворота плоскости относительно центра координат
        /// </summary>
        /// <param name="Value">Плоскость</param>
        /// <param name="Position">Центр новой системы коринат</param>
        /// <param name="ReferenceVector">Вектор для определения направления оси X</param>
        /// <returns>Матрица перехода к новому базису</returns>
        public OpenTK.Matrix4d NewBasis(Plane Value, Vector3 Position, Vector3 ReferenceVector)
        {
            //Вектор X задаем как вектор из начала координат нового базиса, параллельный самой длинной стороне плоскости
            Vector3 NewX = ReferenceVector;
            NewX.Normalize();
            //Вектор Y задаем как вектор X, повернутый вокруг Z на 90 градусов
            Matrix Rotation = Matrix.CreateFromAxisAngle(-Value.Normal, TMath.Angle_ToRadians(-90));
            Vector3 NewY = Vector3.Transform(NewX, Matrix.Invert(Rotation));
            NewY.Normalize();
            //Матрица перехода к новому базису
            OpenTK.Matrix4d A = new OpenTK.Matrix4d(NewX.X, NewX.Y, NewX.Z, Position.X, NewY.X, NewY.Y, NewY.Z, Position.Y, -Value.Normal.X, -Value.Normal.Y, -Value.Normal.Z, Position.Z, 0, 0, 0, 1);
            return A;
        }
//---------------------------------------------------------------
        /// <summary>
        /// Определение максимального и минимального значения координат на плоскости в новом базисе
        /// </summary>
        /// <param name="A">Матрица перехода к новому базису</param>
        /// <param name="OldCenter">Центр старой системы координат в новом базисе</param>
        /// <param name="NewBasisPosition">Центр новой системы координат в старом базисе</param>
        /// <returns>Максимальное и минимальное значение параметра</returns>
        public ((Vector3 NewMin, Vector3 NewMax), (Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY)) GetMinMax(Plane Value, OpenTK.Matrix4d A, OpenTK.Vector3d OldCenter, OpenTK.Vector3d NewBasisPosition, List<Vector3[]> DomainEdges)
        {
            float X_Min = float.MaxValue;
            float X_Max = float.MinValue;
            float Y_Min = float.MaxValue;
            float Y_Max = float.MinValue;
            List<Vector3> IntersectionPoints = LineWithPlaneIntersection(Value, DomainEdges);
            if (IntersectionPoints.Count > 0)
            {
                Matrix AMaxtrix = new Matrix((float)A.M11, (float)A.M12, (float)A.M13, (float)A.M14, (float)A.M21, (float)A.M22, (float)A.M23, (float)A.M24, (float)A.M31, (float)A.M32, (float)A.M33, (float)A.M34, (float)A.M41, (float)A.M42, (float)A.M43, (float)A.M44);
                //Переходим к новому базису и находим максимальные координаты X и Y
                for (int i = 0; i < IntersectionPoints.Count; i++)
                {
                    IntersectionPoints[i] = Vector3.Transform(IntersectionPoints[i], Matrix.Invert(AMaxtrix)) + new Vector3((float)OldCenter.X, (float)OldCenter.Y, (float)OldCenter.Z);
                    if (IntersectionPoints[i].X < X_Min) X_Min = IntersectionPoints[i].X;
                    if (IntersectionPoints[i].X > X_Max) X_Max = IntersectionPoints[i].X;
                    if (IntersectionPoints[i].Y < Y_Min) Y_Min = IntersectionPoints[i].Y;
                    if (IntersectionPoints[i].Y > Y_Max) Y_Max = IntersectionPoints[i].Y;
                }
                //Возвращаемся к старому базису
                Vector3 XMaxYMaxOld = Vector3.Transform(new Vector3(X_Max, Y_Max, IntersectionPoints[0].Z), AMaxtrix) + new Vector3((float)NewBasisPosition.X, (float)NewBasisPosition.Y, (float)NewBasisPosition.Z);
                Vector3 XMaxYMinOld = Vector3.Transform(new Vector3(X_Max, Y_Min, IntersectionPoints[0].Z), AMaxtrix) + new Vector3((float)NewBasisPosition.X, (float)NewBasisPosition.Y, (float)NewBasisPosition.Z);
                Vector3 XMinYMaxOld = Vector3.Transform(new Vector3(X_Min, Y_Max, IntersectionPoints[0].Z), AMaxtrix) + new Vector3((float)NewBasisPosition.X, (float)NewBasisPosition.Y, (float)NewBasisPosition.Z);
                Vector3 XMinYMinOld = Vector3.Transform(new Vector3(X_Min, Y_Min, IntersectionPoints[0].Z), AMaxtrix) + new Vector3((float)NewBasisPosition.X, (float)NewBasisPosition.Y, (float)NewBasisPosition.Z);
                return ((new Vector3(X_Min, Y_Min, IntersectionPoints[0].Z), new Vector3(X_Max, Y_Max, IntersectionPoints[0].Z)), (XMinYMinOld, XMaxYMinOld, XMaxYMaxOld, XMinYMaxOld));
            }
            return ((new Vector3(), new Vector3()),(new Vector3(), new Vector3(), new Vector3(), new Vector3()));
            
        }
//---------------------------------------------------------------\
        /// <summary>
        /// Поиск точек пересечения заданной плоскости с отрезками
        /// </summary>
        /// <param name="Value">Плоскость, пересечение с которой проверяем</param>
        /// <param name="Edges">Грани (отрезки), пересечение с которыми определяем</param>
        /// <returns>Лист точек пересечения отрезков с плоскостью</returns>
        public List<Vector3> LineWithPlaneIntersection(Plane Value, List<Vector3[]> Edges)
        {
            List<Vector3> IntersectionPoints = new List<Vector3>();
            foreach (var Edge in Edges)
            {
                if (WhichSide(Value, Edge[0]) == WhichSide(Value, Edge[1])) continue;
                Vector3 Point2 = Edge[1];
                Vector3 CV = new Vector3(Edge[1].X - Edge[0].X, Edge[1].Y - Edge[0].Y, Edge[1].Z - Edge[0].Z);
                double CN = DistanceTo(Value, Edge[0]);
                double CM = CV.X * Value.Normal.X + CV.Y * Value.Normal.Y + CV.Z * Value.Normal.Z;
                if (CM == 0 || Math.Abs(CN) > Math.Abs(CM)) continue;
                float k = (float)(CN / CM);
                if (k < 0)
                {
                    k *= -1;
                    Point2 = Edge[0];
                }
                Vector3 IntersectionPoint = new Vector3(CV.X * k, CV.Y * k, CV.Z * k) + Point2;
                IntersectionPoints.Add(IntersectionPoint);
            }
            return IntersectionPoints;
        }
//---------------------------------------------------------------
        /// <summary>
        /// Определение, с какой строны от плоскости находится точка
        /// </summary
        public int WhichSide(Plane Value, Vector3 Point)
        {
            double num = DistanceTo(Value, Point);
            if (num < 0.0) return -1;
            if (num > 0.0) return 1;
            return 0;
        }
//---------------------------------------------------------------
        /// <summary>
        /// Расстояние от точки до плоскости (с учетом того, с какой стороны от плоскости находится точка (без Math.Abs))
        /// </summary
        public double DistanceTo(Plane Value, Vector3 Point)
        {
            return (Value.A * Point.X + Value.B * Point.Y + Value.C * Point.Z + Value.D) / Math.Sqrt(Math.Pow(Value.A, 2) + Math.Pow(Value.B, 2) + Math.Pow(Value.C, 2));
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получить углы поворота плоскости относительно центра координат
        /// </summary>
        /// <param name="Normal">Нормаль к плоскости</param>
        public Vector3 PlaneRotation(Vector3 Normal)
        {
            float YX = TMath.Angle_ToDegrees((float)Math.Acos(Normal.Z / Normal.Length()));
            float ZY = TMath.Angle_ToDegrees((float)Math.Acos(Normal.X / Normal.Length()));
            float XZ = TMath.Angle_ToDegrees((float)Math.Acos(Normal.Y / Normal.Length()));
            if (YX >= 180) YX -= 180;
            if (ZY >= 180) ZY -= 180;
            if (XZ >= 180) XZ -= 180;
            return new Vector3(YX, ZY, XZ);
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Получить направляющую для вектора X
        /// </summary>
        public Vector3 ReferenceLine(Plane Value, Vector3 AnglesBetweenPlanes)
        {
            Vector3 Point1 = new Vector3();
            Vector3 Point2 = new Vector3();
            //проверяем пересечение заданной  плоскости с плоскостями YX, ZY, XZ
            if (AnglesBetweenPlanes.X == 0) // ||yx
                return new Vector3(1, 0, 0);
            else if (AnglesBetweenPlanes.Y == 0) // ||zy
                return new Vector3(0, -1, 0);
            else if (AnglesBetweenPlanes.Z == 0) // ||xz
                return new Vector3(1, 0, 0);
            //поиск направляющего вектора для рандомной плоскости
            if (AnglesBetweenPlanes.X != 0)
            {
                if (Value.B != 0)
                {
                    float Y0 = -Value.D / Value.B;
                    Point1 = new Vector3(0, Y0, 0);
                    float Y1 = -(Value.A + Value.D) / Value.B;
                    Point2 = new Vector3(1, Y1, 0);
                }
                else
                {
                    float Y0 = -Value.D / Value.C;
                    Point1 = new Vector3(0, 0, Y0);
                    float Y1 = -(Value.A + Value.D) / Value.C;
                    Point2 = new Vector3(1, 0, Y1);
                }
                
            }
            return Point2 - Point1;
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Получить три точки, принадлежащие плоскости
        /// </summary>
        public Vector3[] GetPlanePoints(Plane Value)
        {
            if (Value.A == 0)
            {
                if(Value.B == 0)
                {
                    float Z = -Value.D / Value.C;
                    return new Vector3[] { new Vector3(0, 0, Z), new Vector3(0, 1, Z), new Vector3(1, 0, Z) };
                }
                else
                {
                    if (Value.C == 0)
                    {
                        float Y = -Value.D / Value.B;
                        return new Vector3[] { new Vector3(0, Y, 0), new Vector3(0, Y, 1), new Vector3(1, Y, 0) };
                    }
                    else
                    {
                        float Z = -Value.D / Value.C;
                        float Y = -Value.D / Value.B;
                        return new Vector3[] { new Vector3(0, Y, 0), new Vector3(1, Y, 0), new Vector3(0, 0, Z) };
                    }
                }
            }
            else
            {
                if (Value.B == 0)
                {
                    if (Value.C == 0)
                    {
                        float X = -Value.D / Value.A;
                        return new Vector3[] { new Vector3(X, 0, 0), new Vector3(X, 0, 1), new Vector3(X, 1, 0) };
                    }
                    else
                    {
                        float Z = -Value.D / Value.C;
                        float X = -Value.D / Value.A;
                        return new Vector3[] { new Vector3(X, 0, 0), new Vector3(X, 1, 0), new Vector3(0, 0, Z) };
                    }
                }
                else
                {
                    if (Value.C == 0)
                    {
                        float X = -Value.D / Value.A;
                        float Y = -Value.D / Value.B;
                        return new Vector3[] { new Vector3(X, 0, 0), new Vector3(X, 0, 1), new Vector3(0, Y, 0) };
                    }
                    else
                    {
                        float X = -Value.D / Value.A;
                        float Z = -Value.D / Value.C;
                        float Y = -Value.D / Value.B;
                        return new Vector3[] { new Vector3(X, 0, 0), new Vector3(0, Y, 0), new Vector3(0, 0, Z) };
                    }
                }
            }
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Получить 4 точки, принадлежащие плоскости и расчетной области
        /// </summary>
        public (Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) GetMinMaxPoints(Plane Value, BoundingBox BB)
        {
            //Получаем проекцию центра исходной системы координат на плоскость
            OpenTK.Vector3d NewBasisPosition = ProjectedPointCoordinate(new OpenTK.Vector3d(0, 0, 0), Value);
            //Собираем матрицу перехода к новому базису
            OpenTK.Matrix4d A = NewBasis(Value, new Vector3((float)NewBasisPosition.X, (float)NewBasisPosition.Y, (float)NewBasisPosition.Z), ReferenceLine(Value, PlaneRotation(Value.Normal)));
            //Находим координаты центра старой системы координат в новом базисе
            OpenTK.Vector3d OldCenter = OpenTK.Vector3d.Transform(NewBasisPosition, OpenTK.Matrix4d.Invert(A * (-1)));
            //Получаем координаты углов плоскости для отрисовки
            var MinMaxPoints = GetMinMax(Value, A, OldCenter, NewBasisPosition, DefineDomainEdges(BB));
            return MinMaxPoints.Item2;
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Определить длину стрелки нормали
        /// </summary>
        /// <param name="BB">ВВ, пересечение с которым проверяем</param>
        /// <param name="Normal">Нормаль, пересечение которой проверяем</param>
        /// <returns>Плоскость, парраллельная плоскости Domain с наибольшей площадью</returns>
        public float GetNormalLength(BoundingBox BB, Vector3 Normal)
        {
            // Вершины ВВ
            Vector3 Point0 = new Vector3(BB.Min.X, BB.Min.Y, BB.Min.Z);
            Vector3 Point1 = new Vector3(BB.Min.X, BB.Min.Y, BB.Max.Z);
            Vector3 Point2 = new Vector3(BB.Min.X, BB.Max.Y, BB.Min.Z);
            Vector3 Point3 = new Vector3(BB.Min.X, BB.Max.Y, BB.Max.Z);
            Vector3 Point4 = new Vector3(BB.Max.X, BB.Min.Y, BB.Min.Z);
            Vector3 Point5 = new Vector3(BB.Max.X, BB.Min.Y, BB.Max.Z);
            Vector3 Point6 = new Vector3(BB.Max.X, BB.Max.Y, BB.Min.Z);
            Vector3 Point7 = new Vector3(BB.Max.X, BB.Max.Y, BB.Max.Z);
            // Плоскости ВВ
            Plane[] Planes = new Plane[6]{
                new Plane(Point0, Point1, Point3),
                new Plane(Point0, Point1, Point4),
                new Plane(Point0, Point2, Point4),
                new Plane(Point7, Point3, Point5),
                new Plane(Point7, Point3, Point6),
                new Plane(Point7, Point5, Point6)
            };
            // Центр ВВ
            Vector3 BB_Center = (BB.Max + BB.Min)*0.5f;
            // Проверяем пересечение плоскостей с нормалью
            List<Vector3> IntersectionPoints = new List<Vector3>();
            for (int i = 0; i < Planes.Length; i++)
            {
                float Denominator = Planes[i].A * Normal.X + Planes[i].B * Normal.Y + Planes[i].C * Normal.Z;
                if (Denominator == 0f) continue;
                float t = -(Planes[i].A * BB_Center.X + Planes[i].B * BB_Center.Y + Planes[i].C * BB_Center.Z + Planes[i].D) / Denominator;
                Vector3 Point = new Vector3(Normal.X * t + BB_Center.X, Normal.Y * t + BB_Center.Y, Normal.Z * t + BB_Center.Z);
                if(Point.X>=BB.Min.X && Point.Y >= BB.Min.Y && Point.Z >= BB.Min.Z &&
                   Point.X <= BB.Max.X && Point.Y <= BB.Max.Y && Point.Z <= BB.Max.Z)
                    IntersectionPoints.Add(Point);
            }
            // Определение длины нормали
            float NormalLength = 0f;
            if (IntersectionPoints.Count >= 2)
            {
                foreach(var Point in IntersectionPoints)
                {
                    float Length = (Point - IntersectionPoints[0]).Length();
                    if (NormalLength < Length) NormalLength = Length;
                }
                return NormalLength;
            }
            else
            {
                TJournalLog.WriteLog("C0017: Error TViewerAero:GetNormalLength(): There is no intersections of the Normal with The BoundingBox");
                return NormalLength;
            }
        }

    }
}
