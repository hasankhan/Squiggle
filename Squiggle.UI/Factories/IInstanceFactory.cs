using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.Factories
{
    interface IInstanceFactory<T>
    {
        T CreateInstance();
    }
}
