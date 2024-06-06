// Класс для отрисовки 2D шкалы
using System;
//
using AstraEngine.Components;
using AstraEngine.Engine.GraphicCore;
using AstraEngine.Geometry.Sprite2D;
using AstraEngine.Render;
using AstraEngine.Scene;
using AstraEngine;
//
using MPT707.Frontend;
using AstraEngine.Inputs;
using AstraEngine.Geometry;
using System.Collections.Generic;
//***************************************************************
namespace Example
{
    /// <summary>
    /// Класс для отрисовки 2D шкалы
    /// </summary>
    public class TViewerAero_Scale2D
    {
        /// <summary>
        /// Рендер
        /// </summary>
        private TRender Render = TStaticContent.Content.Game.Render;
        /// <summary>
        /// Спрайт шкалы
        /// </summary>
        private TSprite2D Scale;
        /// <summary>
        /// Отношение ширины шкалы к высоте
        /// </summary>
        public float WidthToHeight = 0.125f;
        /// <summary>
        /// Отступ от шкалы справа
        /// </summary>
        private int PaddingRight;
        /// <summary>
        /// Отступ от шкалы сверху
        /// </summary>
        private int PaddingTop;
        /// <summary>
        /// Отступ от шкалы снизу
        /// </summary>
        private int PaddingBottom;
        //---------------------------------------------------------------
        /// <summary>
        /// Создать объект шкалы
        /// </summary>
        /// <param name="Texture">Текстура шкалы</param>
        /// <param name="WidthToHeight"> Отношение ширины шкалы к высоте</param>
        /// <param name="PaddingRight">Отступ от шкалы справа</param>
        /// <param name="PaddingTop">Отступ от шкалы сверху</param>
        /// <param name="PaddingBottom">Отступ от шкалы снизу</param>
        public void CreateScale2D(TTexture2D Texture, float WidthToHeight=1/6, int PaddingRight = 10, int PaddingTop = 10, int PaddingBottom = 10)
        {
            try
            {
                this.PaddingRight = PaddingRight;
                this.PaddingTop = PaddingTop;
                this.PaddingBottom = PaddingBottom;
                Scale = new TSprite2D(Render);
                Scale.SetTexture(Texture);
                // Обновление шкалы
                TStaticContent.Content.Game.OnUpdate_Game += SetPositionAndSizeScale2D;
                Scale.OnDoubleClick_MouseButton += ChangeScaleMinMax;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0007: Error TViewerAero_Scale2D:CreateScale2D(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Задать положение и размер шкалы
        /// </summary>
        private void SetPositionAndSizeScale2D(TGame Game)
        {
            try
            {
                // Задаем размер шкалы
                Vector3 RulerPosition = TFrontend_MPT707.GridRuler.Ruler.GetPosition();
                int MainMenuHeight = TFrontend_MPT707.Form_MainMenu.Height;
                int Height = (int)RulerPosition.Y - MainMenuHeight - PaddingTop - PaddingBottom;
                int Width = (int)(Height * WidthToHeight);
                Scale.SetSize(Width, Height);
                // Задаем позицию шкалы
                System.Drawing.Point SideMenuPosition = TFrontend_MPT707.Form_SideMenu.Location;
                Scale.Position = new Vector3(SideMenuPosition.X - Width - PaddingRight, MainMenuHeight + PaddingTop, 0); 
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0007: Error TViewerAero_Scale2D:SetPositionAndSizeScale2D(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Изменить минимум и максимум шкалы
        /// </summary>
        private void ChangeScaleMinMax(List<EButtonMouse> Buttons, Vector2 PositionCursor, IGeometry Geometry)
        {
            try
            {
                UWindow_ChangeScaleMinMax Window_ChangeScaleMinMax = new UWindow_ChangeScaleMinMax();
                Window_ChangeScaleMinMax.ShowDialog();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0007: Error TViewerAero_Scale2D:ChangeScaleMinMax(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить шкалу с экрана
        /// </summary>
        public void DisposeScale2D()
        {
            try
            {
                TStaticContent.Content.Game.OnUpdate_Game -= SetPositionAndSizeScale2D;
                if(Scale != null) Scale.Unload();
                Scale = null;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0007: Error TViewerAero_Scale2D:DisposeScale2D(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
    }
}
