﻿/* ========================================================================
 * Copyright (c) 2005-2021 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

namespace SampleConsoleClient
{
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using UACloudLibClientLibrary;
    using UACloudLibClientLibrary.Models;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine();
                Console.WriteLine("Usage: SampleConsoleClient <UA Cloud Library instance URL> <username> <password>");
                return;
            }

            Console.WriteLine("OPC Foundation UA Cloud Library Console Client Application");

            TestGraphQLInterface(args);

            TestRESTInterface(args);

            TestClientLibrary(args);

            Console.WriteLine();
            Console.WriteLine("Done!");
        }

        private static void TestGraphQLInterface(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Testing GraphQL interface (see https://graphql.org/learn/ for details)...");

            GraphQLHttpClientOptions options = new GraphQLHttpClientOptions
            {
                EndPoint = new Uri(args[0] + "/graphql"),
                HttpMessageHandler = new MessageHandlerWithAuthHeader(args[1], args[2])
            };

            GraphQLHttpClient graphQLClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer());

            Console.WriteLine();
            Console.WriteLine("Testing objecttype query (the other OPC UA types are very similar!)");
            GraphQLRequest request = new GraphQLRequest
            {
                Query = @"
                query {
                    objecttype
                    {
                        objecttype_browsename objecttype_value objecttype_namespace nodeset_id
                    }
                }"

            };
            var response = graphQLClient.SendQueryAsync<UACloudLibGraphQLObjecttypeQueryResponse>(request).GetAwaiter().GetResult();
            Console.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));

            Console.WriteLine();
            Console.WriteLine("Testing metadata query");
            request = new GraphQLRequest
            {
                Query = @"
                query {
                    metadata
                    {
                        metadata_name metadata_value nodeset_id
                    }
                }"

            };
            var response2 = graphQLClient.SendQueryAsync<UACloudLibGraphQLMetadataQueryResponse>(request).GetAwaiter().GetResult();
            Console.WriteLine(JsonSerializer.Serialize(response2, new JsonSerializerOptions { WriteIndented = true }));

            graphQLClient.Dispose();
        }

        private static void TestRESTInterface(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Testing REST interface...");

            WebClient webClient = new WebClient
            {
                BaseAddress = args[0]
            };

            webClient.Headers.Add("Authorization", "basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(args[1] + ":" + args[2])));

            Console.WriteLine();
            Console.WriteLine("Testing /infomodel/find/{keywords}");
            string[] keywords = { "*" }; // return everything
            string address = webClient.BaseAddress + "infomodel/find";
            webClient.Headers.Add("Content-Type", "application/json");
            string response = webClient.UploadString(address, "PUT", JsonSerializer.Serialize(keywords));
            Console.WriteLine(response);

            Console.WriteLine();
            Console.WriteLine("Testing /infomodel/download/{identifier}");
            string[] identifiers = JsonSerializer.Deserialize<string[]>(response);
            string identifier = identifiers[0]; // pick the first identifier returned previously
            address = webClient.BaseAddress + "infomodel/download/" + Uri.EscapeDataString(identifier);
            response = webClient.DownloadString(address);
            Console.WriteLine(response);

            Console.WriteLine();
            Console.WriteLine("For sample code to test /infomodel/upload, see https://github.com/digitaltwinconsortium/UANodesetWebViewer/blob/main/Applications/Controllers/UACL.cs");

            webClient.Dispose();
        }

        private static void TestClientLibrary(string[] args)
        {
            Console.WriteLine("\n\nTesting the client library");

            UACloudLibClient client = new UACloudLibClient(args[0], args[1], args[2]);

            Console.WriteLine("\nTesting object query");
            List<ObjectResult> test = client.GetObjectTypes().GetAwaiter().GetResult();
            foreach(ObjectResult result in test)
            {
                Console.WriteLine($"{result.ID}, {result.Namespace}, {result.Browsename}, {result.Value}");
            }

            Console.WriteLine("\nTesting metadata query");
            List<MetadataResult> metadatas = client.GetMetadata().GetAwaiter().GetResult();
            foreach(MetadataResult metadata in metadatas)
            {
                Console.WriteLine($"{metadata.ID}, {metadata.Name}, {metadata.Value}");
            }

            Console.WriteLine("\nTesting query and convertion of metadata");
            List<AddressSpace> finalResult = client.GetConvertedResult().GetAwaiter().GetResult();
            foreach(AddressSpace result in finalResult)
            {
                Console.WriteLine($"{result.Title} by {result.Contributor.Name} last update on {result.LastModificationTime}");
            }

            if(finalResult.Count > 0)
            {
                Console.WriteLine("Testing download of nodeset");
                AddressSpace result = client.DownloadNodeset(finalResult[0].MetadataID);
                if (!string.IsNullOrEmpty(result.Nodeset.NodesetXml))
                {
                    Console.WriteLine("Nodeset Downloaded");
                }
            }
        }
    }
}
