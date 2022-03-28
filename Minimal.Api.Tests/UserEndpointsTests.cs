using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Minimal.Api.Common.Model;
using Minimal.Api.Common.Services;
using Minimal.Api.Structured;
using NSubstitute;
using Xunit;

namespace Minimal.Api.Tests
{
    public class UserEndpointsTests : IClassFixture<WebApplicationFactory<IApiStructuredMarker>>
    {
        private readonly IUserService _userService;
        private readonly WebApplicationFactory<IApiStructuredMarker> _factory;

        public UserEndpointsTests(WebApplicationFactory<IApiStructuredMarker> factory)
        {
            _userService = Substitute.For<IUserService>();

            _factory = factory.WithWebHostBuilder(x => x.ConfigureServices(services =>
              {
                  services.RemoveAll(typeof(IUserService));
                  services.AddScoped(_ => _userService);
              }));
        }

        [Fact]
        public async Task GetById_ReturnsOkay_WhenUserExists()
        {
            // Arrange.
            User testUser = new()
            {
                DateOfBirth = new DateTime(2020, 10, 11),
                YearsOfExperience = 1,
                Email = "test@mail.com",
                FirstName = "test",
                LastName = "also test",
                Skills = "skill 1,skill 2"
            };

            _userService.GetByIdAsync(Arg.Any<Guid>()).Returns(testUser);

            var client = _factory.CreateClient();

            // Act.
            var result = await client.GetAsync($"User/{new Guid()}", new CancellationToken());
            var resultUser = await result.Content.ReadFromJsonAsync<User>();

            // Assert.
            result.IsSuccessStatusCode.Should().BeTrue();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            resultUser.Should().BeEquivalentTo(testUser);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenUserDoesNotExists()
        {
            // Arrange.
            _userService.GetByIdAsync(Arg.Any<Guid>()).Returns((User)null);

            var client = _factory.CreateClient();

            // Act.
            var result = await client.GetAsync($"User/{new Guid()}", new CancellationToken());

            // Assert.
            result.IsSuccessStatusCode.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}