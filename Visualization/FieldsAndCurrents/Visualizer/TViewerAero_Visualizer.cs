// Класс для визуализации моделей и расчетов
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
//
using AstraEngine;
using AstraEngine.Components;
using AstraEngine.Engine.GraphicCore;
using AstraEngine.Geometry.Model3D;
using AstraEngine.Render;
using AstraEngine.Scene;
//***************************************************************
namespace Example
{
    /// <summary>
    /// Класс для визуализации моделей и расчетов
    /// </summary>
    public partial class TViewerAero_Visualizer
    {
        /// <summary>
        /// Лист, в котором хранятся все вспомогательные модели (кроме моделей, загруженных пользователем, и расчетной области), которые нужно каждый раз пересчитывать
        /// </summary>
        private List<TModel3D> HelpModels = new List<TModel3D>();
        /// <summary>
        /// Лист, в котором хранятся все вспомогательные модели (кроме моделей, загруженных пользователем, и расчетной области), которые досточно повернуть или переместить (оси координат, нормали и т.д)
        /// </summary>
        private List<TModel3D> HelpModels_Transformable = new List<TModel3D>();
        /// <summary>
        /// Лист, в которм хранятся модели для отрисовки расчетной области
        /// </summary>
        private List<TModel3D> DomainModels = new List<TModel3D>();
        /// <summary>
        /// Лист, в которм хранятся модели поверхностей
        /// </summary>
        private List<TModel3D> Surfaces = new List<TModel3D>();
        /// <summary>
        /// Таблица соответствия поверхностей с родительскими партами
        /// </summary>
        public List<(string parentPartID, string surfaceModelID)> ParentPartID_to_SurfaceID = new List<(string parentPartID, string surfaceModelID)>();
        /// <summary>
        /// Лист, в которм хранятся модели плоскостей
        /// </summary>
        private List<TModel3D> Planes = new List<TModel3D>();
        /// <summary>
        /// Лист, в которм хранятся модели линий тока
        /// </summary>
        public List<TModel3D> CurrentLines = new List<TModel3D>();
        /// <summary>
        /// Массив для хранения данных о серии линий тока
        /// </summary>
        public TFemElement_Visual[][][] SeriesCurrentLines;
        /// <summary>
        /// Массив, хранящий в себе цвета для серии линий тока
        /// </summary>
        public float[] Absolute;
        /// <summary>
        /// Лист, в которм хранятся точки, от которых отрисовываются линии тока 
        /// </summary>
        private List<TModel3D> PointsForCurrentLines = new List<TModel3D>();
        /// <summary>
        /// Лист, в которм хранятся модели для отрисовки ребер ячеек
        /// </summary>
        private List<TModel3D> Edges = new List<TModel3D>();
        /// <summary>
        /// Рендер
        /// </summary>
        private TRender Render = TStaticContent.Content.Game.Render;
        /// <summary>
        /// Толщина линий, образуюших BB (от размера BB зависят толщины линий почти каждого объекта)
        /// </summary>
        private float BBSize=-1f;
        /// <summary>
        /// Эмпирический коэффициент, позволяющий изменять толщину линии BB по вкусу пользователя
        /// </summary>
        public float BBSizeK;
        /// <summary>
        /// Эмпирический коэффициент, позволяющий изменять толщину линии ребер сетки по вкусу пользователя
        /// </summary>
        public float EdgesOfCellsSizeK;
        /// <summary>
        /// Эмпирический коэффициент, позволяющий изменять толщину линии тока по вкусу пользователя
        /// </summary>
        public float СurrentLinesSizeK;
        /// <summary>
        /// Цвет области
        /// </summary>
        public Color Colour_Domain = Color.FromArgb(255, 0, 113, 188);
        /// <summary>
        /// Цвет ребер сетки
        /// </summary>
        public Color Colour_EdgesOfCells = Color.Orange;
        /// <summary>
        /// Матрица для поверхностной модели
        /// </summary>
        public List<(string objectName, Matrix matrix)> SurfaceMatrices = new List<(string, Matrix)>();
        /// <summary>
        /// Флаг для сохранения первой матрицы для поверхностной модели
        /// </summary>
        public bool surfaceMatrixBool = false;
        // Цвет для точек линий тока
        public Color ColorForPointsCurrentLine = Color.Red;
        //---------------------------------------------------------------
        /// <summary>
        /// Создать для рендера
        /// </summary>
        /// <param name="BB">bounding box (BB)</param>
        /// <param name="BBSizeK">Коэффициент, влияющий на толщину линий BB</param>>
        /// <param name="EdgesOfCellsSizeK">Коэффициент, влияющий на толщину линий ребер сетки</param>
        /// <param name="СurrentLinesSizeK">Коэффициент, влияющий на толщину линий тока</param>
        private void SetBBSize(BoundingBox BB, float BBSizeK = 0.0025f, float EdgesOfCellsSizeK = 0.5f, float СurrentLinesSizeK = 0.5f)
        {
            float BBSize = Math.Max(BB.Max.X - BB.Min.X, Math.Max(BB.Max.Y - BB.Min.Y, BB.Max.Z - BB.Min.Z));
            this.BBSizeK = BBSizeK;
            this.EdgesOfCellsSizeK = EdgesOfCellsSizeK;
            this.СurrentLinesSizeK = СurrentLinesSizeK;
            this.BBSize = BBSizeK * BBSize;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать модель
        /// </summary>
        public TModel3D ModelRender(List<TModel3D> Scene, string Path, string FileName)
        {
            // Создаем тестовую модель
            TModel3D Model3D_Test = new TModel3D(Render);
            // Грузим тестовый файл
            Model3D_Test.ControlLoad.LoadModel(Path + FileName);
            List<TTriangleContainer> EasyModel = new List<TTriangleContainer>();
            EasyModel.Add(Model3D_Test.ControlOperations.Reduce(100));
            Model3D_Test.ControlLoad.LoadModel(EasyModel);
            Scene.Add(Model3D_Test);
            return Model3D_Test;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать расчетную область
        /// </summary>
        public void DomainRender(BoundingBox BB, List<Vector3[]> DomainEdges)
        {

            SetBBSize(BB);
            for (int i = 0; i < DomainEdges.Count; i++)
            {
                TModel3D Line = new TModel3D(Render);
                DomainModels.Add(Line);
                Line.ControlPrimitive.CreateLine(DomainEdges[i][0], DomainEdges[i][1], BBSize);
                Line.SetColour(Colour_Domain);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать поле на поверхности
        /// </summary>
        /// <param name="Surfaces">Список полигонов поверхности</param>
        /// <param name="AbsoluteMin">Минимальное значение величины в сетке</param>
        /// <param name="AbsoluteMax">Максимальное значение величины в сетке</param>
        public void DrawSurfaces(List<TTriangleContainer> Surfaces, string[] PartIDs, float AbsoluteMin, float AbsoluteMax)
        {
            ChangeUVMapAbsoluteMinMax(AbsoluteMin, AbsoluteMax);
            var Texture = CreateUVMap();
            for(int i = 0 ; i < Surfaces.Count; i++)
            {
                var M3D = new TModel3D(Render);
                this.Surfaces.Add(M3D);
                ParentPartID_to_SurfaceID.Add((PartIDs[i], M3D.ID.ToString()));
                M3D.ControlLoad.LoadModel(new List<TTriangleContainer> { Surfaces[i] });
                M3D.ControlTexture.SetTexture(Texture);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Заменить текстуру поля давления/скорости на поверхности в соответствии с заданными ограничениями
        /// </summary>
        /// <param name="Min">Минимальное значение величины, заданное пользователем</param>
        /// <param name="Max">Максимальное значение величины, заданное пользователем</param>
        public void ChangeSurfaces(float Min, float Max)
        {
            if(Surfaces.Count > 0)
            {
                for(int i=0; i< Surfaces.Count; i++)
                    Surfaces[i].ControlTexture.SetTexture(CreateUVMap(Min, Max));
            }
            else
            {
                TJournalLog.WriteLog("There are no surfaces.");
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать линии тока
        /// </summary>
        /// <param name="Points">Точки, по которым проходит линия тока</param>
        /// <param name="NumberFaces">Количество вершин основания цилиндров, из которых состоит линия тока</param>
        /// <param name="AbsoluteMin">Минимальное значение величины в сетке</param>
        /// <param name="AbsoluteMax">Максимальное значение величины в сетке</param>
        public void СurrentLinesRender(TFemElement_Visual[] Points, TViewerAero_CurrentLinesSettings CurrentLinesSettings, float AbsoluteMin, float AbsoluteMax)
        {
            ChangeUVMapAbsoluteMinMax(AbsoluteMin, AbsoluteMax);
            // Создаем объект линии тока в виде массива полигонов
            TViewerAero_HelperCurrentLines lines = new TViewerAero_HelperCurrentLines(this);
            List<TTriangleContainer> TRI = lines.CreateCurrentLines(Points, CurrentLinesSettings.Radius, CurrentLinesSettings.NumberFaces, AbsoluteMin, AbsoluteMax);
            if (TRI.Count <= 0) return;
            // Собираем линию тока из массива полигонов
            TModel3D CurrentLine = new TModel3D(Render);
            CurrentLines.Add(CurrentLine);
            CurrentLine.ControlLoad.LoadModel(TRI);
            // Разукрашиваем линию тока
            CurrentLine.ControlTexture.SetTexture(CreateUVMap());

            //for (int i = 1; i < Proba2.Length; i++)
            //{
            //    TModel3D Model3D_PR2 = new TModel3D(Render);
            //    HelpModels.Add(Model3D_PR2);
            //    Vector3 Start = new Vector3((float)Proba2[i - 1].Position.X, (float)Proba2[i - 1].Position.Y, (float)Proba2[i - 1].Position.Z);
            //    Vector3 Finish = new Vector3((float)Proba2[i].Position.X, (float)Proba2[i].Position.Y, (float)Proba2[i].Position.Z);
            //    Model3D_PR2.ControlPrimitive.CreateLine(Start, Finish, СurrentLinesSizeK * BBSize);
            //    Model3D_PR2.SetColour(Colour[i - 1]);
            //}
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализация точек, от которых отрисовываются линии тока
        /// </summary>
        /// <param name="Points">Список точек</param>
        /// <param name="Size">Размер</param>
        public void CreatePointsForCurrentLines(List<Vector3> Points, float Size=8f)
        {
            try
            {
                List<TTriangleContainer> PointsTriangle = new List<TTriangleContainer>();
                TViewerAero_Tetrahedron Tetra = new TViewerAero_Tetrahedron();
                for (int i=0; i<Points.Count; i++)
                {
                    var TRI = Tetra.ConvertingPointToTetrahedron(Points[i], new Vector3(0f, 1f, 0f), Size, new Vector4(128f, 0f, 128f, 0.5f));
                    PointsTriangle.Add(TRI);
                }
                TModel3D Tetrahedrons = new TModel3D(Render);
                PointsForCurrentLines.Add(Tetrahedrons);
                Tetrahedrons.ControlLoad.LoadModel(PointsTriangle);
                Tetrahedrons.ControlColour.SetColour(ColorForPointsCurrentLine);
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0013: Error TViewerAero_Visualizer:CreatePointsForCurrentLines(): " + E.Message);
            }

        }
        //------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Заменить цвета линий тока в соответствии с заданными ограничениями
        /// </summary>
        /// <param name="Min">Минимальное значение величины, заданное пользователем</param>
        /// <param name="Max">Максимальное значение величины, заданное пользователем</param>
        public void ChangeСurrentLines(float Min, float Max)
        {
            if (CurrentLines.Count > 0)
            {
                for (int i = 0; i < CurrentLines.Count; i++)
                    CurrentLines[i].ControlTexture.SetTexture(CreateUVMap(Min, Max));
            }
            else
            {
                TJournalLog.WriteLog("There are no current lines.");
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Отрисовать ребра ячеек
        /// </summary>
        /// <param name="FiniteElementModel">Описание конечно-элементной модели</param>
        public void DrawEdgesOfCells(BoundingBox BB, TFiniteElementModel_Visual FiniteElementModel)
        {
            try
            {
                SetBBSize(BB);
                // Массив листов индексов узлов
                // Под каждый узел свой лист в массиве, в каждом листе связанные с узлом узлы
                List<int>[] Grid = new List<int>[FiniteElementModel.Nodes.Length];
                foreach (var Element in FiniteElementModel.Elements)
                {
                    for (int i = 0; i < Element.Nodes.Length; i++)
                    {
                        if (Grid[Element.Nodes[i]] == null) Grid[Element.Nodes[i]] = new List<int>();
                        // проверяем узел справа от рассматриваемого
                        if (i + 1 < Element.Nodes.Length)
                        {
                            if(Element.Nodes[i + 1] > Element.Nodes[i])
                            {
                                // проверяем, что такой узел еще не был добавлен
                                if(Grid[Element.Nodes[i]].Contains(Element.Nodes[i + 1])) continue;
                                Grid[Element.Nodes[i]].Add(Element.Nodes[i + 1]);
                            }    
                        }
                        else
                        {
                            if (Element.Nodes[0] > Element.Nodes[i])
                            {
                                if (Grid[Element.Nodes[i]].Contains(Element.Nodes[0])) continue;
                                Grid[Element.Nodes[i]].Add(Element.Nodes[0]);
                            }   
                        }
                        // проверяем узел слева от рассматриваемого
                        if (i - 1 >= 0)
                        {
                            if (Element.Nodes[i - 1] > Element.Nodes[i])
                            {
                                if (Grid[Element.Nodes[i]].Contains(Element.Nodes[i - 1])) continue;
                                Grid[Element.Nodes[i]].Add(Element.Nodes[i - 1]);
                            } 
                        }
                        else
                        {
                            if (Element.Nodes[Element.Nodes.Length - 1] > Element.Nodes[i])
                            {
                                if (Grid[Element.Nodes[i]].Contains(Element.Nodes.Length - 1)) continue;
                                Grid[Element.Nodes[i]].Add(Element.Nodes[Element.Nodes.Length - 1]);
                            }    
                        }
                    }
                }

                List<Tuple<Vector3, Vector3>> Points = new List<Tuple<Vector3, Vector3>>();

                for (int i = 0; i < Grid.Length; i++)
                {
                    var StartNode = FiniteElementModel.Nodes[i];
                    for (int j = 0; j < Grid[i].Count; j++)
                    {
                        var EndNode = FiniteElementModel.Nodes[Grid[i][j]];
                        Points.Add(new Tuple<Vector3, Vector3>(new Vector3(StartNode.X, StartNode.Y, StartNode.Z), new Vector3(EndNode.X, EndNode.Y, EndNode.Z)));
                    }
                }
                TModel3D Model = new TModel3D(Render);
                Edges.Add(Model);
                Model.ControlPrimitive.CreateMultyLine(Points, EdgesOfCellsSizeK * BBSize);
                Model.SetColour(Colour_EdgesOfCells);
                Model.SetAlpha(0.5f);
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0013: Error TViewerAero_Visualizer:DrawEdgesOfCells(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Отрисовать ребра ячеек
        /// </summary>
        /// <param name="NodesOnPlane">Точки пересечения плоскости с ячейками конечно-элементной модели</param>
        public void DrawEdgesOfCellsOnPlane(BoundingBox BB, List<Vector3>[] NodesOnPlane)
        {
            try
            {
                SetBBSize(BB);
                List<Tuple<Vector3, Vector3>> Points = new List<Tuple<Vector3, Vector3>>();
                for (int i = 0; i < NodesOnPlane.Length; i++)
                {
                    if (NodesOnPlane[i] == null) continue;
                    for (int j = 0; j < NodesOnPlane[i].Count; j++)
                    {
                        if (j + 1 < NodesOnPlane[i].Count)
                        {
                            var StartNode = NodesOnPlane[i][j];
                            var EndNode = NodesOnPlane[i][j + 1];
                            Points.Add(new Tuple<Vector3, Vector3>(new Vector3(StartNode.X, StartNode.Y, StartNode.Z), new Vector3(EndNode.X, EndNode.Y, EndNode.Z)));
                        }
                        else
                        {
                            var StartNode = NodesOnPlane[i][j];
                            var EndNode = NodesOnPlane[i][0];
                            Points.Add(new Tuple<Vector3, Vector3>(new Vector3(StartNode.X, StartNode.Y, StartNode.Z), new Vector3(EndNode.X, EndNode.Y, EndNode.Z)));
                        }
                    }
                }
                TModel3D Model = new TModel3D(Render);
                Edges.Add(Model);
                Model.ControlPrimitive.CreateMultyLine(Points, EdgesOfCellsSizeK * BBSize);
                Model.SetColour(Colour_EdgesOfCells);
                Model.SetAlpha(0.5f);
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C001: Error TViewerAero_Visualizer:DrawEdgesOfCells(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать глобальную систему координат
        /// </summary>
        public void GlobalCoordinateSystemRender(BoundingBox BB)
        {
            SetBBSize(BB);
            TModel3D Model3D_Y = new TModel3D(Render);
            TModel3D Model3D_Z = new TModel3D(Render);
            TModel3D Model3D_X = new TModel3D(Render);
            HelpModels_Transformable.Add(Model3D_Y);
            HelpModels_Transformable.Add(Model3D_Z);
            HelpModels_Transformable.Add(Model3D_X);
            float Length = BBSize * 10f;
            float Thickness = 0.4f * BBSize;
            Model3D_Y.ControlPrimitive.CreateLine(new Vector3(0, 0, 0), new Vector3(0, Length, 0), Thickness);
            Model3D_Y.ControlColour.SetColour("Green");
            Model3D_Z.ControlPrimitive.CreateLine(new Vector3(0, 0, 0), new Vector3(0, 0, Length), Thickness);
            Model3D_Z.ControlColour.SetColour("Blue");
            Model3D_X.ControlPrimitive.CreateLine(new Vector3(0, 0, 0), new Vector3(Length, 0, 0), Thickness);
            Model3D_X.ControlColour.SetColour("Red");
        }
//---------------------------------------------------------------
        /// <summary>
        /// Переместить модель с отрсиванными полями на поверхности в позицию vec
        /// </summary>
        /// <param name="vec">Позиция в которую переместим модель</param>
        public bool MoveSurfaceModel(Vector3 vec, string objectName)
        {
            var surfaceName = ParentPartID_to_SurfaceID.Find(x => x.parentPartID == objectName).surfaceModelID;
            if(surfaceName == null) return false;
            Surfaces.Find(x => x.ID.ToString() == surfaceName).Position += vec;
            return true;
        }
        //---------------------------------------------------------------
    }
}
