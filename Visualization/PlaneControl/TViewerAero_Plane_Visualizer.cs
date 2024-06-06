//  Класс для визуализации плоскости и ее системы координат
using System;
using System.Drawing;
using System.Collections.Generic;
//
using AstraEngine.Scene;
using AstraEngine.Render;
using AstraEngine;
using AstraEngine.Geometry.Model3D;
using AstraEngine.Components;
using AstraEngine.Engine.GraphicCore;
//***************************************************************
namespace Example
{
    /// <summary>
    /// Класс для визуализации плоскости и ее системы координат
    /// </summary>
    public class TViewerAero_Plane_Visualizer
    {
        /// <summary>
        /// Рендер
        /// </summary>
        private TRender Render = TStaticContent.Content.Game.Render;
        /// <summary>
        /// Эмпирический коэффициент, позволяющий изменять толщину линии BB по вкусу пользователя
        /// </summary>
        public float BBSizeK;
        /// <summary>
        /// Эмпирический коэффициент, позволяющий изменять толщину линии плоскости по вкусу пользователя
        /// </summary>
        public float PlaneSizeK;
        /// <summary>
        /// Цвет плоскости
        /// </summary>
        public Color Colour_Plane = Color.DarkRed;
        /// <summary>
        /// Цвет куба локальной системы координат плоскости
        /// </summary>
        public Color Colour_Cube = Color.Orange;
        /// <summary>
        /// Цвет нормали плоскости
        /// </summary>
        public Color Colour_Normal = Color.Orange;
        /// <summary>
        /// Лист, в котором хранятся все вспомогательные модели (кроме моделей, загруженных пользователем, и расчетной области), которые нужно каждый раз пересчитывать
        /// </summary>
        private List<TModel3D> HelpModels = new List<TModel3D>();
        /// <summary>
        /// Лист, в котором хранятся все вспомогательные модели (кроме моделей, загруженных пользователем, и расчетной области), которые досточно повернуть или переместить (оси координат, нормали и т.д)
        /// </summary>
        private List<TModel3D> HelpModels_Transformable = new List<TModel3D>();
        //---------------------------------------------------------------
        /// <summary>
        /// Создать для рендера
        /// </summary>
        /// <param name="BB">bounding box (BB)</param>
        /// <param name="OriginalNormal">Нормаль плоскости, параллельной стороне BB с наибольшей площадью</param>
        /// <param name="BBSizeK">Коэффициент, влияющий на толщину линий BB</param>
        /// <param name="PlaneSizeK">Коэффициент, влияющий на толщину линий плоскости></param>
        public TViewerAero_Plane_Visualizer(BoundingBox BB, float BBSizeK = 0.0025f, float PlaneSizeK = 3f)
        {
            float BBSize = /*Math.Max(BB.Max.X - BB.Min.X, Math.Max(BB.Max.Y - BB.Min.Y, BB.Max.Z - BB.Min.Z))*/((BB.Max.X - BB.Min.X)+(BB.Max.Y - BB.Min.Y) +(BB.Max.Z - BB.Min.Z)) /3;
            this.BBSizeK = BBSize * BBSizeK;
            this.PlaneSizeK = PlaneSizeK;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать плоскость линиями
        /// </summary>
        /// <param name="MinMaxPoints">Точки пересечения плоскости с гранями BB</param>
        public void LinePlaneRender((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints)
        {
            TModel3D Line_OO_XO = new TModel3D(Render);
            TModel3D Line_XO_XY = new TModel3D(Render);
            TModel3D Line_XY_OY = new TModel3D(Render);
            TModel3D Line_OY_OO = new TModel3D(Render);
            HelpModels.Add(Line_OO_XO);
            HelpModels.Add(Line_XO_XY);
            HelpModels.Add(Line_XY_OY);
            HelpModels.Add(Line_OY_OO);
            Line_OO_XO.ControlPrimitive.CreateLine(MinMaxPoints.OO, MinMaxPoints.XO, PlaneSizeK * BBSizeK);
            Line_XO_XY.ControlPrimitive.CreateLine(MinMaxPoints.XO, MinMaxPoints.XY, PlaneSizeK * BBSizeK);
            Line_XY_OY.ControlPrimitive.CreateLine(MinMaxPoints.XY, MinMaxPoints.OY, PlaneSizeK * BBSizeK);
            Line_OY_OO.ControlPrimitive.CreateLine(MinMaxPoints.OY, MinMaxPoints.OO, PlaneSizeK * BBSizeK);
            //
            Line_OO_XO.SetColour(Colour_Plane);
            Line_XO_XY.SetColour(Colour_Plane);
            Line_XY_OY.SetColour(Colour_Plane);
            Line_OY_OO.SetColour(Colour_Plane);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать нормаль плоскости
        /// </summary>
        /// <param name="Center">Центр кординат</param>
        /// <param name="Normal">Нормаль плоскости</param>
        public void PlaneNormalRender(Vector3 Center, Vector3 Normal, float NormalLength)
        {
            TModel3D Model3D_Cube = new TModel3D(Render);
            TModel3D Model3D_Normal = new TModel3D(Render);
            TModel3D Model3D_Cone = new TModel3D(Render);

            HelpModels_Transformable.Add(Model3D_Cube);
            HelpModels_Transformable.Add(Model3D_Normal);
            HelpModels_Transformable.Add(Model3D_Cone);
            float Length;
            if (NormalLength == 0f)
                Length = BBSizeK * 150f;
            else
                Length = NormalLength;
            
            float LengthCube = BBSizeK * 4f;
            Vector3 ArrowHead = Normal * Length + Center;

            Model3D_Cube.ControlPrimitive.CreateCube(new Vector3(LengthCube, LengthCube, LengthCube));
            Model3D_Cube.Position = new Vector3(Center.X - 0.5f * LengthCube, Center.Y - 0.5f * LengthCube, Center.Z - 0.5f * LengthCube);
            Model3D_Cube.SetColour(Colour_Cube);
            Model3D_Cube.ControlLight.SetAmbientLight(Color.White, 0.75f);
            Model3D_Cube.ControlLight.AddDirectionLight(Color.White, 0.5f, new Vector3(1000, 1000, 1000));
            Model3D_Normal.ControlPrimitive.CreateCylinder(2f * BBSizeK, Length);
            Model3D_Normal.SetColour(Colour_Normal);
            Model3D_Normal.ControlLight.SetAmbientLight(Color.White, 0.75f);
            Model3D_Normal.ControlLight.AddDirectionLight(Color.White, 0.5f, new Vector3(1000, 1000, 1000));
            Model3D_Cone.ControlPrimitive.CreateCone(8f * BBSizeK, 0, 32f * BBSizeK);
            Model3D_Cone.SetColour(Colour_Normal);
            Model3D_Cone.ControlLight.SetAmbientLight(Color.White, 0.75f);
            Model3D_Cone.ControlLight.AddDirectionLight(Color.White, 0.5f, new Vector3(1000, 1000, 1000));
            Matrix Rotation = Matrix.Identity;
            if (Normal != Vector3.UnitZ)
            {
                // Угол между повернутой нормалью и исходной (поворот исходной нормали к смещенной)
                float Angle = (float)Math.Acos(Normal.Z / Normal.Length());
                // Определяем ось, вокруг которой будем поворачивать (ось, перпендикулярная плоскости, образованной исходной нормалью и осью Z)
                Vector3 Axis = Vector3.Cross(Vector3.UnitZ, Normal);
                Axis.Normalize();
                // Создаем матрицу поворота
                Rotation = Matrix.CreateFromAxisAngle(Axis, Angle);
            }
            // повернуть вокруг оси, перпендикулярной плоскости, образованной исходной нормалью и осью Z; передвинуть цилиндр в центр
            Model3D_Normal.Content.World = Rotation * Matrix.CreateTranslation(new Vector3(Center.X, Center.Y, Center.Z));
            // повернуть вокруг оси, перпендикулярной плоскости, образованной исходной нормалью и осью Z; передвинуть конус в конец цилиндра
            Model3D_Cone.Content.World = Rotation * Matrix.CreateTranslation(new Vector3(ArrowHead.X, ArrowHead.Y, ArrowHead.Z));
        }
        //---------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Length"></param>
        /// <param name="Thickness"></param>
        public void CoordinateLines(float Length)
        {
            TModel3D[] Lines = new TModel3D[6];
            TModel3D[] Coord = new TModel3D[3];
            
            Coord[0] = new TModel3D(Render);
            Coord[1] = new TModel3D(Render);
            Coord[2] = new TModel3D(Render);

            for (int i = 0; i < 5; i += 2)
            {
                Lines[i] = new TModel3D(Render);
                Lines[i+1] = new TModel3D(Render);
                Lines[i].ControlPrimitive.CreateCylinder(Length/30f, Length);
                Lines[i].ControlLight.SetAmbientLight(Color.White, 0.75f);
                Lines[i].ControlLight.AddDirectionLight(Color.White, 0.5f, new Vector3(1000, 1000, 1000));
                Lines[i + 1].ControlPrimitive.CreateCone(0.1f * Length, 0, 0.5f * Length);
                Lines[i + 1].ControlLight.SetAmbientLight(Color.White, 0.75f);
                Lines[i + 1].ControlLight.AddDirectionLight(Color.White, 0.5f, new Vector3(1000, 1000, 1000));
            }
 
            for (int i=0; i<3; i++)
            {
                TTriangleContainer Cylinder = Lines[i*2].Content.Model.Meshes[0].Polygons;
                TTriangleContainer Cone = Lines[i*2+1].Content.Model.Meshes[0].Polygons;
                foreach (var Vert in Cylinder.Triangles.BaseArray)
                {
                    if (Vert == null) continue;
                    Vert.P0 = Vector3.Transform(Vert.P0, Matrix.CreateTranslation(new Vector3(0f, 0f, 0f)));
                    Vert.P1 = Vector3.Transform(Vert.P1, Matrix.CreateTranslation(new Vector3(0f, 0f, 0f)));
                    Vert.P2 = Vector3.Transform(Vert.P2, Matrix.CreateTranslation(new Vector3(0f, 0f, 0f)));
                }
                
                foreach (var Vert in Cone.Triangles.BaseArray)
                {
                    if (Vert == null) continue;
                    Vert.P0 = Vector3.Transform(Vert.P0, Matrix.CreateTranslation(new Vector3(0f, 0f, Length)));
                    Vert.P1 = Vector3.Transform(Vert.P1, Matrix.CreateTranslation(new Vector3(0f, 0f, Length)));
                    Vert.P2 = Vector3.Transform(Vert.P2, Matrix.CreateTranslation(new Vector3(0f, 0f, Length)));
                }
                List<TTriangleContainer> Lin = new List<TTriangleContainer>();
                Lin.Add(Cylinder);
                Lin.Add(Cone);
                Coord[i].ControlLoad.LoadModel(Lin);
            }


            Coord[0].Content.World = Matrix.Invert(Matrix.CreateFromAxisAngle(new Vector3(0f, 1f, 0f), (float)(-1f * Math.PI / 2))) * Matrix.CreateTranslation(new Vector3(0f, 0f, 1000f));
            Coord[1].Content.World = Matrix.Invert(Matrix.CreateFromAxisAngle(new Vector3(1f, 0f, 0f), (float)(1f * Math.PI / 2))) * Matrix.CreateTranslation(new Vector3(0f, 0f, 1000f));
            Coord[2].Content.World = /*Matrix.Invert(Matrix.CreateFromAxisAngle(new Vector3(1f, 0f, 0f), (float)(1f * Math.PI / 2))) * */Matrix.CreateTranslation(new Vector3(0f, 0f, 1000f));


            foreach (var Model in Lines)
            {
                Model.ControlFrontend.Remove_AxisSystem();
                Model.Unload();
            }
            Array.Clear(Lines, 0, 6);


            Coord[0].SetColour(Color.Red);
            Coord[1].SetColour(Color.Green);
            Coord[2].SetColour(Color.Blue);

            HelpModels.Add(Coord[0]);
            HelpModels.Add(Coord[1]);
            HelpModels.Add(Coord[2]);
        }
        //----------------------------------------------------------------------
        /// <summary>
        /// Удалить все модели
        /// </summary>
        public void DisposeFromRender()
        {
            try
            {
                DisposeHelpModelsFromRender();
                DisposeHelpModelsTransformableFromRender();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0010: Error TViewerAero_Plane_Visualizer:DisposeFromRender(): " + E.Message);
            }
        }
        //----------------------------------------------------------------------
        /// <summary>
        /// Удалить вспомогательные модели, которые каждый раз пересчитываются
        /// </summary>
        public void DisposeHelpModelsFromRender()
        {
            try
            {
                // Удаляем все модели, нарисованные программой (кроме расчетной области)
                foreach (var Model in HelpModels)
                {
                    Model.ControlFrontend.Remove_AxisSystem();
                    Model.Unload();
                }
                HelpModels.Clear();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0010: Error TViewerAero_Plane_Visualizer:DisposeHelpModelsFromRender(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить вспомогательные модели, которые не  нужно пересчитывать каждый раз (оси, нормали)
        /// </summary>
        public void DisposeHelpModelsTransformableFromRender()
        {
            try
            {
                // Удаляем все модели, нарисованные программой (кроме расчетной области)
                foreach (var Model in HelpModels_Transformable)
                {
                    Model.ControlFrontend.Remove_AxisSystem();
                    Model.Unload();
                }
                HelpModels_Transformable.Clear();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0011: Error TViewerAero_Plane_Visualizer:DisposeHelpModelsTransformableFromRender(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
    }
}
