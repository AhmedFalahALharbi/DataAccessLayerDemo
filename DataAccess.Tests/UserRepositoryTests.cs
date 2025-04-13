using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccess.Tests
{
    [TestClass]
    public class UserRepositoryTests
    {
        private AppDbContext _context;
        private UserRepository _repository;
        
        [TestInitialize]
        public void Setup()
        {
            // Setup in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"UsersTestDb_{Guid.NewGuid()}")
                .Options;
                
            _context = new AppDbContext(options);
            _repository = new UserRepository(_context);
        }
        
        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }
        
        #region CRUD Tests
        
        [TestMethod]
        public async Task AddUserAsync_ValidUser_ShouldCreateUser()
        {
            // Arrange
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };
            
            // Act
            var result = await _repository.AddUserAsync(user);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreNotEqual(0, result.Id);
            Assert.AreEqual("John", result.FirstName);
            Assert.AreEqual("Doe", result.LastName);
            Assert.AreEqual("john.doe@example.com", result.Email);
            
            // Verify in database
            var dbUser = await _context.Users.FindAsync(result.Id);
            Assert.IsNotNull(dbUser);
            Assert.AreEqual(user.FirstName, dbUser.FirstName);
            Assert.AreEqual(user.LastName, dbUser.LastName);
            Assert.AreEqual(user.Email, dbUser.Email);
        }
        
        [TestMethod]
        public async Task GetUserByIdAsync_ExistingUser_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _repository.GetUserByIdAsync(user.Id);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.Id, result.Id);
            Assert.AreEqual(user.FirstName, result.FirstName);
            Assert.AreEqual(user.LastName, result.LastName);
            Assert.AreEqual(user.Email, result.Email);
        }
        
        [TestMethod]
        public async Task GetUserByEmailAsync_ExistingEmail_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@example.com"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _repository.GetUserByEmailAsync(user.Email);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.Id, result.Id);
            Assert.AreEqual(user.FirstName, result.FirstName);
            Assert.AreEqual(user.LastName, result.LastName);
            Assert.AreEqual(user.Email, result.Email);
        }
        
        [TestMethod]
        public async Task GetAllUsersAsync_WithUsers_ShouldReturnAllUsers()
        {
            // Arrange
            var users = new User[]
            {
                new User { FirstName = "User1", LastName = "Test", Email = "user1@example.com" },
                new User { FirstName = "User2", LastName = "Test", Email = "user2@example.com" },
                new User { FirstName = "User3", LastName = "Test", Email = "user3@example.com" }
            };
            
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _repository.GetAllUsersAsync();
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            
            foreach (var user in users)
            {
                Assert.IsTrue(result.Any(u => u.Email == user.Email));
            }
        }
        
        [TestMethod]
        public async Task UpdateUserAsync_ExistingUser_ShouldUpdateUser()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Initial",
                LastName = "User",
                Email = "initial@example.com"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            var updatedUser = new User
            {
                Id = user.Id,
                FirstName = "Updated",
                LastName = "Name",
                Email = "updated@example.com"
            };
            
            // Act
            var result = await _repository.UpdateUserAsync(updatedUser);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.Id, result.Id);
            Assert.AreEqual("Updated", result.FirstName);
            Assert.AreEqual("Name", result.LastName);
            Assert.AreEqual("updated@example.com", result.Email);
            
            // Verify in database
            var dbUser = await _context.Users.FindAsync(user.Id);
            Assert.IsNotNull(dbUser);
            Assert.AreEqual("Updated", dbUser.FirstName);
            Assert.AreEqual("Name", dbUser.LastName);
            Assert.AreEqual("updated@example.com", dbUser.Email);
        }
        
        [TestMethod]
        public async Task DeleteUserAsync_ExistingUser_ShouldDeleteUser()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Delete",
                LastName = "Me",
                Email = "delete.me@example.com"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _repository.DeleteUserAsync(user.Id);
            
            // Assert
            Assert.IsTrue(result);
            
            // Verify deletion from database
            var dbUser = await _context.Users.FindAsync(user.Id);
            Assert.IsNull(dbUser);
        }
        
        #endregion
        
        #region Edge Case Tests
        
        [TestMethod]
        public async Task GetUserByIdAsync_NonExistentId_ShouldReturnNull()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await _repository.GetUserByIdAsync(0));
                
            var result = await _repository.GetUserByIdAsync(999);
            Assert.IsNull(result);
        }
        
        [TestMethod]
        public async Task GetUserByEmailAsync_NonExistentEmail_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetUserByEmailAsync("nonexistent@example.com");
            
            // Assert
            Assert.IsNull(result);
        }
        
        [TestMethod]
        public async Task GetUserByEmailAsync_NullOrEmptyEmail_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await _repository.GetUserByEmailAsync(null));
                
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await _repository.GetUserByEmailAsync(""));
                
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await _repository.GetUserByEmailAsync("   "));
        }
        
        [TestMethod]
        public async Task AddUserAsync_NullUser_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () => await _repository.AddUserAsync(null));
        }
        
        [TestMethod]
        public async Task AddUserAsync_DuplicateEmail_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var user1 = new User
            {
                FirstName = "User",
                LastName = "One",
                Email = "duplicate@example.com"
            };
            
            var user2 = new User
            {
                FirstName = "User",
                LastName = "Two",
                Email = "duplicate@example.com"
            };
            
            await _repository.AddUserAsync(user1);
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _repository.AddUserAsync(user2));
        }
        
        [TestMethod]
        public async Task UpdateUserAsync_NullUser_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () => await _repository.UpdateUserAsync(null));
        }
        
        [TestMethod]
        public async Task UpdateUserAsync_NonExistentUser_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var user = new User
            {
                Id = 999,
                FirstName = "NonExistent",
                LastName = "User",
                Email = "nonexistent@example.com"
            };
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _repository.UpdateUserAsync(user));
        }
        
        [TestMethod]
        public async Task UpdateUserAsync_DuplicateEmail_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var user1 = new User
            {
                FirstName = "User",
                LastName = "One",
                Email = "user1@example.com"
            };
            
            var user2 = new User
            {
                FirstName = "User",
                LastName = "Two",
                Email = "user2@example.com"
            };
            
            await _context.Users.AddRangeAsync(new[] { user1, user2 });
            await _context.SaveChangesAsync();
            
            var updateUser = new User
            {
                Id = user2.Id,
                FirstName = user2.FirstName,
                LastName = user2.LastName,
                Email = user1.Email // Trying to use an email that already exists
            };
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _repository.UpdateUserAsync(updateUser));
        }
        
        [TestMethod]
        public async Task DeleteUserAsync_NonExistentId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.DeleteUserAsync(999);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        public async Task DeleteUserAsync_InvalidId_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await _repository.DeleteUserAsync(0));
                
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await _repository.DeleteUserAsync(-1));
        }
        
        [TestMethod]
        public async Task UserExistsAsync_ExistingUser_ShouldReturnTrue()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Exists",
                LastName = "User",
                Email = "exists@example.com"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _repository.UserExistsAsync(user.Id);
            
            // Assert
            Assert.IsTrue(result);
        }
        
        [TestMethod]
        public async Task UserExistsAsync_NonExistentUser_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.UserExistsAsync(999);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        public async Task EmailExistsAsync_ExistingEmail_ShouldReturnTrue()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Email",
                LastName = "Exists",
                Email = "email.exists@example.com"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Act - Check with different case to test case insensitivity
            var result = await _repository.EmailExistsAsync("EMAIL.EXISTS@example.com");
            
            // Assert
            Assert.IsTrue(result);
        }
        
        [TestMethod]
        public async Task EmailExistsAsync_NonExistentEmail_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.EmailExistsAsync("nonexistent.email@example.com");
            
            // Assert
            Assert.IsFalse(result);
        }
        
        #endregion
    }
}