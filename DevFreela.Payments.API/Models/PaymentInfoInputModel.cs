﻿namespace DevFreela.Payments.API.Models
{
    public class PaymentInfoInputModel
    {
  
        public int Id { get; set; }

        public string CreditCardNumber { get; set; }

        public string Cvv { get; set; }

        public string  ExpiresAt { get; set; }

        public string FullName { get; set; }

        public decimal Amount { get; set; }
        public int IdProject { get; internal set; }
    }
}
