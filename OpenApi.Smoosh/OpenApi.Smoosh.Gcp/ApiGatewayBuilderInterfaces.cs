﻿using System;
using Microsoft.OpenApi.Models;
using OpenApi.Smoosh.Common;

namespace OpenApi.Smoosh.Gcp
{
    public interface IApiGatewayFilterStep
    {
        IChooseGcpService ExcludeByPath(params Predicate<string>[] matches);
        IChooseGcpService KeepByPath(params Predicate<string>[] matches);
        IChooseGcpService AdjustPath(Func<string, string> transform);
        IChooseGcpService MapToCloudRun(Func<ICloudRunFilterRoutesStep, ICloudRunNext> config);
    }

    public interface IChooseGcpService
    {
        IChooseGcpService MapToCloudRun(Func<ICloudRunFilterRoutesStep, ICloudRunNext> config);
        OpenApiDocument Build();
        void ToJson(string path);
        void ToYaml(string path);
    }
}
