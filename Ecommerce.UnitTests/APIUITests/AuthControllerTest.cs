using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ecommerce.APIUI.Controllers;
using Ecommerce.APIUI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.APIUITests
{
    public abstract class AuthControllerTest
    {
        protected Mock<UserManager<IdentityUser>> UserManagerMock { get; }
        protected IOptions<JWTSettingsModel> JWTSettings { get; }
        protected Mock<ILogger<AuthController>> LoggerMock { get; }
        protected AuthController ServiceUnderTest { get; }

        public AuthControllerTest()
        {
            JWTSettingsModel a = new JWTSettingsModel()
            {
                Issuer = "Here",
                ValidIssuers = new string[] { "Here" },
                Secret = "GBIDigg[2;RAF16<QReEQkU5W%@SjQ-eWJh_rC+/tJ5c$(`5}VHhd?*xe3o>E}Q",
                ExpirationInDays = 1
            };

            JWTSettings = Options.Create<JWTSettingsModel>(a as JWTSettingsModel);
            var store = new Mock<IUserStore<IdentityUser>>();
            UserManagerMock = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            LoggerMock = new Mock<ILogger<AuthController>>();

            ServiceUnderTest = new AuthController(
                    UserManagerMock.Object,
                    JWTSettings,
                    LoggerMock.Object);
        }

        public class Register : AuthControllerTest
        {
            [Fact]
            public async Task Should_return_OkResult_when_user_is_successfully_created()
            {
                // Arrange
                UserRegisterModel user = new UserRegisterModel() { Email = "Mail@mail.com", Password = "Hi" };
                // var identityUserMock = new Mock<IdentityResult>();

                UserManagerMock
                    .Setup(x => x.CreateAsync(
                        It.Is<IdentityUser>(x => x.UserName == user.Email && x.Email == user.Email),
                        user.Password)
                        )
                    .ReturnsAsync(IdentityResult.Success)
                    .Verifiable();

                // Act
                var result = await ServiceUnderTest.Register(user);

                // Assert
                var OkResult = Assert.IsType<OkResult>(result);
                UserManagerMock.Verify(x => x.CreateAsync(It.Is<IdentityUser>(x => x.UserName == user.Email && x.Email == user.Email),
                        user.Password), Times.Once);
            }

            [Fact]
            public async Task Should_return_BadRequestObjectResult_when_cannot_create_user()
            {
                // Arrange

                UserRegisterModel user = new UserRegisterModel() { Email = "Mail@mail.com", Password = "Hi" };
                // var identityUserMock = new Mock<IdentityResult>();

                IdentityError[] expectedErrors = new[]{
                    new IdentityError(){Code = "error1" , Description = "Description1"},
                    new IdentityError(){Code = "error2" , Description = "Description2"},
                    new IdentityError(){Code = "error3" , Description = "Description3"},
                };
                UserManagerMock
                    .Setup(x => x.CreateAsync(
                        It.Is<IdentityUser>(x => x.UserName == user.Email && x.Email == user.Email),
                        user.Password)
                        )
                    .ReturnsAsync(IdentityResult.Failed(expectedErrors))
                    .Verifiable();


                // Act
                var result = await ServiceUnderTest.Register(user);

                // Assert
                UserManagerMock.Verify(x => x.CreateAsync(
                    It.Is<IdentityUser>(x => x.UserName == user.Email && x.Email == user.Email),
                        user.Password), Times.Once);

                var badRequestObject = Assert.IsType<BadRequestObjectResult>(result);
                var errors = (badRequestObject.Value as IEnumerable<IdentityError>).ToArray();
                Assert.Equal(expectedErrors.Length, errors.Length);
                for (int i = 0; i < expectedErrors.Length; i++)
                {
                    Assert.Equal(expectedErrors[i], errors[i]);
                }
            }

            [Fact]
            public async Task Should_return_Status500InternalServerError_if_something_went_wrong()
            {
                // Arrange
                UserRegisterModel user = new UserRegisterModel() { Email = "Mail@mail.com", Password = "Hi" };

                UserManagerMock
                    .Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                    .ThrowsAsync(new Exception());

                // Act 
                var result = await ServiceUnderTest.Register(user);

                // Assert
                var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
                Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            }
        }

        public class GetToken : AuthControllerTest
        {
            [Fact]
            public async Task Should_return_BadRequestResult_if_passed_null_UserLoginModel()
            {
                // Act
                var result = await ServiceUnderTest.GetToken(null);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }

            [Fact]
            public async Task Should_return_BadRequestResult_if_cannot_find_user_token()
            {
                // Arrange
                var userEmail = "mail@mail.com";
                var userPassword = "Hi";
                var identityUser = new IdentityUser() { UserName = userEmail, Email = userEmail, PasswordHash = userPassword };
                List<IdentityUser> users = new List<IdentityUser>() { identityUser };

                var user = new UserLoginModel() { Email = userEmail + "unExisted", Password = userPassword };
                UserManagerMock.Setup(x => x.Users)
                    .Returns(users.AsQueryable());

                // Act
                var result = await ServiceUnderTest.GetToken(user);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }

            [Fact]
            public async Task Should_return_BadRequestObjectResult_if_password_is_incorrect()
            {
                // Arrange
                var userEmail = "mail@mail.com";
                var userPassword = "Hi";
                var identityUser = new IdentityUser() { UserName = userEmail, Email = userEmail, PasswordHash = userPassword };
                List<IdentityUser> users = new List<IdentityUser>() { identityUser };
                var user = new UserLoginModel() { Email = userEmail, Password = userPassword };
                UserManagerMock.Setup(x => x.Users)
                    .Returns(users.AsQueryable());

                UserManagerMock
                    .Setup(x => x.CheckPasswordAsync(It.Is<IdentityUser>(x => x.Email == user.Email && x.UserName == user.Email), user.Password))
                    .ReturnsAsync(false);

                // Act
                var result = await ServiceUnderTest.GetToken(user);

                // Assert
                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result);

            }

            [Fact]
            public async Task Should_return_OkObjectResult_if_password_is_correct()
            {
                var userEmail = "mail@mail.com";
                var userPassword = "Hi";
                var identityUser = new IdentityUser() { UserName = userEmail, Email = userEmail, PasswordHash = userPassword };
                List<IdentityUser> users = new List<IdentityUser>() { identityUser };
                var userLogin = new UserLoginModel() { Email = userEmail, Password = userPassword };
                UserManagerMock.Setup(x => x.Users)
                    .Returns(users.AsQueryable());

                var roles = new string[] { "Admin", "Moderator", "SuperModerator" };

                UserManagerMock
                    .Setup(x => x.CheckPasswordAsync(It.Is<IdentityUser>(x => x.Email == userLogin.Email && x.UserName == userLogin.Email), userLogin.Password))
                    .ReturnsAsync(true)
                    .Verifiable();
                UserManagerMock
                    .Setup(x => x.GetRolesAsync(It.Is<IdentityUser>(u => u.Email == userLogin.Email && u.UserName == userLogin.Email)))
                    .ReturnsAsync(roles)
                    .Verifiable();

                // Act
                var result = await ServiceUnderTest.GetToken(userLogin);

                // Assert
                var okObjectResult = Assert.IsType<OkObjectResult>(result);
                UserManagerMock.Verify(x => x.GetRolesAsync(It.Is<IdentityUser>(u => u.Email == userLogin.Email && u.UserName == userLogin.Email)), Times.Once);
                UserManagerMock.Verify(
                    x => x.CheckPasswordAsync(It.Is<IdentityUser>(x => x.Email == userLogin.Email && x.UserName == userLogin.Email), userLogin.Password),
                    Times.Once);

                // read token
                var token = Assert.IsType<TokenModel>(okObjectResult.Value);
                var claims = new JwtSecurityTokenHandler().ReadJwtToken(token.Token).Claims;
                var actualRoles = claims.Where(o=>o.Type == ClaimTypes.Role);

                Assert.NotEmpty(actualRoles);
                Assert.Equal(roles.Count(),actualRoles.Count());
                
            }

        }

    }
}