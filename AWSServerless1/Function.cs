using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Util;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSServerless1
{
    public class Function
    {
        public static HttpClient HttpClient { get; } = new HttpClient();

        public static string StackName { get; } = Environment.GetEnvironmentVariable("STACK_NAME");
        public static string HttpEndpoint { get; } = Environment.GetEnvironmentVariable("HTTP_ENDPOINT");

        public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            var s3Event = evnt.Records?[0].S3;
            if (s3Event == null)
            {
                return;
            }

            var json = JsonConvert.SerializeObject(new
            {
                @event = "ObjectCreated",
                bucketName = s3Event.Bucket.Name,
                objectKey = s3Event.Object.Key,
                stackName = StackName
            });
            var responseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(json))
                                                .ConfigureAwait(false);

            responseMessage.EnsureSuccessStatusCode();
            context.Logger.LogLine($"Posted JSON '{json}' to {HttpEndpoint}");
        }
    }
}
