using Bacera.Gateway.Vendor;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests.GeoIp
{
    public class MaxMindGeoIpTests
    {
        private readonly MaxMindGeoIp _svc;

        public MaxMindGeoIpTests()
        {
            _svc = new MaxMindGeoIp(
                @"C:\develop\portal\src\Bacera.Gateway.Web\Resources\GeoLite2-Country_20230606\GeoLite2-Country.mmdb");
        }

        [Fact]
        public void CountryLookup_ValidIPv4_ReturnsCountry()
        {
            // Arrange
            string ipAddress = "10.10.10.10";
            // Act
            var actualCountry = _svc.CountryLookup(ipAddress);
            // Assert
            actualCountry.IsEmpty().ShouldBeTrue();
        }

        [Fact]
        public void CountryLookup_ValidIPv4_ReturnsCountryAsUs()
        {
            // Arrange
            string ipAddress = "137.25.19.180";
            // Act
            var actualCountry = _svc.CountryLookup(ipAddress);
            // Assert
            actualCountry.IsEmpty().ShouldBeFalse();
            actualCountry.IsoCode.ShouldBe("US");
        }

        [Fact]
        public void CountryLookup_ValidIPv6_ReturnsCountry()
        {
            // Arrange
            string ipAddress = "fe80::b280:d2ff:fe48:5e0d%3";


            // Act
            var actualCountry = _svc.CountryLookup(ipAddress);

            // Assert
            //Assert.Equal(expectedCountry, actualCountry);
            actualCountry.IsEmpty().ShouldBeTrue();
        }

        //[Fact]
        //public void CountryLookup_NullIPAddress_ThrowsArgumentNullException()
        //{
        //    // Arrange
        //    string ipAddress = null;

        //    // Act & Assert
        //    Assert.Throws<ArgumentNullException>(() => _svc.CountryLookup(ipAddress));
        //}
    }
}