using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace NeeLaboratory.ComponentModel
{
    public static class ObservableObjectExtensions
    {
        private static readonly Action<ObservableObject> _clearEventsDelegate;

        static ObservableObjectExtensions()
        {
            var type = typeof(ObservableObject);

            var expressions = new List<Expression>();
            var parameter = Expression.Parameter(type, "obj");

            var fieldNames = new[] { nameof(ObservableObject.PropertyChanged), nameof(ObservableObject.PropertyChanging) };
            foreach (var name in fieldNames)
            {
                var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    // obj.(field) = null;
                    var fieldExp = Expression.Field(parameter, field);
                    var assignExp = Expression.Assign(fieldExp, Expression.Constant(null, field.FieldType));
                    expressions.Add(assignExp);
                }
            }

            // { obj.PropertyChanged = null; obj.PropertyChanging = null; }
            var block = Expression.Block(expressions);

            // (obj) => { obj.PropChanged = null; obj.PropertyChanging = null; }
            _clearEventsDelegate = Expression.Lambda<Action<ObservableObject>>(block, parameter).Compile();
        }

        public static void ClearObservableEvents(this ObservableObject obj)
        {
            _clearEventsDelegate(obj);
        }

        public static IDisposable SubscribePropertyChanged(this INotifyPropertyChanged obj, PropertyChangedEventHandler handler)
        {
            obj.PropertyChanged += handler;
            return new AnonymousDisposable(() => obj.PropertyChanged -= handler);
        }

        public static IDisposable SubscribePropertyChanging(this INotifyPropertyChanging obj, PropertyChangingEventHandler handler)
        {
            obj.PropertyChanging += handler;
            return new AnonymousDisposable(() => obj.PropertyChanging -= handler);
        }

        public static IDisposable SubscribePropertyChanged(this INotifyPropertyChanged obj, string? propertyName, PropertyChangedEventHandler handler)
        {
            return obj.SubscribePropertyChanged(PropertyChangedTools.CreateChangedEventHandler(propertyName, handler));
        }

        public static IDisposable SubscribePropertyChanging(this INotifyPropertyChanging obj, string? propertyName, PropertyChangingEventHandler handler)
        {
            return obj.SubscribePropertyChanging(PropertyChangedTools.CreateChangingEventHandler(propertyName, handler));
        }
    }

}
