// Вспомогательный класс для TViewerAero, хранящий методы для отображения полей на плоскости
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//
using AstraEngine;
using AstraEngine.Components;
using AstraEngine.Components.MathHelper;
using MPT707.Aerodynamics.Structs;
//***************************************************************
namespace Example
{
    public partial class TViewerAero
    {
        /// <summary>
        /// Вершины расчетной области
        /// </summary>
        internal List<Vector3[]> DomainEdges = new List<Vector3[]>();
        /// <summary>
        /// Параллелепипед расчетной области
        /// </summary>
        internal BoundingBox BB = new BoundingBox();
        //---------------------------------------------------------------
        /// <summary>
        /// Получение поля давления/скорости на плоскости
        /// </summary>
        /// <param name="eTypeValueAero">Интересующая характеристика</param>
        /// <param name="Value">Плоскость</param>
        /// <param name="DisplayResolution">Разрешение отрендеренной текстуры полей</param>
        internal ((Vector3, Vector3, Vector3, Vector3), float, float, float[,]) CalculationsOnPlane(ETypeValueAero eTypeValueAero, Plane Value, /*TFiniteElementModel_Visual FiniteEM,*/ Vector2 DisplayResolution)
        {
            try
            {
                var FiniteEM = FEM_V;
                // Определяем расчетную область
                Display_Domain(FiniteEM);
                // Проверяем, пересекает ли плоскость расчетную область
                List<Vector3> IntersectionPoints = VA_Helper.LineWithPlaneIntersection(Value, DomainEdges);
                if (IntersectionPoints.Count > 0)
                {
                    // Получаем проекцию центра исходной системы координат на плоскость
                    OpenTK.Vector3d NewBasisPosition = VA_Helper.ProjectedPointCoordinate(new OpenTK.Vector3d(0, 0, 0), Value);
                    // Собираем матрицу перехода к новому базису
                    OpenTK.Matrix4d A = VA_Helper.NewBasis(Value, new Vector3((float)NewBasisPosition.X, (float)NewBasisPosition.Y, (float)NewBasisPosition.Z), VA_Helper.ReferenceLine(Value, VA_Helper.PlaneRotation(Value.Normal)));
                    // Находим координаты центра старой системы координат в новом базисе
                    OpenTK.Vector3d OldCenter = OpenTK.Vector3d.Transform(NewBasisPosition, OpenTK.Matrix4d.Invert(A * (-1)));
                    // Получаем координаты углов плоскости для отрисовки
                    var MinMaxPoints = VA_Helper.GetMinMax(Value, A, OldCenter, NewBasisPosition, DomainEdges);
                    // Проекция значений ближайших точек на плоскость
                    var NewBasisPoints = ForDisplayOnPlain(Value, FiniteEM, A, OldCenter, eTypeValueAero);
                    // Интерполяция значений в точках в каждый пиксель текстуры
                    TFemElement_Visual[,] PlanePoints = Coordinate2D(NewBasisPoints, DisplayResolution, MinMaxPoints.Item1, eTypeValueAero);
                    // Получаем максимальные и минимальные значения характеристики
                    float Max = float.MinValue;
                    float Min = float.MaxValue;
                    float[,] PlanePointsCharacteristic = new float[(int)DisplayResolution.X, (int)DisplayResolution.Y];
                    for (int i = 0; i < DisplayResolution.X; i++)
                    {
                        for (int j = 0; j < DisplayResolution.Y; j++)
                        {
                            if (PlanePoints[i, j] == null) continue;

                            switch (eTypeValueAero)
                            {
                                case ETypeValueAero.Pressure:
                                    PlanePointsCharacteristic[i, j] = PlanePoints[i, j].Pressure;
                                    if (Max < PlanePoints[i, j].Pressure) Max = PlanePoints[i, j].Pressure;
                                    if (Min > PlanePoints[i, j].Pressure) Min = PlanePoints[i, j].Pressure;
                                    break;
                                case ETypeValueAero.Velocity:
                                    PlanePointsCharacteristic[i, j] = PlanePoints[i, j].VelocityModule;
                                    if (Max < PlanePoints[i, j].VelocityModule) Max = PlanePoints[i, j].VelocityModule;
                                    if (Min > PlanePoints[i, j].VelocityModule) Min = PlanePoints[i, j].VelocityModule;
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                    }
                    return (MinMaxPoints.Item2, Min, Max, PlanePointsCharacteristic);
                }
                else
                {
                    TJournalLog.WriteLog("There is no intersection of the plane with the domain.");
                    return ((new Vector3(), new Vector3(), new Vector3(), new Vector3()), -1f, -1f, null);
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C00012: Error TViewerAero:DisplayOnPlane(): " + E.Message);
                return ((new Vector3(), new Vector3(), new Vector3(), new Vector3()), -1f, -1f, null);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Определить расчетную область 
        /// </summary>
        /// <param name="FiniteEM">Сетка, содержащая в себе координаты узлов</param>
        protected void Define_Domain(TFiniteElementModel_Visual FiniteEM)
        {
            //Найти максимальные и минимальные точки параллелепипеда расчетной области по каждой из осей
            var MinP = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var MaxP = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < FiniteEM.Elements.Count(); i++)
            {
                if (MinP.X > FiniteEM.Elements[i].Position.X)
                {
                    MinP.X = FiniteEM.Elements[i].Position.X;
                }
                if (MinP.Y > FiniteEM.Elements[i].Position.Y)
                {
                    MinP.Y = FiniteEM.Elements[i].Position.Y;
                }
                if (MinP.Z > FiniteEM.Elements[i].Position.Z)
                {
                    MinP.Z = FiniteEM.Elements[i].Position.Z;
                }

                if (MaxP.X < FiniteEM.Elements[i].Position.X)
                {
                    MaxP.X = FiniteEM.Elements[i].Position.X;
                }
                if (MaxP.Y < FiniteEM.Elements[i].Position.Y)
                {
                    MaxP.Y = FiniteEM.Elements[i].Position.Y;
                }
                if (MaxP.Z < FiniteEM.Elements[i].Position.Z)
                {
                    MaxP.Z = FiniteEM.Elements[i].Position.Z;
                }
            }
            BB = new BoundingBox(MinP, MaxP);
            //Vector3[] PointsPosition2 = new Vector3[] {new Vector3(998.5776f,956.09f,-0.002009224f), new Vector3(-949.4224f, -963.91f, -974.622f) };
            //BB = BoundingBox.CreateFromPoints(PointsPosition2);
            DomainEdges = VA_Helper.DefineDomainEdges(BB);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Ограничить расчетную область (удалить точки, не входящие в определенную пользователем область)
        /// </summary>
        /// <param name="Points">Исходные точки</param>
        public void DomainLimitation(TFiniteElementModel_Visual FiniteEM, (Vector3 Min, Vector3 Max) DomainBoundaries)
        {
            List<TFemElement_Visual> Limited = new List<TFemElement_Visual>();
            for (int i = 0; i < FiniteEM.Elements.Count(); i++)
            {
                if (FiniteEM.Elements[i].Position.X >= DomainBoundaries.Min.X && FiniteEM.Elements[i].Position.Y >= DomainBoundaries.Min.Y &&
                    FiniteEM.Elements[i].Position.Z >= DomainBoundaries.Min.Z && FiniteEM.Elements[i].Position.X <= DomainBoundaries.Max.X &&
                    FiniteEM.Elements[i].Position.Y <= DomainBoundaries.Max.Y && FiniteEM.Elements[i].Position.Z <= DomainBoundaries.Max.Z)
                {
                    Limited.Add(FiniteEM.Elements[i]);
                }
            }
            FiniteEM.Elements = Limited.ToArray();
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получение значений ближайших к плоскости точек
        /// </summary>
        /// <param name="Value">Расчетная плоскость</param>
        /// <param name="A">Матрица перехода к новому базису</param>
        /// <param name="OldCenter">Центр старой системы координат в новом базисе</param>
        private List<TFemElement_Visual> ForDisplayOnPlain(Plane Value, TFiniteElementModel_Visual FiniteEM, OpenTK.Matrix4d A, OpenTK.Vector3d OldCenter, ETypeValueAero eTypeValueAero)
        {
            try
            {
                // Найти максимальный шаг между Points по X,Y,Z
                float Distance = DistanceBetweenAllPoints(FiniteEM);
                Matrix Matrix_A = new Matrix((float)A.M11, (float)A.M12, (float)A.M13, (float)A.M14, (float)A.M21, (float)A.M22, (float)A.M23, (float)A.M24, (float)A.M31, (float)A.M32, (float)A.M33, (float)A.M34, (float)A.M41, (float)A.M42, (float)A.M43, (float)A.M44);
                // Отобрать ближайшие точки
                List<TFemElement_Visual> NewBasisPoints = new List<TFemElement_Visual>();
                for (int i = 0; i < FiniteEM.Elements.Count(); i++)
                {
                    float PointDistance = (float)Math.Abs((Value.A * FiniteEM.Elements[i].Position.X + Value.B * FiniteEM.Elements[i].Position.Y + Value.C * FiniteEM.Elements[i].Position.Z + Value.D) / Math.Sqrt(Math.Pow(Value.A, 2) + Math.Pow(Value.B, 2) + Math.Pow(Value.C, 2)));
                    //Если расстояние от точки до плоскости не удовлетворяет выбранному расстоянию, то переходим к следующей точке
                    if (PointDistance > Distance) continue;
                    // Если есть вершины ячеек - делаем проверку
                    if (FiniteEM.Elements[i].Nodes.Length > 3)
                        if (!VerticesByDifferentSides(Value, FiniteEM, i)) continue;
                    TFemElement_Visual NewBasisPoint = new TFemElement_Visual();
                    //NewBasisPoint.ID_Element = FiniteEM.Elements[i].ID_Element;
                    //NewBasisPoint.Faces = FiniteEM.Elements[i].Faces;
                    //NewBasisPoint.Position = FiniteEM.Elements[i].Position;
                    switch (eTypeValueAero)
                    {
                        case ETypeValueAero.Pressure:
                            NewBasisPoint.Pressure = FiniteEM.Elements[i].Pressure;
                            break;
                        case ETypeValueAero.Velocity:
                            NewBasisPoint.VelocityModule = FiniteEM.Elements[i].VelocityModule;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    NewBasisPoint.Position = Vector3.Transform(FiniteEM.Elements[i].Position, Matrix.Invert(Matrix_A)) + new Vector3((float)OldCenter.X, (float)OldCenter.Y, (float)OldCenter.Z);
                    NewBasisPoints.Add(NewBasisPoint);
                }
                return NewBasisPoints;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0007: Error TViewerAero:ForDisplayOnPlain(): " + E.Message);
                return null;
            }
        }
        //---------------------------------------------------------------
        private bool VerticesByDifferentSides(Plane Value, TFiniteElementModel_Visual FiniteEM, int i)
        {
            //var coordinates = new List<Vector3>();
            //var Element = FiniteEM.Elements.Find(x => x.ID_Element == (long)i);
            //for (int j = 0; j < Element.ID_Nodes.Count(); j++)
            //{
            //    var Node = FiniteEM.Nodes.Find(x => x.ID_Node == Element.ID_Nodes[j]);
            //    coordinates.Add(new Vector3(
            //        (float)Node.X,
            //        (float)Node.Y,
            //        (float)Node.Z
            //        ));
            //}

            //var RefernceValue = VA_Helper.WhichSide(Value, coordinates[0]);
            //if (RefernceValue == 0) return true;
            //for (int j = 1; j < coordinates.Count(); j++)
            //{
            //    if (RefernceValue != VA_Helper.WhichSide(Value, coordinates[j]))
            //    {
            //        return true;
            //    }
            //}
            //return false;

            // Задаем референсное значение с которым будем сравнивать
            var RefernceValue = VA_Helper.WhichSide(
                Value,
                new Vector3(
                    FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[0]].X,
                    FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[0]].Y,
                    FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[0]].Z
                ));
            // Проверяем не лежит ли референсное значение на самой плоскости
            if (RefernceValue == 0) return false;
            // Проверяем по какую сторону плоскости относительно референсного значения находятся остальные точки
            for (int j = 0; j < FiniteEM.Elements[i].Nodes.Length; j++)
            {
                int result = VA_Helper.WhichSide(Value,
                    new Vector3(
                FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j]].X,
                FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j]].Y,
                FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j]].Z));

                if (result == 0) return false;
                if (RefernceValue != result)
                {
                    return true;
                }
            }
            return false;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Определение максимального шага между точками по 3-м координатам
        /// </summary>
        /// <param name="Points">Исходные точки</param>
        protected float DistanceBetweenAllPoints(TFiniteElementModel_Visual FiniteEM)
        {
            List<Vector3> PointsPosition = new List<Vector3>();
            foreach (var Point in FiniteEM.Elements) PointsPosition.Add(new Vector3((float)Point.Position.X, (float)Point.Position.Y, (float)Point.Position.Z));
            List<float> X = (from Coordinate in PointsPosition select Coordinate.X).Distinct().ToList();
            X.Sort();
            List<float> Y = (from Coordinate in PointsPosition select Coordinate.Y).Distinct().ToList();
            Y.Sort();
            List<float> Z = (from Coordinate in PointsPosition select Coordinate.Z).Distinct().ToList();
            Z.Sort();
            Task<float> Distance_X = new Task<float>(() => MaxDistance(X));
            Task<float> Distance_Y = new Task<float>(() => MaxDistance(Y));
            Task<float> Distance_Z = new Task<float>(() => MaxDistance(Z));
            Distance_X.Start();
            Distance_Y.Start();
            Distance_Z.Start();
            return Math.Max(Distance_X.Result, Math.Max(Distance_Y.Result, Distance_Z.Result)) / 2f;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Определение максимального расстояния между точками по одной кординате
        /// </summary>
        protected float MaxDistance(List<float> Coordinate)
        {
            float MaxDistanceCoordinate = 0;
            for (int i = 0; i < Coordinate.Count - 1; i++)
            {
                float DistanceCoordinate = Math.Abs(Coordinate[i] - Coordinate[i + 1]);
                if (MaxDistanceCoordinate < DistanceCoordinate) MaxDistanceCoordinate = DistanceCoordinate;
            }
            return MaxDistanceCoordinate;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получить координаты точки на плоскости
        /// </summary>
        private TFemElement_Visual[,] Coordinate2D(List<TFemElement_Visual>NewBasisPoints, Vector2 Resolution, (Vector3 NewBasisMin, Vector3 NewBasisMax) NewBasisBB, ETypeValueAero eTypeValueAero)
        {
            // Перевод точек в массив, равный указанному разрешению
            var PreparedForInterpolationPoints = PrepareForInterpolationParallel(NewBasisPoints, Resolution, NewBasisBB, eTypeValueAero);
            // С помощью такого списка можно задать приращение для Parallel.For 
            IEnumerable<int> StepIterator(int startIndex, int endIndex, int stepSize)
            {
                for (int i = startIndex; i < endIndex; i += stepSize)
                {
                    yield return i;
                }
            }
            // Определяем размер блоков, рассматриваемых в каждом потоке
            int BlockSizeI = (int)Math.Ceiling((double)((int)Resolution.X / (Environment.ProcessorCount - 1)));
            int BlockSizeJ = (int)Math.Ceiling((double)((int)Resolution.Y / 8));
            // Параллельный перебор блоков пикселей Nх8, пропуская каждую нечетную строку (j = 1, 3, 5 ...)
            Parallel.ForEach(StepIterator(0, (int)Resolution.X, BlockSizeI), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 }, I =>
            {
                for (int J = 0; J < (int)Resolution.Y; J += BlockSizeJ)
                {
                    // Перебор всех пикселей внутри одного блока
                    for (int i = I; i < Math.Min(I + BlockSizeI, (int)Resolution.X); i++)
                    {
                        for (int j = J; j < Math.Min(J + BlockSizeJ, (int)Resolution.Y); j += 2)
                        {
                            // Пропускаем, если значение в точке есть
                            if (PreparedForInterpolationPoints[i, j] != null) 
                                continue;
                            // Ищем 4 точки
                            Vector2[] PixelPoints = GetClosestPoint(PreparedForInterpolationPoints, i, j, Resolution);
                            // Если хоть одна из 4-х точек не найдена - пропускаем
                            if (PixelPoints.Contains(new Vector2(-1, -1))) continue;
                            // Получаем от одного до двух треугольников из четырехугольника
                            int[][] Triangles = SeparateIntoTriangles(PixelPoints);
                            // Поиск всех точек, принадлежащих четырехугольнику
                            int MinI = (int)Math.Min(PixelPoints[0].X, Math.Min(PixelPoints[1].X, Math.Min(PixelPoints[2].X, PixelPoints[3].X)));
                            int MinJ = (int)Math.Min(PixelPoints[0].Y, Math.Min(PixelPoints[1].Y, Math.Min(PixelPoints[2].Y, PixelPoints[3].Y)));
                            int MaxI = (int)Math.Max(PixelPoints[0].X, Math.Max(PixelPoints[1].X, Math.Max(PixelPoints[2].X, PixelPoints[3].X)));
                            int MaxJ = (int)Math.Max(PixelPoints[0].Y, Math.Max(PixelPoints[1].Y, Math.Max(PixelPoints[2].Y, PixelPoints[3].Y)));
                            for (int x = MinI; x <= MaxI; x++)
                            {
                                for (int y = MinJ; y <= MaxJ; y++)
                                {
                                    // Пропускаем, если значение в точке есть
                                    if (PreparedForInterpolationPoints[x, y] != null) continue;
                                    // Проверяем принадлежность точки четырехугольнику
                                    foreach (var Triangle in Triangles)
                                    {
                                        if (!PointBelongsToTriangle(PixelPoints[Triangle[0]], PixelPoints[Triangle[1]], PixelPoints[Triangle[2]], new Vector2(x, y))) continue;
                                        // Находим значения в найденных точках
                                        PreparedForInterpolationPoints[x, y] = Interpolation(x, y, PixelPoints/*new Vector2[]{ PixelPoints[Triangle[0]], PixelPoints[Triangle[1]], PixelPoints[Triangle[2]]}*/, PreparedForInterpolationPoints, eTypeValueAero);
                                    }
                                }
                            }
                        }
                    }
                }
            });
            // Параллельный перебор каждой пропущенной строки
            Parallel.For(0, (int)Resolution.X, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 }, i =>
            {
                for (int j = 1; j < (int)Resolution.Y; j += 2)
                {
                    // Пропускаем, если значение в точке есть
                    if (PreparedForInterpolationPoints[i, j] != null) continue;
                    // Усредняем значение пикселя, ориентируясь на пиксель сверху и снизу от рассматриваемого
                    if (j - 1 < 0 || j + 1 >= (int)Resolution.Y) continue;
                    if (PreparedForInterpolationPoints[i, j - 1] == null || PreparedForInterpolationPoints[i, j + 1] == null) continue;
                    PreparedForInterpolationPoints[i, j] = new TFemElement_Visual();
                    switch (eTypeValueAero)
                    {
                        case ETypeValueAero.Pressure:
                            PreparedForInterpolationPoints[i, j].Pressure = (PreparedForInterpolationPoints[i, j - 1].Pressure + PreparedForInterpolationPoints[i, j + 1].Pressure) / 2;
                            break;
                        case ETypeValueAero.Velocity:
                            PreparedForInterpolationPoints[i, j].VelocityModule = (PreparedForInterpolationPoints[i, j - 1].VelocityModule + PreparedForInterpolationPoints[i, j + 1].VelocityModule) / 2;
                            break;
                        default:
                            throw new NotImplementedException();
                    }  
                }
            });
            // Непараллельное закрашивание оставшихся точек методом ближайшего соседа
           PreparedForInterpolationPoints = NearestNeighbor(PreparedForInterpolationPoints, eTypeValueAero);

            // Интерполяция недостающих точек
            /*Parallel.For(0, (int)Resolution.X, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 }, i =>
            {
                for (int j = 0; j < (int)Resolution.Y; j++)
                {
                    // Пропускаем, если значение в точке есть
                    if (PreparedForInterpolationPoints[i, j] != null) continue;
                    // Ищем 4 точки
                    List<(float X, float Y)> PixelPoints = GetClosestPoint(PreparedForInterpolationPoints, i, j, Resolution);
                    // Если хоть одна из 4-х точек не найдена - пропускаем
                    if (PixelPoints.Count<4) continue;
                    // Находим промежуточные значения
                    PreparedForInterpolationPoints[i, j] = Interpolation(i, j, PixelPoints, PreparedForInterpolationPoints);
                }
            });*/
            return PreparedForInterpolationPoints;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получить координаты точки на плоскости
        /// </summary>
        private TFemElement_Visual Interpolation(int i, int j, Vector2[] PixelPoints, TFemElement_Visual[,] PreparedForInterpolationPoints, ETypeValueAero eTypeValueAero)
        {
            // Находим промежуточные значения
            (int Y, float Pressure, float VelocityModule) PixelPoint10 = (0, 0, 0);
            (int Y, float Pressure, float VelocityModule) PixelPoint12 = (0, 0, 0);
            PixelPoint10.Y = (int)TMath.LINO(PixelPoints[0].X, PixelPoints[0].Y, PixelPoints[1].X, PixelPoints[1].Y, i);
            PixelPoint12.Y = (int)TMath.LINO(PixelPoints[2].X, PixelPoints[2].Y, PixelPoints[3].X, PixelPoints[3].Y, i);
            
            TFemElement_Visual PixelPoint = new TFemElement_Visual();
            switch (eTypeValueAero)
            {
                case ETypeValueAero.Pressure:
                    PixelPoint10.Pressure = TMath.LINO(PixelPoints[0].X, PreparedForInterpolationPoints[(int)PixelPoints[0].X, (int)PixelPoints[0].Y].Pressure, PixelPoints[1].X, PreparedForInterpolationPoints[(int)PixelPoints[1].X, (int)PixelPoints[1].Y].Pressure, i);
                    PixelPoint12.Pressure = TMath.LINO(PixelPoints[2].X, PreparedForInterpolationPoints[(int)PixelPoints[2].X, (int)PixelPoints[2].Y].Pressure, PixelPoints[3].X, PreparedForInterpolationPoints[(int)PixelPoints[3].X, (int)PixelPoints[3].Y].Pressure, i);
                    PixelPoint.Pressure = TMath.LINO(PixelPoint10.Y, PixelPoint10.Pressure, PixelPoint12.Y, PixelPoint12.Pressure, j);
                    break;
                case ETypeValueAero.Velocity:
                    PixelPoint10.VelocityModule = TMath.LINO(PixelPoints[0].X, PreparedForInterpolationPoints[(int)PixelPoints[0].X, (int)PixelPoints[0].Y].VelocityModule, PixelPoints[1].X, PreparedForInterpolationPoints[(int)PixelPoints[1].X, (int)PixelPoints[1].Y].VelocityModule, i);
                    PixelPoint12.VelocityModule = TMath.LINO(PixelPoints[2].X, PreparedForInterpolationPoints[(int)PixelPoints[2].X, (int)PixelPoints[2].Y].VelocityModule, PixelPoints[3].X, PreparedForInterpolationPoints[(int)PixelPoints[3].X, (int)PixelPoints[3].Y].VelocityModule, i);
                    PixelPoint.VelocityModule = TMath.LINO(PixelPoint10.Y, PixelPoint10.VelocityModule, PixelPoint12.Y, PixelPoint12.VelocityModule, j);
                    break;
                default:
                    break;
            }
            //float d = (PixelPoints[1].Y- PixelPoints[2].Y)*(PixelPoints[0].X - PixelPoints[2].X) + (PixelPoints[2].X - PixelPoints[1].X)*(PixelPoints[0].Y - PixelPoints[2].Y);
            //float t1 = 1 / d * ((PixelPoints[1].Y - PixelPoints[2].Y)*(i- PixelPoints[2].X) + (PixelPoints[2].X - PixelPoints[1].X) * (j - PixelPoints[2].Y));
            //float t2 = 1 / d * ((PixelPoints[2].Y - PixelPoints[0].Y) * (i - PixelPoints[2].X) + (PixelPoints[0].X - PixelPoints[2].X) * (j - PixelPoints[2].Y));
            //float t3 = 1 - t1 - t2;
            //switch (eTypeValueAero)
            //{
            //    case ETypeValueAero.Pressure:
            //        PixelPoint.Pressure = t1* PreparedForInterpolationPoints[(int)PixelPoints[0].X, (int)PixelPoints[0].Y].Pressure + t2 * PreparedForInterpolationPoints[(int)PixelPoints[1].X, (int)PixelPoints[1].Y].Pressure + t3 * PreparedForInterpolationPoints[(int)PixelPoints[2].X, (int)PixelPoints[2].Y].Pressure;
            //        break;
            //    case ETypeValueAero.Velocity:
            //        PixelPoint.VelocityModule = t1 * PreparedForInterpolationPoints[(int)PixelPoints[0].X, (int)PixelPoints[0].Y].VelocityModule + t2 * PreparedForInterpolationPoints[(int)PixelPoints[1].X, (int)PixelPoints[1].Y].VelocityModule + t3 * PreparedForInterpolationPoints[(int)PixelPoints[2].X, (int)PixelPoints[2].Y].VelocityModule;
            //        break;
            //    default:
            //        throw new NotImplementedException();
            //}
            return PixelPoint;
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Подготовка точек к интерполяции
        /// </summary>
        /// <param name="Points">Точки с координатами в новом базисе</param>
        /// <returns>Двумерный массив точек, отсортированный по координатам X и Y
        private TFemElement_Visual[,] PrepareForInterpolationParallel(List<TFemElement_Visual> Points, Vector2 Resolution, (Vector3 NewBasisMin, Vector3 NewBasisMax) NewBasisBB, ETypeValueAero eTypeValueAero)
        {
            double minX;/*double.MaxValue;*/
            double minY;/*double.MaxValue;*/
            double maxX;/*double.MinValue;*/
            double maxY;/*double.MinValue;*/

            // Отбор минимальных и максимальных значений координат
            if(NewBasisBB.NewBasisMin.X < NewBasisBB.NewBasisMax.X)
            {
                minX = NewBasisBB.NewBasisMin.X;
                maxX = NewBasisBB.NewBasisMax.X;
            }
            else
            {
                minX = NewBasisBB.NewBasisMax.X;
                maxX = NewBasisBB.NewBasisMin.X;
            }

            if (NewBasisBB.NewBasisMin.Y < NewBasisBB.NewBasisMax.Y)
            {
                minY = NewBasisBB.NewBasisMin.Y;
                maxY = NewBasisBB.NewBasisMax.Y;
            }
            else
            {
                minY = NewBasisBB.NewBasisMax.Y;
                maxY = NewBasisBB.NewBasisMin.Y;
            }

            // Отбор минимальных и максимальных значений координат
            //double minX = Points1.Item2.Min.X;/*double.MaxValue;*/
            //double minY = Points1.Item2.Min.Y;/*double.MaxValue;*/
            //double maxX = Points1.Item2.Max.X;/*double.MinValue;*/
            //double maxY = Points1.Item2.Max.Y;/*double.MinValue;*/
            //for (int i = 0; i < Points.Count; i++)
            //{
            //    if (Points[i].Position.X < minX) minX = Points[i].Position.X;
            //    if (Points[i].Position.X > maxX) maxX = Points[i].Position.X;
            //    if (Points[i].Position.Y < minY) minY = Points[i].Position.Y;
            //    if (Points[i].Position.Y > maxY) maxY = Points[i].Position.Y;
            //}

            // Определение шага сетки "пикселей"
            double stepX = TMath.Abs((maxX - minX) / Resolution.X);
            double stepY = TMath.Abs((maxY - minY) / Resolution.Y);

            // Отбор по две точки на пиксель
            (TFemElement_Visual maxNegative, TFemElement_Visual minPositive)[,] preparedForProjectionPoints = new (TFemElement_Visual maxNegative, TFemElement_Visual minPositive)[(int)Resolution.X, (int)Resolution.Y];
            for (int i = 0; i < Points.Count; i++)
            {
                // Определение коорднаты пикселя
                int x = (int)TMath.Floor((Points[i].Position.X - minX) / stepX);
                int y = (int)TMath.Floor((Points[i].Position.Y - minY) / stepY);

                // Поправка вышедших за границы пикселей
                if (x < 0)
                    x = 0;
                else if (x > (int)Resolution.X - 1)
                    x = (int)Resolution.X - 1;
                if (y < 0)
                    y = 0;
                else if (y > (int)Resolution.Y - 1)
                    y = (int)Resolution.Y - 1;
                // 
                if (Points[i].Position.Z >= 0)
                {
                    if (preparedForProjectionPoints[x, y].minPositive == null)
                        preparedForProjectionPoints[x, y].minPositive = Points[i];
                    else if (preparedForProjectionPoints[x, y].minPositive.Position.Z > Points[i].Position.Z)
                        preparedForProjectionPoints[x, y].minPositive = Points[i];
                }
                else
                {
                    if (preparedForProjectionPoints[x, y].maxNegative == null)
                        preparedForProjectionPoints[x, y].maxNegative = Points[i];
                    else if (preparedForProjectionPoints[x, y].maxNegative.Position.Z < Points[i].Position.Z)
                        preparedForProjectionPoints[x, y].maxNegative = Points[i];
                }
            }

            // Проецируем точки интерполяцией и "экстрополяцией"
            TFemElement_Visual[,] projectedOnPlanePoints = new TFemElement_Visual[(int)Resolution.X, (int)Resolution.Y];
            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };
            Parallel.For(0, (int)Resolution.X, parallelOptions, x =>
            {
                for (int y = 0; y < (int)Resolution.Y; y++)
                {
                    // "Экстраполяция"
                    if (preparedForProjectionPoints[x, y].minPositive == null && preparedForProjectionPoints[x, y].maxNegative == null)
                        continue;
                    else if (preparedForProjectionPoints[x, y].minPositive == null)
                    {
                        projectedOnPlanePoints[x, y] = preparedForProjectionPoints[x, y].maxNegative;
                        continue;
                    }
                    else if (preparedForProjectionPoints[x, y].maxNegative == null)
                    {
                        projectedOnPlanePoints[x, y] = preparedForProjectionPoints[x, y].minPositive;
                        continue;
                    }

                    // Интерполяция
                    double distance = preparedForProjectionPoints[x, y].minPositive.Position.Z - preparedForProjectionPoints[x, y].maxNegative.Position.Z;
                    double minPositiveK = (distance - preparedForProjectionPoints[x, y].minPositive.Position.Z) / distance;
                    double maxNegativeK = 1d - minPositiveK;
                    projectedOnPlanePoints[x, y] = new TFemElement_Visual();
                    switch (eTypeValueAero)
                    {
                        case ETypeValueAero.Pressure:
                            projectedOnPlanePoints[x, y].Pressure = preparedForProjectionPoints[x, y].maxNegative.Pressure * (float)maxNegativeK + preparedForProjectionPoints[x, y].minPositive.Pressure * (float)minPositiveK;
                            break;
                        case ETypeValueAero.Velocity:
                            projectedOnPlanePoints[x, y].VelocityModule = preparedForProjectionPoints[x, y].maxNegative.VelocityModule * (float)maxNegativeK + preparedForProjectionPoints[x, y].minPositive.VelocityModule * (float)minPositiveK;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            });
            return projectedOnPlanePoints;
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Нахождение ближайшей точки из списка к указаным координатам
        /// </summary>
        /// <param name="PreparedPoints">Список точек</param>
        /// <param name="X">Указанная координата Х</param>
        /// <param name="Y">Указанная координата Y</param>
        /// <returns></returns>
        protected Vector2[] GetClosestPoint(TFemElement_Visual[,] PreparedPoints, int X, int Y, Vector2 Resolution)
        {
            Vector2[] PixelPoints = new Vector2[4];
            // Ищем точку с меньшими X и Y
            List<Vector2> PrevX_PrevY_Points = new List<Vector2>();
            int PrevX_PrevY_CountOfPoints = 1;
            float SearchLimitK = 0.0625f;
            int SearchLimit;
            if (Resolution.X > Resolution.Y)
                SearchLimit = (int)Math.Ceiling(Resolution.X * SearchLimitK);
            else SearchLimit = (int)Math.Ceiling(Resolution.Y * SearchLimitK);
            // Счетчик строк или столбцов (в зависимости от размера)
            for (int RowСounter = 0; RowСounter <= Math.Min(Math.Max(X, Y), SearchLimit); RowСounter++)
            {
                int PCounterindex = 1;
                if (RowСounter > X - 1)
                    PCounterindex = (PrevX_PrevY_CountOfPoints + 1) / 2;
                // Счетчик точек, которые требуется рассмотреть за проход
                for (int PCounter = PCounterindex; PCounter <= PrevX_PrevY_CountOfPoints; PCounter++)
                {
                    // Если счетчик не вышел за максимум, то продолжаем плюсовать по столбцам, иначе - по строкам
                    if (PCounter <= RowСounter)
                    {
                        // Если индекс искомой точки выходит за границы, то прекратить осмотр
                        if (X - RowСounter >= 0 && Y - PCounter >= 0)
                        {
                            if (PreparedPoints[X - RowСounter, Y - PCounter] != null)
                                PrevX_PrevY_Points.Add(new Vector2(X - RowСounter, Y - PCounter));
                        }
                        else continue;
                    }
                    else
                    {
                        if (X - (PrevX_PrevY_CountOfPoints + 1) + PCounter >= 0 && Y - RowСounter >= 0)
                        {
                            if (PreparedPoints[X - (PrevX_PrevY_CountOfPoints + 1) + PCounter, Y - RowСounter] != null)
                                PrevX_PrevY_Points.Add(new Vector2(X - (PrevX_PrevY_CountOfPoints + 1) + PCounter, Y - RowСounter));
                        }
                        else continue;
                    }
                }
                // Если нашлись точки за проход - прекращаем поиск
                if (PrevX_PrevY_Points.Count > 0)
                    break;
                PrevX_PrevY_CountOfPoints += 2;
            }
            // Выбор ближайшей точки среди найденых
            if (PrevX_PrevY_Points.Count <= 0)
            {
                PixelPoints[0] = new Vector2(-1, -1);
                return PixelPoints;
            }
            Vector2 PrevX_PrevY_Point = NearestPoint(PrevX_PrevY_Points, X, Y, Resolution);
            PixelPoints[0] = PrevX_PrevY_Point;

            // Ищем точку с большим X и меньшим Y
            List<Vector2> NextX_PrevY_Points = new List<Vector2>();
            int NextX_PrevY_CountOfPoints = 1;
            // Счетчик строк или столбцов (в зависимости от размера)
            for (int RowСounter = 0; RowСounter <= Math.Min(Math.Max((int)Resolution.X - X, Y), SearchLimit); RowСounter++)
            {
                int PCounterindex = 1;
                if (RowСounter > Resolution.X - X - 1)
                    PCounterindex = (NextX_PrevY_CountOfPoints + 1) / 2;
                // Счетчик точек, которые требуется рассмотреть за проход
                for (int PCounter = PCounterindex; PCounter <= NextX_PrevY_CountOfPoints; PCounter++)
                {
                    // Если счетчик не вышел за максимум, то продолжаем плюсовать по столбцам, иначе - по строкам
                    if (PCounter <= RowСounter)
                    {
                        // Если индекс искомой точки выходит за границы, то прекратить осмотр
                        if (X + RowСounter < Resolution.X && Y - PCounter >= 0)
                        {
                            if (PreparedPoints[X + RowСounter, Y - PCounter] != null)
                                NextX_PrevY_Points.Add(new Vector2(X + RowСounter, Y - PCounter));
                        }
                        else continue;
                    }
                    else
                    {
                        if (X + NextX_PrevY_CountOfPoints - PCounter < Resolution.X && Y - RowСounter >= 0)
                        {
                            if (PreparedPoints[X + NextX_PrevY_CountOfPoints - PCounter, Y - RowСounter] != null)
                                NextX_PrevY_Points.Add(new Vector2(X + NextX_PrevY_CountOfPoints - PCounter, Y - RowСounter));
                        }
                        else continue;
                    }
                }
                // Если нашлись точки за проход - прекращаем поиск
                if (NextX_PrevY_Points.Count > 0) break;
                NextX_PrevY_CountOfPoints += 2;
            }
            // Выбор ближайшей точки среди найденых
            if (NextX_PrevY_Points.Count <= 0)
            {
                PixelPoints[1] = new Vector2(-1, -1);
                return PixelPoints;
            }
            Vector2 NextX_PrevY_Point = NearestPoint(NextX_PrevY_Points, X, Y, Resolution);
            PixelPoints[1] = NextX_PrevY_Point;

            // Ищем точку с большими X и Y
            List<Vector2> NextX_NextY_Points = new List<Vector2>();
            int NextX_NextY_CountOfPoints = 1;
            // Счетчик строк или столбцов (в зависимости от размера)
            for (int RowСounter = 0; RowСounter <= Math.Min(Math.Max((int)Resolution.X - X, (int)Resolution.Y - Y), SearchLimit); RowСounter++)
            {
                int PCounterindex = 1;
                if (RowСounter > Resolution.X - X - 1)
                    PCounterindex = (NextX_NextY_CountOfPoints + 1) / 2;
                // Счетчик точек, которые требуется рассмотреть за проход
                for (int PCounter = PCounterindex; PCounter <= NextX_NextY_CountOfPoints; PCounter++)
                {
                    // Если счетчик не вышел за максимум, то продолжаем плюсовать по столбцам, иначе - по строкам
                    if (PCounter <= RowСounter)
                    {
                        // Если индекс искомой точки выходит за границы, то прекратить осмотр
                        if (X + RowСounter < Resolution.X && Y + PCounter < Resolution.Y)
                        {
                            if (PreparedPoints[X + RowСounter, Y + PCounter] != null)
                                NextX_NextY_Points.Add(new Vector2(X + RowСounter, Y + PCounter));
                        }
                        else continue;
                    }
                    else
                    {
                        if (X + NextX_NextY_CountOfPoints - PCounter < Resolution.X && Y + RowСounter < Resolution.Y)
                        {
                            if (PreparedPoints[X + NextX_NextY_CountOfPoints - PCounter, Y + RowСounter] != null)
                                NextX_NextY_Points.Add(new Vector2(X + NextX_NextY_CountOfPoints - PCounter, Y + RowСounter));
                        }
                        else continue;
                    }
                }
                // Если нашлись точки за проход - прекращаем поиск
                if (NextX_NextY_Points.Count > 0) break;
                NextX_NextY_CountOfPoints += 2;
            }
            // Выбор ближайшей точки среди найденых
            if (NextX_NextY_Points.Count <= 0)
            {
                PixelPoints[2] = new Vector2(-1, -1);
                return PixelPoints;
            }
            Vector2 NextX_NextY_Point = NearestPoint(NextX_NextY_Points, X, Y, Resolution);
            PixelPoints[2] = NextX_NextY_Point;

            // Ищем точку с меньшим X и большим Y
            List<Vector2> PrevX_NextY_Points = new List<Vector2>();
            int PrevX_NextY_CountOfPoints = 1;
            // Счетчик строк или столбцов (в зависимости от размера)
            for (int RowСounter = 0; RowСounter <= Math.Min(Math.Max(X, (int)Resolution.Y - Y), SearchLimit); RowСounter++)
            {
                int PCounterindex = 1;
                if (RowСounter > X - 1)
                    PCounterindex = (PrevX_NextY_CountOfPoints + 1) / 2;
                // Счетчик точек, которые требуется рассмотреть за проход
                for (int PCounter = PCounterindex; PCounter <= PrevX_NextY_CountOfPoints; PCounter++)
                {
                    // Если счетчик не вышел за максимум, то продолжаем плюсовать по столбцам, иначе - по строкам
                    if (PCounter <= RowСounter)
                    {
                        // Если индекс искомой точки выходит за границы, то прекратить осмотр
                        if (X - RowСounter >= 0 && Y + PCounter < Resolution.Y)
                        {
                            if (PreparedPoints[X - RowСounter, Y + PCounter] != null)
                                PrevX_NextY_Points.Add(new Vector2(X - RowСounter, Y + PCounter));
                        }
                        else continue;
                    }
                    else
                    {
                        if (X - (PrevX_NextY_CountOfPoints + 1) + PCounter >= 0 && Y + RowСounter < Resolution.Y)
                        {
                            if (PreparedPoints[X - (PrevX_NextY_CountOfPoints + 1) + PCounter, Y + RowСounter] != null)
                                PrevX_NextY_Points.Add(new Vector2(X - (PrevX_NextY_CountOfPoints + 1) + PCounter, Y + RowСounter));
                        }
                        else continue;
                    }
                }
                // Если нашлись точки за проход - прекращаем поиск
                if (PrevX_NextY_Points.Count > 0) break;
                PrevX_NextY_CountOfPoints += 2;
            }
            // Выбор ближайшей точки среди найденых
            if (PrevX_NextY_Points.Count <= 0)
            {
                PixelPoints[3] = new Vector2(-1, -1);
                return PixelPoints;
            }
            Vector2 PrevX_NextY_Point = NearestPoint(PrevX_NextY_Points, X, Y, Resolution);
            PixelPoints[3] = PrevX_NextY_Point;

            return PixelPoints;
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Выбор ближайшей точки среди найденых
        /// </summary>
        /// <param name="NearestPoins">Отобранные ближайшие точки</param>
        /// <param name="X">Координата X точки, к которой ищем ближайшую</param>
        /// <param name="Y">Координата Y точки, к которой ищем ближайшую</param>
        protected Vector2 NearestPoint(List<Vector2> NearestPoins, int X, int Y, Vector2 Resolution)
        {
            Vector2 NearestPoint = Vector2.Zero;
            double Distance = Math.Sqrt(Math.Pow(Resolution.X, 2) + Math.Pow(Resolution.Y, 2));
            foreach (var Point in NearestPoins)
            {
                double Lenght = Math.Sqrt(Math.Pow(Point.X - X, 2) + Math.Pow(Point.Y - Y, 2));
                if (Distance > Lenght)
                {
                    Distance = Lenght;
                    NearestPoint = Point;
                }
            }
            return NearestPoint;
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Сортировка списка векторов по X и Y
        /// </summary>
        protected int Comparison(TFEMFluidFrame Point1, TFEMFluidFrame Point2)
        {
            int result = Point1.Position.X.CompareTo(Point2.Position.X);
            if (result == 0)
                result = Point1.Position.Y.CompareTo(Point2.Position.Y);
            return result;
        }
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Определение высоты, ширины, позиции плоскости и шкалы
        /// </summary>
        protected (int, int, Vector3, Vector3) WidthHeightCenterScaleCenterParameter(OpenTK.Matrix4d A, OpenTK.Vector3d NewBasisPosition, (float, float, float, float, float) XminXmaxYminYmaxZNewBasis)
        {
            (int Width, int Height, Vector3 PlaneCenter, Vector3) WidthHeightCenterScaleCenter = (0, 0, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            Matrix AMaxtrix = new Matrix((float)A.M11, (float)A.M12, (float)A.M13, (float)A.M14, (float)A.M21, (float)A.M22, (float)A.M23, (float)A.M24, (float)A.M31, (float)A.M32, (float)A.M33, (float)A.M34, (float)A.M41, (float)A.M42, (float)A.M43, (float)A.M44);
            Vector3 XMaxYMaxOld = Vector3.Transform(new Vector3(XminXmaxYminYmaxZNewBasis.Item2, XminXmaxYminYmaxZNewBasis.Item4, XminXmaxYminYmaxZNewBasis.Item5), AMaxtrix) + new Vector3((float)NewBasisPosition.X, (float)NewBasisPosition.Y, (float)NewBasisPosition.Z);
            Vector3 XMinYMinOld = Vector3.Transform(new Vector3(XminXmaxYminYmaxZNewBasis.Item1, XminXmaxYminYmaxZNewBasis.Item3, XminXmaxYminYmaxZNewBasis.Item5), AMaxtrix) + new Vector3((float)NewBasisPosition.X, (float)NewBasisPosition.Y, (float)NewBasisPosition.Z);
            Vector3 PlaneCenter = new Vector3(XMaxYMaxOld.X - XMinYMinOld.X, XMaxYMaxOld.Y - XMinYMinOld.Y, XMaxYMaxOld.Z - XMinYMinOld.Z) * 0.5f + XMinYMinOld;
            WidthHeightCenterScaleCenter.Item1 = (int)(XminXmaxYminYmaxZNewBasis.Item2 - XminXmaxYminYmaxZNewBasis.Item1);
            WidthHeightCenterScaleCenter.Item2 = (int)(XminXmaxYminYmaxZNewBasis.Item4 - XminXmaxYminYmaxZNewBasis.Item3);
            WidthHeightCenterScaleCenter.Item3 = PlaneCenter;
            if (WidthHeightCenterScaleCenter.Item1 >= WidthHeightCenterScaleCenter.Item2)
                WidthHeightCenterScaleCenter.Item4 = Vector3.Transform(new Vector3(XminXmaxYminYmaxZNewBasis.Item1, 0, XminXmaxYminYmaxZNewBasis.Item5 - 90), AMaxtrix) + new Vector3((float)NewBasisPosition.X, (float)NewBasisPosition.Y, (float)NewBasisPosition.Z);
            else WidthHeightCenterScaleCenter.Item4 = Vector3.Transform(new Vector3(0, XminXmaxYminYmaxZNewBasis.Item4, XminXmaxYminYmaxZNewBasis.Item5 - 90), AMaxtrix) + new Vector3((float)NewBasisPosition.X, (float)NewBasisPosition.Y, (float)NewBasisPosition.Z);
            return WidthHeightCenterScaleCenter;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получение угла между векторами в градусах
        /// </summary>
        /// <param name="Vec1"></param>
        /// <param name="Vec2"></param>
        /// <returns>Угл в градусах</returns>
        protected float AngleDegree(Vector3 Vec1, Vector3 Vec2)
        {
            // Проверить, находится ли значения длин векторов в допустимых пределах
            if (Vec1.X == 0d && Vec1.Y == 0d && Vec1.Z == 0d)
            {
                return 0f;
            }
            else if (Vec2.X == 0d && Vec2.Y == 0d && Vec2.Z == 0d)
            {
                return 0f;
            }
            Vec1.Normalize();
            Vec2.Normalize();
            //
            var CosAlpha = (Vec1.X * Vec2.X + Vec1.Y * Vec2.Y + Vec1.Z * Vec2.Z);
            if (CosAlpha > 1)
            {
                CosAlpha = 1;
            }
            else if (CosAlpha < -1)
            {
                CosAlpha = -1;
            }
            // Вернуть значение угла переведенное в градусы       
            return (float)(Math.Acos(CosAlpha) * 180.0d / Math.PI);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Закрашивание пустых ячеек методом ближайшего соседа
        /// </summary>
        /// <param name="Points"></param>
        /// <returns></returns>
        private TFemElement_Visual[,] NearestNeighbor(TFemElement_Visual[,] Points, ETypeValueAero eTypeValueAero)
        {
            //Задаются строчки и столбцы в массиве
            int rows = Points.GetUpperBound(0) + 1;
            int columns = Points.Length / rows;

            //Переменные для цикла по поиску закрашенных точек
            int MinI;
            int MaxI;
            int MinJ;
            int MaxJ;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (Points[i, j] != null)
                    {
                        continue;
                    }
                    else
                    {
                        MinI = i;
                        MaxI = i;
                        MinJ = j;
                        MaxJ = j;

                        //Переменная для отслеживания, чтобы цикл не зацикливался
                        int a = 0;

                        while (true)
                        {
                            MinI--;
                            MaxI++;
                            MinJ--;
                            MaxJ++;

                            if (MinI < 0) MinI = 0;
                            if (MinJ < 0) MinJ = 0;
                            if (MaxI > rows - 1) MaxI = rows - 1;
                            if (MaxJ > columns - 1) MaxJ = columns - 1;

                            //Отслеживание зацикливания
                            if (MinI == 0 && MinJ == 0 && MaxI == rows - 1 && MaxJ == columns - 1)
                            {
                                a++;
                                if (a > 1) return Points;
                            }
                            var Index = Search(Points, MinI, MaxI, MinJ, MaxJ);
                            if (Index.Item1 != -1)
                            {
                                Points[i, j] = new TFemElement_Visual();
                                switch (eTypeValueAero)
                                {
                                    case ETypeValueAero.Pressure:
                                        Points[i, j].Pressure = Points[Index.Item1, Index.Item2].Pressure;
                                        break;
                                    case ETypeValueAero.Velocity:
                                        Points[i, j].VelocityModule = Points[Index.Item1, Index.Item2].VelocityModule;
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                } 
                                break;
                            }
                        }
                    }
                }
            }

            return Points;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Цикл по поиску закрашенной ближайшей точки
        /// </summary>
        /// <param name="Points"></param>
        /// <param name="MinI"></param>
        /// <param name="MaxI"></param>
        /// <param name="MinJ"></param>
        /// <param name="MaxJ"></param>
        /// <returns></returns>
        protected (int, int) Search(TFemElement_Visual[,] Points, int MinI, int MaxI, int MinJ, int MaxJ)
        {

            for (int i = MinI; i <= MaxI; i++)
            {
                for (int j = MinJ; j <= MaxJ; j++)
                {
                    if (Points[i, j] != null)
                    {
                        return (i, j);
                    }
                }
            }

            return (-1, -1);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Определить, как разделить четырехугольник на два теругольника без ошибок в вогнутостями
        /// </summary>
        /// <param name="points">Координаты пикселей</param>
        /// <returns>Зубчатый массив индексов описывающий 2 треугольника</returns>
        private int[][] SeparateIntoTriangles(Vector2[] points)
        {
            // Тангенсы в данном методе расчитываются так, что с каждым переходом от точки к точке (против часовой стрелки) мы "поворачиваем" четырехугольник на 90 градусов по часовой стрелке)
            // Пример:
            // I = 1 (рассматриваемая точка)
            // => мы рассматриваем точки: 0, 1, 2
            // при данном I четырехугольник сейчас считается не повернутым (Ось Y' = Y, X' = X, где ' - измененные оси)
            // I = 2 (надо будет повернуть на 90 по часовой)
            // => рассматриваемые точки: 1, 2, 3
            // в результате поворота получаем: Y' = -X, X' = Y
            // и так далее


            /*    Y ^     I = 1           I = 2
             *      |   3 +----+ 2      0 +----+ 3
             *      |     |    |          |    |          -> и так далее
             *      |   0 +----+ 1      1 +----+ 2
             *      |
             *      +------------>
             *                   X
             */
            // Массив тангенсов для диагоналей четырехугольника (0-2, 1-3)
            var tg1 = new float[4];

            var delta = points[1] - points[3];
            tg1[0] = -delta.X / delta.Y;

            delta = points[2] - points[0];
            tg1[1] = delta.Y / delta.X;

            tg1[2] = tg1[0];

            tg1[3] = tg1[1];

            // Массив тангенсов сторон четырехугольника (0-1, 1-2, 2-3, 3-0)
            var tg2 = new float[4];

            delta = points[0] - points[3];
            tg2[0] = -delta.X / delta.Y;

            delta = points[1] - points[0];
            tg2[1] = delta.Y / delta.X;

            delta = points[2] - points[1];
            tg2[2] = -delta.X / delta.Y;

            delta = points[3] - points[2];
            tg2[3] = delta.Y / delta.X;

            var OUT = new int[2][];
            // Сравниваем тангенсы, и если тангенс стороны окажется меньше тангенса диагонали - нашли вогнутость
            for (int i = 0; i < 4; i++)
            {
                if (tg1[i] < tg2[i])
                {
                    // Берем те индексы, которые минуют получения лишнего объема
                    OUT[0] = GetThreeVertices(i, 4).ToArray();
                    OUT[1] = GetThreeVertices(OUT[0][2], 4).ToArray();
                    return OUT;
                }
            }

            // В четырехугольнике не найдена вогнутость и берем любые индексы
            OUT[0] = GetThreeVertices(0, 4).ToArray();
            OUT[1] = GetThreeVertices(OUT[0][2], 4).ToArray();
            return OUT;
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить индексы вершин рассматриваемого треугольника 
        /// </summary>
        /// <param name="i">Индекс шага в цикле</param>
        /// <returns></returns>
        private List<int> GetThreeVertices(int i, int VerticesCount)
        {
            // Создаем список в который будем записывать инексы
            List<int> Ids = new List<int>();
            // Если третья вершина рассматриваемого треугольника залезает на начало(проходит до конца и возвращается в начало)
            if (i == VerticesCount - 2)
            {
                Ids.Add(i);
                Ids.Add(i + 1);
                Ids.Add(0);
            }
            // Если вторая и третья вершины рассматриваемого треугольника залезают на начало(проходит до конца и возвращается в начало)
            else if (i == VerticesCount - 1)
            {
                Ids.Add(i);
                Ids.Add(0);
                Ids.Add(1);
            }
            // Если не требуется перенос индексов в начало
            else
            {
                Ids.Add(i);
                Ids.Add(i + 1);
                Ids.Add(i + 2);
            }
            return Ids;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Определение принадлежности точки треугольнику
        /// </summary>
        /// <param name="P0">Вершина треугольника</param>
        /// <param name="P1">Вершина треугольника</param>
        /// <param name="P2">Вершина треугольника</param>
        /// <param name="TestPoint">Точка, которую проверяем</param>
        /// <returns>true, если принадлежит, иначе - false</returns>
        protected bool PointBelongsToTriangle(Vector2 P0, Vector2 P1, Vector2 P2, Vector2 TestPoint)
        {
            float a = (P0.X - TestPoint.X) * (P1.Y - P0.Y) - (P1.X - P0.X) * (P0.Y - TestPoint.Y);
            float b = (P1.X - TestPoint.X) * (P2.Y - P1.Y) - (P2.X - P1.X) * (P1.Y - TestPoint.Y);
            float c = (P2.X - TestPoint.X) * (P0.Y - P2.Y) - (P0.X - P2.X) * (P2.Y - TestPoint.Y);
            if ((a >= 0 && b >= 0 && c >= 0) || (a <= 0 && b <= 0 && c <= 0))
                return true;
            else
                return false;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
