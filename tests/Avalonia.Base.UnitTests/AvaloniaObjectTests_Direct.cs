using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Data;
using Avalonia.Logging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.UnitTests;
using Moq;
using Nito.AsyncEx;
using Xunit;

namespace Avalonia.Base.UnitTests
{
    public class AvaloniaObjectTests_Direct
    {
        [Fact]
        public void GetValue_Gets_Default_Value()
        {
            var target = new Class1();

            Assert.Equal("initial", target.GetValue(Class1.FooProperty));
        }

        [Fact]
        public void GetValue_Gets_Value_NonGeneric()
        {
            var target = new Class1();

            Assert.Equal("initial", target.GetValue((AvaloniaProperty)Class1.FooProperty));
        }

        [Fact]
        public void GetValue_On_Unregistered_Property_Throws_Exception()
        {
            var target = new Class2();

            Assert.Throws<ArgumentException>(() => target.GetValue(Class1.BarProperty));
        }

        [Fact]
        public void GetObservable_Returns_Values()
        {
            var target = new Class1();
            List<string> values = new List<string>();

            target.GetObservable(Class1.FooProperty).Subscribe(x => values.Add(x));
            target.Foo = "newvalue";

            Assert.Equal(new[] { "initial", "newvalue" }, values);
        }

        [Fact]
        public void ReadOnly_Property_Cannot_Be_Set_NonGeneric()
        {
            var target = new Class1();

            Assert.Throws<ArgumentException>(() =>
                target.SetValue((AvaloniaProperty)Class1.BarProperty, "newvalue"));
        }

        [Fact]
        public void GetValue_Gets_Value_On_AddOwnered_Property()
        {
            var target = new Class2();

            Assert.Equal("initial2", target.GetValue(Class2.FooProperty));
        }

        [Fact]
        public void GetValue_Gets_Value_On_AddOwnered_Property_Using_Original()
        {
            var target = new Class2();

            Assert.Equal("initial2", target.GetValue(Class1.FooProperty));
        }

        [Fact]
        public void GetValue_Gets_Value_On_AddOwnered_Property_Using_Original_NonGeneric()
        {
            var target = new Class2();

            Assert.Equal("initial2", target.GetValue((AvaloniaProperty)Class1.FooProperty));
        }

        [Fact]
        public void AddOwner_Should_Inherit_DefaultBindingMode()
        {
            var foo = new DirectProperty<Class1, string>(
                "foo",
                o => "foo",
                new DirectPropertyMetadata<string>(defaultBindingMode: BindingMode.TwoWay));
            var bar = foo.AddOwner<Class2>(o => "bar");

            Assert.Equal(BindingMode.TwoWay, bar.GetMetadata<Class1>().DefaultBindingMode);
            Assert.Equal(BindingMode.TwoWay, bar.GetMetadata<Class2>().DefaultBindingMode);
        }

        [Fact]
        public void AddOwner_Can_Override_DefaultBindingMode()
        {
            var foo = new DirectProperty<Class1, string>(
                "foo",
                o => "foo",
                new DirectPropertyMetadata<string>(defaultBindingMode: BindingMode.TwoWay));
            var bar = foo.AddOwner<Class2>(o => "bar", defaultBindingMode: BindingMode.OneWayToSource);

            Assert.Equal(BindingMode.TwoWay, bar.GetMetadata<Class1>().DefaultBindingMode);
            Assert.Equal(BindingMode.OneWayToSource, bar.GetMetadata<Class2>().DefaultBindingMode);
        }

        private class Class1 : AvaloniaObject
        {
            public static readonly DirectProperty<Class1, string> FooProperty =
                AvaloniaProperty.RegisterDirect<Class1, string>(
                    nameof(Foo),
                    o => o.Foo,
                    unsetValue: "unset");

            public static readonly DirectProperty<Class1, string> BarProperty =
                AvaloniaProperty.RegisterDirect<Class1, string>(nameof(Bar), o => o.Bar);

            public static readonly DirectProperty<Class1, int> BazProperty =
                AvaloniaProperty.RegisterDirect<Class1, int>(
                    nameof(Baz),
                    o => o.Baz,
                    unsetValue: -1);

            public static readonly DirectProperty<Class1, double> DoubleValueProperty =
                AvaloniaProperty.RegisterDirect<Class1, double>(
                    nameof(DoubleValue),
                    o => o.DoubleValue);

            public static readonly DirectProperty<Class1, object> FrankProperty =
                AvaloniaProperty.RegisterDirect<Class1, object>(
                    nameof(Frank),
                    o => o.Frank,
                    unsetValue: "Kups");

            private string _foo = "initial";
            private readonly string _bar = "bar";
            private int _baz = 5;
            private double _doubleValue;
            private object _frank;

            public string Foo
            {
                get { return _foo; }
                set { SetAndRaise(FooProperty, ref _foo, value); }
            }

            public string Bar
            {
                get { return _bar; }
            }

            public int Baz
            {
                get { return _baz; }
                set { SetAndRaise(BazProperty, ref _baz, value); }
            }

            public double DoubleValue
            {
                get { return _doubleValue; }
                set { SetAndRaise(DoubleValueProperty, ref _doubleValue, value); }
            }

            public object Frank
            {
                get { return _frank; }
                set { SetAndRaise(FrankProperty, ref _frank, value); }
            }
        }

        private class Class2 : AvaloniaObject
        {
            public static readonly DirectProperty<Class2, string> FooProperty =
                Class1.FooProperty.AddOwner<Class2>(o => o.Foo);

            private string _foo = "initial2";

            static Class2()
            {
            }

            public string Foo
            {
                get { return _foo; }
                set { SetAndRaise(FooProperty, ref _foo, value); }
            }
        }

        private class TestStackOverflowViewModel : INotifyPropertyChanged
        {
            public int SetterInvokedCount { get; private set; }

            public const int MaxInvokedCount = 1000;

            private double _value;

            public event PropertyChangedEventHandler PropertyChanged;

            public double Value
            {
                get { return _value; }
                set
                {
                    if (_value != value)
                    {
                        SetterInvokedCount++;
                        if (SetterInvokedCount < MaxInvokedCount)
                        {
                            _value = (int)value;
                            if (_value > 75) _value = 75;
                            if (_value < 25) _value = 25;
                        }
                        else
                        {
                            _value = value;
                        }

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                    }
                }
            }
        }
    }
}
