// Интерфейс для описания методов отбора точек для построения линий тока
using System.Collections.Generic;
//
using AstraEngine;
//*****************************************************************
namespace Example
{
    /// <summary>
    /// Интерфейс для описания методов отбора точек для построения линий тока
    /// </summary>
    public interface IPointsSelectionMethods
    {
        /// <summary>
        /// Выбор точек
        /// </summary>
        /// <returns>Лист точек</returns>
        List<Vector3> PointsSelection();
        //---------------------------------------------------------------
    }
}
