// Описание грани конечно-элементной модели
using AstraEngine;
//******************************************************************
namespace Example
{
    /// <summary>
    /// Описание нода конечно-элементной модели
    /// в</summary>
    public class TFemFace_Visual
    {
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
        /// Расположение центра плоской ячейке
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Идентификатор вершин из которых состоит данная ячейка
        /// </summary>
        public int[] Nodes = new int[0];
    }
}
