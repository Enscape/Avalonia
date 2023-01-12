using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Avalonia.Data;
using Avalonia.UnitTests;
using Xunit;

namespace Avalonia.Base.UnitTests
{
    public class AvaloniaObjectTests_DataValidation
    {
        [Fact]
        public void Binding_Non_Validated_Styled_Property_Does_Not_Call_UpdateDataValidation()
        {
            var target = new Class1();
            var source = new Subject<BindingValue<int>>();

            target.Bind(Class1.NonValidatedProperty, source);
            source.OnNext(6);
            source.OnNext(BindingValue<int>.BindingError(new Exception()));
            source.OnNext(BindingValue<int>.DataValidationError(new Exception()));
            source.OnNext(6);

            Assert.Empty(target.Notifications);
        }

        private class Class1 : AvaloniaObject
        {
            public static readonly StyledProperty<int> NonValidatedProperty =
                AvaloniaProperty.Register<Class1, int>(
                    nameof(NonValidated));

            public static readonly DirectProperty<Class1, int> NonValidatedDirectProperty =
                AvaloniaProperty.RegisterDirect<Class1, int>(
                    nameof(NonValidatedDirect),
                    o => o.NonValidatedDirect);

            public static readonly DirectProperty<Class1, int> ValidatedDirectIntProperty =
                AvaloniaProperty.RegisterDirect<Class1, int>(
                    nameof(ValidatedDirectInt),
                    o => o.ValidatedDirectInt,
                    enableDataValidation: true);

            public static readonly DirectProperty<Class1, string> ValidatedDirectStringProperty =
                AvaloniaProperty.RegisterDirect<Class1, string>(
                    nameof(ValidatedDirectString),
                    o => o.ValidatedDirectString,
                    enableDataValidation: true);

            private int _nonValidatedDirect;
            private int _directInt;
            private string _directString;

            public int NonValidated
            {
                get { return GetValue(NonValidatedProperty); }
                set { SetValue(NonValidatedProperty, value); }
            }

            public int NonValidatedDirect
            {
                get { return _directInt; }
                set { SetAndRaise(NonValidatedDirectProperty, ref _nonValidatedDirect, value); }
            }

            public int ValidatedDirectInt
            {
                get { return _directInt; }
                set { SetAndRaise(ValidatedDirectIntProperty, ref _directInt, value); }
            }

            public string ValidatedDirectString
            {
                get { return _directString; }
                set { SetAndRaise(ValidatedDirectStringProperty, ref _directString, value); }
            }

            public List<(BindingValueType type, object value)> Notifications { get; } = new();

            protected override void UpdateDataValidation(
                AvaloniaProperty property,
                BindingValueType state,
                Exception error)
            {
                Notifications.Add((state, GetValue(property)));
            }
        }

        private class Class2 : Class1
        {
            static Class2()
            {
                NonValidatedDirectProperty.OverrideMetadata<Class2>(
                    new DirectPropertyMetadata<int>(enableDataValidation: true));
            }
        }

        public class ViewModel : NotifyingBase
        {
            private string _stringValue;

            public string StringValue
            {
                get { return _stringValue; }
                set { _stringValue = value; RaisePropertyChanged(); }
            }
        }
    }
}
