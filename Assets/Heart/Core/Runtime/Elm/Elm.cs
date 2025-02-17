﻿using System;

namespace Pancake.Elm
{
    public class Elm<TModel, TMessage> where TModel : struct where TMessage : struct
    {
        private readonly IUpdater<TModel, TMessage> _updater;
        private readonly IRenderer<TModel, TMessage> _renderer;
        private readonly Func<TModel, Sub<IMessenger<TMessage>>> _subscription;
        private TModel _model;
        private Sub<IMessenger<TMessage>> _currentSubscription;

        public Elm(Func<(TModel, Cmd<TMessage>)> init, IUpdater<TModel, TMessage> updater, IRenderer<TModel, TMessage> renderer)
            : this(init, updater, renderer, _ => Sub<IMessenger<TMessage>>.None)
        {
        }

        public Elm(
            Func<(TModel, Cmd<TMessage>)> init,
            IUpdater<TModel, TMessage> updater,
            IRenderer<TModel, TMessage> renderer,
            Func<TModel, Sub<IMessenger<TMessage>>> subscription)
        {
            _updater = updater;
            _renderer = renderer;
            _subscription = subscription;
            var (model, cmd) = init.Invoke();
            _model = model;
            cmd.Execute(Dispath);
            _renderer.Init(Dispath);
            _renderer.Render(_model);
            UpdateSubscription();
        }

        private void Dispath(IMessenger<TMessage> msg)
        {
            var (model, cmd) = _updater.Update(msg, _model);
            if (!Equals(_model, model))
            {
                _renderer.Render(model);
                _model = model;
            }

            cmd.Execute(Dispath);
            UpdateSubscription();
        }

        private void UpdateSubscription()
        {
            if (_currentSubscription != null) _currentSubscription.OnWatch -= Dispath;

            _currentSubscription = _subscription(_model);
            _currentSubscription.OnWatch += Dispath;
        }
    }
}