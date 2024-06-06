// Класс настроек линий тока
//***************************************************************
namespace Example
{
    /// <summary>
    /// Класс настроек линий тока
    /// </summary>
    public class TViewerAero_CurrentLinesSettings
    {
        /// <summary>
        /// Количество вершин основания цилиндров, из которых состоит линия тока
        /// </summary>
        public int NumberFaces;
        /// <summary>
        /// Радиус линий тока
        /// </summary>
        public float Radius;
        /// <summary>
        /// Строить линии тока с автоматическим (динамическим) шагом
        /// </summary>
        public bool AutoStep = false;
        /// <summary>
        /// Шаг построения линий тока (только если AutoStep == false)
        /// </summary>
        public float Step = -1f;
    }
}
