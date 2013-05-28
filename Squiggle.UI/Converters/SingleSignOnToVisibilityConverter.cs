using System.Windows;

namespace Squiggle.UI.Converters
{
    class SingleSignOnToVisibilityConverter : GenericBooleanConverter<Visibility>
    {
        public SingleSignOnToVisibilityConverter() :
            base(Visibility.Collapsed, Visibility.Visible) { }
    }
}