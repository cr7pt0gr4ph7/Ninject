namespace Ninject.Tests.Unit
{
    using System;
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using Ninject.Infrastructure.Language;
    using Xunit;

#if !SILVERLIGHT
    public class ExtensionsForMemberInfoTest
    {
        [Fact]
        public void HasAttribute()
        {
            this.TestHasAttribute("PublicProperty");
            this.TestHasAttribute("InternalProperty");
            this.TestHasAttribute("ProtectedProperty");
            this.TestHasAttribute("PrivateProperty");
        }

        [Fact]
        public void HasAttributeForAttributesOnBaseClass()
        {
            this.TestHasAttributeForAttributesOnBaseClass("PublicProperty");
            this.TestHasAttributeForAttributesOnBaseClass("InternalProperty");
            this.TestHasAttributeForAttributesOnBaseClass("ProtectedProperty");
        }

        [Fact]
        public void GetCustomAttributesExtended()
        {
            this.TestGetCustomAttributesExtended("PublicProperty");
            this.TestGetCustomAttributesExtended("InternalProperty");
            this.TestGetCustomAttributesExtended("ProtectedProperty");
            this.TestGetCustomAttributesExtended("PrivateProperty");
        }
        
        [Fact]
        public void GetCustomAttributesExtendedForAttributesOnBaseClass()
        {
            this.TestGetCustomAttributesExtendedForAttributesOnBaseClass("PublicProperty");
            this.TestGetCustomAttributesExtendedForAttributesOnBaseClass("InternalProperty");
            this.TestGetCustomAttributesExtendedForAttributesOnBaseClass("ProtectedProperty");
        }

        [Fact]
        public void IndexerHasAttribute()
        {
            this.TestIndexerHasAttribute(typeof(PropertyAttributeTest), typeof(string), typeof(InjectAttribute), true);
            this.TestIndexerHasAttribute(typeof(PropertyAttributeTest), typeof(int), typeof(InjectAttribute), false);
            this.TestIndexerHasAttribute(typeof(PropertyAttributeTest), typeof(string), typeof(NotInheritedInjectAttribute), true);
            this.TestIndexerHasAttribute(typeof(PropertyAttributeTest), typeof(int), typeof(NotInheritedInjectAttribute), false);
            this.TestIndexerHasAttribute(typeof(InheritedPropertyAttributeTest), typeof(string), typeof(InjectAttribute), true);
            this.TestIndexerHasAttribute(typeof(InheritedPropertyAttributeTest), typeof(int), typeof(InjectAttribute), false);
            this.TestIndexerHasAttribute(typeof(InheritedPropertyAttributeTest), typeof(string), typeof(NotInheritedInjectAttribute), false);
            this.TestIndexerHasAttribute(typeof(InheritedPropertyAttributeTest), typeof(int), typeof(NotInheritedInjectAttribute), false);
        }

        public void TestIndexerHasAttribute(Type testObjectType, Type indexerType, Type attributeType, bool expectedResult)
        {
#if !WINRT
            var propertyInfo =
                testObjectType.GetProperties()
                    .First(pi => pi.Name == "Item" && pi.GetIndexParameters().Single().ParameterType == indexerType);
#else
            var propertyInfo =
                testObjectType.GetRuntimeProperties()
                              .First(pi => pi.Name == "Item" && pi.GetIndexParameters()
                                                                  .Single()
                                                                  .ParameterType == indexerType);
#endif

            var hasInjectAttribute = propertyInfo.HasAttribute(attributeType);

            hasInjectAttribute.Should().Be(expectedResult);
        }

        private void TestGetCustomAttributesExtended(string propertyName)
        {
            this.TestGetCustomAttributesExtended(propertyName, true);
            this.TestGetCustomAttributesExtended(propertyName, false);
        }

        private void TestGetCustomAttributesExtended(string propertyName, bool inherit)
        {
            var propertyAttributeClass = new PropertyAttributeTest();
            this.TestGetCustomAttributesExtended(propertyAttributeClass, propertyName, typeof(InjectAttribute), inherit, new[] { new InjectAttribute(), new NotInheritedInjectAttribute() });
            this.TestGetCustomAttributesExtended(propertyAttributeClass, propertyName, typeof(NotInheritedInjectAttribute), inherit, new[] { new NotInheritedInjectAttribute() });
            this.TestGetCustomAttributesExtended(propertyAttributeClass, propertyName, typeof(NamedAttribute), inherit, new NamedAttribute[0]);
        }

        private void TestGetCustomAttributesExtendedForAttributesOnBaseClass(string propertyName)
        {
            var propertyAttributeClass = new InheritedPropertyAttributeTest();
            this.TestGetCustomAttributesExtended(propertyAttributeClass, propertyName, typeof(InjectAttribute), true, new[] { new InjectAttribute() });
            this.TestGetCustomAttributesExtended(propertyAttributeClass, propertyName, typeof(InjectAttribute), false, new InjectAttribute[0]);
            this.TestGetCustomAttributesExtended(propertyAttributeClass, propertyName, typeof(NotInheritedInjectAttribute), true, new NotInheritedInjectAttribute[0]);
            this.TestGetCustomAttributesExtended(propertyAttributeClass, propertyName, typeof(NamedAttribute), true, new NamedAttribute[0]);
        }

        private void TestGetCustomAttributesExtended(object testObject, string attributeName, Type attributeType, bool inherit, object[] expectedAttributes)
        {

            var propertyInfo = testObject.GetType()
                                         .GetRuntimeProperties()
                                         .Single(pi => pi.Name == attributeName);

            var attributes = propertyInfo.GetCustomAttributesExtended(attributeType, inherit);

            attributes.Count().Should().Be(expectedAttributes.Length, "attrib: {0}, attribType: {1}", attributeName, attributeType.Name);
            foreach (Attribute expectedAttribute in expectedAttributes)
            {
                attributes.Should().Contain(expectedAttribute, "attrib: {0}, attribType: {1}", attributeName, attributeType.Name);
            }
        }

        private void TestHasAttribute(string propertyName)
        {
            var propertyAttributeClass = new PropertyAttributeTest();
            this.TestHasAttribute(propertyAttributeClass, propertyName, typeof(InjectAttribute), true);
            this.TestHasAttribute(propertyAttributeClass, propertyName, typeof(NotInheritedInjectAttribute), true);
            this.TestHasAttribute(propertyAttributeClass, propertyName, typeof(NamedAttribute), false);
        }

        private void TestHasAttributeForAttributesOnBaseClass(string propertyName)
        {
            var propertyAttributeClass = new InheritedPropertyAttributeTest();
            this.TestHasAttribute(propertyAttributeClass, propertyName, typeof(InjectAttribute), true);
            this.TestHasAttribute(propertyAttributeClass, propertyName, typeof(NotInheritedInjectAttribute), false);
            this.TestHasAttribute(propertyAttributeClass, propertyName, typeof(NamedAttribute), false);
        }
        
        private void TestHasAttribute(object testObject, string attributeName, Type attributeType, bool expectedValue)
        {
#if !WINRT
            var propertyInfo = testObject.GetType()
                .GetProperty(attributeName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#else
            var propertyInfo = testObject.GetType()
                                         .GetRuntimeProperties()
                                         .Single(pi => pi.Name == attributeName);
#endif
            
            bool hasAttribute = propertyInfo.HasAttribute(attributeType);

            hasAttribute.Should().Be(expectedValue);
        }

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
        public class NotInheritedInjectAttribute : InjectAttribute
        {
        }
        
        public class PropertyAttributeTest
        {
            [Inject]
            [NotInheritedInject]
            public virtual object PublicProperty { get; set; }

            [Inject]
            [NotInheritedInject]
            internal virtual object InternalProperty { get; set; }

            [Inject]
            [NotInheritedInject]
            protected internal virtual object InternalProtectedProperty { get; set; }

            [Inject]
            [NotInheritedInject]
            protected virtual object ProtectedProperty { get; set; }

            [Inject]
            [NotInheritedInject]
            private object PrivateProperty { get; set; }

            [Inject]
            [NotInheritedInject]
            public virtual object this[string name]
            {
                get
                {
                    return string.Empty;
                }

                set
                {
                }
            }

            public virtual object this[int name]
            {
                get
                {
                    return string.Empty;
                }

                set
                {
                }
            }
        }

        public class InheritedPropertyAttributeTest : PropertyAttributeTest
        {
            public override object PublicProperty { get; set; }

            internal override object InternalProperty { get; set; }

            protected internal override object InternalProtectedProperty { get; set; }

            protected override object ProtectedProperty { get; set; }

            public override object this[string name]
            {
                get
                {
                    return string.Empty;
                }

                set
                {
                }
            }

            public override object this[int name]
            {
                get
                {
                    return string.Empty;
                }

                set
                {
                }
            }
        }    
    }
#endif
}