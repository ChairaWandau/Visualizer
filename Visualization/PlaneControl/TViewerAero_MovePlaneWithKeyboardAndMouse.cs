// Класс, содержащий события на премещение плоскости с помощью клавиатуры
using System;
using System.Collections.Generic;
//
using AstraEngine;
using AstraEngine.Components;
using AstraEngine.Inputs;
using AstraEngine.Scene;
//
//***************************************************************
namespace Example
{
    /// <summary>
    /// Класс, содержащий события на премещение плоскости с помощью клавиатуры
    /// </summary>
    public partial class TViewerAero_Plane
    {
        /// <summary>
        /// Величина поворота за одно нажатие
        /// </summary>
        public float RotationStep = 0.25f;
        /// <summary>
        /// Величина линейного перемещения за одно нажатие
        /// </summary>
        public float DeltaStep = 1;
//-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Кнопка нажатие на которую заставит объект вращаться около оси X
        /// </summary>
        private EButtonKeyboard Button_Keyboard_RotationX = EButtonKeyboard.O;
        /// <summary>
        /// Кнопка нажатие на которую заставит объект вращаться в обратную сторону около оси X
        /// </summary>
        private EButtonKeyboard Button_Keyboard_RotationX_Reversed = EButtonKeyboard.K;
//-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Кнопка нажатие на которую заставит объект вращаться около оси Y
        /// </summary>
        private EButtonKeyboard Button_Keyboard_RotationY = EButtonKeyboard.P;
        /// <summary>
        /// Кнопка нажатие на которую заставит объект вращаться в обратную сторону около оси Y
        /// </summary>
        private EButtonKeyboard Button_Keyboard_RotationY_Reversed = EButtonKeyboard.L;
//-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Кнопка нажатие на которую заставит объект вращаться около оси Z
        /// </summary>
        private EButtonKeyboard Button_Keyboard_RotationZ = EButtonKeyboard.OemOpenBrackets; // [
        /// <summary>
        /// Кнопка нажатие на которую заставит объект вращаться в обратную сторону около оси Z
        /// </summary>
        private EButtonKeyboard Button_Keyboard_RotationZ_Reversed = EButtonKeyboard.OemSemicolon; // ;
//-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Кнопка нажатие на которую заставит объект вдоль нормали по направлению вектора нормали
        /// </summary>
        private EButtonKeyboard Button_Keyboard_MoveByNormal = EButtonKeyboard.OemCloseBrackets; // ]
        /// <summary>
        /// Кнопка нажатие на которую заставит объект вдоль нормали по обратному направлению вектора нормали
        /// </summary>
        private EButtonKeyboard Button_Keyboard_MoveByNormal_Reversed = EButtonKeyboard.OemQuotes; // '
//-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Кнопка перехода в положение парллельное плоскости YOZ
        /// </summary>
        private EButtonKeyboard Button_Keyboard_Position_X = EButtonKeyboard.M;
        /// <summary>
        /// Кнопка перехода в положение парллельное плоскости XOZ
        /// </summary>
        private EButtonKeyboard Button_Keyboard_Position_Y = EButtonKeyboard.OemComma; // ,
        /// <summary>
        /// Кнопка перехода в положение парллельное плоскости XOY
        /// </summary>
        private EButtonKeyboard Button_Keyboard_Position_Z = EButtonKeyboard.OemPeriod; // .
        /// <summary>
        /// Кнопка перехода в начальное положение
        /// </summary>
        private EButtonKeyboard Button_Keyboard_Position_Default = EButtonKeyboard.OemQuestion; // /
//-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Инициализация обработчика событий
        /// </summary>
        private void Initialize_EventsKeyboard()
        {
            TStaticContent.Content.Game.OnPressed_KeyboardButton += Game_OnPressed_KeyboardButton;
            TStaticContent.Content.Game.OnPressed_MouseButton += Game_OnPressed_MouseButton;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Обработка события нажатия кнопок на мышке
        /// </summary>
        /// <param name="Buttons">Нажатые в данный момент кнопки</param>
        /// <param name="PositionCursor">Координаты курсора в данный момент</param>
        private void Game_OnPressed_MouseButton(List<EButtonMouse> Buttons, AstraEngine.Vector2 PositionCursor)
        {

        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Обработка события нажатия кнопок на клавиатуре
        /// </summary>
        /// <param name="Buttons"></param>
        private void Game_OnPressed_KeyboardButton(List<SActionKey> Buttons)
        {
            try
            {
                if (!Enable) return;
                // Если нет нажатых кнопок или их колличество больше чем 1, то мы не рассматриваем такой момент
                if (Buttons.Count != 1) return;
                //
                if (Buttons[0].Key == Button_Keyboard_RotationX)
                {
                    MovePlane(new Vector3(RotationStep, 0, 0), 0);
                    Display_Plane(Get_CurrentPlane());
                }
                else if (Buttons[0].Key == Button_Keyboard_RotationX_Reversed)
                {
                    MovePlane(new Vector3(-RotationStep, 0, 0), 0);
                    Display_Plane(Get_CurrentPlane());
                }
                else if (Buttons[0].Key == Button_Keyboard_RotationY)
                {
                    MovePlane(new Vector3(0, RotationStep, 0), 0);
                    Display_Plane(Get_CurrentPlane());
                }
                else if (Buttons[0].Key == Button_Keyboard_RotationY_Reversed)
                {
                    MovePlane(new Vector3(0, -RotationStep, 0), 0);
                    Display_Plane(Get_CurrentPlane());
                }
                else if (Buttons[0].Key == Button_Keyboard_RotationZ)
                {
                    MovePlane(new Vector3(0, 0, RotationStep), 0);
                    Display_Plane(Get_CurrentPlane());
                }
                else if (Buttons[0].Key == Button_Keyboard_RotationZ_Reversed)
                {
                    MovePlane(new Vector3(0, 0, -RotationStep), 0);
                    Display_Plane(Get_CurrentPlane());
                }
                else if (Buttons[0].Key == Button_Keyboard_MoveByNormal)
                {
                    MovePlane(new Vector3(0, 0, 0), DeltaStep);
                    Display_Plane(Get_CurrentPlane());
                }
                else if (Buttons[0].Key == Button_Keyboard_MoveByNormal_Reversed)
                {
                    MovePlane(new Vector3(0, 0, 0), -DeltaStep);
                    Display_Plane(Get_CurrentPlane());
                }
                else if (Buttons[0].Key == Button_Keyboard_Position_X)
                {
                    this.normal = new Vector3(1, 0, 0);
                    Display_Plane(Get_CurrentPlane());
                }
                else if (Buttons[0].Key == Button_Keyboard_Position_Y)
                {
                    this.normal = new Vector3(0, 1, 0);
                    Display_Plane(Get_CurrentPlane());
                }
                else if (Buttons[0].Key == Button_Keyboard_Position_Z)
                {
                    this.normal = new Vector3(0, 0, 1);
                    Display_Plane(Get_CurrentPlane());
                }
                else if (Buttons[0].Key == Button_Keyboard_Position_Default)
                {
                    this.position = (BB.Max - BB.Min) / 2f + BB.Min;
                    this.normal = basePlane.Normal;
                    Display_Plane(Get_CurrentPlane());
                }
            }
            catch (System.Exception E)
            {
                TJournalLog.WriteLog("C0013: Error TViewerAero_Plane:Game_OnPressed_KeyboardButton(): " + E.Message);
            }
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
