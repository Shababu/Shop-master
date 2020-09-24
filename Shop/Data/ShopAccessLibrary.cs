﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Windows;
using Shop.Models;

namespace Shop
{
    class ShopAccessLibrary
    {
        private string _connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

        internal List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT ProductID, ProductName, Brands.BrandName, Type, Colors.Color, Sizes.Size, Price, Amount FROM Products join Colors_Sizes on Colors_Sizes.Color_SizeID = Products.Color_SizeID join Colors on Colors_Sizes.ColorID = Colors.ColorID join Sizes on Colors_Sizes.SizeID = Sizes.SizeID join Brands on Products.BrandID = Brands.BrandID", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    products.Add(new Product(
                        Convert.ToInt32(reader.GetValue(0)),
                        reader.GetValue(1).ToString(),
                        reader.GetValue(2).ToString(),
                        reader.GetValue(3).ToString(),
                        reader.GetValue(4).ToString(),
                        reader.GetValue(5).ToString(),
                        (float)Convert.ToDouble(reader.GetValue(6)),
                        Convert.ToInt32(reader.GetValue(7))));
                }
            }
            return products;
        }
        internal List<Product> SearchProductByCharacteristics(string name, string brand, string type, string color, string size, string price, string amount)
        {
            List<Product> products = new List<Product>();

            string nameForQuery = string.Empty;
            string brandForQuery = string.Empty;
            string typeForQuery = string.Empty;
            string colorForQuery = string.Empty;
            string sizeForQuery = string.Empty;
            string priceForQuery = string.Empty;
            string amountForQuery = string.Empty;

            List<string> chars = new List<string>();

            if (name != null)
            {
                nameForQuery = $"Products.ProductName = '{name}' or Products.ProductName like '{name}%' or Products.ProductName like '%{name}%' or Products.ProductName like '%{name}'";
                chars.Add(nameForQuery);
            }
            if (brand != null)
            {
                brandForQuery = $"Brands.BrandName = '{brand}'";
                chars.Add(brandForQuery);
            }
            if (type != null)
            {
                typeForQuery = $"Type = '{type}'";
                chars.Add(typeForQuery);
            }
            if (color != null)
            {
                colorForQuery = $"Colors.Color = '{color}'";
                chars.Add(colorForQuery);
            }
            if (size != null)
            {
                sizeForQuery = $"Sizes.Size = '{size}'";
                chars.Add(sizeForQuery);
            }
            if (price != null)
            {
                priceForQuery = $"Products.Price = {price}";
                chars.Add(priceForQuery);
            }
            if (amount != null)
            {
                amountForQuery = $"Products.Amount = {amount}";
                chars.Add(amountForQuery);
            }

            string searchQuery = $"SELECT ProductID, ProductName, Brands.BrandName, Type, Colors.Color, Sizes.Size, Price, Amount FROM Products join Colors_Sizes on Colors_Sizes.Color_SizeID = Products.Color_SizeID join Colors on Colors_Sizes.ColorID = Colors.ColorID join Sizes on Colors_Sizes.SizeID = Sizes.SizeID join Brands on Products.BrandID = Brands.BrandID where ";

            int counter = 0;

            foreach (string characteristic in chars)
            {
                if (counter > 0)
                {
                    searchQuery += $" AND {characteristic}";
                }
                else
                {
                    searchQuery += $"{characteristic}";
                    counter++;
                }
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(searchQuery, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    products.Add(new Product(
                        Convert.ToInt32(reader.GetValue(0)),
                        reader.GetValue(1).ToString(),
                        reader.GetValue(2).ToString(),
                        reader.GetValue(3).ToString(),
                        reader.GetValue(4).ToString(),
                        reader.GetValue(5).ToString(),
                        (float)Convert.ToDouble(reader.GetValue(6)),
                        Convert.ToInt32(reader.GetValue(7))));
                }
            }
            return products;
        }
        internal Product GetProductById(int productId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand($"Select ProductID, ProductName, Brands.BrandName, Type, Colors.Color, Sizes.Size, Price, Amount from Products join Brands on Brands.BrandID = Products.BrandID join Colors_Sizes on Products.Color_SizeID = Colors_Sizes.Color_SizeID join Colors on Colors.ColorID = Colors_Sizes.ColorID join Sizes on Sizes.SizeID = Colors_Sizes.SizeID where ProductID = {productId}", connection);
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();
                return new Product(
                    Convert.ToInt32(reader.GetValue(0)),
                    reader.GetValue(1).ToString(),
                    reader.GetValue(2).ToString(),
                    reader.GetValue(3).ToString(),
                    reader.GetValue(4).ToString(),
                    reader.GetValue(5).ToString(),
                    (float)Convert.ToDouble(reader.GetValue(6)),
                    Convert.ToInt32(reader.GetValue(7)));
            }
        }
        internal void AddProduct(string name, string brand, string type, string color, string size, string price, string amount)
        {            
            int brandId = Convert.ToInt32(GetBrandId(brand));
            int colorId = Convert.ToInt32(GetColorId(color));
            int sizeId = Convert.ToInt32(GetSizeId(size));
            int color_sizeId = Convert.ToInt32(GetColor_SizeId(colorId, sizeId));

            string addProductQuery = $"insert into Products values ('{name}', {brandId}, '{type}', {color_sizeId}, {price}, {amount})";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand(addProductQuery, connection);
                    command.ExecuteNonQuery();
                    MessageBox.Show("Товар был успешно добавлен в базу данных", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        internal int GetBrandId(string brand)
        {
            string brandIdQuery = $"select Brands.BrandID from Brands where Brands.BrandName = '{brand}'";
            int brandId = default;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(brandIdQuery, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    brandId = Convert.ToInt32(reader.GetValue(0));
                }
            }

            return brandId;
        }
        internal int GetColorId(string color)
        {
            string colorIdQuery = $"select Colors.ColorID from Colors where Colors.Color = '{color}'";
            int colorId = default;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(colorIdQuery, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    colorId = Convert.ToInt32(reader.GetValue(0));
                }
            }

            return colorId;
        }
        internal int GetSizeId(string size)
        {
            string sizeIdQuery = $"select Sizes.SizeID from Sizes where Sizes.Size = '{size}'";
            int sizeId = default;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sizeIdQuery, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    sizeId = Convert.ToInt32(reader.GetValue(0));
                }
            }

            return sizeId;
        }
        internal int GetColor_SizeId(int colorId, int sizeId)
        {
            string color_sizeIdQuery = $"select Color_SizeID from Colors_Sizes join Colors on Colors.ColorID = Colors_Sizes.ColorID join Sizes on Sizes.SizeID = Colors_Sizes.SizeID where Colors.ColorID = '{colorId}' AND Sizes.SizeID = '{sizeId}'";
            int color_sizeId = default;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(color_sizeIdQuery, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    color_sizeId = Convert.ToInt32(reader.GetValue(0));
                }
            }

            return color_sizeId;
        }

        internal List<ActiveOrder> GetActiveOrders()
        {
            List<ActiveOrder> orders = new List<ActiveOrder>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("select distinct Orders.OrderID, Managers.ManagerLName, Managers.ManagerFName, Customers.CustomerLName, Customers.CustomerFName, Orders.OrderDate from Orders join Managers on Orders.ManagerID = Managers.ManagerID join Customers on Orders.CustomerID = Customers.CustomerID where Orders.Status = 'Не выполнен'", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string customerFullName = reader.GetValue(1).ToString() + " " + reader.GetValue(2).ToString();
                    string managerFullName = reader.GetValue(3).ToString() + " " + reader.GetValue(4).ToString();
                    orders.Add(new ActiveOrder(
                        Convert.ToInt32(reader.GetValue(0)),
                        customerFullName,
                        managerFullName,
                        reader.GetValue(5).ToString()
                        ));
                }
            }

            return orders;
        }
        internal List<ActiveOrder> GetActiveOrderById(int id)
        {
            List<ActiveOrder> order = new List<ActiveOrder>();

            using(SqlConnection connection = new SqlConnection())
            {
                connection.Open();
                SqlCommand command = new SqlCommand($"Select Orders.OrderID, Managers.ManagerLName, Managers.ManagerFName, Customers.CustomerLName, Customers.CustomerFName, Orders.OrderDate from Orders join Managers on Orders.ManagerID = Managers.ManagerID join Customers on Orders.CustomerID = Customers.CustomerID where Orders.OrderID = {id}", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string customerFullName = reader.GetValue(1).ToString() + " " + reader.GetValue(2).ToString();
                    string managerFullName = reader.GetValue(3).ToString() + " " + reader.GetValue(4).ToString();
                    order.Add(new ActiveOrder(
                        Convert.ToInt32(reader.GetValue(0)),
                        customerFullName,
                        managerFullName,
                        reader.GetValue(5).ToString()
                        ));
                }

                return order;
            }
        }
    }
}