﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace InterReact
{
    // For requests that do not use requestId. 
    public static partial class Extensions
    {
        // Returns a single result: CurrentTime, ManagedAccounts, ScannerParameters
        internal static IObservable<T> ToObservableSingle<T>
            (this IObservable<object> source, Action startRequest) where T : class
        {
            return Observable.Create<T>(observer =>
            {
                var subscription = source.OfType<T>().SubscribeSafe(
                    Observer.Create<T>(
                        onNext: t =>
                        {
                            observer.OnNext(t);
                            observer.OnCompleted();
                        },
                        onError: observer.OnError,
                        onCompleted: observer.OnCompleted));

                startRequest();

                return subscription;
            });
        }


        // Multiple results: AccountPositions, OpenOrders
        internal static IObservable<T> ToObservableMultiple<T,TEnd>(this IObservable<object> source,
            Action startRequest, Action? stopRequest = null) where T : class
        {
            return Observable.Create<T>(observer =>
            {
                bool? cancelable = null;

                var subscription = source
                    .Finally(() => cancelable = false)
                    .SubscribeSafe(Observer.Create<object>(
                        onNext: o =>
                        {
                            if (o is T t)
                                observer.OnNext(t);
                            else if (o is TEnd)
                            {
                                cancelable = false;
                                observer.OnCompleted();
                            }
                        },
                        onError: observer.OnError,
                        onCompleted: observer.OnCompleted));

                if (cancelable == null)
                    startRequest();
                if (cancelable == null)
                    cancelable = true;

                return Disposable.Create(() =>
                {
                    if (cancelable == true)
                        stopRequest?.Invoke();
                    subscription.Dispose();
                });
            });
        }


        // Continuous results: AccountUpdate, NewsBulletins
        internal static IObservable<T> ToObservableContinuous<T>(this IObservable<object> source,
            Action startRequest, Action stopRequest) where T : class
        {
            return Observable.Create<T>(observer =>
            {
                bool? cancelable = null;

                var subscription = source
                    .OfType<T>()
                    .Finally(() => cancelable = false)
                    .SubscribeSafe(Observer.Create<T>(
                        onNext: o =>
                        {
                            observer.OnNext(o);
                        },
                        onError: observer.OnError,
                        onCompleted: observer.OnCompleted));

                if (cancelable == null)
                    startRequest();
                if (cancelable == null)
                    cancelable = true;

                return Disposable.Create(() =>
                {
                    if (cancelable == true)
                        stopRequest.Invoke();
                    subscription.Dispose();
                });
            });
        }

    }
}