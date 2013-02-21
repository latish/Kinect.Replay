using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Kinect.Recorder
{
	public static class INotifyPropertyChangedHelper
	{
		public static void Raise<T>(this PropertyChangedEventHandler handler, params Expression<Func<T>>[] propertyExpressions)
		{
			foreach (var propertyExpression in propertyExpressions)
			{
				handler.Raise<T>(propertyExpression);
			}
		}
		public static void Raise<T>(this PropertyChangedEventHandler handler, Expression<Func<T>> propertyExpression)
		{
			if (handler != null)
			{
				var body = propertyExpression.Body as MemberExpression;
				var expression = body.Expression as ConstantExpression;
				handler(expression.Value, new PropertyChangedEventArgs(body.Member.Name));
			}
		}
	}
}
