using System;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.PullToRefresh;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;

namespace Avalonia.Controls
{
    /// <summary>
    /// Represents a container control that provides a <see cref="RefreshVisualizer"/> and pull-to-refresh functionality for scrollable content.
    /// </summary>
    public class RefreshContainer : ContentControl
    {
        internal const int DefaultPullDimensionSize = 100;

        private bool _hasDefaultRefreshInfoProviderAdapter;

        private RefreshInfoProvider? _refreshInfoProvider;
        private IDisposable? _visualizerSizeSubscription;
        private Grid? _visualizerPresenter;
        private bool _hasDefaultRefreshVisualizer;

        /// <summary>
        /// Defines the <see cref="RefreshRequested"/> event.
        /// </summary>
        public static readonly RoutedEvent<RefreshRequestedEventArgs> RefreshRequestedEvent =
            RoutedEvent.Register<RefreshContainer, RefreshRequestedEventArgs>(nameof(RefreshRequested), RoutingStrategies.Bubble);

        internal static readonly StyledProperty<ScrollViewerIRefreshInfoProviderAdapter?> RefreshInfoProviderAdapterProperty =
            AvaloniaProperty.Register<RefreshContainer, ScrollViewerIRefreshInfoProviderAdapter?>(nameof(RefreshInfoProviderAdapter));

        /// <summary>
        /// Defines the <see cref="Visualizer"/> event.
        /// </summary>
        public static readonly StyledProperty<RefreshVisualizer?> VisualizerProperty =
            AvaloniaProperty.Register<RefreshContainer, RefreshVisualizer?>(nameof(Visualizer));

        /// <summary>
        /// Defines the <see cref="PullDirection"/> event.
        /// </summary>
        public static readonly StyledProperty<PullDirection> PullDirectionProperty =
            AvaloniaProperty.Register<RefreshContainer, PullDirection>(nameof(PullDirection), PullDirection.TopToBottom);

        internal ScrollViewerIRefreshInfoProviderAdapter? RefreshInfoProviderAdapter
        {
            get => GetValue(RefreshInfoProviderAdapterProperty);
            set => SetValue(RefreshInfoProviderAdapterProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="RefreshVisualizer"/> for this container.
        /// </summary>
        public RefreshVisualizer? Visualizer
        {
            get => GetValue(VisualizerProperty);
            set => SetValue(VisualizerProperty, value);
        }

        private static void OnVisualizerChanged(RefreshContainer sender, AvaloniaPropertyChangedEventArgs<RefreshVisualizer?> e)
        {
            if (e.OldValue.Value is { } value)
            {
                sender._visualizerSizeSubscription?.Dispose();
                value.RefreshRequested -= sender.Visualizer_RefreshRequested;
            }
        }

        /// <summary>
        /// Gets or sets a value that specifies the direction to pull to initiate a refresh.
        /// </summary>
        public PullDirection PullDirection
        {
            get => GetValue(PullDirectionProperty);
            set => SetValue(PullDirectionProperty, value);
        }

        /// <summary>
        /// Occurs when an update of the content has been initiated.
        /// </summary>
        public event EventHandler<RefreshRequestedEventArgs>? RefreshRequested
        {
            add => AddHandler(RefreshRequestedEvent, value);
            remove => RemoveHandler(RefreshRequestedEvent, value);
        }

        public RefreshContainer()
        {
            _hasDefaultRefreshInfoProviderAdapter = true;
            RefreshInfoProviderAdapter = new ScrollViewerIRefreshInfoProviderAdapter(PullDirection);
        }

        static RefreshContainer()
        {
            VisualizerProperty.Changed.AddClassHandler<RefreshContainer, RefreshVisualizer?>(OnVisualizerChanged);
            RefreshInfoProviderAdapterProperty.Changed.AddClassHandler<RefreshContainer>((s, e) => s._hasDefaultRefreshInfoProviderAdapter = false);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _visualizerPresenter = e.NameScope.Find<Grid>("PARTVisualizerPresenter");

            if (Visualizer == null)
            {
                _hasDefaultRefreshVisualizer = true;
                Visualizer = new RefreshVisualizer();
            }
            else
            {
                _hasDefaultRefreshVisualizer = false;
            }

            OnPullDirectionChanged();
        }

        private void OnVisualizerSizeChanged(Rect obj)
        {
            if (_hasDefaultRefreshInfoProviderAdapter)
            {
                RefreshInfoProviderAdapter = new ScrollViewerIRefreshInfoProviderAdapter(PullDirection);
            }
        }

        private void Visualizer_RefreshRequested(object? sender, RefreshRequestedEventArgs e)
        {
            var ev = new RefreshRequestedEventArgs(e.GetDeferral(), RefreshRequestedEvent);
            RaiseEvent(ev);
            ev.DecrementCount();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == RefreshInfoProviderAdapterProperty)
            {
                if (Visualizer != null)
                {
                    if (_refreshInfoProvider != null)
                    {
                        Visualizer.RefreshInfoProvider = _refreshInfoProvider;
                    }
                    else
                    {
                        if (RefreshInfoProviderAdapter != null && Visualizer != null)
                        {
                            _refreshInfoProvider = RefreshInfoProviderAdapter?.AdaptFromTree(this, Visualizer.Bounds.Size);

                            if (_refreshInfoProvider != null)
                            {
                                Visualizer.RefreshInfoProvider = _refreshInfoProvider;
                                RefreshInfoProviderAdapter?.SetAnimations(Visualizer);
                            }
                        }
                    }
                }
            }
            else if (change.Property == VisualizerProperty)
            {
                if (_visualizerPresenter != null)
                {
                    _visualizerPresenter.Children.Clear();
                    if (Visualizer != null)
                    {
                        _visualizerPresenter.Children.Add(Visualizer);
                    }
                }

                if (Visualizer != null)
                {
                    Visualizer.RefreshRequested += Visualizer_RefreshRequested;
                    _visualizerSizeSubscription = Visualizer.GetObservable(Control.BoundsProperty).Subscribe(OnVisualizerSizeChanged);
                }
            }
            else if (change.Property == PullDirectionProperty)
            {
                OnPullDirectionChanged();
            }
        }

        private void OnPullDirectionChanged()
        {
            if (_visualizerPresenter != null && Visualizer != null)
            {
                switch (PullDirection)
                {
                    case PullDirection.TopToBottom:
                        _visualizerPresenter.VerticalAlignment = Layout.VerticalAlignment.Top;
                        _visualizerPresenter.HorizontalAlignment = Layout.HorizontalAlignment.Stretch;
                        if (_hasDefaultRefreshVisualizer)
                        {
                            Visualizer.PullDirection = PullDirection.TopToBottom;
                            Visualizer.Height = DefaultPullDimensionSize;
                            Visualizer.Width = double.NaN;
                        }
                        break;
                    case PullDirection.BottomToTop:
                        _visualizerPresenter.VerticalAlignment = Layout.VerticalAlignment.Bottom;
                        _visualizerPresenter.HorizontalAlignment = Layout.HorizontalAlignment.Stretch;
                        if (_hasDefaultRefreshVisualizer)
                        {
                            Visualizer.PullDirection = PullDirection.BottomToTop;
                            Visualizer.Height = DefaultPullDimensionSize;
                            Visualizer.Width = double.NaN;
                        }
                        break;
                    case PullDirection.LeftToRight:
                        _visualizerPresenter.VerticalAlignment = Layout.VerticalAlignment.Stretch;
                        _visualizerPresenter.HorizontalAlignment = Layout.HorizontalAlignment.Left;
                        if (_hasDefaultRefreshVisualizer)
                        {
                            Visualizer.PullDirection = PullDirection.LeftToRight;
                            Visualizer.Width = DefaultPullDimensionSize;
                            Visualizer.Height = double.NaN;
                        }
                        break;
                    case PullDirection.RightToLeft:
                        _visualizerPresenter.VerticalAlignment = Layout.VerticalAlignment.Stretch;
                        _visualizerPresenter.HorizontalAlignment = Layout.HorizontalAlignment.Right;
                        if (_hasDefaultRefreshVisualizer)
                        {
                            Visualizer.PullDirection = PullDirection.RightToLeft;
                            Visualizer.Width = DefaultPullDimensionSize;
                            Visualizer.Height = double.NaN;
                        }
                        break;
                }

                if (_hasDefaultRefreshInfoProviderAdapter &&
                    _hasDefaultRefreshVisualizer &&
                    Visualizer.Bounds.Height == DefaultPullDimensionSize &&
                    Visualizer.Bounds.Width == DefaultPullDimensionSize)
                {
                    RefreshInfoProviderAdapter = new ScrollViewerIRefreshInfoProviderAdapter(PullDirection);
                }
            }
        }

        /// <summary>
        /// Initiates an update of the content.
        /// </summary>
        public void RequestRefresh()
        {
            Visualizer?.RequestRefresh();
        }
    }
}
