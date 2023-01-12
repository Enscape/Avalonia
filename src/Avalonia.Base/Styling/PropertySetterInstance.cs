using System;
using Avalonia.Data;
using Avalonia.Reactive;

#nullable enable

namespace Avalonia.Styling
{
    /// <summary>
    /// A <see cref="Setter"/> which has been instanced on a control.
    /// </summary>
    /// <typeparam name="T">The target property type.</typeparam>
    internal class PropertySetterInstance<T> : SingleSubscriberObservableBase<BindingValue<T>>,
        ISetterInstance
    {
        private readonly StyledElement _target;
        private readonly StyledPropertyBase<T> _styledProperty;
        private readonly T _value;
        private IDisposable? _subscription;
        private State _state;

        public PropertySetterInstance(
            StyledElement target,
            StyledPropertyBase<T> property,
            T value)
        {
            _target = target;
            _styledProperty = property;
            _value = value;
        }

        private bool IsActive => _state == State.Active;

        public void Start(bool hasActivator)
        {
            if (hasActivator)
            {
                _subscription = _target.Bind(_styledProperty, this, BindingPriority.StyleTrigger);
            }
            else
            {
                var target = (AvaloniaObject) _target;
                
                _subscription = target.SetValue(_styledProperty!, _value, BindingPriority.Style);
            }
        }

        public void Activate()
        {
            if (!IsActive)
            {
                _state = State.Active;
                PublishNext();
            }
        }

        public void Deactivate()
        {
            if (IsActive)
            {
                _state = State.Inactive;
                PublishNext();
            }
        }

        public override void Dispose()
        {
            if (_state == State.Disposed)
                return;
            _state = State.Disposed;

            if (_subscription is object)
            {
                var sub = _subscription;
                _subscription = null;
                sub.Dispose();
            }
            else if (IsActive)
            {
                _target.ClearValue(_styledProperty);
            }

            base.Dispose();
        }

        protected override void Subscribed() => PublishNext();
        protected override void Unsubscribed() { }

        private void PublishNext()
        {
            PublishNext(IsActive ? new BindingValue<T>(_value) : default);
        }

        private enum State
        {
            Inactive,
            Active,
            Disposed,
        }
    }
}
