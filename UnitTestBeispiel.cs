
namespace Backend_3.Tests
{
    public class WeatherForecastTests
    {
        [Test]
        public void TemperatureF_Should_Convert_TemperatureC_Correctly()
        {
            // Arrange
            var weatherForecast = new WeatherForecast
            {
                TemperatureC = 25
            };

            // Act
            var temperatureF = weatherForecast.TemperatureF;

            // Assert
            Assert.AreEqual(77, temperatureF);
        }
    }
}