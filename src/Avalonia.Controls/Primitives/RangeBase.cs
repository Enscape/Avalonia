using System;
using Avalonia.Data;
using Avalonia.Utilities;

namespace Avalonia.Controls.Primitives
{
    /// <summary>
    /// Base class for controls that display a value within a range.
    /// </summary>
    public abstract class RangeBase : TemplatedControl
    {
        /// <summary>
        /// Defines the <see cref="Minimum"/> property.
        /// </summary>
        public static readonly StyledProperty<double> MinimumProperty =
            AvaloniaProperty.Register<RangeBase, double>(nameof(Minimum), coerce: CoerceMinimum);

        /// <summary>
        /// Defines the <see cref="Maximum"/> property.
        /// </summary>
        public static readonly StyledProperty<double> MaximumProperty =
            AvaloniaProperty.Register<RangeBase, double>(nameof(Maximum), 100, coerce: CoerceMaximum);

        /// <summary>
        /// Defines the <see cref="Value"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<RangeBase, double>(nameof(Value),
                defaultBindingMode: BindingMode.TwoWay,
                coerce: CoerceValue);

        /// <summary>
        /// Defines the <see cref="SmallChange"/> property.
        /// </summary>
        public static readonly StyledProperty<double> SmallChangeProperty =
            AvaloniaProperty.Register<RangeBase, double>(nameof(SmallChange), 1);

        /// <summary>
        /// Defines the <see cref="LargeChange"/> property.
        /// </summary>
        public static readonly StyledProperty<double> LargeChangeProperty =
            AvaloniaProperty.Register<RangeBase, double>(nameof(LargeChange), 10);

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeBase"/> class.
        /// </summary>
        public RangeBase()
        {
        }

        static RangeBase()
        {
            MinimumProperty.Changed.AddClassHandler<RangeBase>((s, e) => s.OnMinimumChanged());
            MaximumProperty.Changed.AddClassHandler<RangeBase>((s, e) => s.OnMaximumChanged());
        }

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public double Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        private static double CoerceMinimum(AvaloniaObject sender, double value)
        {
            var range = (RangeBase)sender;

            if (!ValidateDouble(value))
            {
                return range.Minimum;
            }

            return value;
        }

        private void OnMinimumChanged()
        {
            if (IsInitialized)
            {
                CoerceValue(MaximumProperty);
                CoerceValue(ValueProperty);
            }
        }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public double Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        private static double CoerceMaximum(AvaloniaObject sender, double value)
        {
            var range = (RangeBase)sender;

            if (!ValidateDouble(value))
            {
                return range.Maximum;
            }

            return range.ValidateMaximum(value);
        }

        private void OnMaximumChanged()
        {
            if (IsInitialized)
            {
                CoerceValue(ValueProperty);
            }
        }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public double Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static double CoerceValue(AvaloniaObject sender, double value)
        {
            var range = (RangeBase)sender;

            if (!ValidateDouble(value))
            {
                return range.Value;
            }

            return range.ValidateValue(value);
        }

        public double SmallChange
        {
            get => GetValue(SmallChangeProperty);
            set => SetValue(SmallChangeProperty, value);
        }

        public double LargeChange
        {
            get => GetValue(LargeChangeProperty);
            set => SetValue(LargeChangeProperty, value);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            Maximum = ValidateMaximum(Maximum);
            Value = ValidateValue(Value);
        }

        /// <summary>
        /// Checks if the double value is not infinity nor NaN.
        /// </summary>
        /// <param name="value">The value.</param>
        private static bool ValidateDouble(double value)
        {
            return !double.IsInfinity(value) && !double.IsNaN(value);
        }

        /// <summary>
        /// Validates/coerces the <see cref="Maximum"/> property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The coerced value.</returns>
        private double ValidateMaximum(double value)
        {
            return Math.Max(value, Minimum);
        }

        /// <summary>
        /// Validates/coerces the <see cref="Value"/> property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The coerced value.</returns>
        private double ValidateValue(double value)
        {
            return MathUtilities.Clamp(value, Minimum, Maximum);
        }
    }
}
