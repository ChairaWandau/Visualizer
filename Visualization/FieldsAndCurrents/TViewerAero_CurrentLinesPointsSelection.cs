// Класс, содержащий методы для отбора точек, по которым будут построенны линии тока
using System.Collections.Generic;
//
using AstraEngine;
//***************************************************************
namespace Example
{
    public partial class TViewerAero
    {
        /// <summary>
        /// Лист для хранения методов, заданных пользователем с введенными параметрами
        /// </summary>
        public List<IPointsSelectionMethods> PointsSelectionMethods = new List<IPointsSelectionMethods>();
        //---------------------------------------------------------------
        /// <summary>
        /// Добавить метод в лист
        /// </summary>
        /// <param name="ValueForCurrentLines">Заполненный объект метода</param>
        public void AddListInterface (IPointsSelectionMethods ValueForCurrentLines)
        {
            PointsSelectionMethods.Add(ValueForCurrentLines);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить метод из листа
        /// </summary>
        /// <param name="Index">Индекс удаляемого элемента</param>
        public void DeleteMethods(int Index)
        {
            PointsSelectionMethods.RemoveAt(Index);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Заменить метод в листе
        /// </summary>
        /// <param name="ValueForCurrentLines">новый заполненный объект метода</param>
        /// <param name="Index">индекс заменяемого метода</param>
        public void ChangeMethods (IPointsSelectionMethods ValueForCurrentLines, int Index)
        {
            // Переменная, служащая, чтобы не потерять индекс метода
           
            PointsSelectionMethods[Index]= ValueForCurrentLines;
           
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Создать точки для линий тока по методам
        /// </summary>
        /// <returns>Список точек</returns>
        public List<Vector3> CreatePointsForCurrentLine()
        {
            List<Vector3> Points = new List<Vector3>();
            for (int i=0; i<PointsSelectionMethods.Count; i++)
            {
                Points.AddRange(PointsSelectionMethods[i].PointsSelection());
            }
            return Points;
        }
        //---------------------------------------------------------------
    }
}
