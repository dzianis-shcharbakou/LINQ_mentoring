// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();

		[Category("Restriction Operators")]
		[Title("Sum - Task 1")]
		[Description("Return all customers which have Total orders more than X")]

		public void LinqH1()
		{
			decimal[] XArray = { 20000.5M, 1500, 1000 };

			XArray.ToList().ForEach(x => {

				Console.WriteLine($"Cutomers with cost more than {x}:");

				var result = dataSource.Customers.Where(y => y.Orders.Sum(z => z.Total) > x).Select(a => new 
				{
					CustomerID = a.CustomerID,
					TotalCost = a.Orders.Sum(z => z.Total)
				});
				ObjectDumper.Write(result);

				Console.WriteLine("============================================================================================");
			});
		}

		[Category("Restriction Operators")]
		[Title("With group by - Task 2")]
		[Description("Return customers and suppliers which from one country and from one city")]

		public void LinqH21()
		{

			var customers = dataSource.Customers;
			var suppliers = dataSource.Suppliers;

			var result = customers.GroupBy(item => new { item.CompanyName, item.Country, item.City  },
				(key, group) => new {
					CustomerName = key.CompanyName,
					Country = key.Country,
					City = key.City,
					Suppliers = (from sup in suppliers
								 where key.Country == sup.Country && key.City == sup.City
								 select sup.SupplierName).ToList()
				}).OrderBy(x => x.CustomerName).ToList();

			foreach (var x in result)
			{
				Console.WriteLine($"Customer name {x.CustomerName}, Country {x.Country}, City {x.City}");
				Console.WriteLine("Suppliers which from one country and from one city with customer:");
				foreach (var item in x.Suppliers)
				{
					Console.WriteLine(item);
				}
				Console.WriteLine("============================================================================================");
			}
		}

		[Category("Restriction Operators")]
		[Title("Without group by - Task 2")]
		[Description("Return customers and suppliers which from one country and from one city")]

		public void LinqH22()
		{

			var customers = dataSource.Customers;
			var suppliers = dataSource.Suppliers;

			var result = from cust in customers
						  join sup in suppliers
						  on new { Country = cust.Country, City = cust.City } equals new { Country = sup.Country, City = sup.City }
						  into joinData
						  orderby cust.CompanyName
						  select new
						  {
							  CustomerName = cust.CompanyName,
							  Country = cust.Country,
							  City = cust.City,
							  Suppliers = joinData.ToList()
						  };

			foreach (var x in result)
			{
				Console.WriteLine($"Customer name {x.CustomerName}, Country {x.Country}, City {x.City}");
				Console.WriteLine("Suppliers which from one country and from one city with customer:");
				foreach (var item in x.Suppliers)
				{
					Console.WriteLine(item.SupplierName);
				}
				Console.WriteLine("============================================================================================");
			}
		}

		[Category("Restriction Operators")]
		[Title("Max - Task 3")]
		[Description("Return all customers which have Max order cost more than X")]

		public void LinqH3()
		{
			decimal[] XArray = { 1000.5M, 5000, 15000 };

			XArray.ToList().ForEach(x => {

				Console.WriteLine($"Cutomers have something order more than {x} cost:");

				var result = dataSource.Customers.Where(y => y.Orders.Max(z => z?.Total) > x);
				ObjectDumper.Write(result);

				Console.WriteLine("============================================================================================");
			});
		}

		[Category("Restriction Operators")]
		[Title("First Date - Task 4")]
		[Description("Return all customers with date when they became clients")]
		public void LinqH4()
		{
			var customers = dataSource.Customers;

			var result = customers.Select(y =>  new 
			{ 
				y.CompanyName,
				FirstOrderMonth = y.Orders.Min(z => z?.OrderDate.Month),
				FirstOrderYear = y.Orders.Min(z => z?.OrderDate.Year),
			});

			ObjectDumper.Write(result);

			Console.WriteLine("============================================================================================");
		}

		[Category("Restriction Operators")]
		[Title("Sorting - Task 5")]
		[Description("Return all customers with date when they became clients with sorting")]
		public void LinqH5()
		{
			var customers = dataSource.Customers;

			var result = from customer in customers
						 orderby customer.Orders.Min(z => z?.OrderDate.Year) descending, customer.Orders.Min(z => z?.OrderDate.Month) descending, customer.Orders.Sum(z => z?.Total) descending
						 select new
						 {
							 customer.CompanyName,
							 FirstOrderMonth = customer.Orders.Min(z => z?.OrderDate.Month),
							 FirstOrderYear = customer.Orders.Min(z => z?.OrderDate.Year),
							 TotalSum = customer.Orders.Sum(z => z?.Total)
						 };

			ObjectDumper.Write(result);

			Console.WriteLine("============================================================================================");
		}

		[Category("Restriction Operators")]
		[Title("Where - Task 6")]
		[Description("Return all customers with no numerical postcode or without region or phone number doesn't have code of operator")]
		public void LinqH6()
		{
			var customers = dataSource.Customers;

			int isParse;
			var result = from customer in customers
						 where !int.TryParse(customer.PostalCode, out isParse) || customer.Region == null || !(customer.Phone.Contains("(") && customer.Phone.Contains(")"))
						 select new
						 {
							 customer.CompanyName,
							 customer.PostalCode,
							 customer.Region,
							 customer.Phone
						 };

			ObjectDumper.Write(result);

			Console.WriteLine("============================================================================================");
		}

		[Category("Restriction Operators")]
		[Title("Group and Sort - Task 7")]
		[Description("Group all products by category, unitInStock. Sort by UnitPrice")]
		public void LinqH7()
		{
			var products = dataSource.Products;

			var result = from product in products
						 group product by product.Category into grC
						 select new
						 {
							 key = grC.Key,
							 Products = from product in grC
										group product by product.UnitsInStock into grU
										select new
										{
											key = grU.Key,
											Products = from product in grU
													   orderby product.UnitPrice descending
													   select product
										}
						 };

			foreach (var x in result)
			{
				Console.WriteLine($"Product group by category {x.key}:");
				foreach (var item in x.Products)
				{
					Console.WriteLine($"Product group by unitInStock {item.key}:");
					foreach (var item2 in item.Products)
					{
						Console.WriteLine($"{item2.ProductName}, price {item2.UnitPrice}");
					}
				}
				Console.WriteLine("============================================================================================");
			}
		}

		[Category("Restriction Operators")]
		[Title("Group by cost - Task 8")]
		[Description("Group all products by UnitPrice")]
		public void LinqH8()
		{
			var products = dataSource.Products;

			//var result = from product in products
			//			 select new
			//			 {
			//				 CheapProduct = from prod in products
			//								where product.UnitPrice < 15
			//								group prod by prod.UnitPrice into cheap
			//								select cheap,

			//				 StandardProduct = from prod in products
			//								   group prod by prod.UnitPrice into standard
			//								   where product.UnitPrice > 15 && product.UnitPrice < 20
			//								   select standard,

			//				 ExpensiveProduct = from prod in products
			//									group prod by prod.UnitPrice into expensive
			//									where product.UnitPrice > 20
			//									select expensive,
			//			 };

			var CheapProduct = from prod in products
							   where prod.UnitPrice < 15
							   group prod by prod.UnitPrice into cheap
							   select cheap;

			var StandardProduct = from prod in products
								  where prod.UnitPrice > 15 && prod.UnitPrice < 20
								  group prod by prod.UnitPrice into standard
								  select standard;

			var ExpensiveProduct = from prod in products
								   where prod.UnitPrice > 20
								   group prod by prod.UnitPrice into expensive
								   select expensive;

			Console.WriteLine($"Group by Cheaper products:");
			foreach (var item in CheapProduct)
			{
				foreach (var product in item)
				{
					Console.WriteLine($"{product.ProductName}, price {product.UnitPrice}");
				}
			}
			Console.WriteLine("============================================================================================");

			Console.WriteLine($"Group by Standard price products:");
			foreach (var item in StandardProduct)
			{
				foreach (var product in item)
				{
					Console.WriteLine($"{product.ProductName}, price {product.UnitPrice}");
				}
			}
			Console.WriteLine("============================================================================================");

			Console.WriteLine($"Group by Expensive price products:");
			foreach (var item in ExpensiveProduct)
			{
				foreach (var product in item)
				{
					Console.WriteLine($"{product.ProductName}, price {product.UnitPrice}");
				}
			}
			Console.WriteLine("============================================================================================");
		}

		[Category("Restriction Operators")]
		[Title("Average - Task 9")]
		[Description("Average profitability every city and average order count every city")]
		public void LinqH9()
		{
			var customers = dataSource.Customers;

			var result = from customer in customers
						 group customer by customer.City into grC
						 select new
						 {
							 key = grC.Key,
							 AverageCost =  (from cust in grC
											select cust.Orders).Average(x => x.Average(y => y?.Total)),
							 AverageOrders = (from cust in grC
											 select cust.Orders).Average(x => x.Count()),

						 };

			foreach (var x in result)
			{
				Console.WriteLine($"City {x.key}, Average costs {x.AverageCost}, average count of orders {x.AverageOrders}");
				Console.WriteLine("============================================================================================");
			}
		}

		[Category("Restriction Operators")]
		[Title("Count and group - Task 10")]
		[Description("Count orders by month")]
		public void LinqH10()
		{
			var customers = dataSource.Customers;

			var result = from customer in customers
						 from order in customer.Orders
						 group order by order.OrderDate.Month into grO
						 select new
						 {
							 key = grO.Key,
							 OrderCount = (from abc in grO
											select abc.OrderDate).Count(x => x.Month == grO.Key),
						 };

			foreach (var x in result)
			{
				Console.WriteLine($"Month {x.key}, Order count {x.OrderCount}");
				Console.WriteLine("============================================================================================");
			}
		}

		[Category("Restriction Operators")]
		[Title("Count and group - Task 10")]
		[Description("Count orders by years")]
		public void LinqH102()
		{
			var customers = dataSource.Customers;

			var result = from customer in customers
						 from order in customer.Orders
						 group order by order.OrderDate.Year into grO
						 select new
						 {
							 key = grO.Key,
							 OrderCount = (from abc in grO
										   select abc.OrderDate).Count(x => x.Year == grO.Key),
						 };

			foreach (var x in result)
			{
				Console.WriteLine($"Year {x.key}, Order count {x.OrderCount}");
				Console.WriteLine("============================================================================================");
			}
		}

		[Category("Restriction Operators")]
		[Title("Count and group - Task 10")]
		[Description("Count orders by years and month")]
		public void LinqH103()
		{
			var customers = dataSource.Customers;

			var result = from customer in customers
						 from order in customer.Orders
						 group order by new { order.OrderDate.Year, order.OrderDate.Month } into grO
						 select new
						 {
							 key = grO.Key,
							 OrderCount = (from abc in grO
										   select abc.OrderDate).Count(x => x.Year == grO.Key.Year && x.Month == grO.Key.Month),
						 };

			foreach (var x in result)
			{
				Console.WriteLine($"Year {x.key.Year}, Month {x.key.Month}, Order count {x.OrderCount}");
				Console.WriteLine("============================================================================================");
			}
		}
	}
}
