﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.TestClasses
{
    public class AsyncCaseTests
    {
        public void ShouldPassUponSuccessfulAsyncExecution()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(AwaitThenPassFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.AsyncCaseTests+AwaitThenPassFixture.Test passed.");
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsAfterAwaiting()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(AwaitThenFailFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.AsyncCaseTests+AwaitThenFailFixture.Test failed: Assert.Equal() Failure" + Environment.NewLine +
                "Expected: 0" + Environment.NewLine +
                "Actual:   3");
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsWithinTheAwaitedTask()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(AwaitOnTaskThatThrowsFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.AsyncCaseTests+AwaitOnTaskThatThrowsFixture.Test failed: Attempted to divide by zero.");
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsBeforeAwaitingOnAnyTask()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(FailBeforeAwaitFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.AsyncCaseTests+FailBeforeAwaitFixture.Test failed: 'Test' failed!");
        }

        public void ShouldFailUnsupportedAsyncVoidCases()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(UnsupportedAsyncVoidFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.AsyncCaseTests+UnsupportedAsyncVoidFixture.Test failed: " +
                "Async void methods are not supported. Declare async methods with a return type of " +
                "Task to ensure the task actually runs to completion.");
        }

        abstract class SampleFixtureBase
        {
            protected static void ThrowException([CallerMemberName] string member = null)
            {
                throw new FailureException(member);
            }

            protected static Task<int> Divide(int numerator, int denominator)
            {
                return Task.Run(() => numerator/denominator);
            }
        }

        class AwaitThenPassFixture : SampleFixtureBase
        {
            public async Task Test()
            {
                var result = await Divide(15, 5);

                result.ShouldEqual(3);
            }
        }

        class AwaitThenFailFixture : SampleFixtureBase
        {
            public async Task Test()
            {
                var result = await Divide(15, 5);

                result.ShouldEqual(0);
            }
        }

        class AwaitOnTaskThatThrowsFixture : SampleFixtureBase
        {
            public async Task Test()
            {
                await Divide(15, 0);

                throw new ShouldBeUnreachableException();
            }
        }

        class FailBeforeAwaitFixture : SampleFixtureBase
        {
            public async Task Test()
            {
                ThrowException();

                await Divide(15, 5);
            }
        }

        class UnsupportedAsyncVoidFixture : SampleFixtureBase
        {
            public async void Test()
            {
                await Divide(15, 5);

                throw new ShouldBeUnreachableException();
            }
        }
    }
}