using AutomationTool.DataSource;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutomationTool.Model
{
    public interface IMessage { }

    public sealed class MessageBus
    {
        private readonly ConcurrentDictionary<Type, List<ISubscription>> _subs = new();
        private readonly SynchronizationContext? _uiContext;

        public MessageBus(SynchronizationContext? uiContext = null)
        {
            _uiContext = uiContext;
        }

        private interface ISubscription
        {
            void Invoke(object message);
        }

        private sealed class Subscription<T> : ISubscription where T : IMessage
        {
            private readonly Action<T> _handler;
            private readonly bool _deliverOnUI;
            private readonly SynchronizationContext? _uiContext;

            public Subscription(Action<T> handler, bool deliverOnUI, SynchronizationContext? uiContext)
            {
                _handler = handler;
                _deliverOnUI = deliverOnUI;
                _uiContext = uiContext;
            }

            public void Invoke(object message)
            {
                if (message is not T m) return;

                if (_deliverOnUI && _uiContext != null)
                {
                    _uiContext.Post(_ => SafeInvoke(m), null);
                }
                else
                {
                    SafeInvoke(m);
                }
            }

            private void SafeInvoke(T m)
            {
                try
                {
                    _handler(m);
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("Message handler canceled.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        public IDisposable Subscribe<T>(Action<T> handler) where T : IMessage
            => AddSubscription(typeof(T), new Subscription<T>(handler, deliverOnUI: false, _uiContext));

        public IDisposable SubscribeUIThread<T>(Action<T> handler) where T : IMessage
            => AddSubscription(typeof(T), new Subscription<T>(handler, deliverOnUI: true, _uiContext));

        private IDisposable AddSubscription(Type type, ISubscription sub)
        {
            var list = _subs.GetOrAdd(type, _ => new List<ISubscription>());
            lock (list) list.Add(sub);
            return new Unsubscriber(type, sub, _subs);
        }

        public void Publish<T>(T message) where T : IMessage
        {
            if (!_subs.TryGetValue(typeof(T), out var list)) return;

            ISubscription[] snapshot;
            lock (list) snapshot = list.ToArray();

            foreach (var s in snapshot)
                s.Invoke(message!);
        }

        private sealed class Unsubscriber : IDisposable
        {
            private readonly Type _type;
            private readonly ISubscription _sub;
            private readonly ConcurrentDictionary<Type, List<ISubscription>> _dict;
            private int _disposed;

            public Unsubscriber(Type type, ISubscription sub, ConcurrentDictionary<Type, List<ISubscription>> dict)
            {
                _type = type; _sub = sub; _dict = dict;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
                if (_dict.TryGetValue(_type, out var list))
                {
                    lock (list) list.Remove(_sub);
                }
            }
        }
    }

    public record ShowMessage(string Message, string Title) : IMessage;

    public record EnqueueTask(AutoGroup autoGroup, Func<Task> task) : IMessage;

    public record BeginEnqueueTask(AutoGroup autoGroup) : IMessage;

    public record FinishEnqueueTask(string guid) : IMessage;

    public record CloseAllTabs : IMessage;
}
