using DevFreela.Payments.API.Models;

namespace DevFreela.Payments.API.Service
{
    public class PaymentService : IPaymentService
    {
        public Task<bool> Process(PaymentInfoInputModel paymentInfoInputModel)
        {
            return Task.FromResult(true);
        }
    }
}
