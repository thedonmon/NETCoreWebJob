using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NETCoreWebJob.Domain;
using NETCoreWebJob.Domain.Models.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NETCoreWebJob
{
    public class Functions
    {
        private readonly IConfiguration _config;
        private readonly IDoWork _doWork;
        public Functions(IConfiguration config, IDoWork doWork)
        {
            _config = config;
            _doWork = doWork;
        }
        [FunctionName("MyTimedFunction")]
        public async Task Run([TimerTrigger("%CRONTIME%", RunOnStartup = false)] TimerInfo timerInfo, ILogger logger,
           CancellationToken cancellationToken,
           [ServiceBus("%MyQueueToProcess%")]IAsyncCollector<Message> queueCollector)
        {
            bool isEnabled = _config.GetValue<bool>("IsFeatureEnabled");
            if (isEnabled)
            {
                var myEntity = await _doWork.IDoStuff(1);

                logger.LogInformation("Log some stuff here");
                var messageBodyString = JsonConvert.SerializeObject(myEntity);
                var messageToQueue = new Message
                {
                    SessionId = myEntity.ModelId.ToString(),
                    Body = Encoding.UTF8.GetBytes(messageBodyString),
                    ContentType = "application/json;charset=utf-8"
                };
                await queueCollector.AddAsync(messageToQueue, cancellationToken).ConfigureAwait(false);
                queueCollector.FlushAsync(cancellationToken);
            }


        }
        [FunctionName("MyFunctionToReceive")]
        public async Task Receive(
        [ServiceBusTrigger("%MyQueueToProcess%", IsSessionsEnabled = true)]Message requestMessage,
        ILogger logger,
        CancellationToken cancellationToken)
        {
            string messageBody = Encoding.UTF8.GetString(requestMessage.Body, 0, requestMessage.Body.Length);

            logger.LogInformation($"Message received at  {DateTime.UtcNow}, Body = @ObjectBody", messageBody);

            var requestObject = JsonConvert.DeserializeObject<MyDBModelDTO>(messageBody);
            
            await _doWork.SaveMyEntity(requestObject).ConfigureAwait(false);
        }
    }
}
