using ServerMonitorApp.Application.Features.IoT.Events;

namespace ServerMonitorApp.UnitTests.Features.IoT.Events
{
    public class SensorDataRecordedEventTests
    {
        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            Guid expectedDeviceId = Guid.NewGuid();
            long expectedSensorDataId = 9999L;
            decimal expectedTemperature = 25.4m;
            decimal expectedHumidity = 60.5m;

            SensorDataRecordedEvent @event = new SensorDataRecordedEvent(
                expectedDeviceId,
                expectedSensorDataId,
                expectedTemperature,
                expectedHumidity);

            Assert.Equal(expectedDeviceId, @event.DeviceId);
            Assert.Equal(expectedSensorDataId, @event.SensorDataId);
            Assert.Equal(expectedTemperature, @event.Temperature);
            Assert.Equal(expectedHumidity, @event.Humidity);
        }
    }
}