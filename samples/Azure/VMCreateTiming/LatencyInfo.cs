// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace VMCreateTiming
{
    class LatencyInfo
    {
        public string Name { get; set; }

        public TimeSpan Latency { get; set; }

        public string ProvisioningState { get; set; }
    }
}
