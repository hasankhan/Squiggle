using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Squiggle.UI.ViewModel
{
    public class ViewModelBase: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void OnPropertyChanged(params Expression<Func<object>>[] propertySelectors)
        {
            IEnumerable<string> propertyNames = propertySelectors.Select(s => GetPropertyName(s));
            foreach (string propertyName in propertyNames)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }        

        protected void Set<T>(Expression<Func<object>> propertySelector, ref T property, T value )
        {
            property = value;
            OnPropertyChanged(propertySelector);
        }

        static string GetPropertyName(Expression<Func<object>> propertySelector)
        {
            Expression selector = propertySelector.Body;
            if (selector.NodeType == ExpressionType.Convert)
                selector = ((UnaryExpression)selector).Operand;
            string propertyName = ((MemberExpression)selector).Member.Name;
            return propertyName;
        }
    }
}
