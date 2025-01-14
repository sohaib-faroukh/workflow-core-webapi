using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WebApiWorkflow.Steps
{

    public class CustomMessage : StepBody
    {
        public string Message { get; set; }
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine(Message);
            return ExecutionResult.Next();
        }
    }

}