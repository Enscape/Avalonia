using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Animation.Animators;
using Avalonia.Data;
using Avalonia.Reactive;

namespace Avalonia.Animation
{
    /// <summary>
    /// Defines a KeyFrame that is used for
    /// <see cref="Animator{T}"/> objects.
    /// </summary>
    public class AnimatorKeyFrame : AvaloniaObject
    {
        public static readonly StyledProperty<object?> ValueProperty = AvaloniaProperty.Register<AnimatorKeyFrame, object?>(nameof(Value));

        public AnimatorKeyFrame()
        {

        }

        public AnimatorKeyFrame(Type? animatorType, Func<IAnimator>? animatorFactory, Cue cue)
        {
            AnimatorType = animatorType;
            AnimatorFactory = animatorFactory;
            Cue = cue;
            KeySpline = null;
        }

        public AnimatorKeyFrame(Type? animatorType, Func<IAnimator>? animatorFactory, Cue cue, KeySpline? keySpline)
        {
            AnimatorType = animatorType;
            AnimatorFactory = animatorFactory;
            Cue = cue;
            KeySpline = keySpline;
        }

        internal bool isNeutral;
        public Type? AnimatorType { get; }
        public Func<IAnimator>? AnimatorFactory { get; }
        public Cue Cue { get; }
        public KeySpline? KeySpline { get; }
        public AvaloniaProperty? Property { get; private set; }

        public object? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public IDisposable BindSetter(IAnimationSetter setter, Animatable targetControl)
        {
            Property = setter.Property;
            var value = setter.Value;

            if (value is IBinding binding)
            {
                return this.Bind(ValueProperty, binding, targetControl);
            }
            else
            {
                return this.Bind(ValueProperty, Observable.SingleValue(value).ToBinding(), targetControl);
            }
        }

        [RequiresUnreferencedCode(TrimmingMessages.TypeConvertionRequiresUnreferencedCodeMessage)]
        public T GetTypedValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
        {
            var typeConv = TypeDescriptor.GetConverter(typeof(T));

            if (Value == null)
            {
                throw new ArgumentNullException($"KeyFrame value can't be null.");
            }
            if(Value is T typedValue)
            {
                return typedValue;
            }
            if (!typeConv.CanConvertTo(Value.GetType()))
            {
                throw new InvalidCastException($"KeyFrame value doesnt match property type.");
            }

            return (T)typeConv.ConvertTo(Value, typeof(T))!;
        }
    }
}
