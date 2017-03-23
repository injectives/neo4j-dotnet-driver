﻿// Copyright (c) 2002-2017 "Neo Technology,"
// Network Engine for Objects in Lund AB [http://neotechnology.com]
// 
// This file is part of Neo4j.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;
using System.IO;
using static Neo4j.Driver.IntegrationTests.Internals.WindowsPowershellRunner;

namespace Neo4j.Driver.IntegrationTests.Internals
{
    public class ExternalBoltkitClusterInstaller : IInstaller
    {
        public static readonly string ClusterDir = Path.Combine(BoltkitHelper.TargetDir, "cluster");
        private const int Cores = 3;
        //TODO Add readreplicas into the cluster too
        //private const int ReadReplicas = 2;

        private const string Password = "cluster";

        public void Install()
        {
            if (Directory.Exists(ClusterDir))
            {
                Debug($"Found and using cluster intalled at `{ClusterDir}`.");
                // no need to redownload and change the password if already downloaded locally
                return;
            }

            RunCommand("neoctrl-cluster", new[] {
                "install",
                "--cores", $"{Cores}", //"--read-replicas", $"{ReadReplicas}", TODO
                "--password", Password,
                BoltkitHelper.ServerVersion(), ClusterDir});
            Debug($"Installed cluster at `{ClusterDir}`.");
        }

        public ISet<ISingleInstance> Start()
        {
            Debug("Starting cluster...");
            var ret = ParseClusterMember(
                RunCommand("neoctrl-cluster", new[] { "start", ClusterDir }));
            Debug("Cluster started.");
            return ret;
        }

        private ISet<ISingleInstance> ParseClusterMember(string[] lines)
        {
            var members = new HashSet<ISingleInstance>();
            foreach (var line in lines)
            {
                if (line.Trim().Equals(string.Empty))
                {
                    // ignore empty lines in the output
                    continue;
                }
                var tokens = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != 3)
                {
                    throw new ArgumentException(
                        "Failed to parse cluster memeber created by boltkit. " +
                        "Expected output to have 'http_uri, bolt_uri, path' in each line. " +
                        $"The output:{Environment.NewLine}{string.Join(Environment.NewLine, lines)}" +
                        $"{Environment.NewLine}The error found in line: {line}");
                }
                members.Add(new SingleInstance(tokens[0], tokens[1], tokens[2], Password));
            }
            return members;
        }

        public void Stop()
        {
            Debug("Stopping cluster...");
            RunCommand("neoctrl-cluster", new []{ "stop", ClusterDir });
            Debug("Cluster stopped.");
        }

        public void Kill()
        {
            Debug("Killing cluster...");
            RunCommand("neoctrl-cluster", new []{ "stop", "--kill", ClusterDir });
            Debug("Cluster killed.");
        }
    }
}
