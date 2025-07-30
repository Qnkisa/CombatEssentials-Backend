using CombatEssentials.Application.DTOs.AuthDtos;
using CombatEssentials.Application.Services;
using CombatEssentials.Domain.Entities;
using CombatEssentials.Domain.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace CombatEssentials.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly JwtSettings _jwtSettings;

        public AuthServiceTests()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);

            _jwtSettings = new JwtSettings
            {
                SecretKey = "your-super-secret-key-with-at-least-32-characters",
                Issuer = "CombatEssentials",
                Audience = "CombatEssentialsUsers",
                ExpiryMinutes = 60
            };
        }

        private AuthService GetService()
        {
            var jwtOptions = Options.Create(_jwtSettings);
            return new AuthService(_userManagerMock.Object, _signInManagerMock.Object, jwtOptions);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsSuccess_WhenUserDoesNotExist()
        {
            // Arrange
            var service = GetService();
            var dto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "TestPassword123!",
                FirstName = "John",
                LastName = "Doe"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await service.RegisterAsync(dto);

            // Assert
            Assert.Equal("Registration successful.", result);
            _userManagerMock.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u => 
                u.Email == dto.Email && 
                u.UserName == dto.Email && 
                u.FirstName == dto.FirstName && 
                u.LastName == dto.LastName), dto.Password), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsFailure_WhenUserAlreadyExists()
        {
            // Arrange
            var service = GetService();
            var dto = new RegisterDto
            {
                Email = "existing@example.com",
                Password = "TestPassword123!",
                FirstName = "John",
                LastName = "Doe"
            };

            var existingUser = new ApplicationUser
            {
                Id = "user1",
                Email = dto.Email,
                UserName = dto.Email
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await service.RegisterAsync(dto);

            // Assert
            Assert.Equal("User already exists.", result);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsFailure_WhenUserCreationFails()
        {
            // Arrange
            var service = GetService();
            var dto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "weak",
                FirstName = "John",
                LastName = "Doe"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser)null);

            var errors = new List<IdentityError>
            {
                new IdentityError { Description = "Password is too short." },
                new IdentityError { Description = "Password must contain uppercase letters." }
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

            // Act
            var result = await service.RegisterAsync(dto);

            // Assert
            Assert.Equal("Password is too short.; Password must contain uppercase letters.", result);
        }

        [Fact]
        public async Task LoginAsync_ReturnsValidToken_WhenCredentialsAreValid()
        {
            // Arrange
            var service = GetService();
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            var user = new ApplicationUser
            {
                Id = "user1",
                Email = dto.Email,
                UserName = dto.Email,
                FirstName = "John",
                LastName = "Doe"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await service.LoginAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual("Invalid login credentials.", result);
            
            // Verify the token is valid JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(result);
            
            Assert.Equal(_jwtSettings.Issuer, token.Issuer);
            Assert.Equal(_jwtSettings.Audience, token.Audiences.First());
            Assert.Contains(token.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);
            Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Name && c.Value == user.UserName);
            Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
        }

        [Fact]
        public async Task LoginAsync_ReturnsFailure_WhenUserNotFound()
        {
            // Arrange
            var service = GetService();
            var dto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "TestPassword123!"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await service.LoginAsync(dto);

            // Assert
            Assert.Equal("Invalid login credentials.", result);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ReturnsFailure_WhenPasswordIsIncorrect()
        {
            // Arrange
            var service = GetService();
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword123!"
            };

            var user = new ApplicationUser
            {
                Id = "user1",
                Email = dto.Email,
                UserName = dto.Email
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(false);

            // Act
            var result = await service.LoginAsync(dto);

            // Assert
            Assert.Equal("Invalid login credentials.", result);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_IncludesUserRolesInToken()
        {
            // Arrange
            var service = GetService();
            var dto = new LoginDto
            {
                Email = "admin@example.com",
                Password = "AdminPassword123!"
            };

            var user = new ApplicationUser
            {
                Id = "admin1",
                Email = dto.Email,
                UserName = dto.Email
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin", "User" });

            // Act
            var result = await service.LoginAsync(dto);

            // Assert
            Assert.NotNull(result);
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(result);
            
            var roleClaims = token.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            Assert.Equal(2, roleClaims.Count);
            Assert.Contains(roleClaims, c => c.Value == "Admin");
            Assert.Contains(roleClaims, c => c.Value == "User");
        }

        [Fact]
        public async Task LoginAsync_HandlesUserWithNoRoles()
        {
            // Arrange
            var service = GetService();
            var dto = new LoginDto
            {
                Email = "user@example.com",
                Password = "UserPassword123!"
            };

            var user = new ApplicationUser
            {
                Id = "user1",
                Email = dto.Email,
                UserName = dto.Email
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await service.LoginAsync(dto);

            // Assert
            Assert.NotNull(result);
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(result);
            
            var roleClaims = token.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            Assert.Empty(roleClaims);
        }

        [Fact]
        public async Task LoginAsync_TokenExpiresAfterConfiguredTime()
        {
            // Arrange
            var service = GetService();
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            var user = new ApplicationUser
            {
                Id = "user1",
                Email = dto.Email,
                UserName = dto.Email
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await service.LoginAsync(dto);

            // Assert
            Assert.NotNull(result);
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(result);
            
            // Token should expire after the configured expiry time
            var expectedExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
            Assert.True(token.ValidTo > DateTime.UtcNow);
            Assert.True(token.ValidTo <= expectedExpiry);
        }

        [Fact]
        public async Task RegisterAsync_HandlesNullDtoProperties()
        {
            // Arrange
            var service = GetService();
            var dto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "TestPassword123!",
                FirstName = null,
                LastName = null
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await service.RegisterAsync(dto);

            // Assert
            Assert.Equal("Registration successful.", result);
            _userManagerMock.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u => 
                u.Email == dto.Email && 
                u.UserName == dto.Email && 
                u.FirstName == null && 
                u.LastName == null), dto.Password), Times.Once);
        }
    }
}
