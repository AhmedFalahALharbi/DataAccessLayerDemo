 
 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
 public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            return await _context.Orders
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid order ID", nameof(id));

            return await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            // Verify the user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == order.UserId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {order.UserId} not found");

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var existingOrder = await _context.Orders.FindAsync(order.OrderId);
            if (existingOrder == null)
                throw new InvalidOperationException($"Order with ID {order.OrderId} not found");

            // Check if user exists if userId is being changed
            if (existingOrder.UserId != order.UserId)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == order.UserId);
                if (!userExists)
                    throw new InvalidOperationException($"User with ID {order.UserId} not found");
            }

            // Update properties
            existingOrder.UserId = order.UserId;
            existingOrder.Product = order.Product;
            existingOrder.Quantity = order.Quantity;
            existingOrder.Price = order.Price;

            _context.Orders.Update(existingOrder);
            await _context.SaveChangesAsync();
            return existingOrder;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid order ID", nameof(id));

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> OrderExistsAsync(int id)
        {
            if (id <= 0)
                return false;

            return await _context.Orders.AnyAsync(o => o.OrderId == id);
        }
    }
} 