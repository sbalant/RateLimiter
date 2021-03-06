﻿using NSubstitute;
using RateLimiter;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace RateLimiterTest
{
    public class RateLimiterTest
    {
        private readonly RateLimiter.TimeLimiter _TimeConstraint;
        private readonly IAwaitableConstraint _IAwaitableConstraint;
        private readonly Func<Task> _FuncTask;
        private readonly Func<Task<int>> _FuncTaskInt;
        private readonly IDisposable _Diposable;

        public RateLimiterTest()
        {
            _FuncTask = Substitute.For<Func<Task>>();
            _FuncTaskInt = Substitute.For<Func<Task<int>>>();
            _IAwaitableConstraint = Substitute.For<IAwaitableConstraint>();
            _Diposable = Substitute.For<IDisposable>();
            _IAwaitableConstraint.WaitForReadiness(Arg.Any<CancellationToken>()).Returns(Task.FromResult(_Diposable));
            _TimeConstraint = new RateLimiter.TimeLimiter(_IAwaitableConstraint);
        }

        [Fact]
        public async Task Perform_CallFuncAndIAwaitableConstraintMethods()
        {
            await _TimeConstraint.Perform(_FuncTask);

            CheckSequence();
        }

        [Fact]
        public void Perform_InCaseOfException_rethrowException()
        {
            _FuncTask.When(ft => ft.Invoke()).Do(_ => { throw new Exception(); });

            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTask);
            act.ShouldThrow<Exception>();
        }

        [Fact]
        public async Task Perform_CallExcecuteInCaseOfException()
        {
            _FuncTask.When(ft => ft.Invoke()).Do(_ => { throw new Exception(); });
            try
            {
                await _TimeConstraint.Perform(_FuncTask);
            }
            catch
            {
            }

            CheckSequence();
        }

        [Fact]
        public void Perform_WhenCancelled_ThrowException()
        {
            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTask, new CancellationToken(true));
            act.ShouldThrow<TaskCanceledException>();
        }

        [Fact]
        public async Task Perform_WhenCancelled_DoNotCallFunction()
        {
            try
            {
                await _TimeConstraint.Perform(_FuncTask, new CancellationToken(true));
            }
            catch
            {
            }
            await _FuncTask.DidNotReceive().Invoke();
        }

        [Fact]
        public void Perform_WhenAwaitableConstraintIsCancelled_ThrowException()
        {
            SetUpAwaitableConstraintIsCancelled();
            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTask, new CancellationToken(true));
            act.ShouldThrow<TaskCanceledException>();
        }

        [Fact]
        public async Task Perform_WhenAwaitableConstraintIsCancelled_DoNotCallFunction()
        {
            SetUpAwaitableConstraintIsCancelled();
            try
            {
                await _TimeConstraint.Perform(_FuncTask, new CancellationToken(true));
            }
            catch
            {
            }
            await _FuncTask.DidNotReceive().Invoke();
        }

        private void CheckSequence()
        {
            Received.InOrder(() => {
                _IAwaitableConstraint.WaitForReadiness(CancellationToken.None);
                _FuncTask.Invoke();
                _Diposable.Dispose();
            });
        }

        [Fact]
        public async Task PerformGeneric_CallFuncAndIAwaitableConstraintMethods()
        {
            await _TimeConstraint.Perform(_FuncTaskInt);

            CheckGenericSequence();
        }

        [Fact]
        public void PerformGeneric_InCaseOfException_rethrowException()
        {
            _FuncTaskInt.When(ft => ft.Invoke()).Do(_ => { throw new Exception(); });

            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTaskInt);
            act.ShouldThrow<Exception>();
        }

        [Fact]
        public async Task PerformGeneric_CallExcecuteInCaseOfException()
        {
            _FuncTaskInt.When(ft => ft.Invoke()).Do(_ => { throw new Exception(); });
            try
            {
                await _TimeConstraint.Perform(_FuncTaskInt);
            }
            catch
            {
            }

            CheckGenericSequence();
        }

        [Fact]
        public void PerformGeneric_WhenCancelled_ThrowException()
        {
            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTaskInt, new CancellationToken(true));
            act.ShouldThrow<TaskCanceledException>();
        }

        [Fact]
        public async Task PerformGeneric_WhenCancelled_DoNotCallFunction()
        {
            try
            {
                await _TimeConstraint.Perform(_FuncTaskInt, new CancellationToken(true));
            }
            catch
            {
            }
            await _FuncTaskInt.DidNotReceive().Invoke();
        }

        [Fact]
        public async Task PerformGeneric_WhenAwaitableConstraintIsCancelled_DoNotCallFunction()
        {
            SetUpAwaitableConstraintIsCancelled();
            try
            {
                await _TimeConstraint.Perform(_FuncTaskInt, new CancellationToken(true));
            }
            catch
            {
            }
            await _FuncTaskInt.DidNotReceive().Invoke();
        }

        [Fact]
        public void PerformGeneric_WhenAwaitableConstraintIsCancelled_ThrowException()
        {
            SetUpAwaitableConstraintIsCancelled();
            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTaskInt, new CancellationToken(true));
            act.ShouldThrow<TaskCanceledException>();
        }

        private void SetUpAwaitableConstraintIsCancelled()
        {
            _IAwaitableConstraint.WaitForReadiness(Arg.Any<CancellationToken>()).Returns(x => { throw new TaskCanceledException(); });
        }

        private void CheckGenericSequence()
        {
            Received.InOrder(() => {
                _IAwaitableConstraint.WaitForReadiness(CancellationToken.None);
                _FuncTaskInt.Invoke();
                _Diposable.Dispose();
            });
        }
    }
}
