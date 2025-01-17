﻿using System;
using System.IO;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Smoosh.OpenApi.Common;
using Smoosh.OpenApi.Gcp.Operations;

namespace Smoosh.OpenApi.Gcp
{
    public class ApiGatewayBuilder : IApiGatewayFilterStep, IChooseGcpService
    {
        internal Builder Builder;
        protected internal ApiGatewayBuilder(){}

        public static IApiGatewayFilterStep FromOpenApi(Stream stream)
        {
            return new ApiGatewayBuilder
            {
                Builder = (Builder)Builder.FromOpenApi(stream)
            };
        }

        public static IApiGatewayFilterStep FromBuilder(IBuilderBuilt builder)
        {
            if (!(builder is Builder genericBuilder))
            {
                throw new ArgumentException();
            }

            return new ApiGatewayBuilder
            {
                Builder = genericBuilder
            };
        }

        public static IApiGatewayFilterStep FromOpenApi(string file)
        {
            return FromOpenApi(File.OpenRead(file));
        }

        public IChooseGcpService ExcludeByPath(params Predicate<string>[] matches)
        {
            Builder.ExcludeByPath(matches);
            return this;
        }

        public IChooseGcpService KeepByPath(params Predicate<string>[] matches)
        {
            Builder.KeepByPath(matches);
            return this;
        }

        public IChooseGcpService AdjustPath(Func<string, string> transform)
        {
            Builder.AdjustPath(transform);
            return this;
        }


        public IChooseGcpService MapToCloudRun(Func<ICloudRunFilterRoutesStep, ICloudRunNext> config)
        {
            var cloudRunBuilder = CloudRunBuilder();

            config.Invoke(cloudRunBuilder);

            return this;
        }

        public IChooseGcpService MapToCloudRun(Func<ICloudRunFilterRoutesStep, ITimeoutStep> config)
        {
            var cloudRunBuilder = CloudRunBuilder();

            config.Invoke(cloudRunBuilder);

            return this;
        }

        private CloudRunBuilder CloudRunBuilder()
        {
            var operation = new CloudRunOperation
            {
                RemappedPathsLookup = Builder.GetRemappedPathReverseLookup()
            };

            Builder.AddOperation(operation);
            var cloudRunBuilder = new CloudRunBuilder(operation);
            return cloudRunBuilder;
        }

        private IBuilderBuilt _built;

        public OpenApiDocument Build()
        {
            if (_built == null)
            {
                _built = Builder.Build();
            }
            return _built.ToOpenApiDocument();
        }

        public void ToJson(string path)
        {
            var doc = Build();
            var json = doc.Serialize(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json);
             File.WriteAllText(path, json);
        }

        public void ToYaml(string path)
        {
            var doc = Build();
            var yaml = doc.Serialize(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Yaml);
            File.WriteAllText(path, yaml);
        }
    }
}
