using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace VisualHg.ViewModel
{
    public class ListViewSortingIcons : Adorner
    {
        private static Geometry ascGeometry = Geometry.Parse("M 0 5 L 4.5 0 L 9 5 Z");
        private static Geometry descGeometry = Geometry.Parse("M 0 0 L 4.5 5 L 9 0 Z");

        public ListSortDirection Direction { get; private set; }

        public ListViewSortingIcons(UIElement element, ListSortDirection dir)
            : base(element)
        {
            Direction = dir;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var size = AdornedElement.RenderSize;

            if (size.Width < 20)
            {
                return;
            }

            var transform = new TranslateTransform(size.Width - 15, (size.Height - 5) / 2);
            
            drawingContext.PushTransform(transform);

            var geometry = ascGeometry;

            if (Direction == ListSortDirection.Descending)
            {
                geometry = descGeometry;
            }

            drawingContext.DrawGeometry(Brushes.Gray, null, geometry);
            drawingContext.Pop();
        }
    }
}