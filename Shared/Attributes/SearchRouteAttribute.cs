using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class SearchRouteAttribute : Attribute
    {
        public string Route { get; }
        public string? TextProperty { get; }
        public string? ValueProperty { get; set; }

        public SearchRouteAttribute(string route, string? textProperty = null)
        {
            Route = route;
            TextProperty = textProperty;
        }

        public SearchRouteAttribute(string route, string valueProperty, string textProperty)
        {
            Route = route;
            ValueProperty = valueProperty;
            TextProperty = textProperty;
        }
    }
}
