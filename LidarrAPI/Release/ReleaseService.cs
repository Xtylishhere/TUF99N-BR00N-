﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using LidarrAPI.Database;
using LidarrAPI.Release.AppVeyor;
using LidarrAPI.Release.Github;
using LidarrAPI.Update;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace LidarrAPI.Release
{
    public class ReleaseService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IServiceProvider _serviceProvider;
        
        private readonly ConcurrentDictionary<Branch, Type> _releaseBranches;

        public ReleaseService(IServiceProvider serviceProvider, DatabaseContext databaseContext)
        {
            _serviceProvider = serviceProvider;
            _releaseBranches = new ConcurrentDictionary<Branch, Type>();
            _releaseBranches.TryAdd(Branch.Develop, typeof(GithubReleaseSource)); // new GithubReleaseSource(serviceProvider, Branch.Develop));
            _releaseBranches.TryAdd(Branch.Nightly, typeof(AppVeyorReleaseSource)); //new AppVeyorReleaseSource(serviceProvider, Branch.Nightly));
        }

        public async Task UpdateReleasesAsync(Branch branch)
        {
            if (!_releaseBranches.TryGetValue(branch, out var releaseSourceBaseType))
            {
                throw new NotImplementedException($"{branch} does not have a release source.");
            }
            
            var releaseSourceInstance = (ReleaseSourceBase) _serviceProvider.GetRequiredService(releaseSourceBaseType);

            releaseSourceInstance.ReleaseBranch = branch;

            await releaseSourceInstance.StartFetchReleasesAsync();
        }
    }
}