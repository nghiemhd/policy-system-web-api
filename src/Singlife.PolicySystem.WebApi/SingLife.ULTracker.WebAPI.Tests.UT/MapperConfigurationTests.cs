using NUnit.Framework;

namespace SingLife.ULTracker.WebAPI.Tests.UT
{
    [TestFixture]
    public class MapperConfigurationTests
    {
        [Test]
        public void Mapper_configuration_is_valid()
        {
            // Arrange
            var configuration = Startup.CreateMapperConfiguration();

            // Act
            TestDelegate act = () => configuration.AssertConfigurationIsValid();

            // Assert
            Assert.That(act, Throws.Nothing);
        }
    }
}