using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using WebApiWorkflow.Utilities;
using WorkflowCore.Interface;
using WorkflowCore.Models;


namespace WebApiWorkflow.Steps
{

    public class HttpRequestStep : StepBodyAsync
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public object Payload { get; set; }
        public object Headers { get; set; }
        public object QueryParams { get; set; }
        public object Response { get; set; }
        public object ErrorResponse { get; set; }

        private readonly HttpClientUtility _httpClientUtility;

        public HttpRequestStep(HttpClientUtility httpClientUtility)
        {
            _httpClientUtility = httpClientUtility;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            try
            {
                // Call the utility to execute the HTTP request
                Response = await _httpClientUtility.SendHttpRequestAsync(
                    Url,
                    Method,
                    Payload,
                    Headers as Dictionary<string, string>,
                    QueryParams as Dictionary<string, string>
                );

                context.PersistenceData = Response;

                return ExecutionResult.Persist(Response);
                // return ExecutionResult.Next();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in HttpRequestStep: {ex.Message}");


                ErrorResponse = ex.ToJson();

                context.PersistenceData = new { Error = ex.ToJson() };

                // Return ExecutionResult with a failure status
                return new ExecutionResult
                {
                    Proceed = false,// Indicates the workflow should stop
                    OutcomeValue = ex.ToJson(),
                    PersistenceData = ex.ToJson(),
                };
                // return ExecutionResult.Persist(Response);
            }
        }
    }
}