﻿using Microsoft.EntityFrameworkCore;
using ShoesStore.IRepository;
using ShoesStore.Model;
using ShoesStore.ViewModel.RequestModel;
using WebApi.Data;

namespace ShoesStore.Repository
{
	public class OrdersRepository : IOrdersRepository
	{
		private readonly AppDbContext _context;

		public OrdersRepository(AppDbContext context)
		{
			_context = context;
		}


		public async Task<IEnumerable<OrderViewModel>> GetAllOrdersAsync()
		{
			return await _context.Orders
				.Include(o => o.OrderItems)
				.Select(o => new OrderViewModel
				{
					Id = o.Id,
					UserId = o.UserId,
					OrderDate = o.OrderDate,
					TotalAmount = o.TotalAmount,
					Status = o.Status,
					OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
					{
						ProductId = oi.ProductId,
						Quantity = oi.Quantity,
						Price = oi.Price
					}).ToList()
				}).ToListAsync();
		}

		public async Task<OrderViewModel> GetOrderByIdAsync(int orderId)
		{
			var order = await _context.Orders
				.Include(o => o.OrderItems)
				.FirstOrDefaultAsync(o => o.Id == orderId);

			if (order == null) return null;

			return new OrderViewModel
			{
				Id = order.Id,
				UserId = order.UserId,
				OrderDate = order.OrderDate,
				TotalAmount = order.TotalAmount,
				Status = order.Status,
				OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
				{
					ProductId = oi.ProductId,
					Quantity = oi.Quantity,
					Price = oi.Price
				}).ToList()
			};
		}

		public async Task AddOrderAsync(OrderViewModel orderViewModel)
		{
			var order = new Order
			{
				UserId = orderViewModel.UserId,
				OrderDate = DateTime.UtcNow,
				TotalAmount = orderViewModel.TotalAmount,
				Status = orderViewModel.Status,
				OrderItems = orderViewModel.OrderItems.Select(oi => new OrderItem
				{
					ProductId = oi.ProductId,
					Quantity = oi.Quantity,
					Price = oi.Price,
					SizeId = oi.SizeId, // Thêm SizeId nếu cần
					
					
				}).ToList()
			};

			_context.Orders.Add(order);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateOrderAsync(OrderViewModel orderViewModel)
		{
			var order = await _context.Orders
				.Include(o => o.OrderItems)
				.FirstOrDefaultAsync(o => o.Id == orderViewModel.Id);

			if (order != null)
			{
				order.Status = orderViewModel.Status;
				order.TotalAmount = orderViewModel.TotalAmount;
				order.OrderItems = orderViewModel.OrderItems.Select(oi => new OrderItem
				{
					ProductId = oi.ProductId,
					Quantity = oi.Quantity,
					Price = oi.Price,
					SizeId = oi.SizeId
				}).ToList();

				await _context.SaveChangesAsync();
			}
		}

		public async Task DeleteOrderAsync(int orderId)
		{
			var order = await _context.Orders.FindAsync(orderId);
			if (order != null)
			{
				_context.Orders.Remove(order);
				await _context.SaveChangesAsync();
			}
		}
	}

}
