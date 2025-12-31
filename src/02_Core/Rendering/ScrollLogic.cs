using System.Windows.Controls;
using System.Windows.Media;

namespace ForestM8rix.Rendering
{
    public class ScrollLogic
    {
        private readonly ScrollViewer _scroll;
        private readonly Canvas _canvas;
        private readonly Control _header;

        public ScrollLogic(ScrollViewer scroll, Canvas canvas, Control header)
        {
            _scroll = scroll;
            _canvas = canvas;
            _header = header;

            // Подписываемся на прокрутку
            _scroll.ScrollChanged += (s, e) => SyncHeader();
        }

        // [СУТЬ] Метод "Стрелочник": синхронизирует заголовок
        public void SyncHeader()
        {
            if (_header != null)
            {
                // Сдвигаем шапку через трансформацию (без пересчета Layout)
                _header.RenderTransform = new TranslateTransform(-_scroll.HorizontalOffset, 0);
            }
        }

        // [СУТЬ] Обновляет физический размер виртуальной области
        public void UpdateExtent(double totalWidth, double totalHeight)
        {
            _canvas.Width = totalWidth;
            _canvas.Height = totalHeight;
        }
    }
}