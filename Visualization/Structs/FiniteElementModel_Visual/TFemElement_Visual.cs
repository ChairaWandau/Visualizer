// Описание элемента конечно-элементной модели
using AstraEngine;
using AstraEngine.Engine.GraphicCore;
//******************************************************************
namespace Example
{
    /// <summary>
    /// Описание элемента конечно-элементной модели
    /// </summary>
    public class TFemElement_Visual
    {
        /// <summary>
        /// Id нод принадлежащих данному элементу
        /// </summary>
        public int[] Nodes = new int[0];
        /// <summary>
        /// Давление в данной ячейки
        /// </summary>
        public float Pressure;
        /// <summary>
        /// Вектор скорости в данной ячейке
        /// </summary>
        public Vector3 Velocity;
        /// <summary>
        /// Модуль скорости в данной ячейке
        /// </summary>
        public float VelocityModule;
        /// <summary>
        /// Расположение центра объемной ячейки
        /// </summary>
        public Vector3 Position;
    }
}