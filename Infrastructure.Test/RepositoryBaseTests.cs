
using Microsoft.Diagnostics.Runtime.DacInterface;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Infrastructure.Test
{
    [TestClass]
    public class RepositoryBaseTests
    {
        public class TestRepository : RepositoryBase
        {
            public readonly PropertyInfo _setId = typeof(TestObject)
                .GetProperty("Id", BindingFlags.NonPublic | 
                                   BindingFlags.Instance | 
                                   BindingFlags.Public)!;

            public readonly System.Reflection.FieldInfo _setSecret = typeof(TestObject)
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
            public int Id { get; private set; }
            public int S => secret;
        }

        [TestMethod]
        public void SetPropertyValueWithFunction_AssignsValue_WhenPropertyIsProtected()
        {
            var testObject = new TestObject();
            var test2 = new TestObject();
            var repository = new TestRepository();

            TestRepository.SetPropertyWithDelegate<TestObject, int>(testObject, 5, "Id");
            TestRepository.SetPropertyWithDelegate<TestObject, int>(test2, 6, "Id");
            //TestRepository.SetPropertyWithExpression(() => testObject.Id, 4);
            //repository.TestSetPropertyValue(() => testObject.Id, 4);

            Assert.AreEqual(5, testObject.Id);
            Assert.AreEqual(6, test2.Id);

        }

        [TestMethod]
        public void MethodInfo_AssignsValue_WhenPropertyIsProtected()
        {
            var testObject = new TestObject();
            var test2 = new TestObject();
            var repository = new TestRepository();

            repository._setId.SetValue(testObject, 4);

            Assert.AreEqual(4, testObject.Id);

        }

        [TestMethod]
        public void SetWithIL_AssignsValue_WhenPropertyIsProtected()
        {
            var testObject = new TestObject();
            var test2 = new TestObject();
            var repository = new TestRepository();

            
            var delg = TestRepository.CreateDelegateSetterWithIL<TestObject, int>(repository._setSecret);

            Stopwatch sw = Stopwatch.StartNew();

            delg.DynamicInvoke(testObject, 2);
            var firstTime = sw.ElapsedTicks;

            delg.DynamicInvoke(testObject, 3);
            var secondTime = sw.ElapsedTicks;

            sw.Stop();
            Assert.AreEqual(3, testObject.S);
            Console.WriteLine(String.Format("First Time = {0} Second Time = {1}", firstTime, secondTime));

        }
    }
}