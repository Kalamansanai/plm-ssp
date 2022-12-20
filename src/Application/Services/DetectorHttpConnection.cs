using System.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.Interfaces;
using Domain.Common;
using Domain.Common.DetectorCommand;
using Domain.Entities;
using Domain.Interfaces;
using FluentResults;

namespace Application.Services;

public class DetectorHttpConnection : IDetectorConnection
{
    // all of these methods involve a HTTP client which must be initialized with the detector's IP address.
    // Detectors call the Identify endpoint on startup, and send their MAC and IP addresses
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDetectorStreamCollection _detectorStreams;

    public const string Scheme = "http";
    public const int Port = 3000;

    public DetectorHttpConnection(IHttpClientFactory httpClientFactory, IDetectorStreamCollection detectorStreams)
    {
        _httpClientFactory = httpClientFactory;
        _detectorStreams = detectorStreams;
    }

    public async Task<Result> SendCommand(Detector detector, DetectorCommand command)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(command);

            var response = await client.PostAsync($"{Scheme}://{detector.IpAddress}:{Port}/command", new StringContent(json));
            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail($"Response failed with status code {response.StatusCode}: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            // TODO(rg): logging
            Console.WriteLine(ex);
            return Result.Fail($"Failed to connect to detector '{detector.Name}'");
        }

        return Result.Ok();
    }

    public async Task<Result<byte[]>> RequestSnapshot(Detector detector)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var snapshot = await client.GetByteArrayAsync($"{Scheme}://{detector.IpAddress}:{Port}/snapshot");

            return snapshot;
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }


    public async Task<Result<Stream>> RequestStream(Detector detector)
    {
        var client = _httpClientFactory.CreateClient();
        try
        {
            var existingStream = _detectorStreams.GetStream(detector.IpAddress);

            if (existingStream is not null)
            {
                // TODO(rg): 1st stream request works, but subsequent requests will fail;
                // find out why
                // Stream newStream = new MemoryStream();
                // await existingStream.CopyToAsync(newStream);
                return existingStream;
            }

            var stream = await client.GetStreamAsync($"{Scheme}://{detector.IpAddress}:{Port}/stream");
            _detectorStreams.AddStream(detector.IpAddress, stream);

            return stream;
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public record CalibrationMessageData(int Id, List<CalibrationCoordinates.Koordinates> Qr, List<CalibrationCoordinates.Koordinates>? Tray);
    public async Task<Result<List<CalibrationCoordinates.Koordinates>>> SendCalibrationData(Detector detector, CalibrationCoordinates coordinates, List<CalibrationCoordinates.Koordinates>? newTrayPoints)
    {
        var data = new CalibrationMessageData(coordinates.Id, coordinates.Qr, newTrayPoints);

        try
        {
            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(data);

            var response = await client.PostAsync($"{Scheme}://{detector.IpAddress}:{Port}/calibrate", new StringContent(json));
            Console.WriteLine(response);
            var res = await response.Content.ReadFromJsonAsync<List<CalibrationCoordinates.Koordinates>>();

            if (res is null)
            {
                return Result.Fail("invalid response content");
            }
            return Result.Ok(res);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<byte[]>> RequestCalibrationPreview(Detector detector, CalibrationCoordinates coordinates, List<CalibrationCoordinates.Koordinates>? newTrayPoints )
    {
        var data = new CalibrationMessageData(coordinates.Id, coordinates.Qr, newTrayPoints);

        try
        {
            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(data);

            var response = await client.PostAsync($"{Scheme}://{detector.IpAddress}:{Port}/snapshot", new StringContent(json));

            var res = await response.Content.ReadFromJsonAsync<byte[]>();

            if (res is null)
            {
                return Result.Fail("result does not contain any data");
            }

            return Result.Ok(res);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
    public async Task<Result<byte[]>> RequestCollectData(Detector detector)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            
            var response = await client.GetByteArrayAsync($"{Scheme}://{detector.IpAddress}:{Port}/collect");

            return response;
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
