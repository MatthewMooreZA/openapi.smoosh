﻿using System;
using System.Linq;
using Microsoft.OpenApi.Any;
using OpenApi.Smoosh.Common;
using OpenApi.Smoosh.Gcp;
using OpenApi.Smoosh.Gcp.Models;
using Xunit;

namespace OpenApi.Smoosh.Tests.Gcp
{
    public class CloudRun
    {
        [Fact]
        public void MapToCloudRunBasic()
        {
            var next =
                ApiGatewayBuilder
                    .FromOpenApi("./Samples/2.0.uber.json")
                    .MapToCloudRun(config => config
                        .WithUrl("https://is-this-thing-on.com")
                        .WithProtocol(Protocols.Http2)
                        .WithApiKey()
                        .WithTimeout(TimeSpan.FromSeconds(15)));
            
            var build = next.Build();

            Assert.True(build.Components.SecuritySchemes.Any());

            next.ToJson("test.json");
        }

        [Fact]
        public void MapToCloudRunBasic2()
        {
            var url = "https://uber-thingy.a.run.app";
            var next =
                ApiGatewayBuilder
                    .FromOpenApi("./Samples/2.0.uber.json")
                    .MapToCloudRun(config => config
                        .WithUrl(url)
                        .WithProtocol(Protocols.Http2)
                        .WithApiKey()
                        .WithTimeout(TimeSpan.FromSeconds(15)))
                    .MapToCloudRun(config => config
                        .WithPaths(p => p.StartsWith("/estimates"))
                        .WithUrl(url)
                        .WithProtocol(Protocols.Http2)
                        .WithNoAuth()
                        .WithTimeout(TimeSpan.FromSeconds(30)));

            var build = next.Build();

            Assert.True(build.Components.SecuritySchemes.Any());
            next.ToJson("test2.json");
        }

        [Fact]
        public void MergeThenMapToCloudRun()
        {
            var url = "https://uber-pets.a.run.app";
            var next = Builder
                    .FromOpenApi("./Samples/2.0.uber.json")
                    .Build()
                    .Merge(Builder
                        .FromOpenApi("./Samples/2.0.petstore-simple.json")
                        .Build())
                    .WithApiGateway()
                    .MapToCloudRun(config => config
                        .WithUrl(url)
                        .WithProtocol(Protocols.Http2)
                        .WithApiKey()
                        .WithTimeout(TimeSpan.FromSeconds(15)))
                    .MapToCloudRun(config => config
                        .WithPaths(p => p.StartsWith("/estimates"))
                        .WithUrl(url)
                        .WithProtocol(Protocols.Http2)
                        .WithNoAuth()
                        .WithTimeout(TimeSpan.FromSeconds(30)));

            var build = next.Build();

            Assert.True(build.Components.SecuritySchemes.Any());
            next.ToJson("test3.json");
        }

        [Fact]
        public void AdjustedPathShouldMapToConstantAddress()
        {
            var next = Builder
                .FromOpenApi("./Samples/2.0.uber.json")
                .AdjustPath(p => "/v1" + p)
                .Build().WithApiGateway()
                .MapToCloudRun(config => config
                        .WithUrl("https://is-this-thing-on.com")
                        .WithProtocol(Protocols.Http2)
                        .WithApiKey()
                        .WithTimeout(TimeSpan.FromSeconds(15)));

            var build = next.Build();

            var xGoogleBackend = build.Paths.Values.SelectMany(p => p.Operations.Values.Select(o => o.Extensions["x-google-backend"])).OfType<OpenApiObject>();

            foreach (var operationXGoogleBackend in xGoogleBackend)
            {
                var pathTranslation = operationXGoogleBackend["path_translation"] as OpenApiString;
                Assert.Equal("CONSTANT_ADDRESS", pathTranslation?.Value);
            }
        }
    }
}
