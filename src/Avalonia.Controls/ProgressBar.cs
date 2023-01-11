using System;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Reactive;

namespace Avalonia.Controls
{
    /// <summary>
    /// A control used to indicate the progress of an operation.
    /// </summary>
    [TemplatePart("PART_Indicator", typeof(Border))]
    [PseudoClasses(":vertical", ":horizontal", ":indeterminate")]
    public class ProgressBar : RangeBase
    {
        public class ProgressBarTemplateProperties : AvaloniaObject
        {
            public static readonly StyledProperty<double> ContainerAnimationStartPositionProperty =
                AvaloniaProperty.Register<ProgressBarTemplateProperties, double>(nameof(ContainerAnimationStartPosition));

            public static readonly StyledProperty<double> ContainerAnimationEndPositionProperty =
                AvaloniaProperty.Register<ProgressBarTemplateProperties, double>(nameof(ContainerAnimationEndPosition));

            public static readonly StyledProperty<double> Container2AnimationStartPositionProperty =
                AvaloniaProperty.Register<ProgressBarTemplateProperties, double>(nameof(Container2AnimationStartPosition));

            public static readonly StyledProperty<double> Container2AnimationEndPositionProperty =
                AvaloniaProperty.Register<ProgressBarTemplateProperties, double>(nameof(Container2AnimationEndPosition));

            public static readonly StyledProperty<double> Container2WidthProperty =
                AvaloniaProperty.Register<ProgressBarTemplateProperties, double>(nameof(Container2Width));

            public static readonly StyledProperty<double> ContainerWidthProperty =
                AvaloniaProperty.Register<ProgressBarTemplateProperties, double>(nameof(ContainerWidth));

            public double ContainerAnimationStartPosition
            {
                get => GetValue(ContainerAnimationStartPositionProperty);
                set => SetValue(ContainerAnimationStartPositionProperty, value);
            }

            public double ContainerAnimationEndPosition
            {
                get => GetValue(ContainerAnimationEndPositionProperty);
                set => SetValue(ContainerAnimationEndPositionProperty, value);
            }

            public double Container2AnimationStartPosition
            {
                get => GetValue(Container2AnimationStartPositionProperty);
                set => SetValue(Container2AnimationStartPositionProperty, value);
            }

            public double Container2Width
            {
                get => GetValue(Container2WidthProperty);
                set => SetValue(Container2WidthProperty, value);
            }

            public double ContainerWidth
            {
                get => GetValue(ContainerWidthProperty);
                set => SetValue(ContainerWidthProperty, value);
            }

            public double Container2AnimationEndPosition
            {
                get => GetValue(Container2AnimationEndPositionProperty);
                set => SetValue(Container2AnimationEndPositionProperty, value);
            }
        }

        private double _percentage;
        private Border? _indicator;
        private IDisposable? _trackSizeChangedListener;

        public static readonly StyledProperty<bool> IsIndeterminateProperty =
            AvaloniaProperty.Register<ProgressBar, bool>(nameof(IsIndeterminate));

        public static readonly StyledProperty<bool> ShowProgressTextProperty =
            AvaloniaProperty.Register<ProgressBar, bool>(nameof(ShowProgressText));

        public static readonly StyledProperty<string> ProgressTextFormatProperty =
            AvaloniaProperty.Register<ProgressBar, string>(nameof(ProgressTextFormat), "{1:0}%");

        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<ProgressBar, Orientation>(nameof(Orientation), Orientation.Horizontal);

        public static readonly DirectProperty<ProgressBar, double> PercentageProperty =
            AvaloniaProperty.RegisterDirect<ProgressBar, double>(
                nameof(Percentage),
                o => o.Percentage);

        public static readonly StyledProperty<double> IndeterminateStartingOffsetProperty =
            AvaloniaProperty.Register<ProgressBar, double>(nameof(IndeterminateStartingOffset));

        public static readonly StyledProperty<double> IndeterminateEndingOffsetProperty =
            AvaloniaProperty.Register<ProgressBar, double>(nameof(IndeterminateEndingOffset));

        public double Percentage
        {
            get { return _percentage; }
            private set { SetAndRaise(PercentageProperty, ref _percentage, value); }
        }

        public double IndeterminateStartingOffset
        {
            get => GetValue(IndeterminateStartingOffsetProperty);
            set => SetValue(IndeterminateStartingOffsetProperty, value);
        }

        public double IndeterminateEndingOffset
        {
            get => GetValue(IndeterminateEndingOffsetProperty);
            set => SetValue(IndeterminateEndingOffsetProperty, value);
        }

        static ProgressBar()
        {
            ValueProperty.OverrideMetadata<ProgressBar>(new(defaultBindingMode: BindingMode.OneWay));
            ValueProperty.Changed.AddClassHandler<ProgressBar>((x, e) => x.UpdateIndicatorWhenPropChanged(e));
            MinimumProperty.Changed.AddClassHandler<ProgressBar>((x, e) => x.UpdateIndicatorWhenPropChanged(e));
            MaximumProperty.Changed.AddClassHandler<ProgressBar>((x, e) => x.UpdateIndicatorWhenPropChanged(e));
            IsIndeterminateProperty.Changed.AddClassHandler<ProgressBar>((x, e) => x.UpdateIndicatorWhenPropChanged(e));
            OrientationProperty.Changed.AddClassHandler<ProgressBar>((x, e) => x.UpdateIndicatorWhenPropChanged(e));
        }

        public ProgressBar()
        {
            UpdatePseudoClasses(IsIndeterminate, Orientation);
        }

        public ProgressBarTemplateProperties TemplateProperties { get; } = new ProgressBarTemplateProperties();

        public bool IsIndeterminate
        {
            get => GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        public bool ShowProgressText
        {
            get => GetValue(ShowProgressTextProperty);
            set => SetValue(ShowProgressTextProperty, value);
        }

        public string ProgressTextFormat
        {
            get => GetValue(ProgressTextFormatProperty);
            set => SetValue(ProgressTextFormatProperty, value);
        }

        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var result = base.ArrangeOverride(finalSize);
            UpdateIndicator();
            return result;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsIndeterminateProperty)
            {
                UpdatePseudoClasses(change.GetNewValue<bool>(), null);
            }
            else if (change.Property == OrientationProperty)
            {
                UpdatePseudoClasses(null, change.GetNewValue<Orientation>());
            }
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            // dispose any previous track size listener
            _trackSizeChangedListener?.Dispose();

            _indicator = e.NameScope.Get<Border>("PART_Indicator");

            // listen to size changes of the indicators track (parent) and update the indicator there. 
            _trackSizeChangedListener = _indicator.Parent?.GetPropertyChangedObservable(BoundsProperty)
                .Subscribe(_ => UpdateIndicator());

            UpdateIndicator();
        }

        private void UpdateIndicator()
        {
            // Gets the size of the parent indicator container
            var barSize = _indicator?.Parent?.Bounds.Size ?? Bounds.Size;

            if (_indicator != null)
            {
                if (IsIndeterminate)
                {
                    // Pulled from ModernWPF.

                    var dim = Orientation == Orientation.Horizontal ? barSize.Width : barSize.Height;
                    var barIndicatorWidth = dim * 0.4; // Indicator width at 40% of ProgressBar
                    var barIndicatorWidth2 = dim * 0.6; // Indicator width at 60% of ProgressBar

                    TemplateProperties.ContainerWidth = barIndicatorWidth;
                    TemplateProperties.Container2Width = barIndicatorWidth2;

                    TemplateProperties.ContainerAnimationStartPosition = barIndicatorWidth * -1.8; // Position at -180%
                    TemplateProperties.ContainerAnimationEndPosition = barIndicatorWidth * 3.0; // Position at 300%

                    TemplateProperties.Container2AnimationStartPosition = barIndicatorWidth2 * -1.5; // Position at -150%
                    TemplateProperties.Container2AnimationEndPosition = barIndicatorWidth2 * 1.66; // Position at 166%


                    // Remove these properties when we switch to fluent as default and removed the old one.
                    IndeterminateStartingOffset = -dim;
                    IndeterminateEndingOffset = dim;

                    var padding = Padding;
                    var rectangle = new RectangleGeometry(
                        new Rect(
                            padding.Left,
                            padding.Top,
                            barSize.Width - (padding.Right + padding.Left),
                            barSize.Height - (padding.Bottom + padding.Top)
                            ));
                }
                else
                {
                    double percent = Maximum == Minimum ? 1.0 : (Value - Minimum) / (Maximum - Minimum);

                    // When the Orientation changed, the indicator's Width or Height should set to double.NaN.
                    // Indicator size calculation should consider the ProgressBar's Padding property setting
                    if (Orientation == Orientation.Horizontal)
                    {
                        _indicator.Width = (barSize.Width - _indicator.Margin.Left - _indicator.Margin.Right) * percent;
                        _indicator.Height = double.NaN;
                    }
                    else
                    {
                        _indicator.Width = double.NaN;
                        _indicator.Height = (barSize.Height - _indicator.Margin.Top - _indicator.Margin.Bottom) * percent;
                    }


                    Percentage = percent * 100;
                }
            }
        }

        private void UpdateIndicatorWhenPropChanged(AvaloniaPropertyChangedEventArgs e)
        {
            UpdateIndicator();
        }

        private void UpdatePseudoClasses(
            bool? isIndeterminate,
            Orientation? o)
        {
            if (isIndeterminate.HasValue)
            {
                PseudoClasses.Set(":indeterminate", isIndeterminate.Value);
            }

            if (o.HasValue)
            {
                PseudoClasses.Set(":vertical", o == Orientation.Vertical);
                PseudoClasses.Set(":horizontal", o == Orientation.Horizontal);
            }
        }
    }
}
