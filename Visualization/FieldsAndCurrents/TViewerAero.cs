// Структура класса визуализатора
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//
using AstraEngine;
using AstraEngine.Components;
using AstraEngine.Engine.GraphicCore;
using AstraEngine.Geometry.Model3D;
using AstraEngine.Scene;
//
using MPT707.Aerodynamics.Structs;
using MPT707.Frontend;
//***************************************************************
namespace Example
{
    public partial class TViewerAero
    {
        /// <summary>
        /// Вспомогательный класс, хранящий методы, потребные для TViewerAero и TViewerAero_Plane
        /// </summary>
        private TViewerAero_Helper VA_Helper = new TViewerAero_Helper();
        /// <summary>
        /// Класс для отрисовки моделей и расчетов
        /// </summary>
        private TViewerAero_Visualizer Visualizer = new TViewerAero_Visualizer();
        /// <summary>
        /// Класс управления сохранением результатов расчета
        /// </summary>
        public TResultsManager ResultsManager = new TResultsManager();
        /// <summary>
        /// Класс для отрисовки шкалы 2D
        /// </summary>
        public TViewerAero_Scale2D Scale2D;
        /// <summary>
        /// Объект сцены, содержащий все модели
        /// </summary>
        public List<TModel3D> Scene;
        /// <summary>
        /// Конечно элементная модель для внутриклассового пользования
        /// </summary>
        private TFiniteElementModel_Visual FEM_V;
        /// <summary>
        /// Получить или присвоить конечно эллементную модель
        /// </summary>
        public TFiniteElementModel_Visual FiniteElementModel_Visual
        { 
            get 
            { 
                return FEM_V;
            } 
            set 
            {
                FEM_V = value;
                if (Save)
                {
                    ResultsManager.RememberResult(FEM_V);
                }
            } 
        }
        /// <summary>
        /// Флаг для указания, нужно ли сохранять результаты расчета
        /// </summary>
        public bool Save = false;
//---------------------------------------------------------------
        /// <summary>
        /// Контроллер управления плоскостью сечения для вывода визуализации на выбранную плоскость
        /// </summary>
        public TViewerAero_Plane ControlPlane;
        /// <summary>
        /// BB модели
        /// </summary>
        public BoundingBox ModelBB; 
//---------------------------------------------------------------
        /// <summary>
        /// Создать для рендера
        /// </summary>
        public TViewerAero()
        {
        }
//---------------------------------------------------------------
        /// <summary>
        /// Включить управление пользователем плоскостью для вывода визуализации на выбранную плоскость. Создать, сдвинуть и нарисовать плоскость
        /// </summary>
        /// <param name="EnableUserUI">Включить меню пользователя для действий с плоскостью</param>
        public void Enable_ControlPlane(BoundingBox Box)
        {
            // Создаем экземляр класса для управления плоскостью
            if (ControlPlane == null)
            {
                ControlPlane = new TViewerAero_Plane(Box);
            }
            else
            {
                ControlPlane.PlaneUpdate(Box);
            }
            ControlPlane.Enable_ControlPlane();
        }
//---------------------------------------------------------------
        /// <summary>
        /// Выключить управление пользователем плоскостью для вывода визуализации на выбранную плоскость.
        /// </summary>
        public void Disable_ControlPlane()
        {
            if (ControlPlane != null) ControlPlane.Disable_ControlPlane();
        }
//---------------------------------------------------------------
        /// <summary>
        /// Удалить, в том числе из рендера
        /// </summary>
        /// <param name="Scene">сцена из моделей</param>
        public void Dispose(List<TModel3D> Scene)
        {
            try
            {
                if (Visualizer != null) Visualizer.DisposeFromRender(Scene);
            }
            catch
            {

            }
        }
//---------------------------------------------------------------
        /// <summary>
        /// Инициализировать данные, по которым будем строить визуализацию  
        /// </summary>
        /// <param name="Scene">сцена из моделей</param>
        public void Initialize(List<TModel3D> Scene)
        {
            this.Scene = Scene;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Загрузить данные, по которым будем строить визуализацию из внешнего файла
        /// </summary>
        /// <param name="Path">Путь к файлу, хранящему данные о сетке расчетной области</param>
        /// <param name="Points">Узлы сетки расчетной области</param>
        //public void LoadFromFile(string Path, List<TFEMFluidFrame> Points)
        //{
        //    try
        //    {
        //        TViewerAero_Parser Parser = new TViewerAero_Parser();
        //        // Выбор парсера по расширению файла
        //        switch (System.IO.Path.GetExtension(Path).ToLower())
        //        {
        //            case "txt":
        //                Parser.ParseInSingleThread(Path, Points);
        //                break;
        //            case "csv":
        //                Parser.ParseInSingleThread(Path, Points);
        //                break;
        //            case "geo":
        //                break;
        //            case "geo":
        //                break;
        //            case "geo":
        //                break;
        //            case "geo":
        //                break;
        //            case "geo":
        //                break;
        //            default:
        //                throw new Exception("Such file format is not supported \"." + System.IO.Path.GetExtension(Path) + "\"");
        //                break;
        //        }
        //        //Parser.ParseInSingleThread(Path, System.IO.File.ReadAllLines(Path), Points);


        //        //Parser.ParseInParallel(Path, System.IO.File.ReadAllLines(Path), Points);
        //        //Parser.ParseInParallelArr(Path, System.IO.File.ReadAllLines(Path), Points);
        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0001: Error TViewerAero:LoadFromFile(): " + E.Message);
        //    }
        //}
        //---------------------------------------------------------------
        /// <summary>
        /// Отобразить расчетную область 
        /// </summary>
        /// <param name="FiniteEM">Конечно-элементная модель</param>
        public void Display_Domain(TFiniteElementModel_Visual FiniteEM)
        {
            try
            {
                // Определяем расчетную область
                Define_Domain(FiniteEM);
                //Рисуем расчетную область в виде проволочного каркаса
                //Visualizer = new TViewerAero_Visualizer(BB);
                //Visualizer.DomainRender(BB, DomainEdges);
                // Отрисовка глобальной системы координат
                //Visualizer.GlobalCoordinateSystemRender();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0002: Error TViewerAero:Display_Domain(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Отобразить на поверхности
        /// </summary>
        /// <param name="eTypeValueAero">Интересующая физическая величина</param>
        /// <param name="Surfaces">Массив названий объектов, на поверхности которых нужно визуализировать</param>
        internal void Display_OnSurface(ETypeValueAero eTypeValueAero, params string[] Surfaces)
        {
            try
            {
                // Отключить секущую плоскость, если она была создана
                if (ControlPlane != null) Disable_ControlPlane();
                // Удаляем 2D шкалу, если она была создана
                if (Scale2D != null) Scale2D.DisposeScale2D();

                // Проверяем, что список присланных поверехностей не пуст. Если пуст, то рисуем все.
                if (Surfaces.Length < 1)
                {
                    var AllSurfaces = new List<string>();
                    foreach (var surface in FEM_V.Surfaces)
                    {
                        AllSurfaces.Add(surface.ObjectName);
                    }
                    Surfaces = AllSurfaces.ToArray();
                }

                // Получаем минимальное и максимальное значение на поверхностях
                var MinMax = GetMinimumAndMaximumForSurfaces(eTypeValueAero, Surfaces);

                // Список контейнеров. Каждый контейнер - поверхность
                var SurfacesInContainers = new List<TTriangleContainer>();
                // Составляем список траэнглов
                foreach (var surface in Surfaces)
                {
                    SurfacesInContainers.Add(CreateTTriangleContainerFromSurface(eTypeValueAero, surface, MinMax));
                }
                // Отправляем в отрисовку
                Visualizer.DrawSurfaces(SurfacesInContainers, Surfaces, MinMax.X, MinMax.Y);
                // Создание объекта шкалы 2D
                Scale2D = new TViewerAero_Scale2D();
                Scale2D.CreateScale2D(GetScaleField(new Vector2(1920, 1080), MinMax.X, MinMax.Y));
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0004: Error TViewerAero:DisplayPressure_OnSurface(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Отобразить поле в сечении, созданном Value, заливкой цветом
        /// </summary>
        /// <param name="eTypeValueAero">Интересующая характеристика</param>
        /// <param name="Value">Плоскость</param>
        /// <param name="DisplayResolution">Разрешение текстуры</param>
        /// <param name="Min">Значение, которому будет соответствовать на шкале HSV 0 Темносиний</param>
        /// <param name="Max">Значение, которому будет соответствовать на шкале HSV 240 Красный</param>
        internal void Display_OnPlane(ETypeValueAero eTypeValueAero, Plane Value, /*TFiniteElementModel_Visual FiniteEM, */Vector2 DisplayResolution, float Min = float.MinValue, float Max = float.MaxValue)
        {
            try
            {
                // Включить отображение плоскости построения
                Enable_ControlPlane(ModelBB);
                var FiniteEM = FEM_V;
                // Удаляем 2D шкалу, если она была создана
                if (Scale2D != null) Scale2D.DisposeScale2D();
                // Получаем поле на плоскости
                ((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, float Min, float Max, float[,] PlanePointsCharacteristic) Results = CalculationsOnPlane(eTypeValueAero, Value, /*FiniteEM,*/ DisplayResolution);
                if (Results.PlanePointsCharacteristic != null)
                {
                    // Проверяем введенные пользователем значения максимума и минимума
                    if (Results.Min > Min) Min = Results.Min;
                    if (Results.Max < Max) Max = Results.Max;

                    // Отрисовка плоскости и шкалы 2D
                    Visualizer.PlaneWithScaleRender(DisplayResolution, Min, Max, Results.MinMaxPoints, Visualizer.FieldTextureRender(Visualizer.ColorRender(DisplayResolution, Results.PlanePointsCharacteristic, Min, Max)));
                    // Visualizer.PlaneRender(Normal, MinMaxPoints, Visualizer.FieldTextureRender(Visualizer.ColorRender(DisplayResolution, PlanePointsCharacteristic, (float)Min, (float)Max)));
                    // Visualizer.ScaleRender(DisplayResolution, (float)Min, (float)Max, MinMaxPoints);
                    // Создание объекта шкалы 2D
                    Scale2D = new TViewerAero_Scale2D();
                    Scale2D.CreateScale2D(GetScaleField(DisplayResolution, Min, Max));
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0008: Error TViewerAero:Display_OnPlane(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Отрисовка множества полей в виде слайд-шоу
        /// </summary>
        /// <param name="eTypeValueAero">Интересующая характеристика</param>
        /// <param name="Value">Плоскость</param>
        /// <param name="DisplayResolution">Разрешение отрендеренной текстуры полей</param>
        internal void SlideShow(ETypeValueAero eTypeValueAero, Plane Value, Vector2 DisplayResolution)
        {
            try
            {
                if (Visualizer.ResultsForSeriesOfCalculation == null)
                {
                    // Получаем текстуры и точки, по коротым будут построены плоскости, для множества расчетов
                    ((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, TTexture2D Textures)[] MinMaxPlanePointsAndTextures =
                        SeriesOfCalculationsOnPlane(eTypeValueAero, Value, DisplayResolution);
                    if (MinMaxPlanePointsAndTextures != null)
                        // Отрисовываем поля
                        Visualizer.SeriesOfCalculationRender(Value.Normal, MinMaxPlanePointsAndTextures);
                }
                else
                {
                    // Отрисовываем поля
                    Visualizer.SeriesOfCalculationRender(Value.Normal, Visualizer.ResultsForSeriesOfCalculation);
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C00012: Error TViewerAero:SlideShow(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Слайд-шоу для линий тока
        /// </summary>
        /// <param name="BasePoints">Список базовых точек</param>
        /// <param name="CurrentLinesSettings">Настройки для линий тока</param>
        /// <param name="Timeout">Таймаут</param>
        public void SlideShow_CurrentLines (List<Vector3> BasePoints, TViewerAero_CurrentLinesSettings CurrentLinesSettings, int Timeout=2)
        {
            try
            {
                // Если для визуализации еще не были расчитаны линии тока
                if (Visualizer.SeriesCurrentLines == null)
                {
                    var (Points, Absoluty) = SeriesOfCalculationForCurrentLines(BasePoints, CurrentLinesSettings);
                    Visualizer.SeriesCurrentLines = Points;
                    Visualizer.Absolute = Absoluty;
                    Visualizer.SeriesForCurrentLinesVisualizer(CurrentLinesSettings);
                }
                else
                {
                    // Если количество расчетов поменялось, то считает заново
                    if (Visualizer.SeriesCurrentLines.Length != ResultsManager.GetNumberOfResults)
                    {
                        var (Points, Absoluty) = SeriesOfCalculationForCurrentLines(BasePoints, CurrentLinesSettings);
                        Visualizer.SeriesCurrentLines = Points;
                        Visualizer.Absolute = Absoluty;
                        Visualizer.SeriesForCurrentLinesVisualizer(CurrentLinesSettings);
                    }
                    else
                    {
                        // Если ничего не поменялось, то по старым расчетам рисуем визуализацию
                        Visualizer.SeriesForCurrentLinesVisualizer(CurrentLinesSettings);
                    }

                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C00012: Error TViewerAero:SlideShow_CurrentLines(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Отобразить линии тока
        /// </summary>
        /// <param name="Base">Точки, через которые будут построены линии тока</param>
        /// <param name="NumberFaces">Количество вершин основания цилиндров, из которых состоит линия тока (минимум 2)</param>
        public void Display_СurrentLines(/*List<TFemElement_Visual> Points,*/ List<Vector3> Base, TViewerAero_CurrentLinesSettings CurrentLinesSettings)
        {
            // Отключить секущую плоскость, если она была создана
            if (ControlPlane != null) Disable_ControlPlane();
            // Удаляем 2D шкалу, если она была создана
            if (Scale2D != null) Scale2D.DisposeScale2D();
            // Определяем расчетную область
            Display_Domain(FEM_V);
            TFemElement_Visual[] BasePoint = TransferVector3BasePoint(Base);
            // Массив массивов с линями тока
            TFemElement_Visual[][] CurrentLines = new TFemElement_Visual[Base.Count][];
            Parallel.For(0, CurrentLines.Length, I =>
            {
                TFemElement_Visual[] ArrayPoints = CreateCurrentLine(BasePoint[I], CurrentLinesSettings);
                if (ArrayPoints.Length == 0) CurrentLines[I]=null;
                CurrentLines[I] = ArrayPoints;
            });
            // Получаем максимальные и минимальные значения характеристики
            float AbsoluteMax = float.MinValue;
            float AbsoluteMin = float.MaxValue;
            for (int i = 0; i < CurrentLines.Length; i++)
            {
                if (CurrentLines[i] == null) continue;
                for (int j = 0; j < CurrentLines[i].Length; j++)
                {
                    if (AbsoluteMax < CurrentLines[i][j].VelocityModule) AbsoluteMax = CurrentLines[i][j].VelocityModule;
                    if (AbsoluteMin > CurrentLines[i][j].VelocityModule) AbsoluteMin = CurrentLines[i][j].VelocityModule;
                }
            }
            for (int i = 0; i < CurrentLines.Length; i++)
            {
                if (CurrentLines[i] == null) continue;
                //Отрисовка линий тока
                Visualizer.СurrentLinesRender(CurrentLines[i], CurrentLinesSettings, AbsoluteMin, AbsoluteMax);
            }
            // Создание объекта шкалы 2D
            Scale2D = new TViewerAero_Scale2D();
            Scale2D.CreateScale2D(GetScaleField(new Vector2(1920, 1080), AbsoluteMin, AbsoluteMax));
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Отобразить грани сетки расчетной области
        /// </summary>
        /// <param name="FiniteEM">Конечно-элементная модель</param>
        public void Display_EdgesOfCells(/*TFiniteElementModel_Visual FiniteEM*/)
        {
            try
            {
                // Отключить секущую плоскость, если она была создана
                if (ControlPlane != null) Disable_ControlPlane();
                var FiniteEM = FEM_V;
                // Определяем наличие вершин в модели
                if (FiniteEM.Nodes == null)
                    TJournalLog.WriteLog("There is no Nodes in the model. Grid can't be displayed");
                else
                {
                    //Определяем расчетную область
                    Display_Domain(FiniteEM);
                    // Рисуем грани сетки 
                    Visualizer.DrawEdgesOfCells(BB,FiniteEM);
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C00010: Error TViewerAero:Display_EdgesOfCells(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Отобразить грани сетки расчетной области на плоскости
        /// </summary>
        /// <param name="FiniteEM">Конечно-элементная модель</param>
        public void Display_EdgesOfCellsOnPlane(Plane Value/*, TFiniteElementModel_Visual FiniteEM*/)
        {
            try
            {
                // Включить отображение плоскости построения
                Enable_ControlPlane(ModelBB);
                var FiniteEM = FEM_V;
                // Определяем наличие вершин в модели
                if (FiniteEM.Nodes == null)
                    TJournalLog.WriteLog("There is no Nodes in the model. Grid on plane can't be displayed");
                else
                {
                    // Определяем расчетную область
                    Display_Domain(FiniteEM);
                    // Проверяем, пересекает ли плоскость расчетную область
                    List<Vector3> IntersectionPoints = VA_Helper.LineWithPlaneIntersection(Value, DomainEdges);
                    if (IntersectionPoints.Count > 0)
                    {
                        // Массив точек пересечения ячеек модели с плоскостью (каждый новый лист в массиве - это ячейка, вектора в листе - точки пересечения ячейки с плоскостью)
                        List<Vector3>[] NodesOnPlane = new List<Vector3>[FiniteEM.Elements.Count()];
                        // Получаем все ячейки модели, пересекаемые плоскостью
                        for (int i = 0; i < FiniteEM.Elements.Count(); i++)
                        {
                            if (!VerticesByDifferentSides(Value, FiniteEM, i)) continue;
                            // Формируем список отрезков (ребер ячейки)
                            List<Vector3[]> Edges = new List<Vector3[]>();
                            for(int j = 0; j < FiniteEM.Elements[i].Nodes.Length; j++)
                            {
                                if (j + 1 < FiniteEM.Elements[i].Nodes.Length)
                                {
                                    Vector3 StartPoint = new Vector3(FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j]].X, FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j]].Y, FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j]].Z);
                                    Vector3 EndPoint = new Vector3(FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j + 1]].X, FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j + 1]].Y, FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j + 1]].Z);
                                    Edges.Add(new Vector3[2] { StartPoint, EndPoint });
                                }
                                else
                                {
                                    Vector3 StartPoint = new Vector3(FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j]].X, FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j]].Y, FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j]].Z);
                                    Vector3 EndPoint = new Vector3(FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[0]].X, FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[0]].Y, FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[0]].Z);
                                    Edges.Add(new Vector3[2] { StartPoint, EndPoint });
                                }
                            }
                            // Получаем точки пересечения ребер ячеек с плоскостью
                            NodesOnPlane[i] = VA_Helper.LineWithPlaneIntersection(Value, Edges);
                        }
                        // Отрисовываем сетку на плоскости
                        Visualizer.DrawEdgesOfCellsOnPlane(BB,NodesOnPlane);
                    }
                    else
                    {
                        TJournalLog.WriteLog("There is no intersection of the plane with the domain.");
                    }
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C00011: Error TViewerAero:Display_EdgesOfCellsOnPlane(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Создает текстуру шкалы и подписей. Объединяет их с текстурой полей. Создает Model3D_Control, на который натягивает текстуру.
        /// </summary>
        /// <param name="DisplayResolution">Разрешение отрендеренной текстуры полей</param>
        /// <param name="Min">Значение, которому будет соответствовать HSV 0 Темносиний</param>
        /// <param name="Max">Значение, которому будет соответствовать HSV 240 Красный</param>
        public TTexture2D GetScaleField(Vector2 DisplayResolution, float Min, float Max)
        {
            return Visualizer.GetScaleField(DisplayResolution, Min, Max);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Перерисовать визуализацию на поверхности в соответствии с заданными ограничениями на шкале
        /// </summary>
        /// <param name="Min">Значение, которому будет соответствовать HSV 0 Темносиний</param>
        /// <param name="Max">Значение, которому будет соответствовать HSV 240 Красный</param>
        internal void ChangeScaleMinMaxOnSurface(float Min, float Max)
        {
            // Удаляем 2D шкалу, если она была создана
            if (Scale2D != null) Scale2D.DisposeScale2D();
            Visualizer.ChangeSurfaces(Min, Max);
            // Создание объекта шкалы 2D
            Scale2D = new TViewerAero_Scale2D();
            Scale2D.CreateScale2D(GetScaleField(new Vector2(1920, 1080), Min, Max));
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Перерисовать визуализацию на плоскости в соответствии с заданными ограничениями на шкале
        /// </summary>
        /// <param name="eTypeValueAero">Интересующая характеристика</param>
        /// <param name="Value">Плоскость</param>
        /// <param name="DisplayResolution">Разрешение текстуры</param>
        /// <param name="Min">Значение, которому будет соответствовать HSV 0 Темносиний</param>
        /// <param name="Max">Значение, которому будет соответствовать HSV 240 Красный</param>
        internal void ChangeScaleMinMaxOnPlane(ETypeValueAero eTypeValueAero, Plane Value, Vector2 DisplayResolution, float Min, float Max)
        {
            Display_OnPlane(eTypeValueAero, Value, DisplayResolution, Min, Max);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Перерисовать визуализацию линиями тока в соответствии с заданными ограничениями на шкале
        /// </summary>
        /// <param name="Min">Значение, которому будет соответствовать HSV 0 Темносиний</param>
        /// <param name="Max">Значение, которому будет соответствовать HSV 240 Красный</param>
        internal void ChangeScaleMinMaxСurrentLines(float Min, float Max)
        {
            // Удаляем 2D шкалу, если она была создана
            if (Scale2D != null) Scale2D.DisposeScale2D();
            Visualizer.ChangeСurrentLines(Min, Max);
            // Создание объекта шкалы 2D
            Scale2D = new TViewerAero_Scale2D();
            Scale2D.CreateScale2D(GetScaleField(new Vector2(1920, 1080), Min, Max));
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Синхронезировать расположение геометрии по аэродинамике с пересчитанной геометрией динамики
        /// </summary>
        public void SyncNewDynamicPositionWithAerodynamic()
        {
            var Mods = TFrontend_MPT707.Get_AerodynamicModels3D();
            if (Visualizer == null) return; 
            // Если это первый результат расчетов 
            if (Visualizer.surfaceMatrixBool == false)
            {
                // Запись положений объектов для дальнейшего вычисления дельты(смещения)
                foreach ( var model in Mods )
                {
                    Visualizer.SurfaceMatrices.Add((model.Part_ID.ToString(), model.GetWorldMatrix()));
                }
                Visualizer.surfaceMatrixBool = true;
                return;
            }
            //
            foreach (var model in Mods)
            {
                var oldPosition = new Vector3();
                var q = new Quaternion();
                var s = new Vector3();
                var currentRecord = Visualizer.SurfaceMatrices.Find(x => x.objectName == model.Part_ID.ToString());
                currentRecord.matrix.Decompose(out s, out q, out oldPosition);
                var newPosition = new Vector3();
                model.GetWorldMatrix().Decompose(out s, out q, out newPosition);
                var DELTA = newPosition - oldPosition;
                //currentRecord.matrix = model.GetWorldMatrix();
                Visualizer.SurfaceMatrices.Remove(currentRecord);
                Visualizer.SurfaceMatrices.Add((model.Part_ID.ToString(), model.GetWorldMatrix()));

                if(!Visualizer.MoveSurfaceModel(DELTA, model.Part_ID.ToString()))
                {
                    var Part = TStaticContent.Content.GetPart_ID(model.Part_ID);
                    Visualizer.MoveSurfaceModel(DELTA, Part["ItemLabel"].Split('.')[1]);
                }
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Отрисовка точек, через которые пройдут линии тока
        /// </summary>
        /// <param name="Size">Размер тетраэдров</param>
        public void PointsForCurrentLine (float Size=8f)
        {
            // Создание списка точек
            var Points = CreatePointsForCurrentLine();
            // Визуализация этих точек
            Visualizer.CreatePointsForCurrentLines(Points, Size);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Сброс флагов отвечающих за смещение поверхностей во время связанного расчета
        /// </summary>
        public void ResetSyncFlags()
        {
            Visualizer.SurfaceMatrices.Clear();
            Visualizer.surfaceMatrixBool = false;
            Visualizer.ParentPartID_to_SurfaceID.Clear();
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Инициализировать массив результатов расчетов
        /// </summary>
        public void InitializeArrayOfResults(int NumberOfResults)
        {
            Visualizer.InitializeArrayOfResults(NumberOfResults);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Сохранить последний результат визуализации
        /// </summary>
        internal void SaveLastResultOfVisualisation(int IndexOfCalculation, ETypeViewAero Viewer_TypeOutAero)
        {
            Visualizer.SaveLastResultOfVisualisation(IndexOfCalculation, Viewer_TypeOutAero);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Вернуть результаты визуализации
        /// </summary>
        internal List<TModel3D>[] GetResultsOfVisualisation()
        {
            return Visualizer.GetResultsOfVisualisation();
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Обновить кадр
        /// </summary>
        public void UpdateFrame(int FrameID)
        {
            Visualizer.UpdateFrame(FrameID);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить результаты визуализации
        /// </summary>
        public void ClearResultsOfVisualisation()
        {
            Visualizer.ClearResultsOfVisualisation();
        }
        //---------------------------------------------------------------
        #region old
        ///// <summary>
        ///// Отобразить поле давления в сечении, созданном Value
        ///// </summary>
        ///// <param name="Value">Плоскость</param>
        ///// <param name="FiniteEM">Конечно-элементная модель</param>
        ///// <param name="DisplayResolution">Разрешение текстуры</param>
        //public void DisplayPressure_OnPlane(Plane Value, /*TFiniteElementModel_Visual FiniteEM,*/ Vector2 DisplayResolution)
        //{
        //    try
        //    {
        //        var FiniteEM = FEM_V;
        //        // Удаляем 2D шкалу, если она была создана
        //        if (Scale2D != null) Scale2D.DisposeScale2D();
        //        // Получаем поле давления на плоскости
        //        ((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, float Min, float Max, float[,] PlanePointsPressure) Results = DisplayOnPlane(Value, /*FiniteEM,*/ DisplayResolution, true, false);
        //        // Отрисовка плоскости и шкалы 2D
        //        if (Results.PlanePointsPressure != null)
        //        {
        //            Visualizer.PlaneWithScaleRender(DisplayResolution, Results.Min, Results.Max, Results.MinMaxPoints, Visualizer.FieldTextureRender(Visualizer.ColorRender(DisplayResolution, Results.PlanePointsPressure, Results.Min, Results.Max)));
        //            // Visualizer.PlaneRender(Normal, MinMaxPoints, Visualizer.FieldTextureRender(Visualizer.ColorRender(DisplayResolution, PlanePointsVelocityModule, (float)Min, (float)Max)));
        //            // Visualizer.ScaleRender(DisplayResolution, (float)Min, (float)Max, MinMaxPoints);
        //            // Создание объекта шкалы 2D
        //            Scale2D = new TViewerAero_Scale2D();
        //            Scale2D.CreateScale2D(GetScaleField(DisplayResolution, Results.Min, Results.Max));
        //        }
        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0007: Error TViewerAero:DisplayPressure_OnPlane(): " + E.Message);
        //    }
        //}
        ////---------------------------------------------------------------
        ///// <summary>
        ///// Отобразить поле скоростей в сечении, созданном Value, заливкой цветом
        ///// </summary>
        ///// <param name="Value">Плоскость</param>
        ///// <param name="FiniteEM">Конечно-элементная модель</param>
        ///// <param name="DisplayResolution">Разрешение текстуры</param>
        //public void DisplayVelocity_OnPlane(Plane Value, /*TFiniteElementModel_Visual FiniteEM, */Vector2 DisplayResolution)
        //{
        //    try
        //    {
        //        var FiniteEM = FEM_V;
        //        // Удаляем 2D шкалу, если она была создана
        //        if (Scale2D != null) Scale2D.DisposeScale2D();
        //        // Получаем поле скорости на плоскости
        //        ((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, float Min, float Max, float[,] PlanePointsVelocityModule) Results = DisplayOnPlane(Value, /*FiniteEM,*/ DisplayResolution, false, true);
        //        if (Results.PlanePointsVelocityModule != null)
        //        {
        //            // Отрисовка плоскости и шкалы 2D
        //            Visualizer.PlaneWithScaleRender(DisplayResolution, Results.Min, Results.Max, Results.MinMaxPoints, Visualizer.FieldTextureRender(Visualizer.ColorRender(DisplayResolution, Results.PlanePointsVelocityModule, Results.Min, Results.Max)));
        //            // Visualizer.PlaneRender(Normal, MinMaxPoints, Visualizer.FieldTextureRender(Visualizer.ColorRender(DisplayResolution, PlanePointsVelocityModule, (float)Min, (float)Max)));
        //            // Visualizer.ScaleRender(DisplayResolution, (float)Min, (float)Max, MinMaxPoints);
        //            // Создание объекта шкалы 2D
        //            Scale2D = new TViewerAero_Scale2D();
        //            Scale2D.CreateScale2D(GetScaleField(DisplayResolution, Results.Min, Results.Max));
        //        }
        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0008: Error TViewerAero:DisplayVelocity_OnPlane(): " + E.Message);
        //    }
        //}
        #endregion
        //---------------------------------------------------------------
    }
}
