using backendTinTuc.Service;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class DailyCrawlerService : BackgroundService
{
    private readonly CrawlingData _crawler;
    private readonly TimeSpan _interval;

    public DailyCrawlerService(CrawlingData crawler)
    {
        _crawler = crawler;
        _interval = TimeSpan.FromDays(1); // Set the interval to 1 day
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _crawler.GetLatestData();
            await Task.Delay(_interval, stoppingToken);
        }
    }
}
