//Класс, содержащий методы для отрисовки линий тока
using System;
using System.Collections.Generic;
using System.Linq;
//
using AstraEngine;
using AstraEngine.Components;
using AstraEngine.Engine.GraphicCore;
using AstraEngine.Geometry.Collide;
//***************************************************************
namespace Example
{
    public partial class TViewerAero
    {
        /// <summary>
        /// Для более удобного пользования вся сетка была разделена на ячейки, Количество ячеек по длине, ширине, высоте
        /// </summary>
        private const int Number = 15;
        /// <summary>
        /// Число нужное для задачи индексов в методах
        /// </summary>
        private int Number2 = Number - 1;
        /// <summary>
        /// Это расстояние между точками в линии тока, VES был задан рандомно
        /// </summary>
        private float VESConst;
        /// <summary>
        /// Динамический VES 
        /// </summary>
        private float VESDinamic;
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Создание большой ячейки, для поиска необходимых точек для маленькой ячейки
        /// </summary>
        /// <param name="BasePoint">Базовая точка</param>
        /// <param name="LengthCellX">Длина по X ячейки</param>
        /// <param name="WidthCellZ">Длина по Z ячейки</param>
        /// <param name="HeightCellY">Длина по Y ячейки</param>
        /// <returns>Массив из точек большой ячейки</returns>
        protected TFemElement_Visual[] CreateBigCell (TFemElement_Visual BasePoint, float LengthCellX, float WidthCellZ, float HeightCellY)
        {
            try
            {
                // Лист с нужными точками
                List<TFemElement_Visual> NeedPoint = new List<TFemElement_Visual>();
                // Поиск точек
                for (int i = 0; i < FEM_V.Elements.Length; i++)
                {
                    // Если разница между координатами по модулю меньше, чем половина длины ячейки, то точка считается нужной
                    if (Math.Abs(BasePoint.Position.X - FEM_V.Elements[i].Position.X) < LengthCellX / 2)
                    {
                        if (Math.Abs(BasePoint.Position.Y - FEM_V.Elements[i].Position.Y) < HeightCellY / 2)
                        {
                            if (Math.Abs(BasePoint.Position.Z - FEM_V.Elements[i].Position.Z) < WidthCellZ / 2)
                            {
                                NeedPoint.Add(FEM_V.Elements[i]);
                            }
                        }
                    }
                }
                TFemElement_Visual[] PointArray = NeedPoint.ToArray();
                return PointArray;
            }
            catch(Exception E)
            {
                TJournalLog.WriteLog("C0007: Error TViewerAero_CurrentLines:CreateBigCell(): " + E.Message);
                return (null);
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Пока что пробный метод создания линии тока, почти отработан
        /// </summary>
        /// <param name="BasePoint">Базовая точка</param>
        /// <returns>Список точек, принадлежащих линии тока</returns>
        protected TFemElement_Visual[] CreateCurrentLine (TFemElement_Visual BasePoint, TViewerAero_CurrentLinesSettings CS)
        {
            try
            { 
                //(float MinVes, float MaxCount) = MinVESandMaxCount(R);
                //if (MinVes < 0) AutoStep = false;
                // Значение максимального моделя скорости
                (float MaxVelocityModel, float MinVelocityModel) = MaxMinVelocity();
                //var Triangles = CreateTTriangleContainerFromModels();
                // Лист из точек, принадлежащих линии тока
                List<TFemElement_Visual> PointsInCurrentLines = new List<TFemElement_Visual>();
                // Координаты максимального и минимального значений на сетке
                (Vector3 Max, Vector3 Min)= MaxMinCoordinate(FEM_V.Elements);
                // Флаг для проверки осталась ли следующая точка в большой ячейке
                bool flag = true;
                // Максимальные и минимальные значения большой ячейки
                Vector3 MaxBC = new Vector3();
                Vector3 MinBC = new Vector3();
                // Расчет длин большой ячейки
                float LengthCellX = (Max.X - Min.X) / (Number);
                float WidthCellZ =(Max.Z - Min.Z) / (Number);
                float HeightCellY =(Max.Y - Min.Y) / (Number);

                float Volume = Math.Min(LengthCellX, (Math.Min(WidthCellZ, HeightCellY)));
                // Расчет VES
                if (CS.AutoStep==false)
                {
                    VESConst = (Math.Max(LengthCellX, (Math.Max(WidthCellZ, HeightCellY)))) / 100f;
                }
                else
                {
                    VESConst = CS.Step;
                }
                // Проверка лежит ли базовая точка внутри сетки
                if (LeavingTheLineOutsideTheGrid(Max, Min, BasePoint.Position) == true) return new TFemElement_Visual[0];

                //Координаты максимального и минимального значений модели
                //var MaxMinModeL = MaxMinModel(Triangles[0]);
                //Vector3 MaxModel = MaxMinModeL.Max;
                //Vector3 MinModel = MaxMinModeL.Min;
                //if (CheckingPointInModel(MinModel, MaxModel, BasePoint.Position, Triangles[0]) == true) return new TFemElement_Visual[0];

                // Создание большой ячейки, где находится базовая точка
                var CellArray = CreateBigCell(BasePoint, LengthCellX, WidthCellZ, HeightCellY);
                // Создание маленькой ячейки, где находится базовая точка
                var CellAndBool = FindingCellsBasePoint(CellArray, BasePoint.Position);
                TFemElement_Visual[] Cell = CellAndBool.Item1;
                bool Test = true;
                //Если не все значения нашлись в большой ячейке, то они находятся дополнительно в увеличенной ячейке
                if (CellAndBool.Item2 == false)
                {
                    var BigArray= CreateBigCell(BasePoint, LengthCellX*2, WidthCellZ*2, HeightCellY*2);
                    var Res = FindingCellsBasePoint(BigArray, BasePoint.Position);
                    Cell = Res.Item1;
                    Test = Res.Item2;
                }
                //Если в ячейке есть не заполененные данные, это может быть если базовая точка выходит за пределы сетки, то построение линии не начинается
                if (Test == false) return new TFemElement_Visual[0];
                //Находит вектор скорости в базовой точке
                BasePoint.Velocity = FindingVectorVelocityPointInCell(Cell, BasePoint.Position);
                BasePoint.VelocityModule = BasePoint.Velocity.Length();
                // Есть ли в векторе скорости нерациональные числа
                if (CheckNan(BasePoint.Velocity)) return new TFemElement_Visual[0];
                // Если модуль скорости полученный, больше, чем максимальный в сетке, значит, что-то пошло не так при построении
                if (BasePoint.VelocityModule > MaxVelocityModel || BasePoint.VelocityModule<MinVelocityModel) return new TFemElement_Visual[0];
                PointsInCurrentLines.Add(BasePoint);
            
                //Цикл, в котором находятся точки для одной линии
                while (true)
                {
                    TFemElement_Visual NextPoint = new TFemElement_Visual();
                    //Следующая точка на векторе скорости предыдущей точки
                    NextPoint.Position = PointInVectorBasePoint(PointsInCurrentLines[PointsInCurrentLines.Count - 1], VESConst);
                    //Если точка выходит за пределы сетки, то цикл прекращается
                    if (LeavingTheLineOutsideTheGrid(Max, Min, NextPoint.Position) == true) break;
                    else
                    {
                        //Если точка находится в старой маленькой ячейке, то строим вектор в ней же
                        if (NewPointInOldCell(Cell, NextPoint) == true)
                        {
                            NextPoint.Velocity = FindingVectorVelocityPointInCell(Cell, NextPoint.Position);
                            NextPoint.VelocityModule = NextPoint.Velocity.Length();
                            // Есть ли в векторе скорости нерациональные числа
                            if (CheckNan(NextPoint.Velocity)) break;
                            // Если модуль скорости полученный, больше, чем максимальный в сетке, значит, что-то пошло не так при построении
                            if (NextPoint.VelocityModule>MaxVelocityModel) return new TFemElement_Visual[0];
                            PointsInCurrentLines.Add(NextPoint);
                        }
                        //Если нет, то ищем новую маленькую ячейку и строим вектор уже из неё
                        else
                        {
                            // Если флаг, значит, была создана новая большая ячейка, значит, нужно заново искать максимальные и минимальные координаты для неё
                            if (flag)
                            {
                                (MaxBC, MinBC) = MaxMinCoordinate(CellArray);
                            }
                            // Если точка находится в диапазоне старой большой ячейки, значит, создавать новую не нужно
                            if (MaxBC.X > NextPoint.Position.X && MaxBC.Y > NextPoint.Position.Y && MaxBC.Z > NextPoint.Position.Z && MinBC.X < NextPoint.Position.X && MinBC.Y < NextPoint.Position.Y && MinBC.Z < NextPoint.Position.Z)
                            {
                                flag = false;
                            }
                            else
                            {
                                CellArray = CreateBigCell(NextPoint, LengthCellX, WidthCellZ, HeightCellY);
                                flag = true;
                            }

                            var CellAndBoolNew = FindingCellsBasePoint(CellArray, NextPoint.Position);
                            TFemElement_Visual[] CellNew = CellAndBoolNew.Item1;
                            bool TestNew = true;
                            //Если не все значения нашлись в маленькой ячейке, то они находятся дополнительно в других больших ячейках
                            if (CellAndBoolNew.Item2 == false)
                            {
                                var BigArrayNew = CreateBigCell(NextPoint, LengthCellX * 2, WidthCellZ * 2, HeightCellY * 2);
                                var ResNew = FindingCellsBasePoint(BigArrayNew, NextPoint.Position);
                                CellNew = ResNew.Item1;
                                TestNew = ResNew.Item2;
                            }
                            //Если не все значения нашлись, то построение линии тока заканчивается
                            if (TestNew == false)
                            {
                                break;
                            }

                            //Находятся вектор и модель скорости
                            NextPoint.Velocity = FindingVectorVelocityPointInCell(CellNew, NextPoint.Position);
                            NextPoint.VelocityModule = NextPoint.Velocity.Length();
                            // Есть ли в векторе скорости нерациональные числа
                            if (CheckNan(NextPoint.Velocity)) break;
                            // Если модуль скорости полученный, больше, чем максимальный в сетке, значит, что-то пошло не так при построении, эту линию не выводим
                            if (NextPoint.VelocityModule > MaxVelocityModel || NextPoint.VelocityModule < MinVelocityModel) return new TFemElement_Visual[0];
                            PointsInCurrentLines.Add(NextPoint);

                            //Проверка зашла ли линия тока в модель
                            //(Contains, Flag) = CheckingPointInModel(MinModel, MaxModel, NextPoint.Position);
                            //if (Contains) break;
                            //if (CheckingPointInModel(MinModel, MaxModel, NextPoint.Position, Triangles[0]) == true)
                            //{ break; }
                            Cell = CellNew;
                        }

                    }

                }

                TFemElement_Visual[] PointsInCurrentLinesArray = new TFemElement_Visual[PointsInCurrentLines.Count];
                PointsInCurrentLinesArray = PointsInCurrentLines.ToArray();
                return PointsInCurrentLinesArray;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0007: Error TViewerAero_CurrentLines:CreateCurrentLine(): " + E.Message);
                return (null);
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Поиск максимального и минимального модуля скорости
        /// </summary>
        /// <returns></returns>
        protected (float Max, float Min) MaxMinVelocity ()
        {
            float Max = 0f;
            float Min = float.MaxValue;
            for (int i=0; i<FEM_V.Elements.Length; i++)
            {
                if (FEM_V.Elements[i].VelocityModule > Max)
                {
                    Max = FEM_V.Elements[i].VelocityModule;
                }
                if (FEM_V.Elements[i].VelocityModule < Min)
                {
                    Min= FEM_V.Elements[i].VelocityModule;
                }
            }
            return (Max, Min);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Нахождение ячейки, где находится базовая точка
        /// </summary>
        /// <param name="Points">Исходные точки на сетке</param>
        /// <param name="BasePoint">Точка, для которой требуется найти ячейку</param>
        /// <returns>Массив, в котором в определенном порядке располагаются точки в ячейке, проверка нашлись ли все точки в ячейке</returns>
        protected (TFemElement_Visual[], bool) FindingCellsBasePoint(TFemElement_Visual[] Points, Vector3 BasePoint)
        {
            TFemElement_Visual[] Cell = new TFemElement_Visual[8];

            //Цикл проходит по всем точкам и заносит подходящие в массив, если расстояние от предыдущей точки до базовой точки, меньше чем у точки, которая уже добавлена в массив, то заменяем её
            for (int i = 0; i < Points.Length; i++)
            {
                //0 point
                if (Points[i].Position.X <= BasePoint.X && Points[i].Position.Y <= BasePoint.Y && Points[i].Position.Z <= BasePoint.Z)
                {
                    if (Cell[0] == null)
                    {
                        Cell[0] = Points[i];
                    }
                    else
                    {
                        if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(Cell[0], BasePoint))
                        {
                            Cell[0] = Points[i];
                        }
                    }
                }

                //1 point
                if (Points[i].Position.X >= BasePoint.X && Points[i].Position.Y <= BasePoint.Y && Points[i].Position.Z <= BasePoint.Z)
                {
                    if (Cell[1] == null)
                    {
                        Cell[1] = Points[i];
                    }
                    else
                    {
                        if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(Cell[1], BasePoint))
                        {
                            Cell[1] = Points[i];
                        }
                    }
                }

                //2 point
                if (Points[i].Position.X >= BasePoint.X && Points[i].Position.Y <= BasePoint.Y && Points[i].Position.Z >= BasePoint.Z)
                {
                    if (Cell[2] == null)
                    {
                        Cell[2] = Points[i];
                    }
                    else
                    {
                        if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(Cell[2], BasePoint))
                        {
                            Cell[2] = Points[i];
                        }
                    }
                }

                //3 point
                if (Points[i].Position.X <= BasePoint.X && Points[i].Position.Y <= BasePoint.Y && Points[i].Position.Z >= BasePoint.Z)
                {
                    if (Cell[3] == null)
                    {
                        Cell[3] = Points[i];
                    }
                    else
                    {
                        if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(Cell[3], BasePoint))
                        {
                            Cell[3] = Points[i];
                        }
                    }
                }

                //4 point
                if (Points[i].Position.X <= BasePoint.X && Points[i].Position.Y >= BasePoint.Y && Points[i].Position.Z <= BasePoint.Z)
                {
                    if (Cell[4] == null)
                    {
                        Cell[4] = Points[i];
                    }
                    else
                    {
                        if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(Cell[4], BasePoint))
                        {
                            Cell[4] = Points[i];
                        }
                    }
                }

                //5 point
                if (Points[i].Position.X >= BasePoint.X && Points[i].Position.Y >= BasePoint.Y && Points[i].Position.Z <= BasePoint.Z)
                {
                    if (Cell[5] == null)
                    {
                        Cell[5] = Points[i];
                    }
                    else
                    {
                        if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(Cell[5], BasePoint))
                        {
                            Cell[5] = Points[i];
                        }
                    }
                }

                //6 point
                if (Points[i].Position.X >= BasePoint.X && Points[i].Position.Y >= BasePoint.Y && Points[i].Position.Z >= BasePoint.Z)
                {
                    if (Cell[6] == null)
                    {
                        Cell[6] = Points[i];
                    }
                    else
                    {
                        if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(Cell[6], BasePoint))
                        {
                            Cell[6] = Points[i];
                        }
                    }
                }

                //7 point
                if (Points[i].Position.X <= BasePoint.X && Points[i].Position.Y >= BasePoint.Y && Points[i].Position.Z >= BasePoint.Z)
                {
                    if (Cell[7] == null)
                    {
                        Cell[7] = Points[i];
                    }
                    else
                    {
                        if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(Cell[7], BasePoint))
                        {
                            Cell[7] = Points[i];
                        }
                    }
                }

            }

            // Проверка на заполннность всего массива
            for (int i = 0; i < 8; i++)
            {
                if (Cell[i] == null)
                {
                    return (Cell, false);
                }
            }
            return (Cell, true);
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Проверка лежит ли новая точка в старой ячейке
        /// </summary>
        /// <param name="Cell">Старая ячейка</param>
        /// <param name="BasePoint">Точка, которую нужно проверить</param>
        /// <returns>true - лежит, false - не лежит </returns>
        protected bool NewPointInOldCell(TFemElement_Visual[] Cell, TFemElement_Visual BasePoint)
        {
            int a = 0;
            if (Cell[0].Position.X <= BasePoint.Position.X && Cell[0].Position.Y <= BasePoint.Position.Y && Cell[0].Position.Z <= BasePoint.Position.Z) a++;
            if (Cell[1].Position.X >= BasePoint.Position.X && Cell[1].Position.Y <= BasePoint.Position.Y && Cell[1].Position.Z <= BasePoint.Position.Z) a++;
            if (Cell[2].Position.X >= BasePoint.Position.X && Cell[2].Position.Y <= BasePoint.Position.Y && Cell[2].Position.Z >= BasePoint.Position.Z) a++;
            if (Cell[3].Position.X <= BasePoint.Position.X && Cell[3].Position.Y <= BasePoint.Position.Y && Cell[3].Position.Z >= BasePoint.Position.Z) a++;
            if (Cell[4].Position.X <= BasePoint.Position.X && Cell[4].Position.Y >= BasePoint.Position.Y && Cell[4].Position.Z <= BasePoint.Position.Z) a++;
            if (Cell[5].Position.X >= BasePoint.Position.X && Cell[5].Position.Y >= BasePoint.Position.Y && Cell[5].Position.Z <= BasePoint.Position.Z) a++;
            if (Cell[6].Position.X >= BasePoint.Position.X && Cell[6].Position.Y >= BasePoint.Position.Y && Cell[6].Position.Z >= BasePoint.Position.Z) a++;
            if (Cell[7].Position.X <= BasePoint.Position.X && Cell[7].Position.Y >= BasePoint.Position.Y && Cell[7].Position.Z >= BasePoint.Position.Z) a++;

            if (a == 8) return true;
            else return false;
        }
//-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Находит точку на векторе направления линии тока
        /// </summary>
        /// <param name="StartPoint">Точка, на векторе которой нужно найти нследующую точку</param>
        /// <param name="VES">Расстояние, через которое строится точка</param>
        /// <returns>Координаты новой точки</returns>
        protected Vector3 PointInVectorBasePoint(TFemElement_Visual StartPoint, float VES)
        {
            Vector3 Direction =StartPoint.Velocity;
            Direction.Normalize();
            Vector3 FinishPoint = StartPoint.Position + Direction * VES;
            return FinishPoint;
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Расстояние между точками
        /// </summary>
        /// <param name="PointsInGrid">Точка в сетке</param>
        /// <param name="BasePoint">Точка, для которой требуется расстояние</param>
        /// <returns>Значение расстояния</returns>
        protected float DistanceBetweenPoints(TFemElement_Visual PointsInGrid, Vector3 BasePoint)
        {
            float Distance =(float) Math.Sqrt((PointsInGrid.Position.X - BasePoint.X) * (PointsInGrid.Position.X - BasePoint.X) + (PointsInGrid.Position.Y - BasePoint.Y) * (PointsInGrid.Position.Y - BasePoint.Y) + (PointsInGrid.Position.Z - BasePoint.Z) * (PointsInGrid.Position.Z - BasePoint.Z));
            return Distance;
        }
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Находит вектор направления линии тока
        /// </summary>
        /// <param name="Cell">Ячейка, в которой располагается точка</param>
        /// <param name="BasePoint">Точка, для которой находится вектор направления</param>
        /// <returns>Вектор направления</returns>
        protected Vector3 FindingVectorVelocityPointInCell(TFemElement_Visual[] Cell, Vector3 BasePoint)
        {
            Vector3 B = new Vector3(Cell[0].Position.X, BasePoint.Y, BasePoint.Z);
            Vector3 C = new Vector3(Cell[1].Position.X, BasePoint.Y, BasePoint.Z);
            Vector3 BB1 = new Vector3(B.X, B.Y, Cell[3].Position.Z);
            Vector3 BB2 = new Vector3(B.X, B.Y, Cell[0].Position.Z);
            Vector3 CC1 = new Vector3(C.X, C.Y, Cell[2].Position.Z);
            Vector3 CC2 = new Vector3(C.X, C.Y, Cell[1].Position.Z);

            float tB1 =(float) Math.Abs(Vector3.Dot(Cell[7].Position, BB1)) / Math.Abs(Vector3.Dot(Cell[3].Position, BB1));
            float tB2 = (float)Math.Abs(Vector3.Dot(Cell[4].Position, BB2)) / Math.Abs(Vector3.Dot(Cell[0].Position, BB2));
            float tC1 = (float)Math.Abs(Vector3.Dot(Cell[6].Position, CC1)) / Math.Abs(Vector3.Dot(Cell[2].Position, CC1));
            float tC2 = (float)Math.Abs(Vector3.Dot(Cell[5].Position, CC2)) / Math.Abs(Vector3.Dot(Cell[1].Position, CC2));

            float UB1 = (float)tB1 * Cell[7].Velocity.X + (1 - tB1) * Cell[3].Velocity.X;
            float VB1 = (float)tB1 * Cell[7].Velocity.Y + (1 - tB1) * Cell[3].Velocity.Y;
            float WB1 = (float)tB1 * Cell[7].Velocity.Z + (1 - tB1) * Cell[3].Velocity.Z;

            float UB2 = (float)tB1 * Cell[4].Velocity.X + (1 - tB2) * Cell[0].Velocity.X;
            float VB2 = (float)tB1 * Cell[4].Velocity.Y + (1 - tB2) * Cell[0].Velocity.Y;
            float WB2 = (float)tB1 * Cell[4].Velocity.Z + (1 - tB2) * Cell[0].Velocity.Z;

            double UC1 = (float)tC1 * Cell[6].Velocity.X + (1 - tC1) * Cell[2].Velocity.X;
            double VC1 = (float)tC1 * Cell[6].Velocity.Y + (1 - tC1) * Cell[2].Velocity.Y;
            double WC1 = (float)tC1 * Cell[6].Velocity.Z + (1 - tC1) * Cell[2].Velocity.Z;

            float UC2 = (float)tC1 * Cell[5].Velocity.X + (1 - tC2) * Cell[1].Velocity.X;
            float VC2 = (float)tC1 * Cell[5].Velocity.Y + (1 - tC2) * Cell[1].Velocity.Y;
            float WC2 = (float)tC1 * Cell[5].Velocity.Z + (1 - tC2) * Cell[1].Velocity.Z;

            float pB = (float)Math.Abs(Vector3.Dot(B, BB1)) / Math.Abs(Vector3.Dot(B, BB2));
            float pC = (float)Math.Abs(Vector3.Dot(C, CC1)) / Math.Abs(Vector3.Dot(C, CC2));

            float UB = (float)(pB * UB1 + (1 - pB) * UB2);
            float VB = (float)(pB * VB1 + (1 - pB) * VB2);
            float WB = (float)(pB * WB1 + (1 - pB) * WB2);

            float UC = (float)(pC * UC1 + (1 - pC) * UC2);
            float VC = (float)(pC * VC1 + (1 - pC) * VC2);
            float WC = (float)(pC * WC1 + (1 - pC) * WC2);

            float p = (float)Math.Abs(Vector3.Dot(BasePoint, B)) / Math.Abs(Vector3.Dot(BasePoint, C));

            float U = (float)(p * UB + (1 - p) * UC);
            float V = (float)(p * VB + (1 - p) * VC);
            float W = (float)(p * WB + (1 - p) * WC);

            Vector3 VelocityPoint = new Vector3(U, V, W);

            return VelocityPoint;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Проверяет, имеет ли вектор не рациональные числа
        /// </summary>
        /// <param name="Vec"></param>
        /// <returns></returns>
        protected bool CheckNan (Vector3 Vec)
        {
            if (float.IsNaN(Vec.X) == true || float.IsNaN(Vec.Y) == true || float.IsNaN(Vec.Z) == true)
            {
                return true;
            }
            else return false;
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Переводит список векторов 3 в список точек  TFemElement_Visual
        /// </summary>
        /// <param name="Points">Список точек, которые нужно перевести в массив TFemElement_Visual</param>
        /// <returns>Массив с точками в формате TFemElement_Visual</returns>
        protected TFemElement_Visual[] TransferVector3BasePoint(List<Vector3> Points)
        {
            TFemElement_Visual[] NewPoints = new TFemElement_Visual[Points.Count];
            for (int i = 0; i < Points.Count; i++)
            {
                NewPoints[i] = new TFemElement_Visual();
                Vector3 Vector = new Vector3(Points[i].X, Points[i].Y, Points[i].Z);
                NewPoints[i].Position = Vector;
            }

            return NewPoints;
        }
        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает минимальные и макисмальные значения координат, первый макс, второй мин
        /// </summary>
        /// <param name="Points">ТОчки в сетке</param>
        /// <returns>Макисмальное, минимальное</returns>
        protected (Vector3 Max, Vector3 Min) MaxMinCoordinate(TFemElement_Visual[] Points)
        {
            try
            {
                float MinX = Points[0].Position.X;
                float MinY = Points[0].Position.Y;
                float MinZ = Points[0].Position.Z;
                float MaxX = Points[0].Position.X;
                float MaxY = Points[0].Position.Y;
                float MaxZ = Points[0].Position.Z;
                for (int i = 1; i < Points.Length; i++)
                {
                    if (Points[i].Position.X < MinX) MinX = Points[i].Position.X;
                    if (Points[i].Position.Y < MinY) MinY = Points[i].Position.Y;
                    if (Points[i].Position.Z < MinZ) MinZ = Points[i].Position.Z;
                    if (Points[i].Position.X > MaxX) MaxX = Points[i].Position.X;
                    if (Points[i].Position.Y > MaxY) MaxY = Points[i].Position.Y;
                    if (Points[i].Position.Z > MaxZ) MaxZ = Points[i].Position.Z;
                }
                return (new Vector3(MaxX, MaxY, MaxZ), new Vector3(MinX, MinY, MinZ));
            }
            catch
            {
                return (new Vector3(), new Vector3());
            }
            
        }
//------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Проврека лежит ли точка внутри сетки
        /// </summary>
        /// <param name="Max">Значения максимальных значений в сетке</param>
        /// <param name="Min">Значения минимальных значений</param>
        /// <param name="BasePoint">Точка, которая проверяется</param>
        /// <returns>true - если не лежит в сетке, false - если лежит </returns>
        protected bool LeavingTheLineOutsideTheGrid(Vector3 Max, Vector3 Min, Vector3 BasePoint)
        {

            if (Max.X <= BasePoint.X) return true;
            if (Max.Y <= BasePoint.Y) return true;
            if (Max.Z <= BasePoint.Z) return true;
            if (Min.X >= BasePoint.X) return true;
            if (Min.Y >= BasePoint.Y) return true;
            if (Min.Z >= BasePoint.Z) return true;
            return false;
        }
        #region Test1
        ////--------------------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Раскидывает точки в большие ячейки, для более удобного построения линимй тока
        ///// </summary>
        ///// <param name="Points">Массив точек</param>
        ///// <returns>Массив листов, в которых находятся точки</returns>
        //protected List<TFemElement_Visual>[,,] DistributionOfPointsAcrossLargeCells(TFemElement_Visual[] Points)
        //{
        //    var MaxMin = MaxMinCoordinate(Points);
        //    float LengthCellX = (MaxMin.Item1.X - MaxMin.Item2.X) / Number;
        //    float WidthCellZ = (MaxMin.Item1.Z - MaxMin.Item2.Z) / Number;
        //    float HeightCellY = (MaxMin.Item1.Y - MaxMin.Item2.Y) / Number;

        //    List<TFemElement_Visual>[,,] ArrayPoints = new List<TFemElement_Visual>[Number, Number, Number];

        //    for (int i = 0; i < Points.Length; i++)
        //    {
        //        int aX = (int)((Points[i].Position.X - MaxMin.Item2.X) / LengthCellX);
        //        int bY = (int)((Points[i].Position.Y - MaxMin.Item2.Y) / HeightCellY);
        //        int cZ = (int)((Points[i].Position.Z - MaxMin.Item2.Z) / WidthCellZ);

        //        if (aX == Number) aX = Number2;
        //        if (bY == Number) bY = Number2;
        //        if (cZ == Number) cZ = Number2;
        //        if (ArrayPoints[aX, bY, cZ] == null)
        //        {
        //            ArrayPoints[aX, bY, cZ] = new List<TFemElement_Visual>();
        //        }

        //        ArrayPoints[aX, bY, cZ].Add(Points[i]);
        //    }

        //    return ArrayPoints;
        //}
        ////---------------------------------------------------------------------------------------------------------------------------------------------------

        ///// <summary>
        ///// Переводит вектор 3 в TFemElement_Visual
        ///// </summary>
        ///// <param name="Point">Точка, которую нужно перевести в TFemElement_Visual</param>
        ///// <returns>Точка в формате TFemElement_Visual</returns>
        //protected TFemElement_Visual TransferVector3BasePoint(Vector3 Point)
        //{

        //    TFemElement_Visual NewPoints = new TFemElement_Visual();
        //    Vector3 Vector = new Vector3(Point.X, Point.Y, Point.Z);
        //    NewPoints.Position = Vector;
        //    return NewPoints;
        //}
        ////-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Находит индексы у ячейки, в которой находится базовая точка
        ///// </summary>
        ///// <param name="Point">Базовая точка, у которой нужно найти ячейку</param>
        ///// <param name="Min">Минимальная координата сетки</param>
        ///// <param name="Max">Максимальная координата сетки</param>
        ///// <returns>Набор ijk, индексы ячейки</returns>
        //protected (int, int, int) FindingIndexCellInBasePoint(TFemElement_Visual Point, Vector3 Min, Vector3 Max)
        //{
        //    //Находится длина большой ячейки по X Y Z 
        //    float LengthCellX = (Max.X - Min.X) / Number;
        //    float WidthCellZ = (Max.Z - Min.Z) / Number;
        //    float HeightCellY = (Max.Y - Min.Y) / Number;

        //    //Вычисляется индекс, где находится базовая точка
        //    int aX = (int)((Point.Position.X - Min.X) / LengthCellX);
        //    int bY = (int)((Point.Position.Y - Min.Y) / HeightCellY);
        //    int cZ = (int)((Point.Position.Z - Min.Z) / WidthCellZ);

        //    //Если индекс вышел за рамки
        //    if (aX == Number) aX = Number2;
        //    if (bY == Number) bY = Number2;
        //    if (cZ == Number) cZ = Number2;

        //    return (aX, bY, cZ);
        //}
        ////---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Создание листа из трианг конйтнеров из моделей
        ///// </summary>
        ///// <returns></returns>
        //protected List<TTriangleContainer> CreateTTriangleContainerFromModels()
        //{
        //    List<TTriangleContainer> ModelsTriangle = new List<TTriangleContainer>();
        //    for (int i = 0; i < FEM_V.Surfaces.Count; i++)
        //    {
        //        var Triangles = new List<TTriangle>();
        //        for (int j = 0; j < FEM_V.Surfaces[i].Faces.Length; j++)
        //        {
        //            var NumberOfTriangles = FEM_V.Surfaces[i].Faces[j].Nodes.Length - 2;
        //            // Делим грани на треугольники
        //            for (int k = 0; k < NumberOfTriangles; k++)
        //            {
        //                // Делим на треугольники грани и записываем в список
        //                Triangles.Add(new TTriangle
        //                {
        //                    P0 = new Vector3(FEM_V.Nodes[FEM_V.Surfaces[i].Faces[j].Nodes[0]].X, FEM_V.Nodes[FEM_V.Surfaces[i].Faces[j].Nodes[0]].Y, FEM_V.Nodes[FEM_V.Surfaces[i].Faces[j].Nodes[0]].Z),
        //                    P1 = new Vector3(FEM_V.Nodes[FEM_V.Surfaces[i].Faces[j].Nodes[k + 1]].X, FEM_V.Nodes[FEM_V.Surfaces[i].Faces[j].Nodes[k + 1]].Y, FEM_V.Nodes[FEM_V.Surfaces[i].Faces[j].Nodes[k + 1]].Z),
        //                    P2 = new Vector3(FEM_V.Nodes[FEM_V.Surfaces[i].Faces[j].Nodes[k + 2]].X, FEM_V.Nodes[FEM_V.Surfaces[i].Faces[j].Nodes[k + 2]].Y, FEM_V.Nodes[FEM_V.Surfaces[i].Faces[j].Nodes[k + 2]].Z),

        //                });
        //            }
        //        }
        //        ModelsTriangle.Add(new TTriangleContainer(Triangles));
        //    }
        //    return ModelsTriangle;
        //}
        ////---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------        
        ///// <summary>
        ///// Максимальные и минимальные координаты модели
        ///// </summary>
        ///// <param name="triangles">Треугольники, из которых состоит модель</param>
        ///// <returns> 1 max, 2 min</returns>
        //protected (Vector3 Max, Vector3 Min) MaxMinModel(TTriangleContainer triangles)
        //{
        //    // Переменные, в которых будут координаты максимального и минимального значения
        //    Vector3 MaxModel = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        //    Vector3 MinModel = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

        //    // Поиск Мин и Макс
        //    for (int i = 0; i < triangles.Triangles.Count; i++)
        //    {
        //        if (MaxModel.X < triangles.Triangles[i].P0.X)
        //        {
        //            MaxModel.X = triangles.Triangles[i].P0.X;
        //        }
        //        if (MaxModel.Y < triangles.Triangles[i].P0.Y)
        //        {
        //            MaxModel.Y = triangles.Triangles[i].P0.Y;
        //        }
        //        if (MaxModel.Z < triangles.Triangles[i].P0.Z)
        //        {
        //            MaxModel.Z = triangles.Triangles[i].P0.Z;
        //        }
        //        if (MinModel.X > triangles.Triangles[i].P0.X)
        //        {
        //            MinModel.X = triangles.Triangles[i].P0.X;
        //        }
        //        if (MinModel.Y > triangles.Triangles[i].P0.Y)
        //        {
        //            MinModel.Y = triangles.Triangles[i].P0.Y;
        //        }
        //        if (MinModel.Z > triangles.Triangles[i].P0.Z)
        //        {
        //            MinModel.Z = triangles.Triangles[i].P0.Z;
        //        }
        //        if (MaxModel.X < triangles.Triangles[i].P1.X)
        //        {
        //            MaxModel.X = triangles.Triangles[i].P1.X;
        //        }
        //        if (MaxModel.Y < triangles.Triangles[i].P1.Y)
        //        {
        //            MaxModel.Y = triangles.Triangles[i].P1.Y;
        //        }
        //        if (MaxModel.Z < triangles.Triangles[i].P1.Z)
        //        {
        //            MaxModel.Z = triangles.Triangles[i].P1.Z;
        //        }
        //        if (MinModel.X > triangles.Triangles[i].P1.X)
        //        {
        //            MinModel.X = triangles.Triangles[i].P1.X;
        //        }
        //        if (MinModel.Y > triangles.Triangles[i].P1.Y)
        //        {
        //            MinModel.Y = triangles.Triangles[i].P1.Y;
        //        }
        //        if (MinModel.Z > triangles.Triangles[i].P1.Z)
        //        {
        //            MinModel.Z = triangles.Triangles[i].P1.Z;
        //        }
        //        if (MaxModel.X < triangles.Triangles[i].P2.X)
        //        {
        //            MaxModel.X = triangles.Triangles[i].P2.X;
        //        }
        //        if (MaxModel.Y < triangles.Triangles[i].P2.Y)
        //        {
        //            MaxModel.Y = triangles.Triangles[i].P2.Y;
        //        }
        //        if (MaxModel.Z < triangles.Triangles[i].P2.Z)
        //        {
        //            MaxModel.Z = triangles.Triangles[i].P2.Z;
        //        }
        //        if (MinModel.X > triangles.Triangles[i].P2.X)
        //        {
        //            MinModel.X = triangles.Triangles[i].P2.X;
        //        }
        //        if (MinModel.Y > triangles.Triangles[i].P2.Y)
        //        {
        //            MinModel.Y = triangles.Triangles[i].P2.Y;
        //        }
        //        if (MinModel.Z > triangles.Triangles[i].P2.Z)
        //        {
        //            MinModel.Z = triangles.Triangles[i].P2.Z;
        //        }
        //    }
        //    return (MaxModel, MinModel);
        //}
        ////-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Проверка принадлежности точки модели
        ///// </summary>
        ///// <param name="MinModel">Минимальные координаты модели</param>
        ///// <param name="MaxModel">Максимальные координаты модели</param>
        ///// <param name="Point">Точка, которую надо проверить</param>
        ///// <param name="triangles">Треугольники модели</param>
        ///// <returns>true - если внутри модели, false - если не лежит</returns>
        //protected bool CheckingPointInModel(Vector3 MinModel, Vector3 MaxModel, Vector3 Point, TTriangleContainer triangles)
        //{
        //    // Задача воображаемой сферы, для дальнейшей проверки принадлежности. Центр сферы - точка, которую проверяем. Радиус сферы - 0,01 от VES 
        //    Vector3 PointF = new Vector3((float)Point.X, (float)Point.Y, (float)Point.Z);
        //    BoundingSphere BS = new BoundingSphere(PointF, (float)VESConst / 50);

        //    // Сначала проверяем находится ли точка в воображаемом параллепипеде, координаты которого минимальные и макимальные значения модели
        //    if (Point.X > MinModel.X && Point.Y > MinModel.Y && Point.Z > MinModel.Z && Point.X < MaxModel.X && Point.Y < MaxModel.Y && Point.Z < MaxModel.Z)
        //    {
        //        // Далее проходим по каждому треугольнику, так как модель не простая фигура, то точка может и не лежать в ней
        //        for (int i = 0; i < triangles.Triangles.Count; i++)
        //        {
        //            bool a = TCollideHelper.Intersection_Triangle(triangles.Triangles[i], BS);
        //            if (a == false) continue;
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        #endregion
        #region Test

        ///// <summary>
        ///// Подбор соседних ячеек, в которых находятся недостающие значения, которых не было в базовой ячейке
        ///// </summary>
        ///// <param name="BasePoint">Точка, для который находятся ячейки</param>
        ///// <param name="i">Индекс i базовой ячейки</param>
        ///// <param name="j">Индекс j базовой ячейки</param>
        ///// <param name="k">Индекс k базовой ячейки</param>
        ///// <param name="Min">Минимальная координата сетки</param>
        ///// <param name="Max">Максимальная кооридната сетки</param>
        ///// <returns>Список индексов нужных соседних ячеек</returns>
        //protected List<(int, int, int)> IndexAdditionsCell(TFemElement_Visual BasePoint, int i, int j, int k, Vector3 Min, Vector3 Max)
        //{
        //    // Список индексов нужных соседних ячеек
        //    List<(int, int, int)> Index = new List<(int, int, int)>();
        //    // Габариты ячейки
        //    float LengthCellX = (Max.X - Min.X) / Number;
        //    float WidthCellZ = (Max.Z - Min.Z) / Number;
        //    float HeightCellY = (Max.Y - Min.Y) / Number;

        //    //Координата нулевой точки большой ячейки
        //    Vector3 StartBigCell = new Vector3(Min.X + LengthCellX * i, Min.Y + HeightCellY * j, Min.Z + WidthCellZ * k);
        //    //Вычисляется доля, которая показывает насколько близко точка лежит к началу или концу большой ячейки по X Y Z 
        //    float PresentPositionX = (float)(BasePoint.Position.X - StartBigCell.X) / LengthCellX;
        //    float PresentPositionY = (float)(BasePoint.Position.Y - StartBigCell.Y) / HeightCellY;
        //    float PresentPositionZ = (float)(BasePoint.Position.Z - StartBigCell.Z) / WidthCellZ;

        //    // Переменные для проверки нахождения точки
        //    int ProverkaX = 0;
        //    int ProverkaY = 0;
        //    int ProverkaZ = 0;

        //    // Здесь узнается, насколько близко точка лежит к граням ячейки
        //    if (PresentPositionX < 0.3) ProverkaX = -1;
        //    if (PresentPositionX > 0.7) ProverkaX = 1;
        //    if (PresentPositionY < 0.3) ProverkaY = -1;
        //    if (PresentPositionY > 0.7) ProverkaY = 1;
        //    if (PresentPositionZ < 0.3) ProverkaZ = -1;
        //    if (PresentPositionZ > 0.7) ProverkaZ = 1;

        //    //В зависимости от того, где лежит точка, добавляются индексы соседних ячеек,чтобы найти там недостающие точки базовой ячейки
        //    if (ProverkaX == -1)
        //    {
        //        Index.Add((i - 1, j, k));
        //        if (ProverkaY == -1)
        //        {
        //            Index.Add((i - 1, j - 1, k));
        //            Index.Add((i, j - 1, k));
        //            if (ProverkaZ == -1)
        //            {
        //                Index.Add((i, j, k - 1));
        //                Index.Add((i, j - 1, k - 1));
        //                Index.Add((i - 1, j - 1, k - 1));
        //                Index.Add((i - 1, j, k - 1));
        //            }
        //            if (ProverkaZ == 1)
        //            {
        //                Index.Add((i, j, k + 1));
        //                Index.Add((i, j - 1, k + 1));
        //                Index.Add((i - 1, j - 1, k + 1));
        //                Index.Add((i - 1, j, k + 1));
        //            }
        //        }
        //        if (ProverkaY == 1)
        //        {
        //            Index.Add((i - 1, j + 1, k));
        //            Index.Add((i, j + 1, k));
        //            if (ProverkaZ == -1)
        //            {
        //                Index.Add((i, j, k - 1));
        //                Index.Add((i, j + 1, k - 1));
        //                Index.Add((i - 1, j + 1, k - 1));
        //                Index.Add((i - 1, j, k - 1));
        //            }
        //            if (ProverkaZ == 1)
        //            {
        //                Index.Add((i, j, k + 1));
        //                Index.Add((i, j + 1, k + 1));
        //                Index.Add((i - 1, j + 1, k + 1));
        //                Index.Add((i - 1, j, k + 1));
        //            }
        //        }
        //        if (ProverkaY == 0)
        //        {
        //            if (ProverkaZ == 1)
        //            {
        //                Index.Add((i - 1, j, k + 1));
        //                Index.Add((i, j, k + 1));
        //            }
        //            if (ProverkaZ == -1)
        //            {
        //                Index.Add((i - 1, j, k - 1));
        //                Index.Add((i, j, k - 1));
        //            }
        //        }
        //    }
        //    if (ProverkaX == 1)
        //    {
        //        Index.Add((i + 1, j, k));
        //        if (ProverkaY == -1)
        //        {
        //            Index.Add((i + 1, j - 1, k));
        //            Index.Add((i, j - 1, k));
        //            if (ProverkaZ == -1)
        //            {
        //                Index.Add((i, j, k - 1));
        //                Index.Add((i, j - 1, k - 1));
        //                Index.Add((i + 1, j - 1, k - 1));
        //                Index.Add((i + 1, j, k - 1));
        //            }
        //            if (ProverkaZ == 1)
        //            {
        //                Index.Add((i, j, k + 1));
        //                Index.Add((i, j - 1, k + 1));
        //                Index.Add((i + 1, j - 1, k + 1));
        //                Index.Add((i + 1, j, k + 1));
        //            }
        //        }
        //        if (ProverkaY == 1)
        //        {
        //            Index.Add((i + 1, j + 1, k));
        //            Index.Add((i, j + 1, k));
        //            if (ProverkaZ == -1)
        //            {
        //                Index.Add((i, j, k - 1));
        //                Index.Add((i, j + 1, k - 1));
        //                Index.Add((i + 1, j + 1, k - 1));
        //                Index.Add((i + 1, j, k - 1));
        //            }
        //            if (ProverkaZ == 1)
        //            {
        //                Index.Add((i, j, k + 1));
        //                Index.Add((i, j + 1, k + 1));
        //                Index.Add((i + 1, j + 1, k + 1));
        //                Index.Add((i + 1, j, k + 1));
        //            }
        //        }
        //        if (ProverkaY == 0)
        //        {
        //            if (ProverkaZ == 1)
        //            {
        //                Index.Add((i + 1, j, k + 1));
        //                Index.Add((i, j, k + 1));
        //            }
        //            if (ProverkaZ == -1)
        //            {
        //                Index.Add((i + 1, j, k - 1));
        //                Index.Add((i, j, k - 1));
        //            }
        //        }
        //    }
        //    if (ProverkaX == 0)
        //    {
        //        if (ProverkaY == 1)
        //        {
        //            Index.Add((i, j + 1, k));
        //            if (ProverkaZ == 1)
        //            {
        //                Index.Add((i, j + 1, k + 1));
        //                Index.Add((i, j, k + 1));
        //            }
        //            if (ProverkaZ == -1)
        //            {
        //                Index.Add((i, j, k - 1));
        //                Index.Add((i, j + 1, k - 1));
        //            }
        //        }
        //        if (ProverkaY == -1)
        //        {
        //            Index.Add((i, j - 1, k));
        //            if (ProverkaZ == 1)
        //            {
        //                Index.Add((i, j - 1, k + 1));
        //                Index.Add((i, j, k + 1));
        //            }
        //            if (ProverkaZ == -1)
        //            {
        //                Index.Add((i, j, k - 1));
        //                Index.Add((i, j - 1, k - 1));
        //            }
        //        }
        //        if (ProverkaY == 0)
        //        {
        //            if (ProverkaZ == 1)
        //            {
        //                Index.Add((i, j, k + 1));
        //            }
        //            if (ProverkaZ == -1)
        //            {
        //                Index.Add((i, j, k - 1));
        //            }
        //        }
        //    }

        //    //Если так вышло, что соовественные индексы не соотвествуют действительности, то эта проверка для них
        //    for (int m = 0; m < Index.Count; m++)
        //    {
        //        if (Index[m].Item1 < 0)
        //            Index[m] = (0, Index[m].Item2, Index[m].Item3);
        //        if (Index[m].Item2 < 0)
        //            Index[m] = (Index[m].Item1, 0, Index[m].Item3);
        //        if (Index[m].Item3 < 0)
        //            Index[m] = (Index[m].Item1, Index[m].Item2, 0);
        //        if (Index[m].Item1 > Number2)
        //            Index[m] = (Number2, Index[m].Item2, Index[m].Item3);
        //        if (Index[m].Item2 > Number2)
        //            Index[m] = (Index[m].Item1, Number2, Index[m].Item3);
        //        if (Index[m].Item3 > Number2)
        //            Index[m] = (Index[m].Item1, Index[m].Item2, Number2);
        //    }
        //    return Index;
        //}
        ////----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Нахождение индексов, если в предыдущую проверку они так же были не найдены. Такие случаи редко, чаще всего, когда изначально нашлись только 2 точки для маленькой ячейки
        ///// </summary>
        ///// <param name="i">Индекс i базовой ячейки</param>
        ///// <param name="j">Индекс j базовой ячейки</param>
        ///// <param name="k">Индекс k базовой ячейки</param>
        ///// <param name="Cell">Ячейка, в которой уже есть какие-то найденные точки</param>
        ///// <returns>Список индексов у найденных нужных ячеек</returns>
        //protected List<(int, int, int)> IndexAdditionsCellForTwo(int i, int j, int k, TFemElement_Visual[] Cell)
        //{
        //    List<(int, int, int)> Index = new List<(int, int, int)>();

        //    //Суть этого метода, что каждая точка в массиве имеет определенное положение. Значит, если какая-то точка не найдена, можно точно сказать какие ячейки к этой точке ближайшие, и потом в них искать недостающие значения
        //    if (Cell[0] == null)
        //    {
        //        Index.Add((i - 1, j - 1, k - 1));
        //        Index.Add((i - 1, j - 1, k));
        //    }

        //    if (Cell[1] == null)
        //    {
        //        Index.Add((i + 1, j - 1, k - 1));
        //        Index.Add((i + 1, j - 1, k));
        //    }

        //    if (Cell[2] == null)
        //    {
        //        Index.Add((i, j - 1, k + 1));
        //        Index.Add((i + 1, j - 1, k + 1));
        //    }

        //    if (Cell[3] == null)
        //    {
        //        Index.Add((i + 1, j - 1, k + 1));
        //        Index.Add((i, j - 1, k + 1));
        //    }

        //    if (Cell[4] == null)
        //    {
        //        Index.Add((i - 1, j + 1, k - 1));
        //        Index.Add((i - 1, j + 1, k));
        //    }

        //    if (Cell[5] == null)
        //    {
        //        Index.Add((i + 1, j + 1, k - 1));
        //        Index.Add((i + 1, j + 1, k));
        //    }

        //    if (Cell[6] == null)
        //    {
        //        Index.Add((i, j + 1, k + 1));
        //        Index.Add((i + 1, j + 1, k + 1));
        //    }

        //    if (Cell[7] == null)
        //    {
        //        Index.Add((i - 1, j + 1, k + 1));
        //        Index.Add((i, j + 1, k + 1));
        //    }
        //    //Проверка, если нашлись индкесы, не соответсвующие действительности
        //    for (int m = 0; m < Index.Count; m++)
        //    {
        //        if (Index[m].Item1 < 0 || Index[m].Item2 < 0 || Index[m].Item3 < 0 || Index[m].Item1 > Number2 || Index[m].Item2 > Number2 || Index[m].Item3 > Number2)
        //        {
        //            Index.RemoveAt(m);
        //            m--;
        //        }
        //    }
        //    return Index;
        //}
        ////-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Данный метод дополняет маленькую ячейку, недостающимим элементами 
        ///// </summary>
        ///// <param name="Cell">Ячейка, в которой уже нашлись какие-то значения</param>
        ///// <param name="BasePoint">Точка,для которой ищем значения</param>
        ///// <param name="i">Индекс i базовой ячейки</param>
        ///// <param name="j">Индекс j базовой ячейки</param>
        ///// <param name="k">Индекс k базовой ячейки</param>
        ///// <param name="ArrayPoints">Массив листов больших ячеек</param>
        ///// <param name="Min">Минимальная координата сетки</param>
        ///// <param name="Max">Максимальная координата сетки</param>
        ///// <returns>Массив из 8 точек</returns>
        //protected (TFemElement_Visual[], bool) AdditionalOfCellMissingElements(TFemElement_Visual[] Cell, TFemElement_Visual BasePoint, int i, int j, int k, List<TFemElement_Visual>[,,] ArrayPoints, Vector3 Min, Vector3 Max)
        //{
        //    //Находятся индексы ячеек, в которых могут быть недостающие точки
        //    List<(int, int, int)> Index = new List<(int, int, int)>();
        //    Index = IndexAdditionsCell(BasePoint, i, j, k, Min, Max);

        //    List<TFemElement_Visual> Points = new List<TFemElement_Visual>();
        //    for (int I = 0; I < Index.Count; I++)
        //    {
        //        if (ArrayPoints[Index[I].Item1, Index[I].Item2, Index[I].Item3] == null)
        //        {
        //            continue;
        //        }
        //        Points.AddRange(ArrayPoints[Index[I].Item1, Index[I].Item2, Index[I].Item3]);
        //    }

        //    TFemElement_Visual[] PointsArray = new TFemElement_Visual[Points.Count];
        //    PointsArray = Points.ToArray();
        //    //Маленькая чейка дополняется
        //    var NewCell = SearchForAdditionalPoints(Cell, BasePoint.Position, PointsArray);
        //    TFemElement_Visual[] NewCellArray = NewCell.Item1;
        //    //Если опять не нашлись все 8 точек, то ищем в больших ячейках, которые были состалвены по недостающим точкам
        //    if (NewCell.Item2 == false)
        //    {
        //        var IndexNew = IndexAdditionsCellForTwo(i, j, k, NewCellArray);
        //        List<TFemElement_Visual> Points1 = new List<TFemElement_Visual>();
        //        for (int I = 0; I < IndexNew.Count; I++)
        //        {
        //            if (ArrayPoints[IndexNew[I].Item1, IndexNew[I].Item2, IndexNew[I].Item3] == null)
        //            {
        //                continue;
        //            }
        //            Points1.AddRange(ArrayPoints[IndexNew[I].Item1, IndexNew[I].Item2, IndexNew[I].Item3]);
        //        }
        //        TFemElement_Visual[] PointsArray1 = new TFemElement_Visual[Points.Count];
        //        PointsArray1 = Points1.ToArray();
        //        NewCell = SearchForAdditionalPoints(NewCell.Item1, BasePoint.Position, PointsArray1);
        //    }

        //    //Если всё еще не нашлись точки, значит, линия тока обрывается
        //    if (NewCell.Item2 == false)
        //    {
        //        if (NewCell.Item1[0] == null)
        //        {
        //            NewCell.Item1[0] = new TFemElement_Visual();
        //        }
        //        return (NewCell.Item1, false);
        //    }
        //    return (NewCell.Item1, true);
        //}
        ////-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Ищет доп точки в ячейку, которые не нашлись в прошлом
        ///// </summary>
        ///// <param name="Cell">Массив из 8 элементов, который нужно дополнять недостающими точками</param>
        ///// <param name="BasePoint">Базовая точка</param>
        ///// <param name="Points">Массив всех точек</param>
        ///// <returns>Item1 - Массив из 8 точек, Item2 - true, если все точки нашлись, false, если нет </returns>
        //protected (TFemElement_Visual[], bool) SearchForAdditionalPoints(TFemElement_Visual[] Cell, Vector3 BasePoint, TFemElement_Visual[] Points)
        //{
        //    TFemElement_Visual[] DopCell = new TFemElement_Visual[8];
        //    for (int i = 0; i < Points.Length; i++)
        //    {

        //        //0 point
        //        if (Points[i].Position.X <= BasePoint.X && Points[i].Position.Y <= BasePoint.Y && Points[i].Position.Z <= BasePoint.Z)
        //        {
        //            if (DopCell[0] == null)
        //            {
        //                DopCell[0] = Points[i];
        //            }
        //            else
        //            {
        //                if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(DopCell[0], BasePoint))
        //                {
        //                    DopCell[0] = Points[i];
        //                }
        //            }
        //        }

        //        //1 point
        //        if (Points[i].Position.X >= BasePoint.X && Points[i].Position.Y <= BasePoint.Y && Points[i].Position.Z <= BasePoint.Z)
        //        {
        //            if (DopCell[1] == null)
        //            {
        //                DopCell[1] = Points[i];
        //            }
        //            else
        //            {
        //                if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(DopCell[1], BasePoint))
        //                {
        //                    DopCell[1] = Points[i];
        //                }
        //            }
        //        }

        //        //2 point
        //        if (Points[i].Position.X >= BasePoint.X && Points[i].Position.Y <= BasePoint.Y && Points[i].Position.Z >= BasePoint.Z)
        //        {
        //            if (DopCell[2] == null)
        //            {
        //                DopCell[2] = Points[i];
        //            }
        //            else
        //            {
        //                if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(DopCell[2], BasePoint))
        //                {
        //                    DopCell[2] = Points[i];
        //                }
        //            }
        //        }

        //        //3 point
        //        if (Points[i].Position.X <= BasePoint.X && Points[i].Position.Y <= BasePoint.Y && Points[i].Position.Z >= BasePoint.Z)
        //        {
        //            if (DopCell[3] == null)
        //            {
        //                DopCell[3] = Points[i];
        //            }
        //            else
        //            {
        //                if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(DopCell[3], BasePoint))
        //                {
        //                    DopCell[3] = Points[i];
        //                }
        //            }
        //        }

        //        //4 point
        //        if (Points[i].Position.X <= BasePoint.X && Points[i].Position.Y >= BasePoint.Y && Points[i].Position.Z <= BasePoint.Z)
        //        {
        //            if (DopCell[4] == null)
        //            {
        //                DopCell[4] = Points[i];
        //            }
        //            else
        //            {
        //                if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(DopCell[4], BasePoint))
        //                {
        //                    DopCell[4] = Points[i];
        //                }
        //            }
        //        }

        //        //5 point
        //        if (Points[i].Position.X >= BasePoint.X && Points[i].Position.Y >= BasePoint.Y && Points[i].Position.Z <= BasePoint.Z)
        //        {
        //            if (DopCell[5] == null)
        //            {
        //                DopCell[5] = Points[i];
        //            }
        //            else
        //            {
        //                if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(DopCell[5], BasePoint))
        //                {
        //                    DopCell[5] = Points[i];
        //                }
        //            }
        //        }

        //        //6 point
        //        if (Points[i].Position.X >= BasePoint.X && Points[i].Position.Y >= BasePoint.Y && Points[i].Position.Z >= BasePoint.Z)
        //        {
        //            if (DopCell[6] == null)
        //            {
        //                DopCell[6] = Points[i];
        //            }
        //            else
        //            {
        //                if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(DopCell[6], BasePoint))
        //                {
        //                    DopCell[6] = Points[i];
        //                }
        //            }
        //        }

        //        //7 point
        //        if (Points[i].Position.X <= BasePoint.X && Points[i].Position.Y >= BasePoint.Y && Points[i].Position.Z >= BasePoint.Z)
        //        {
        //            if (DopCell[7] == null)
        //            {
        //                DopCell[7] = Points[i];
        //            }
        //            else
        //            {
        //                if (DistanceBetweenPoints(Points[i], BasePoint) < DistanceBetweenPoints(DopCell[7], BasePoint))
        //                {
        //                    DopCell[7] = Points[i];
        //                }
        //            }
        //        }
        //    }

        //    //Записываются недостающие точки в маленькую ячейку, а так же проверяется вдруг в прошлом была найдена точка, которая на большем расстоянии, чем которая нашлась сейчас
        //    bool Proverka = true;
        //    for (int i = 0; i < 8; i++)
        //    {
        //        if (Cell[i] == null)
        //        {
        //            Cell[i] = DopCell[i];
        //        }
        //        if (DopCell[i] != null)
        //        {
        //            if (DistanceBetweenPoints(DopCell[i], BasePoint) < DistanceBetweenPoints(Cell[i], BasePoint))
        //            {
        //                Cell[i] = DopCell[i];
        //            }
        //        }
        //        if (Cell[i] == null)
        //        {
        //            Proverka = false;
        //        }
        //    }
        //    return (Cell, Proverka);
        //}
        ////------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Построение линии тока
        ///// </summary>
        ///// <param name="ArrayPoints">Массив листов больших ячеек с точками</param>
        ///// <param name="BasePoint">Базовая точка, с которой начинается построение</param>
        ///// <param name="Points">Массив всех точек</param>
        ///// <param name="Triangles">Треугольники модели</param>
        ///// <returns>Массив точек, по которым строится линия тока</returns>
        //protected TFemElement_Visual[] ConstructionOfCurrentLines(List<TFemElement_Visual>[,,] ArrayPoints, TFemElement_Visual BasePoint, TFemElement_Visual[] Points)
        //{
        //    VES = 1f;
        //    var Triangles = CreateTTriangleContainerFromModels();

        //    List<TFemElement_Visual> PointsInCurrentLines = new List<TFemElement_Visual>();

        //    //Координаты максимального и минимального значений на сетке
        //    var MaxMin = MaxMinCoordinate(Points);
        //    Vector3 Max = MaxMin.Item1;
        //    Vector3 Min = MaxMin.Item2;
        //    // Проверка лежит ли базовая точка внутри сетки
        //    if (LeavingTheLineOutsideTheGrid(Max, Min, BasePoint.Position) == true) return new TFemElement_Visual[0];
        //    //Координаты максимального и минимального значений модели
        //    var MaxMinModeL = MaxMinModel(Triangles[0]);
        //    Vector3 MaxModel = MaxMinModeL.Item1;
        //    Vector3 MinModel = MaxMinModeL.Item2;
        //    // Проверка, лежит ли базовая точка на модели
        //    if (CheckingPointInModel(MinModel, MaxModel, BasePoint.Position, Triangles[0]) == true) return new TFemElement_Visual[0];
        //    //Находит индекс большой ячейки, в которой находится базовая точка
        //    var Index = FindingIndexCellInBasePoint(BasePoint, Min, Max);
        //    //Массив точек из большой ячейки
        //    TFemElement_Visual[] CellArray = ArrayPoints[Index.Item1, Index.Item2, Index.Item3].ToArray();

        //    //Находит базовую ячейку, с которой начинается построение
        //    var CellAndBool = FindingCellsBasePoint(CellArray, BasePoint.Position);
        //    TFemElement_Visual[] Cell = CellAndBool.Item1;
        //    bool Test = true;
        //    //Если не все значения нашлись в маленькой ячейке, то они находятся дополнительно в других больших ячейках
        //    if (CellAndBool.Item2 == false)
        //    {
        //        var Res = AdditionalOfCellMissingElements(Cell, BasePoint, Index.Item1, Index.Item2, Index.Item3, ArrayPoints, Min, Max);
        //        Cell = Res.Item1;
        //        Test = Res.Item2;
        //    }

        //    //Если в ячейке есть не заполененные данные, это может быть если базовая точка выходит за пределы сетки, то построение линии не начинается
        //    if (Test == false) return new TFemElement_Visual[0];

        //    //Находит вектор скорости в базовой точке
        //    BasePoint.Velocity = FindingVectorVelocityPointInCell(Cell, BasePoint.Position);
        //    BasePoint.VelocityModule = BasePoint.Velocity.Length();
        //    // Есть ли в векторе скорости нерациональные числа
        //    if (CheckNan(BasePoint.Velocity)) return new TFemElement_Visual[0];
        //    PointsInCurrentLines.Add(BasePoint);

        //    //Цикл, в котором находятся точки для одной линии
        //    while (true)
        //    {
        //        TFemElement_Visual NextPoint = new TFemElement_Visual();
        //        //Следующая точка на векторе скорости предыдущей точки
        //        NextPoint.Position = PointInVectorBasePoint(PointsInCurrentLines[PointsInCurrentLines.Count - 1], VES);

        //        //Если точка выходит за пределы сетки, то цикл прекращается
        //        if (LeavingTheLineOutsideTheGrid(Max, Min, NextPoint.Position) == true) break;
        //        else
        //        {
        //            //Если точка находится в старой маленькой ячейке, то строим вектор в ней же
        //            if (NewPointInOldCell(Cell, NextPoint) == true)
        //            {
        //                NextPoint.Velocity = FindingVectorVelocityPointInCell(Cell, NextPoint.Position);
        //                NextPoint.VelocityModule = NextPoint.Velocity.Length();
        //                // Есть ли в векторе скорости нерациональные числа
        //                if (CheckNan(NextPoint.Velocity)) break;
        //                PointsInCurrentLines.Add(NextPoint);
        //            }
        //            //Если нет, то ищем новую маленькую ячейку и строим вектор уже из неё
        //            else
        //            {
        //                //Находим индексы большой ячейки, в которой находится базовая точка
        //                var IndexNew = FindingIndexCellInBasePoint(NextPoint, Min, Max);
        //                //if (ArrayPoints[IndexNew.Item1, IndexNew.Item2, IndexNew.Item3] == null)
        //                //{

        //                //}
        //                TFemElement_Visual[] CellArrayNew = ArrayPoints[IndexNew.Item1, IndexNew.Item2, IndexNew.Item3].ToArray();

        //                //Составляется новая маленькая ячейка
        //                var CellAndBoolNew = FindingCellsBasePoint(CellArrayNew, NextPoint.Position);
        //                TFemElement_Visual[] CellNew = CellAndBoolNew.Item1;

        //                Test = true;
        //                //Если не все значения нашлись в маленькой ячейке, то они находятся дополнительно в других больших ячейках
        //                if (CellAndBoolNew.Item2 == false)
        //                {
        //                    var Res = AdditionalOfCellMissingElements(CellNew, NextPoint, IndexNew.Item1, IndexNew.Item2, IndexNew.Item3, ArrayPoints, Min, Max);
        //                    CellNew = Res.Item1;
        //                    Test = Res.Item2;
        //                }
        //                //Если не все значения нашлись, то построение линии тока заканчивается
        //                if (Test == false)
        //                {
        //                    break;
        //                }

        //                //Находятся вектор и модель скорости
        //                NextPoint.Velocity = FindingVectorVelocityPointInCell(CellNew, NextPoint.Position);
        //                NextPoint.VelocityModule = NextPoint.Velocity.Length();
        //                // Есть ли в векторе скорости нерациональные числа
        //                if (CheckNan(NextPoint.Velocity)) break;
        //                PointsInCurrentLines.Add(NextPoint);

        //                //Проверка зашла ли линия тока в модель
        //                if (CheckingPointInModel(MinModel, MaxModel, NextPoint.Position, Triangles[0]) == true) break;
        //                Cell = CellNew;
        //            }

        //        }

        //    }

        //    TFemElement_Visual[] PointsInCurrentLinesArray = new TFemElement_Visual[PointsInCurrentLines.Count];
        //    PointsInCurrentLinesArray = PointsInCurrentLines.ToArray();
        //    return PointsInCurrentLinesArray;
        //}
        ///// <summary>
        ///// Создает список базовых точек //Пока что на рандоме можно сказать задает
        ///// </summary>
        ///// <param name="Points"></param>
        ///// <param name="Step"></param>
        ///// <returns></returns>
        //protected List<TFEMFluidFrame> BasePointCurrentLines(TFEMFluidFrame[] Points, double Step)
        //{
        //    var MaxMin = MaxMinCoordinate(Points);
        //    List<TFEMFluidFrame> BasePoints = new List<TFEMFluidFrame>();
        //    double HorizontaStart = (MaxMin.Item1.Z - MaxMin.Item2.Z) * 0.47d;  //0,47  - 0,55
        //    double HorizontaFinish = (MaxMin.Item1.Z - MaxMin.Item2.Z) * 0.55d;
        //    double VerticaStart = (MaxMin.Item1.Y - MaxMin.Item2.Y) * 0.1d;   //0,25-0,55
        //    double VerticaFinish = (MaxMin.Item1.Y - MaxMin.Item2.Y) * 0.5d;
        //    int i = 0;
        //    int j = 1;
        //    while (true)
        //    {
        //        TFEMFluidFrame PointV = new TFEMFluidFrame();
        //        PointV.Position = new Vector3d(MaxMin.Item2.X + 2d, MaxMin.Item2.Y + VerticaStart + i * Step, MaxMin.Item2.Z + HorizontaStart);
        //        if (PointV.Position.Y > MaxMin.Item2.Y + VerticaFinish) break;
        //        BasePoints.Add(PointV);
        //        while (true)
        //        {
        //            TFEMFluidFrame Point = new TFEMFluidFrame();
        //            Point.Position = new Vector3d(MaxMin.Item2.X + 2d, MaxMin.Item2.Y + VerticaStart + i * Step, MaxMin.Item2.Z + HorizontaStart + j * Step);
        //            if (Point.Position.Z > MaxMin.Item2.Z + HorizontaFinish) break;
        //            BasePoints.Add(Point);
        //            j++;
        //        }
        //        j = 1;
        //        i++;
        //    }
        //    return BasePoints;
        //}
        //---------------------------------------------------------------
        ///// <summary>
        ///// Нахождение точек на кривой безье
        ///// </summary>
        ///// <param name="PointLine">Список начальных точек, через которые нужно провести кривую безье</param>
        ///// <param name="Step">Шаг, через который располагаются новые точки на кривой</param>
        ///// <returns>Список точек в кривой Безье</returns>
        //internal (List<Vector3d>, List<Vector3d>) BezierInterpolation(List<TFEMFluidFrame> PointLine, double Step)
        //{

        //    List<Vector3d> PointOfSpline = new List<Vector3d>();
        //    //Этот список нужен, чтобы изображать кривые пересечения векторов скорости, нужен был для проверки
        //    List<Vector3d> PointKasat = new List<Vector3d>();
        //    if (PointLine.Count < 2)
        //    {
        //        return (PointOfSpline, PointKasat);
        //    }
        //    for (int i = 0; i < PointLine.Count - 1; i++)
        //    {
        //        (List<Vector3d> PointsInLine, List<Vector3d> PointsInKasat) = PointsOfInterectionOfVelocityVectors(PointLine[i].Position, PointOfVelocityVector(PointLine[i].Position, PointLine[i].Velocity), PointLine[i + 1].Position, PointOfVelocityVector(PointLine[i + 1].Position, PointLine[i + 1].Velocity), Step);

        //        PointOfSpline.AddRange(PointsInLine);
        //        PointKasat.AddRange(PointsInKasat);
        //    }

        //    return (PointOfSpline, PointKasat);
        //}
        ////------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Определяется точка на векторе скорости, для дальнейшего построения кривой Безье
        ///// </summary>
        ///// <param name="Point">Позиция точки</param>
        ///// <param name="Skorost">Вектор скорости точки</param>
        ///// <returns>Точка на векторе скорости</returns>
        //internal Vector3d PointOfVelocityVector(Vector3d Point, Vector3d Skorost)
        //{

        //    Vector3d Vec = new Vector3d((Skorost.X + Point.X), (Skorost.Y + Point.Y), (Skorost.Z + Point.Z));
        //    double a, b, c;
        //    //Уравнение прямой по направлению: (x-xt)/vx=(y-yt)/vy=(z-zt)/vz=a , где xt,yt,zt - это координаты известной нам точки,  vx,vy,vz - координаты вектора направления  
        //    if ((Skorost.X == 0 && Skorost.Y == 0) || (Skorost.X == 0 && Skorost.Z == 0) || (Skorost.Z == 0 && Skorost.Y == 0))
        //    {
        //        return Vec;
        //    }

        //    if (Skorost.X == 0)
        //    {
        //        b = (Vec.Y - Point.Y) / Skorost.Y;
        //        c = (Vec.Z - Point.Z) / Skorost.Z;
        //        a = b;
        //    }
        //    else
        //    {
        //        if (Skorost.Y == 0)
        //        {
        //            a = (Vec.X - Point.X) / Skorost.X;
        //            c = (Vec.Z - Point.Z) / Skorost.Z;
        //            b = c;
        //        }
        //        else
        //        {
        //            if (Skorost.Z == 0)
        //            {
        //                a = (Vec.X - Point.X) / Skorost.X;
        //                b = (Vec.Y - Point.Y) / Skorost.Y;
        //                c = a;
        //            }
        //            else
        //            {
        //                a = (Vec.X - Point.X) / Skorost.X;
        //                b = (Vec.Y - Point.Y) / Skorost.Y;
        //                c = (Vec.Z - Point.Z) / Skorost.Z;
        //            }
        //        }
        //    }


        //    //0,0001 взяла такую погрешность, так как в расчетах присутсвует сложение, то погрешность есть, в дальнейшем её можно уменьшать, так как где видела я, цифры после запятой начинают не совпадать где-то на 8-10 знаках.
        //    if ((Math.Abs(a - b) < 0.0001d) && (Math.Abs(a - c) < 0.0001d) && (Math.Abs(b - c) < 0.0001d))
        //    {
        //        return Vec;
        //    }
        //    else
        //    {
        //        return new Vector3d((-1d * (10d * Skorost.X / Skorost.Length) + Point.X), (-1d * (10d * Skorost.Y / Skorost.Length) + Point.Y), (-1d * (10d * Skorost.Z / Skorost.Length + Point.Z)));

        //    }
        //}
        ////----------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Находит точки пересечения векторов скорости
        ///// </summary>
        ///// <param name="line1PointStart">Исходная точка первая</param>
        ///// <param name="line1PointEnd">Точка на первом векторе скорости</param>
        ///// <param name="line2PointStart">Исходная вторая точка</param>
        ///// <param name="line2PointEnd">Точка на втором векторе скорости</param>
        ///// <param name="Step">Шаг</param>
        ///// <returns></returns>
        //internal (List<Vector3d>, List<Vector3d>) PointsOfInterectionOfVelocityVectors(Vector3d line1PointStart, Vector3d line1PointEnd, Vector3d line2PointStart, Vector3d line2PointEnd, double Step)
        //{
        //    //Здесь реализован метод Пол Бурка

        //    List<Vector3d> PointsBezie = new List<Vector3d>();
        //    Vector3d p1 = line1PointStart;
        //    Vector3d p2 = line1PointEnd;
        //    Vector3d p3 = line2PointStart;
        //    Vector3d p4 = line2PointEnd;
        //    Vector3d p13 = p3 - p1;
        //    Vector3d p43 = p3 - p4;

        //    if (p43.Length < 0.0001d)
        //    {
        //        TJournalLog.WriteLog("Error E0000");
        //        List<Vector3d> Zero = new List<Vector3d>();
        //        List<Vector3d> Zero1 = new List<Vector3d>();
        //        return (Zero, Zero1);
        //    }
        //    Vector3d p21 = p1 - p2;
        //    if (p21.Length < 0.0001d)
        //    {
        //        TJournalLog.WriteLog("Error E0000");
        //        List<Vector3d> Zero = new List<Vector3d>();
        //        List<Vector3d> Zero1 = new List<Vector3d>();
        //        return (Zero, Zero1);
        //    }

        //    //Этот список нужен только для проверки, когда всё будет работать идеально, его можно удалить
        //    List<Vector3d> PointKasat = new List<Vector3d>();

        //    //Если вектора параллельны, значит, их минимальный отрезок пересечения, это соединение концов их векторов скорости
        //    if (AngleDegree(p21, p43) < 0.0001d || AngleDegree(p21, p43) + 0.0001d > 180d)
        //    {
        //        PointsBezie.AddRange(BezieFourPoints(line1PointStart, line1PointEnd, line2PointEnd, line2PointStart, Step));
        //        return (PointsBezie, PointKasat);
        //    }

        //    double d1343 = p13.X * p43.X + p13.Y * p43.Y + p13.Z * p43.Z;
        //    double d4321 = p43.X * p21.X + p43.Y * p21.Y + p43.Z * p21.Z;
        //    double d1321 = p13.X * p21.X + p13.Y * p21.Y + p13.Z * p21.Z;
        //    double d4343 = p43.X * p43.X + p43.Y * p43.Y + p43.Z * p43.Z;
        //    double d2121 = p21.X * p21.X + p21.Y * p21.Y + p21.Z * p21.Z;

        //    double denom = d2121 * d4343 - d4321 * d4321;
        //    if (Math.Abs(denom) < 0.0001d)
        //    {
        //        TJournalLog.WriteLog("Error E0000");
        //        List<Vector3d> Zero = new List<Vector3d>();
        //        List<Vector3d> Zero1 = new List<Vector3d>();
        //        return (Zero, Zero1);
        //    }
        //    double numer = d1343 * d4321 - d1321 * d4343;

        //    double mua = numer / denom;
        //    double mub = (d1343 + d4321 * (mua)) / d4343;

        //    Vector3d resultSegmentPoint1 = new Vector3d(p1.X + mua * p21.X, p1.Y + mua * p21.Y, p1.Z + mua * p21.Z);
        //    Vector3d resultSegmentPoint2 = new Vector3d(p3.X + mub * p43.X, p3.Y + mub * p43.Y, p3.Z + mub * p43.Z);

        //    //Здесь расходятся пути, или у нас 1 точка пересечения или у нас отрезок пересечения, т.е 2 точки
        //    if (Math.Abs(resultSegmentPoint1.X - resultSegmentPoint2.X) < 0.0001d && Math.Abs(resultSegmentPoint1.Y - resultSegmentPoint2.Y) < 0.0001d && Math.Abs(resultSegmentPoint1.Z - resultSegmentPoint2.Z) < 0.0001d)
        //    {
        //        PointsBezie.AddRange(BezieThreePoints(line1PointStart, resultSegmentPoint1, line2PointStart, Step));
        //        PointKasat.Add(line1PointStart);
        //        PointKasat.Add(resultSegmentPoint1);
        //        PointKasat.Add(line2PointStart);
        //    }
        //    else
        //    {
        //        PointsBezie.AddRange(BezieFourPoints(line1PointStart, resultSegmentPoint1, resultSegmentPoint2, line2PointStart, Step));
        //        PointKasat.Add(line1PointStart);
        //        PointKasat.Add(resultSegmentPoint1);
        //        PointKasat.Add(resultSegmentPoint2);
        //        PointKasat.Add(line2PointStart);

        //    }
        //    return (PointsBezie, PointKasat);
        //}
        ////----------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Построение кривой Безье по 3 точкам
        ///// </summary>
        ///// <param name="Point1">1 точка</param>
        ///// <param name="Point2">Точка, которая лежит на пересечении векторов скоростей</param>
        ///// <param name="Point3">2 точка</param>
        ///// <param name="Step">Шаг</param>
        ///// <returns></returns>
        //public static List<Vector3d> BezieThreePoints(Vector3d Point1, Vector3d Point2, Vector3d Point3, double Step)
        //{
        //    List<Vector3d> PointsBezie = new List<Vector3d>();
        //    for (double t = 0; t < 1; t += Step)
        //    {
        //        Vector3d Point = new Vector3d(Math.Pow(1 - t, 2) * Point1 + 2 * (1 - t) * t * Point2 + t * t * Point3);
        //        PointsBezie.Add(Point);
        //    }
        //    return PointsBezie;
        //}
        ////----------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Построение кривой Безье по 4 точкам
        ///// </summary>
        ///// <param name="Point1">1 точка</param>
        ///// <param name="Point2">Точка, которая лежит на первом векторе скорости на отрезке пересечении векторов скоростей</param>
        ///// <param name="Point3">Точка, которая лежит на втором векторе скорости на отрезке пересечении векторов скоростей</param>
        ///// <param name="Point4">2 точка</param>
        ///// <param name="Step">Шаг</param>
        ///// <returns></returns>
        //public static List<Vector3d> BezieFourPoints(Vector3d Point1, Vector3d Point2, Vector3d Point3, Vector3d Point4, double Step)
        //{
        //    List<Vector3d> PointsBezie = new List<Vector3d>();
        //    for (double t = 0; t < 1; t += Step)
        //    {
        //        Vector3d Point = new Vector3d(Math.Pow(1 - t, 3) * Point1 + 3 * Math.Pow(1 - t, 2) * t * Point2 + 3 * (1 - t) * t * t * Point3 + t * t * t * Point4);
        //        PointsBezie.Add(Point);
        //    }
        //    return PointsBezie;
        //}
        ////----------------------------------------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Получение угла в градусах между векторами
        ///// </summary>
        ///// <param name="Vec1"></param>
        ///// <param name="Vec2"></param>
        ///// <returns>Угл в градусах</returns>
        //internal double AngleDegree(Vector3d Vec1, Vector3d Vec2)
        //{
        //    // Проверить находится ли значения длин векторов в допустимых пределах
        //    if (Vec1.X == 0d && Vec1.Y == 0d && Vec1.Z == 0d)
        //    {
        //        TJournalLog.WriteLog("C0120: Error TImporterFbx:AngleDegree(): this method support non zero vector. One of vectors = (0;0;0), FileName: ");
        //        return -1d;
        //    }
        //    else if (Vec2.X == 0d && Vec2.Y == 0d && Vec2.Z == 0d)
        //    {
        //        TJournalLog.WriteLog("C0120: Error TImporterFbx:AngleDegree(): this method support non zero vector. One of vectors = (0;0;0), FileName: ");
        //        return -1d;
        //    }
        //    //
        //    Vec1.Normalize();
        //    Vec2.Normalize();
        //    //
        //    double CosAlpha = (Vec1.X * Vec2.X + Vec1.Y * Vec2.Y +
        //                     Vec1.Z * Vec2.Z) / (Vec1.Length * Vec2.Length);
        //    // Проверить находится ли значение косинуса в допустимых пределах
        //    if (CosAlpha > 1d)
        //        CosAlpha = 1d;
        //    else if (CosAlpha < -1d)
        //        CosAlpha = -1d;
        //    // Вернуть значение угла переведенное в градусы       
        //    return (Math.Acos(CosAlpha) * 180.0d / Math.PI);
        //}
        #endregion
    }
}

