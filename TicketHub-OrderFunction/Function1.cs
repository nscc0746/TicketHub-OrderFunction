using System;
using System.Text.Json;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace TicketHub_OrderFunction
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        public class Order
        {
            public int ConcertId { get; set; }
            public string Email { get; set; }
            public string Name { get; set; }
            public string Phone { get; set; }
            public int Quantity { get; set; }
            public string CreditCard { get; set; }
            public string Expiration { get; set; }
            public string SecurityCode { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string Province { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }
        }

        [Function(nameof(Function1))]
        public async Task RunAsync([QueueTrigger("tickethub", Connection = "AzureWebJobsStorage")] QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
            
            //Deserialize the string to our Json object
            Order? newOrder = JsonSerializer.Deserialize<Order>(message.MessageText);

            // get connection string from app settings
            string? connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("SQL connection string is not set in the environment variables.");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync(); // Note the ASYNC

                var query = @"INSERT INTO dbo.Orders (ConcertID, Email, Name, Phone, Quantity, CreditCard, Expiration, SecurityCode, Address, City, Province, PostalCode, Country) VALUES (@ConcertId, @Email, @Name, @Phone, @Quantity, @CreditCard, @Expiration, @SecurityCode, @Address, @City, @Province, @PostalCode, @Country)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ConcertId", newOrder.ConcertId);
                    cmd.Parameters.AddWithValue("@Email", newOrder.Email);
                    cmd.Parameters.AddWithValue("@Name", newOrder.Name);
                    cmd.Parameters.AddWithValue("@Phone", newOrder.Phone);
                    cmd.Parameters.AddWithValue("@Quantity", newOrder.Quantity);
                    cmd.Parameters.AddWithValue("@CreditCard", newOrder.CreditCard);
                    cmd.Parameters.AddWithValue("@Expiration", newOrder.Expiration);
                    cmd.Parameters.AddWithValue("@SecurityCode", newOrder.SecurityCode);
                    cmd.Parameters.AddWithValue("@Address", newOrder.Address);
                    cmd.Parameters.AddWithValue("@City", newOrder.City);
                    cmd.Parameters.AddWithValue("@Province", newOrder.Province);
                    cmd.Parameters.AddWithValue("@PostalCode", newOrder.PostalCode);
                    cmd.Parameters.AddWithValue("@Country", newOrder.Country);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
