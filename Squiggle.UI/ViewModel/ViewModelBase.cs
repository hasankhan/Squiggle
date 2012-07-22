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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Set<T>(Expression<Func<T>> propertySelector, ref T property, T value )
        {
            string propertyName = (propertySelector.Body as System.Linq.Expressions.MemberExpression).Member.Name;
            property = value;
            OnPropertyChanged(propertyName);
        }
    }
}
