using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IValidator<User> _validator;
        private readonly WebApplicationFactory<IApiStructuredMarker> _factory;

        public UserEndpointsTests(WebApplicationFactory<IApiStructuredMarker> factory)
        {
            _userService = Substitute.For<IUserService>();
            _validator = Substitute.For<IValidator<User>>();
            _factory = factory.WithWebHostBuilder(x => x.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IUserService));
                services.RemoveAll(typeof(IValidator<User>));
                services.AddScoped(_ => _userService);
                services.AddScoped(_ => _validator);
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

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenValidationFails()
        {
            // Arrange.
            _validator.ValidateAsync(Arg.Any<User>()).Returns(new ValidationResult(
                new List<ValidationFailure>
                {
                    new("Test Property", "Error 1"),
                    new("Test Property", "Error 2")
                }
            ));

            StringContent content = new(JsonSerializer.Serialize(new User()), Encoding.UTF8, "application/json");

            var client = _factory.CreateClient();

            // Act.
            var result = await client.PostAsync("User", content, new CancellationToken());

            var resultValidation = await result.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            // Assert.
            result.IsSuccessStatusCode.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            resultValidation!.Errors.Should().BeEquivalentTo(new Dictionary<string, string[]>()
            {
                { "Test Property", new[] { "Error 1", "Error 2" } }
            });
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenUserAlreadyExistsWithSameEmail()
        {
            // Arrange.
            _validator.ValidateAsync(Arg.Any<User>()).Returns(new ValidationResult());

            _userService.GetByEmailAsync(Arg.Any<string>()).Returns(new User());

            StringContent content = new(JsonSerializer.Serialize(new User()), Encoding.UTF8, "application/json");

            var client = _factory.CreateClient();

            // Act.
            var result = await client.PostAsync("User", content, new CancellationToken());

            var resultValidation = await result.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            // Assert.
            result.IsSuccessStatusCode.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            resultValidation!.Errors.Should().BeEquivalentTo(new Dictionary<string, string[]>()
            {
                { "Email", new[] { "User with email address already exists" } }
            });
        }

        [Fact]
        public async Task Create_ReturnsInternalServerError_WhenCreatingUserFails()
        {
            // Arrange.
            _validator.ValidateAsync(Arg.Any<User>()).Returns(new ValidationResult());

            _userService.GetByEmailAsync(Arg.Any<string>()).Returns((User)null);

            _userService.CreateAsync(Arg.Any<User>()).Returns(false);

            StringContent content = new(JsonSerializer.Serialize(new User()), Encoding.UTF8, "application/json");

            var client = _factory.CreateClient();

            // Act.
            var result = await client.PostAsync("User", content, new CancellationToken());

            var resultValidation = await result.Content.ReadFromJsonAsync<ProblemDetails>();

            // Assert.
            result.IsSuccessStatusCode.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            resultValidation!.Title.Should().Be("An error occurred while processing your request.");
            resultValidation.Detail.Should().Be("User could not be created");
        }

        [Fact]
        public async Task Create_ReturnsOkayWithUser_WhenCreatingUserSucceeds()
        {
            // Arrange.
            User testUser = new()
            {
                DateOfBirth = new DateTime(2020, 10, 11),
                YearsOfExperience = 1,
                Email = "test@mail.com",
                FirstName = "test",
                LastName = "also test",
                Skills = "skill 1,skill 2",
                Id = Guid.NewGuid().ToString()
            };

            _validator.ValidateAsync(Arg.Any<User>()).Returns(new ValidationResult());

            _userService.GetByEmailAsync(Arg.Any<string>()).Returns((User)null);

            _userService.CreateAsync(Arg.Any<User>()).Returns(true);

            StringContent content = new(JsonSerializer.Serialize(testUser), Encoding.UTF8, "application/json");

            var client = _factory.CreateClient();

            // Act.
            var result = await client.PostAsync("User", content);
            var reus = await result.Content.ReadAsStringAsync();

            var resultUser = await result.Content.ReadFromJsonAsync<User>();

            // Assert.
            result.IsSuccessStatusCode.Should().BeTrue();
            result.StatusCode.Should().Be(HttpStatusCode.Created);
            resultUser.Should().BeEquivalentTo(testUser);
            result.Headers.Location = new Uri($"{client.BaseAddress}/User/{testUser.Id}");
        }
    }
}