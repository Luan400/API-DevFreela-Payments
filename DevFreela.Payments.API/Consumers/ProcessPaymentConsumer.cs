
using DevFreela.Payments.API.Models;
using DevFreela.Payments.API.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace DevFreela.Payments.API.Consumers
{
    public class ProcessPaymentConsumer : BackgroundService
    {
        private const string QUEUE = "Payments";
        private const string PAYMENT_APPROVED_QUEUE = "PaymentsApproved";
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        public ProcessPaymentConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue:QUEUE,
                durable: false,
                autoDelete: false,
                exclusive: false,
                arguments: null
                );


            _channel.QueueDeclare(
                queue: PAYMENT_APPROVED_QUEUE,
                durable: false,
                autoDelete: false,
                exclusive: false,
                arguments: null
                );
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //escuta fila do canal e aguarda msg
            var consumer = new EventingBasicConsumer(_channel);

            //manipula evento apos receber msg

            consumer.Received += (sender, eventArgs) =>
            {
                var byteArray = eventArgs.Body.ToArray();

                // converte byte pra string
                var paymentInfoJson = Encoding.UTF8.GetString(byteArray);

                var paymentInfo = JsonSerializer.Deserialize<PaymentInfoInputModel>(paymentInfoJson);

                ProcessPayment(paymentInfo);

                var paymentApproved = new PaymentApprovedIntegrationEvent(paymentInfo.IdProject);

                var paymentApprovedJson = JsonSerializer.Serialize(paymentApproved);
                var paymentApprovedBytes = Encoding.UTF8.GetBytes(paymentApprovedJson);

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: PAYMENT_APPROVED_QUEUE,
                    basicProperties: null,
                    body: paymentApprovedBytes
                    );

                //identificar msg recebida
                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(QUEUE, false, consumer);

            return Task.CompletedTask;
        }

        public void ProcessPayment (PaymentInfoInputModel paymentInfo)
        {
            //criar escopo para poder utilizar IPaymentService  
            using(var scope = _serviceProvider.CreateScope())
            {
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                //processar pagamentos
                paymentService.Process(paymentInfo);
            };
        }
    }
}
