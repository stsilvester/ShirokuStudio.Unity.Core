using System;
using System.Linq;
using UniRx;
using Zenject;

namespace ShirokuStudio.Siganls
{
    /// <summary>
    /// 訊息中心
    /// </summary>
    public class SignalCenter
    {
        /// <summary>
        /// 訊息中心實體
        /// </summary>
        public static SignalCenter Instance { get; } = new();

        private readonly MessageBroker broker = new MessageBroker();
        private readonly AsyncMessageBroker asyncBroker = new AsyncMessageBroker();

        private SignalCenter()
        {
        }

        /// <summary>
        /// 註冊訊息監聽事件
        /// </summary>
        public IDisposable Subscribe<TSignal>(
            Action<TSignal> onNext,
            Action<Exception> onError = null,
            Action onCompleted = null)
            where TSignal : ISignal
        {
            onError ??= handleError;
            onCompleted ??= handleCompleted;
            return broker.Receive<TSignal>().Subscribe(onNext, onError, onCompleted);
        }

        private static void handleError(Exception ex) => throw ex;

        private static void handleCompleted()
        { }

        /// <summary>
        /// 取得可監聽的非同步訊息事件串流
        /// </summary>
        public IDisposable SubscribeAsync<TSignal>(Func<TSignal, IObservable<Unit>> asyncMessageReceiver)
            where TSignal : ISignal
        {
            return asyncBroker.Subscribe(asyncMessageReceiver);
        }

        /// <summary>
        /// 廣播訊息
        /// </summary>
        public async void Publish<TSignal>(TSignal signal)
            where TSignal : ISignal
        {
            var isBase = typeof(ISignal) == typeof(TSignal);

            broker.Publish(signal);

            if (isBase == false)
                broker.Publish((ISignal)signal);

            await asyncBroker.PublishAsync(signal);

            if (isBase == false)
                await asyncBroker.PublishAsync((ISignal)signal);

            if (signal is ITransmitSignal transmit)
                transmit.TransmitTarget?.HandleSignal(signal);
        }
    }
}