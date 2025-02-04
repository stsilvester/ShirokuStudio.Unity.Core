using System;
using UniRx;

namespace ShirokuStudio.Siganls
{
    /// <summary>
    /// 訊息監聽處理
    /// </summary>
    public class SignalHandler : IDisposable
    {
        /// <summary>
        /// 訊息ID
        /// </summary>
        public SignalID ID { get; }

        /// <summary>
        /// 可監聽訊息
        /// </summary>
        public IObservable<object> Subject => subject;

        private Subject<object> subject = new();

        public IObservable<object> AsyncSubject => asyncSubject;

        private AsyncSubject<object> asyncSubject = new();

        /// <summary>
        /// 是否已銷毀
        /// </summary>
        public bool IsDisposed { get; private set; }

        public SignalHandler(SignalID id)
        {
            ID = id;

            subject.Subscribe(
                onNext: asyncSubject.OnNext,
                onError: asyncSubject.OnError,
                onCompleted: asyncSubject.OnCompleted);
        }

        /// <summary>
        /// 廣播訊息
        /// </summary>
        public void Publish(object data)
        {
            if (asyncSubject.HasObservers)
                asyncSubject.OnNext(data);

            if (subject.HasObservers)
                subject.OnNext(data);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            subject.OnCompleted();
            subject.Dispose();
            asyncSubject.Dispose();
        }
    }
}