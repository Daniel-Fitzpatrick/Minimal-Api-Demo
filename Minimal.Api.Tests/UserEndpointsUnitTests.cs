using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Minimal.Api.Common.Model;
using Minimal.Api.Common.Services;
using Minimal.Api.Structured.Endpoints;
using NSubstitute;
using Xunit;

namespace Minimal.Api.Tests
{
    public class UserEndpointsUnitTests
    {
        private readonly IUserService _userService;

        public UserEndpointsUnitTests()
        {
            _userService = Substitute.For<IUserService>();
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenNoUserExistsWithId()
        {
            // Arrange.
            _userService.GetByIdAsync(Arg.Any<Guid>()).Returns((User?)null);

            // Act.
            IResult result = await UserEndpoints.GetUserById(Guid.NewGuid(), _userService);
            //Controller version public = Microsoft.AspNetCore.Mvc.NotFoundObjectResult
            //var notFoundResult = (Microsoft.AspNetCore.Http.Result.NotFoundObjectResult)
            //Results.NotFound()

            // Assert.
            await Task.Delay(1000);
        }

        [Fact]
        public async Task GetUserById_ReturnsOkay_WhenUserExistsWithId()
        {
            // Arrange.
            _userService.GetByIdAsync(Arg.Any<Guid>()).Returns(new User());

            // Act.
            IResult result = await UserEndpoints.GetUserById(Guid.NewGuid(), _userService);
            //Controller version public = Microsoft.AspNetCore.Mvc.OkObjectResult
            //var okResult = (Microsoft.AspNetCore.Http.Result.OkObjectResult)
            //Results.Ok()

            // Assert.
            await Task.Delay(1000);
        }

    }

}
