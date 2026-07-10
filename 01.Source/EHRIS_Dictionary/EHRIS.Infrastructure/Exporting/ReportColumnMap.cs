using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Infrastructure.Exporting
{
    public class ReportColumnMap
    {
        public List<ReportColumn> Columns { get; } = new();

        public void Add<T>(Expression<Func<T, object>> property, string title, double width = 15, string format = null)
        {
            var propInfo = ((MemberExpression)(property.Body is UnaryExpression unary ? unary.Operand : property.Body)).Member as PropertyInfo;
            Columns.Add(new ReportColumn { Property = propInfo, Title = title, Width = width, Format = format });
        }
         
        public void Add(Type modelType, string propertyName, string title, double width = 15, string format = null)
        {
            var propInfo = modelType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (propInfo == null)
                throw new ArgumentException($"Property '{propertyName}' not found on type '{modelType.Name}'.");

            Columns.Add(new ReportColumn { Property = propInfo, Title = title, Width = width, Format = format });
        }

    }

    public class ReportColumn
    {
        public PropertyInfo Property { get; set; }
        public string Title { get; set; }
        public double Width { get; set; }
        public string Format { get; set; }
    }
}
