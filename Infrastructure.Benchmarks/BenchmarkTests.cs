using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Benchmarks
{
    public class BenchmarkTests
    {
        public class TestRepository : RepositoryBase
        {
            public readonly PropertyInfo _setId = typeof(TestObject)
                .GetProperty("Id", BindingFlags.NonPublic |
                                   BindingFlags.Instance |
                                   BindingFlags.Public)!;

            public readonly FieldInfo _setSecret = typeof(TestObject)
                .GetField("secret", BindingFlags.NonPublic |
                                    BindingFlags.Instance)!;

            public void TestSetPropertyValue<T>(Expression<Func<T>> expression, object value)
            {
                SetPropertyValue(expression.Body, value);
            }

            public void TestSetPropertyValue<T>(Expression<Action<T>> expression, object value)
            {
                SetPropertyValue(expression.Body, value);
            }
        }

        public class TestObject
        {
            private readonly int secret = 1;
            public static string SecretName = nameof(secret);
            public int Id { get; protected set; }
        }

        public TestRepository _repository { get; private set; } = new TestRepository();
        public TestObject _testObject { get; private set; } = new TestObject();

        [Benchmark]
        public void SetWithDynamicInvokeDelegate()
        {
            var delg = TestRepository.CreateDelegateSetterWithIL<TestObject, int>(_repository._setSecret);
            delg.DynamicInvoke(_testObject, 2);
            delg.DynamicInvoke(_testObject, 2);

        }

        [Benchmark]
        public void SetWithInvokeDelegate()
        {
            var delg = TestRepository.CreateDelegateSetterWithIL<TestObject, int>(_repository._setSecret);
            delg.Invoke(_testObject, 2);
            delg.Invoke(_testObject, 2);

        }

        //[Benchmark]
        //public void SetField()
        //{
        //    TestRepository.SetWithILDelegate<TestObject, int>(_testObject, 2, _repository._setSecret);
        //    TestRepository.SetWithILDelegate<TestObject, int>(_testObject, 3, _repository._setSecret);
        //}

        //[Benchmark]
        //public void SetPrivatePropertyWithDelegate()
        //{
        //    TestRepository.SetPropertyWithDelegate<TestObject, int>(_testObject, 5, "Id");
        //    TestRepository.SetPropertyWithDelegate<TestObject, int>(_testObject, 6, "Id");
        //}

        //[Benchmark]
        //public void SetPrivatePropertyWithSetPropertyValue()
        //{
        //    TestRepository.SetPropertyValue(() => _testObject.Id, 4);
        //}

        [Benchmark]
        public void SetId()
        {
            _repository._setId.SetValue(_testObject, 4);
        }
    }
}
