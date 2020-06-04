﻿using System;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;

namespace Mobile.Controls.Behaviors
{
    class LongPressBehavior : Behavior<Button>
    {
        private readonly object _syncObject = new object();
        private const int Duration = 500;
        private Timer _timer;
        private readonly int _duration;
        private volatile bool _isReleased;

        public event EventHandler LongPressed;

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command),
            typeof(ICommand), typeof(LongPressBehavior), default(ICommand));

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(LongPressBehavior));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttachedTo(Button button)
        {
            base.OnAttachedTo(button);
            this.BindingContext = button.BindingContext;
            button.Pressed += ButtonPressed;
            button.Released += ButtonReleased;
        }

        protected override void OnDetachingFrom(Button button)
        {
            base.OnDetachingFrom(button);
            this.BindingContext = null;
            button.Pressed -= ButtonPressed;
            button.Released -= ButtonReleased;
        }

        private void DeInitializeTimer()
        {
            lock (_syncObject)
            {
                if (_timer == null)
                {
                    return;
                }

                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
            }
        }

        private void InitializeTimer()
        {
            lock (_syncObject)
            {
                _timer = new Timer(TimerElapsed, null, _duration, Timeout.Infinite);
            }
        }

        private void ButtonPressed(object sender, EventArgs e)
        {
            _isReleased = false;
            InitializeTimer();
        }

        private void ButtonReleased(object sender, EventArgs e)
        {
            _isReleased = true;
            DeInitializeTimer();
        }

        protected virtual void OnLongPressed()
        {
            var handler = LongPressed;
            handler?.Invoke(this, EventArgs.Empty);

            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }

        public LongPressBehavior()
        {
            _isReleased = true;
            _duration = Duration;
        }

        public LongPressBehavior(int duration) : this()
        {
            _duration = duration;
        }

        private void TimerElapsed(object state)
        {
            DeInitializeTimer();

            if (_isReleased)
            {
                return;
            }

            Device.BeginInvokeOnMainThread(OnLongPressed);
        }
    }
}
