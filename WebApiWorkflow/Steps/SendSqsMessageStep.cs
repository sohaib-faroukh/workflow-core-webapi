using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using WebApiWorkflow.Utilities;
using WorkflowCore.Interface;
using WorkflowCore.Models;


namespace WebApiWorkflow.Steps
{

    public class SendSqsMessageStep : StepBodyAsync
    {
        public string QueueUrl { get; set; }
        public object MessagePayload { get; set; }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Console.WriteLine(QueueUrl, MessagePayload);
            return ExecutionResult.Next();
        }

    }
}