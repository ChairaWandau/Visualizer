// Класс для управления секущей плоскостью
using System;
//
using AstraEngine;
using AstraEngine.Components;
using AstraEngine.Components.MathHelper;
using AstraEngine.Engine.GraphicCore;
using AstraEngine.Inputs;
//***************************************************************
namespace Example
{
    /// <summary>
    /// Класс для управления секущей плоскостью
    /// </summary>
    public partial class TViewerAero_Plane
    {
        /// <summary>
        /// Вспомогательный класс, хранящий методы, потребные для TViewerAero и TViewerAero_Plane
        /// </summary>
        protected TViewerAero_Helper TVA_Helper = new TViewerAero_Helper();
        /// <summary>
        /// Класс для отрисовки моделей и расчетов
        /// </summary>
        protected TViewerAero_Plane_Visualizer Visualizer;
        /// <summary>
        /// Плоскость, параллельная стороне BB с наибольшей площадью
        /// </summary>
        protected Plane basePlane = new Plane();
        /// <summary>
        /// Нормаль, определяющая текущую плоскость
        /// </summary>
        public Vector3 normal = new Vector3();
        /// <summary>
        /// Точка, определяющая текущую плоскость
        /// </summary>
        protected Vector3 position = new Vector3();
        /// <summary>
        /// Параллелепипед модели
        /// </summary>
        protected BoundingBox BB = new BoundingBox();
        /// <summary>
        /// Вкл/выкл обработку
        /// </summary>
        public bool Enable { get; private set; } = true;
        /// <summary>
        /// Диалог ожидания действий пользователя
        /// </summary>
        //public UWindow_ViewerAero_Plane_UI Window_UI { get; set; }
//---------------------------------------------------------------
        /// <summary>
        /// Класс для управления секущей плоскостью
        /// </summary>
        /// <param name="BB">Параллелепипед модели</param>
        public TViewerAero_Plane(BoundingBox BB)
        {
            PlaneUpdate(BB);
            // События для перемещения плоскости
            Initialize_EventsKeyboard();
            // Защита от вылетов
            //TStaticEngine.GraphicWindow.Window.Invoke((Action)(() =>
            //{
            //    Window_UI = new UWindow_ViewerAero_Plane_UI(this, PositionWindow);
            //    if (EnableUserUI) Window_UI.Show();
            //}));
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Обновить параметры плоскости и визуализатор
        /// </summary>        
        public void PlaneUpdate(BoundingBox BB)
        {
            this.BB = BB;
            // Задаем базовую плоскость (параллельную стороне BB с наибольшей площадью)
            if (this.normal == Vector3.Zero)
            {
                this.position = (BB.Max - BB.Min) / 2f + BB.Min;
                Set_InitialNormal();
                basePlane = Get_CurrentPlane();
            }
            if (this.Visualizer != null) this.Visualizer.DisposeFromRender();
            this.Visualizer = new TViewerAero_Plane_Visualizer(BB);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Включить систему
        /// </summary>        
        public void Enable_ControlPlane()
        {
            this.Enable = true;
            Display_Plane(Get_CurrentPlane());
            // Защита от вылетов
            //TStaticEngine.GraphicWindow.Window.Invoke((Action)(() =>
            //{
            //    Window_UI = new UWindow_ViewerAero_Plane_UI(this, PositionWindow);
            //    if (EnableUserUI) Window_UI.Show();
            //}));
        }
//---------------------------------------------------------------
        /// <summary>
        /// Выключить систему (отображение и обработку)
        /// </summary>
        public void Disable_ControlPlane()
        {
            this.Enable = false;
            // Защита от вылетов
            TStaticEngine.GraphicWindow.Window.Invoke((Action)(() =>
            {
                // Удаляем плоскость отрисованную до этого
                Visualizer.DisposeHelpModelsFromRender();
                //Удаляем оси, нормали и т.д.
                Visualizer.DisposeHelpModelsTransformableFromRender();
            }));
            //Window_UI.Close();
        }
//---------------------------------------------------------------
        /// <summary>
        /// Получить в начальном положении инструмент плоскости посередине, парраллельно плоскости Domain с наибольшей площадью (если площадь одинаковая - любой плоскости)
        /// </summary>
        /// <returns>Плоскость, парраллельная плоскости Domain с наибольшей площадью</returns>
        public Plane Get_BasePlane()
        {
            normal = basePlane.Normal;
            position = (BB.Max - BB.Min) / 2f + BB.Min;
            return basePlane;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получить плоскость в текущем положении
        /// </summary>
        /// <returns></returns>
        public Plane Get_CurrentPlane()
        {
            var D = 0f - Vector3.Dot(this.normal, this.position);
            return new Plane(this.normal, D);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Сравнить стороны расчетной области по площади и построить нормаль к строне с наибольшей площадью
        /// </summary>
        private void Set_InitialNormal()
        {
            float BB_Depth = BB.Max.Z - BB.Min.Z;
            float XYArea = BB.Width * BB.Height;
            float ZXArea = BB_Depth * BB.Width;
            float YZArea = BB.Height * BB_Depth;
            float MaxArea = Math.Max(Math.Max(XYArea, ZXArea), YZArea);
            if (XYArea == MaxArea) this.normal = new Vector3(0f, 0f, 1f);
            else if (ZXArea == MaxArea) this.normal = new Vector3(0f, 1f, 0f);
            else this.normal = new Vector3(1f, 0f, 0f);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Отобразить плоскость (линиями)
        /// </summary>
        /// <param name="Plane">Плоскость, которая будет нарисована</param>
        public void Display_Plane(Plane Plane)
        {
            // Защита от вылетов
            TStaticEngine.GraphicWindow.Window.Invoke((Action)(() =>
            {
                // Удаляем плоскость отрисованную до этого
                Visualizer.DisposeHelpModelsFromRender();
                //Удаляем оси, нормали и т.д.
                Visualizer.DisposeHelpModelsTransformableFromRender();
                // Рисуем локальную систему координат
                Visualizer.PlaneNormalRender(this.position, this.normal, TVA_Helper.GetNormalLength(BB, this.normal));
                try
                {
                    // Рисуем плоскость линиями по контуру
                    Visualizer.LinePlaneRender(TVA_Helper.GetMinMaxPoints(Plane, BB));
                }
                catch (Exception E)
                {
                    TJournalLog.WriteLog("C0017: Error TViewerAero_Plane:Display_Plane(): " + E.Message);
                }
            }));
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Вращать/двигать плоскость
        /// </summary>
        /// <param name="dRotation">вектор поворота</param>
        /// <param name="Delta">перемещение перпендикулярно плоскости</param>
        public Plane MovePlane(Vector3 dRotation, float Delta)
        {
            try
            {
                // Перемещение
                if (Delta != 0f) position += this.normal * Delta;

                // Составляем матрицу вращения плоскости (сначала по х, потом по у, затем по z) (в точности, как с fbx)
                Matrix MatrixRotationX = Matrix.CreateFromYawPitchRoll(0, TMath.Angle_ToRadians(dRotation.X), 0);
                Matrix MatrixRotationY = Matrix.CreateFromYawPitchRoll(TMath.Angle_ToRadians(dRotation.Y), 0, 0);
                Matrix MatrixRotationZ = Matrix.CreateFromYawPitchRoll(0, 0, TMath.Angle_ToRadians(dRotation.Z));
                Matrix MatrixRotation = MatrixRotationX * MatrixRotationY * MatrixRotationZ;

                // Поворачиваем нормаль
                this.normal = Vector3.Transform(this.normal, MatrixRotation);
                this.normal.Normalize();


                // Под удаление/изменение
                /*var MinMax = VA_Helper.GetMinMaxPoints(Get_CurrentPlane(), this.BB);
                // Определяем оси
                Vector3 NewX = (MinMax.XY + MinMax.XO) / 2 - this.position;
                NewX.Normalize();
                Vector3 NewY = (MinMax.XY + MinMax.OY) / 2 - this.position;
                NewY.Normalize();*/

                // Возвращаем текущую плоскость(измененную)
                return Get_CurrentPlane();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0011: Error TViewerAero_Plane:MovePlane(): " + E.Message);
                return Get_CurrentPlane();
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Вращение в связанной с плоскостью системе координат, не учитывая вращение вокруг оси перпендикулярной плоскости, с помощью клавиатуры
        /// </summary>
        /// <param name="MoveRight">Движение вдоль нормали по ее направлению</param>
        /// <param name="MoveLeft">Движение вдоль нормали в обратном направлении</param>
        /// <param name="RotationXRight">Кнопка поворота объекта около оси Х против часовой стрелки(ед вектор оси Х смотрит на нас)</param>
        /// <param name="RotationXLeft">Кнопка поворота объекта около оси Х по часовой стрелке(ед вектор оси Х смотрит на нас)</param>
        /// <param name="RotationYRight">Кнопка поворота объекта около оси Y против часовой стрелки(ед вектор оси Y смотрит на нас)</param>
        /// <param name="RotationYLeft">Кнопка поворота объекта около оси Y по часовой стрелке(ед вектор оси Y смотрит на нас)</param>
        /// <param name="RotationZRight">Кнопка поворота объекта около оси Z против часовой стрелки(ед вектор оси Z смотрит на нас)</param>
        /// <param name="RotationZLeft">Кнопка поворота объекта около оси Z по часовой стрелке(ед вектор оси Z смотрит на нас)</param>
        public void Set_Keyboard_MovePlane(EButtonKeyboard MoveRight, EButtonKeyboard MoveLeft,
        EButtonKeyboard RotationXRight, EButtonKeyboard RotationXLeft,
        EButtonKeyboard RotationYRight, EButtonKeyboard RotationYLeft,
        EButtonKeyboard RotationZRight, EButtonKeyboard RotationZLeft)
        {
            Button_Keyboard_RotationX = RotationXRight;
            Button_Keyboard_RotationX_Reversed = RotationXLeft;
            //
            Button_Keyboard_RotationY = RotationYRight;
            Button_Keyboard_RotationY_Reversed = RotationYLeft;
            //
            Button_Keyboard_RotationZ = RotationZRight;
            Button_Keyboard_RotationZ_Reversed = RotationZLeft;
            //
            Button_Keyboard_MoveByNormal = MoveRight;
            Button_Keyboard_MoveByNormal_Reversed = MoveLeft;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Вращение в связанной с плоскостью системе координат, не учитывая вращение вокруг оси перпендикулярной плоскости, с помощью мыши
        /// </summary>
        /// <param name="Move"></param>
        /// <param name="RotationX"></param>
        /// <param name="RotationY"></param>
        public void Set_Mouse_MovePlane(EButtonMouse Move, EButtonMouse RotationX, EButtonMouse RotationY)
        {
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Cоздает кубик с системой координат, двигая который пользователь вращает плоскость
        /// </summary>
        public void Set_Visual_MovePlane()
        {
            //Для итого используйте:
            //Model3D.ControlFrontend.Create_AxisSystem...
            //или
            //TAxisSystem и связанные с этими классами события движения
        }
        //---------------------------------------------------------------
    }
}
