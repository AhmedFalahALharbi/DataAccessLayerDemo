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
    public class OrderRepositoryTests
    {
        private AppDbContext _context;
        private OrderRepository _repository;
        
        [TestInitialize]
        public void Setup()
        {
            // Setup in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"OrdersTestDb_{Guid.NewGuid()}")
                .Options;
                
            _context = new AppDbContext(options);
            _repository = new OrderRepository(_context);
        }
        
        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }
        
        // Helper method to create a test user
        private async Task<User> CreateTestUserAsync()
        {
            var user = new User
            {
                FirstName = "Test",
                LastName = "User",
                Email = $"test.user.{Guid.NewGuid()}@example.com"
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        
        #region CRUD Tests
        
        [TestMethod]
        public async Task AddOrderAsync_ValidOrder_ShouldCreateOrder()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            
            var order = new Order
            {
                UserId = user.Id,
                Product = "Test Product",
                Quantity = 2,
                Price = 19.99m
            };
            
            // Act
            var result = await _repository.AddOrderAsync(order);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreNotEqual(0, result.OrderId);
            Assert.AreEqual(user.Id, result.UserId);
            Assert.AreEqual("Test Product", result.Product);
            Assert.AreEqual(2, result.Quantity);
            Assert.AreEqual(19.99m, result.Price);
            
            // Verify in database
            var dbOrder = await _context.Orders.FindAsync(result.OrderId);
            Assert.IsNotNull(dbOrder);
            Assert.AreEqual(order.UserId, dbOrder.UserId);
            Assert.AreEqual(order.Product, dbOrder.Product);
            Assert.AreEqual(order.Quantity, dbOrder.Quantity);
            Assert.AreEqual(order.Price, dbOrder.Price);
        }
        
        [TestMethod]
        public async Task GetOrderByIdAsync_ExistingOrder_ShouldReturnOrder()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            
            var order = new Order
            {
                UserId = user.Id,
                Product = "Test Product",
                Quantity = 1,
                Price = 29.99m
            };
            
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _repository.GetOrderByIdAsync(order.OrderId);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(order.OrderId, result.OrderId);
            Assert.AreEqual(order.UserId, result.UserId);
            Assert.AreEqual(order.Product, result.Product);
            Assert.AreEqual(order.Quantity, result.Quantity);
            Assert.AreEqual(order.Price, result.Price);
            Assert.IsNotNull(result.User); // Navigation property should be included
            Assert.AreEqual(user.FirstName, result.User.FirstName);
        }
        
        [TestMethod]
        public async Task GetAllOrdersAsync_WithOrders_ShouldReturnAllOrders()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            
            var orders = new Order[]
            {
                new Order { UserId = user.Id, Product = "Product 1", Quantity = 1, Price = 10.99m },
                new Order { UserId = user.Id, Product = "Product 2", Quantity = 2, Price = 20.99m },
                new Order { UserId = user.Id, Product = "Product 3", Quantity = 3, Price = 30.99m }
            };
            
            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _repository.GetAllOrdersAsync();
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            
            foreach (var order in orders)
            {
                Assert.IsTrue(result.Any(o => o.Product == order.Product));
            }
            
            // Check that navigation properties are loaded
            foreach (var order in result)
            {
                Assert.IsNotNull(order.User);
                Assert.AreEqual(user.FirstName, order.User.FirstName);
            }
        }
        
        [TestMethod]
        public async Task GetOrdersByUserIdAsync_ExistingUserId_ShouldReturnUserOrders()
        {
            // Arrange
            var user1 = await CreateTestUserAsync();
            var user2 = await CreateTestUserAsync();
            
            var user1Orders = new Order[]
            {
                new Order { UserId = user1.Id, Product = "User1 Product 1", Quantity = 1, Price = 10.99m },
                new Order { UserId = user1.Id, Product = "User1 Product 2", Quantity = 2, Price = 20.99m }
            };
            
            var user2Orders = new Order[]
            {
                new Order { UserId = user2.Id, Product = "User2 Product", Quantity = 3, Price = 30.99m }
            };
            
            await _context.Orders.AddRangeAsync(user1Orders);
            await _context.Orders.AddRangeAsync(user2Orders);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _repository.GetOrdersByUserIdAsync(user1.Id);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            
            foreach (var order in user1Orders)
            {
                Assert.IsTrue(result.Any(o => o.Product == order.Product));
            }
            
            Assert.IsFalse(result.Any(o => o.Product == user2Orders[0].Product));
        }
        
        [TestMethod]
        public async Task UpdateOrderAsync_ExistingOrder_ShouldUpdateOrder()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            
            var order = new Order
            {
                UserId = user.Id,
                Product = "Initial Product",
                Quantity = 1,
                Price = 9.99m
            };
            
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            var updatedOrder = new Order
            {
                OrderId = order.OrderId,
                UserId = user.Id,
                Product = "Updated Product",
                Quantity = 5,
                Price = 49.99m
            };
            
            // Act
            var result = await _repository.UpdateOrderAsync(updatedOrder);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(order.OrderId, result.OrderId);
            Assert.AreEqual(user.Id, result.UserId);
            Assert.AreEqual("Updated Product", result.Product);
            Assert.AreEqual(5, result.Quantity);
            Assert.AreEqual(49.99m, result.Price);
            
            // Verify in database
            var dbOrder = await _context.Orders.FindAsync(order.OrderId);
            Assert.IsNotNull(dbOrder);
            Assert.AreEqual("Updated Product", dbOrder.Product);
            Assert.AreEqual(5, dbOrder.Quantity);
            Assert.AreEqual(49.99m, dbOrder.Price);
        }
        
        [TestMethod]
        public async Task DeleteOrderAsync_ExistingOrder_ShouldDeleteOrder()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            
            var order = new Order
            {
                UserId = user.Id,
                Product = "Delete Me",
                Quantity = 1,
                Price = 99.99m
            };
            
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _repository.DeleteOrderAsync(order.OrderId);
            
            // Assert
            Assert.IsTrue(result);
            
            // Verify deletion from database
            var dbOrder = await _context.Orders.FindAsync(order.OrderId);
            Assert.IsNull(dbOrder);
        }
        
        #endregion
        
        #region Edge Case Tests
        
        [TestMethod]
        public async Task GetOrderByIdAsync_NonExistentId_ShouldReturnNull()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await _repository.GetOrderByIdAsync(0));
                
            var result = await _repository.GetOrderByIdAsync(999);
            Assert.IsNull(result);
        }
        
        [TestMethod]
        public async Task GetOrdersByUserIdAsync_NonExistentUserId_ShouldReturnEmptyCollection()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await _repository.GetOrdersByUserIdAsync(0));
                
            // For valid but non-existent ID, should return empty collection
            var result = await _repository.GetOrdersByUserIdAsync(999);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }
        
        [TestMethod]
        public async Task AddOrderAsync_NullOrder_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () => await _repository.AddOrderAsync(null));
        }
        
        [TestMethod]
        public async Task AddOrderAsync_NonExistentUserId_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var order = new Order
            {
                UserId = 999, // Non-existent user ID
                Product = "Test Product",
                Quantity = 1,
                Price = 9.99m
            };
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _repository.AddOrderAsync(order));
        }
        
        [TestMethod]
        public async Task UpdateOrderAsync_NullOrder_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () => await _repository.UpdateOrderAsync(null));
        }
        
        [TestMethod]
        public async Task UpdateOrderAsync_NonExistentOrder_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            
            var order = new Order
            {
                OrderId = 999, // Non-existent order ID
                UserId = user.Id,
                Product = "Non-existent Order",
                Quantity = 1,
                Price = 9.99m
            };
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _repository.UpdateOrderAsync(order));
        }
        
        [TestMethod]
        public async Task UpdateOrderAsync_NonExistentUserId_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            
            var order = new Order
            {
                UserId = user.Id,
                Product = "Original Product",
                Quantity = 1,
                Price = 9.99m
            };
            
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            var updatedOrder = new Order
            {
                OrderId = order.OrderId,
                UserId = 999, // Non-existent user ID
                Product = "Updated Product",
                Quantity = 2,
                Price = 19.99m
            };
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _repository.UpdateOrderAsync(updatedOrder));
        }
        
        [TestMethod]
        public async Task DeleteOrderAsync_NonExistentId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.DeleteOrderAsync(999);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        public async Task DeleteOrderAsync_InvalidId_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await _repository.DeleteOrderAsync(0));
                
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await _repository.DeleteOrderAsync(-1));
        }
        
        [TestMethod]
        public async Task OrderExistsAsync_ExistingOrder_ShouldReturnTrue()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            
            var order = new Order
            {
                UserId = user.Id,
                Product = "Exists Product",
                Quantity = 1,
                Price = 9.99m
            };
            
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _repository.OrderExistsAsync(order.OrderId);
            
            // Assert
            Assert.IsTrue(result);
        }
        
        [TestMethod]
        public async Task OrderExistsAsync_NonExistentOrder_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.OrderExistsAsync(999);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        public async Task OrderExistsAsync_InvalidId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.OrderExistsAsync(0);
            
            // Assert
            Assert.IsFalse(result);
            
            result = await _repository.OrderExistsAsync(-1);
            Assert.IsFalse(result);
        }
        
        #endregion
    }
}