using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Reactive;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Animation.Animators;
using Avalonia.Animation.Easings;
using Avalonia.Data;
using Avalonia.Metadata;

namespace Avalonia.Animation
{
    /// <summary>
    /// Tracks the progress of an animation.
    /// </summary>
    public class Animation : AvaloniaObject, IAnimation
    {
        /// <summary>
        /// Defines the <see cref="Duration"/> property.
        /// </summary>
        public static readonly StyledProperty<TimeSpan> DurationProperty = AvaloniaProperty.Register<Animation, TimeSpan>(nameof(Duration));

        /// <summary>
        /// Defines the <see cref="IterationCount"/> property.
        /// </summary>
        public static readonly StyledProperty<IterationCount> IterationCountProperty = AvaloniaProperty.Register<Animation, IterationCount>(nameof(IterationCount), new(1));

        /// <summary>
        /// Defines the <see cref="PlaybackDirection"/> property.
        /// </summary>
        public static readonly StyledProperty<PlaybackDirection> PlaybackDirectionProperty =
            AvaloniaProperty.Register<Animation, PlaybackDirection>(nameof(PlaybackDirection));

        /// <summary>
        /// Defines the <see cref="FillMode"/> property.
        /// </summary>
        public static readonly StyledProperty<FillMode> FillModeProperty = AvaloniaProperty.Register<Animation, FillMode>(nameof(FillMode));

        /// <summary>
        /// Defines the <see cref="Easing"/> property.
        /// </summary>
        public static readonly StyledProperty<Easing> EasingProperty = AvaloniaProperty.Register<Animation, Easing>(nameof(Easing), new LinearEasing());

        /// <summary>
        /// Defines the <see cref="Delay"/> property.
        /// </summary>
        public static readonly StyledProperty<TimeSpan> DelayProperty = AvaloniaProperty.Register<Animation, TimeSpan>(nameof(Delay));

        /// <summary>
        /// Defines the <see cref="DelayBetweenIterations"/> property.
        /// </summary>
        public static readonly StyledProperty<TimeSpan> DelayBetweenIterationsProperty = AvaloniaProperty.Register<Animation, TimeSpan>(nameof(DelayBetweenIterations));

        /// <summary>
        /// Defines the <see cref="SpeedRatio"/> property.
        /// </summary>
        public static readonly StyledProperty<double> SpeedRatioProperty = AvaloniaProperty.Register<Animation, double>(nameof(SpeedRatio), 1d, defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        /// Gets or sets the active time of this animation.
        /// </summary>
        public TimeSpan Duration
        {
            get => GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        /// <summary>
        /// Gets or sets the repeat count for this animation.
        /// </summary>
        public IterationCount IterationCount
        {
            get => GetValue(IterationCountProperty);
            set => SetValue(IterationCountProperty, value);
        }

        /// <summary>
        /// Gets or sets the playback direction for this animation.
        /// </summary>
        public PlaybackDirection PlaybackDirection
        {
            get => GetValue(PlaybackDirectionProperty);
            set => SetValue(PlaybackDirectionProperty, value);
        }

        /// <summary>
        /// Gets or sets the value fill mode for this animation.
        /// </summary> 
        public FillMode FillMode
        {
            get => GetValue(FillModeProperty);
            set => SetValue(FillModeProperty, value);
        }

        /// <summary>
        /// Gets or sets the easing function to be used for this animation.
        /// </summary>
        public Easing Easing
        {
            get => GetValue(EasingProperty);
            set => SetValue(EasingProperty, value);
        }

        /// <summary> 
        /// Gets or sets the initial delay time for this animation. 
        /// </summary> 
        public TimeSpan Delay
        {
            get => GetValue(DelayProperty);
            set => SetValue(DelayProperty, value);
        }

        /// <summary> 
        /// Gets or sets the delay time in between iterations.
        /// </summary> 
        public TimeSpan DelayBetweenIterations
        {
            get => GetValue(DelayBetweenIterationsProperty);
            set => SetValue(DelayBetweenIterationsProperty, value);
        }

        /// <summary>
        /// Gets or sets the speed multiple for this animation.
        /// </summary> 
        public double SpeedRatio
        {
            get => GetValue(SpeedRatioProperty);
            set => SetValue(SpeedRatioProperty, value);
        }

        /// <summary>
        /// Gets the children of the <see cref="Animation"/>.
        /// </summary>
        [Content]
        public KeyFrames Children { get; } = new KeyFrames();

        // Store values for the Animator attached properties for IAnimationSetter objects.
        private static readonly Dictionary<IAnimationSetter, (Type Type, Func<IAnimator> Factory)> s_animators = new();

        /// <summary>
        /// Gets the value of the Animator attached property for a setter.
        /// </summary>
        /// <param name="setter">The animation setter.</param>
        /// <returns>The property animator type.</returns>
        public static (Type Type, Func<IAnimator> Factory)? GetAnimator(IAnimationSetter setter)
        {
            if (s_animators.TryGetValue(setter, out var type))
            {
                return type;
            }
            return null;
        }

        /// <summary>
        /// Sets the value of the Animator attached property for a setter.
        /// </summary>
        /// <param name="setter">The animation setter.</param>
        /// <param name="value">The property animator value.</param>
        public static void SetAnimator(IAnimationSetter setter, 
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicMethods)]
            Type value)
        {
            s_animators[setter] = (value, () => (IAnimator)Activator.CreateInstance(value)!);
        }

        private readonly static List<(Func<AvaloniaProperty, bool> Condition, Type Animator, Func<IAnimator> Factory)> Animators = new()
        {
            ( prop => typeof(bool).IsAssignableFrom(prop.PropertyType), typeof(BoolAnimator), () => new BoolAnimator() ),
            ( prop => typeof(byte).IsAssignableFrom(prop.PropertyType), typeof(ByteAnimator), () => new ByteAnimator() ),
            ( prop => typeof(Int16).IsAssignableFrom(prop.PropertyType), typeof(Int16Animator), () => new Int16Animator() ),
            ( prop => typeof(Int32).IsAssignableFrom(prop.PropertyType), typeof(Int32Animator), () => new Int32Animator() ),
            ( prop => typeof(Int64).IsAssignableFrom(prop.PropertyType), typeof(Int64Animator), () => new Int64Animator() ),
            ( prop => typeof(UInt16).IsAssignableFrom(prop.PropertyType), typeof(UInt16Animator), () => new UInt16Animator() ),
            ( prop => typeof(UInt32).IsAssignableFrom(prop.PropertyType), typeof(UInt32Animator), () => new UInt32Animator() ),
            ( prop => typeof(UInt64).IsAssignableFrom(prop.PropertyType), typeof(UInt64Animator), () => new UInt64Animator() ),
            ( prop => typeof(float).IsAssignableFrom(prop.PropertyType), typeof(FloatAnimator), () => new FloatAnimator() ),
            ( prop => typeof(double).IsAssignableFrom(prop.PropertyType), typeof(DoubleAnimator), () => new DoubleAnimator() ),
            ( prop => typeof(decimal).IsAssignableFrom(prop.PropertyType), typeof(DecimalAnimator), () => new DecimalAnimator() ),
        };

        /// <summary>
        /// Registers a <see cref="Animator{T}"/> that can handle
        /// a value type that matches the specified condition.
        /// </summary>
        /// <param name="condition">
        /// The condition to which the <see cref="Animator{T}"/>
        /// is to be activated and used.
        /// </param>
        /// <typeparam name="TAnimator">
        /// The type of the animator to instantiate.
        /// </typeparam>
        public static void RegisterAnimator<TAnimator>(Func<AvaloniaProperty, bool> condition)
            where TAnimator : IAnimator, new()
        {
            Animators.Insert(0, (condition, typeof(TAnimator), () => new TAnimator()));
        }

        private static (Type Type, Func<IAnimator> Factory)? GetAnimatorType(AvaloniaProperty property)
        {
            foreach (var (condition, type, factory) in Animators)
            {
                if (condition(property))
                {
                    return (type, factory);
                }
            }
            return null;
        }

        private (IList<IAnimator> Animators, IList<IDisposable> subscriptions) InterpretKeyframes(Animatable control)
        {
            var handlerList = new Dictionary<(Type type, AvaloniaProperty Property), Func<IAnimator>>();
            var animatorKeyFrames = new List<AnimatorKeyFrame>();
            var subscriptions = new List<IDisposable>();

            foreach (var keyframe in Children)
            {
                foreach (var setter in keyframe.Setters)
                {
                    if (setter.Property is null)
                    {
                        throw new InvalidOperationException("No Setter property assigned.");
                    }

                    var handler = Animation.GetAnimator(setter) ?? GetAnimatorType(setter.Property);

                    if (handler == null)
                    {
                        throw new InvalidOperationException($"No animator registered for the property {setter.Property}. Add an animator to the Animation.Animators collection that matches this property to animate it.");
                    }

                    var (type, factory) = handler.Value;

                    if (!handlerList.ContainsKey((type, setter.Property)))
                        handlerList[(type, setter.Property)] = factory;

                    var cue = keyframe.Cue;

                    if (keyframe.TimingMode == KeyFrameTimingMode.TimeSpan)
                    {
                        cue = new Cue(keyframe.KeyTime.TotalSeconds / Duration.TotalSeconds);
                    }

                    var newKF = new AnimatorKeyFrame(type, factory, cue, keyframe.KeySpline);

                    subscriptions.Add(newKF.BindSetter(setter, control));

                    animatorKeyFrames.Add(newKF);
                }
            }

            var newAnimatorInstances = new List<IAnimator>();

            foreach (var handler in handlerList)
            {
                var newInstance = handler.Value();
                newInstance.Property = handler.Key.Property;
                newAnimatorInstances.Add(newInstance);
            }

            foreach (var keyframe in animatorKeyFrames)
            {
                var animator = newAnimatorInstances.First(a => a.GetType() == keyframe.AnimatorType &&
                                                             a.Property == keyframe.Property);
                animator.Add(keyframe);
            }

            return (newAnimatorInstances, subscriptions);
        }

        /// <inheritdoc/>
        public IDisposable Apply(Animatable control, IClock? clock, IObservable<bool> match, Action? onComplete)
        {
            var (animators, subscriptions) = InterpretKeyframes(control);
            if (animators.Count == 1)
            {
                var subscription = animators[0].Apply(this, control, clock, match, onComplete);
                
                if (subscription is not null)
                {
                    subscriptions.Add(subscription);
                }
            }
            else
            {
                var completionTasks = onComplete != null ? new List<Task>() : null;
                foreach (IAnimator animator in animators)
                {
                    Action? animatorOnComplete = null;
                    if (onComplete != null)
                    {
                        var tcs = new TaskCompletionSource<object?>();
                        animatorOnComplete = () => tcs.SetResult(null);
                        completionTasks!.Add(tcs.Task);
                    }

                    var subscription = animator.Apply(this, control, clock, match, animatorOnComplete);

                    if (subscription is not null)
                    {
                        subscriptions.Add(subscription);
                    }
                }

                if (onComplete != null)
                {
                    Task.WhenAll(completionTasks!).ContinueWith(
                        (_, state) => ((Action)state!).Invoke(),
                        onComplete);
                }
            }
            return new CompositeDisposable(subscriptions);
        }

        /// <inheritdoc/>
        public Task RunAsync(Animatable control, IClock? clock = null)
        {
            return RunAsync(control, clock, default);
        }

        /// <inheritdoc/>
        public Task RunAsync(Animatable control, IClock? clock = null, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            var run = new TaskCompletionSource<object?>();

            if (this.IterationCount == IterationCount.Infinite)
                run.SetException(new InvalidOperationException("Looping animations must not use the Run method."));

            IDisposable? subscriptions = null, cancellation = null;
            subscriptions = this.Apply(control, clock, Observable.Return(true), () =>
            {
                run.TrySetResult(null);
                subscriptions?.Dispose();
                cancellation?.Dispose();
            });

            cancellation = cancellationToken.Register(() =>
            {
                run.TrySetResult(null);
                subscriptions?.Dispose();
                cancellation?.Dispose();
            });

            return run.Task;
        }
    }
}
