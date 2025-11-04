using System.Text.Json;
using SalusWeb.Models;
using SalusWeb.Exceptions;

namespace SalusWeb.Services;

public class SalusGatewayService : ISalusGatewayService
{
    private readonly ILogger<SalusGatewayService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public SalusGatewayService(ILogger<SalusGatewayService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Tests connectivity to the gateway and validates authentication
    /// </summary>
    public async Task<bool> TestConnectionAsync(string host, string euid)
    {
        try
        {
            // Try to connect and get basic info from the gateway
            var response = await MakeEncryptedRequestAsync(host, euid, "read", new
            {
                requestAttr = "readall"
            });

            // If we get here, connection and authentication were successful
            _logger.LogInformation("Successfully connected to gateway at {Host}", host);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to gateway at {Host}", host);
            throw; // Re-throw to let caller handle the specific exception type
        }
    }

    public async Task<List<DeviceBase>> GetAllDevicesAsync(string host, string euid)
    {
        var allDevices = new List<DeviceBase>();
        
        // Get all devices from the gateway using real API calls
        allDevices.AddRange(await GetClimateDevicesAsync(host, euid));
        allDevices.AddRange(await GetSensorDevicesAsync(host, euid));
        allDevices.AddRange(await GetSwitchDevicesAsync(host, euid));
        allDevices.AddRange(await GetBinarySensorDevicesAsync(host, euid));
        allDevices.AddRange(await GetCoverDevicesAsync(host, euid));

        return allDevices;
    }

    public async Task<List<ClimateDevice>> GetClimateDevicesAsync(string host, string euid)
    {
        var devices = new List<ClimateDevice>();
        
        try
        {
            var allDevices = await MakeEncryptedRequestAsync(host, euid, "read", new
            {
                requestAttr = "readall"
            });

            // Filter climate devices (thermostats)
            var climateDevices = new List<JsonElement>();
            if (allDevices.TryGetProperty("id", out var idArray))
            {
                foreach (var device in idArray.EnumerateArray())
                {
                    if (device.TryGetProperty("sIT600TH", out _) || device.TryGetProperty("sTherS", out _))
                    {
                        climateDevices.Add(device);
                    }
                }
            }

            if (climateDevices.Any())
            {
                // Get detailed status for climate devices
                var deviceDataList = climateDevices
                    .Where(d => d.TryGetProperty("data", out _))
                    .Select(d => new { data = d.GetProperty("data") })
                    .ToArray();

                var status = await MakeEncryptedRequestAsync(host, euid, "read", new
                {
                    requestAttr = "deviceid",
                    id = deviceDataList
                });

                if (status.TryGetProperty("id", out var statusArray))
                {
                    foreach (var deviceStatus in statusArray.EnumerateArray())
                    {
                        try
                        {
                            if (!deviceStatus.TryGetProperty("data", out var data)) continue;
                            if (!data.TryGetProperty("UniID", out var uniqueIdProp)) continue;
                            
                            var uniqueId = uniqueIdProp.GetString();
                            if (string.IsNullOrEmpty(uniqueId)) continue;

                            var deviceName = "Unknown";
                            if (deviceStatus.TryGetProperty("sZDO", out var sZDO) &&
                                sZDO.TryGetProperty("DeviceName", out var deviceNameProp))
                            {
                                var deviceNameJson = deviceNameProp.GetString();
                                if (!string.IsNullOrEmpty(deviceNameJson))
                                {
                                    try
                                    {
                                        var nameObj = JsonSerializer.Deserialize<Dictionary<string, string>>(deviceNameJson);
                                        deviceName = nameObj?.GetValueOrDefault("deviceName", "Unknown") ?? "Unknown";
                                    }
                                    catch { }
                                }
                            }

                            double? currentTemp = null;
                            double? targetTemp = null;
                            
                            if (deviceStatus.TryGetProperty("sIT600TH", out var sIT600TH))
                            {
                                if (sIT600TH.TryGetProperty("LocalTemperature_x100", out var ct))
                                    currentTemp = ct.GetInt32() / 100.0;
                                    
                                if (sIT600TH.TryGetProperty("HeatingSetpoint_x100", out var tt))
                                    targetTemp = tt.GetInt32() / 100.0;
                            }

                            var online = true;
                            if (deviceStatus.TryGetProperty("sZDOInfo", out var sZDOInfo) &&
                                sZDOInfo.TryGetProperty("OnlineStatus_i", out var onlineStatus))
                            {
                                online = onlineStatus.GetInt32() == 1;
                            }

                            var model = "Unknown";
                            if (deviceStatus.TryGetProperty("sBasicS", out var basicS) &&
                                basicS.TryGetProperty("ModelIdentifier", out var modelId))
                            {
                                model = modelId.GetString() ?? "Unknown";
                            }

                            var manufacturer = "SALUS";
                            if (deviceStatus.TryGetProperty("sBasicS", out var basicS2) &&
                                basicS2.TryGetProperty("ManufactureName", out var mfr))
                            {
                                manufacturer = mfr.GetString() ?? "SALUS";
                            }

                            var device = new ClimateDevice
                            {
                                UniqueId = uniqueId,
                                Name = deviceName,
                                Model = model,
                                CurrentTemperature = currentTemp,
                                TargetTemperature = targetTemp,
                                MinTemperature = 5.0,
                                MaxTemperature = 35.0,
                                TemperatureUnit = "°C",
                                Available = online,
                                Manufacturer = manufacturer
                            };

                            devices.Add(device);
                            _logger.LogInformation("Found climate device: {Name} ({UniqueId})", device.Name, device.UniqueId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse climate device");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching climate devices from gateway");
            throw;
        }

        return devices;
    }

    public async Task<List<SensorDevice>> GetSensorDevicesAsync(string host, string euid)
    {
        var devices = new List<SensorDevice>();
        
        try
        {
            var allDevices = await MakeEncryptedRequestAsync(host, euid, "read", new
            {
                requestAttr = "readall"
            });

            // Filter sensor devices
            var sensorDevices = new List<JsonElement>();
            if (allDevices.TryGetProperty("id", out var idArray))
            {
                foreach (var device in idArray.EnumerateArray())
                {
                    if (device.TryGetProperty("sTempS", out _))
                    {
                        sensorDevices.Add(device);
                    }
                }
            }

            if (sensorDevices.Any())
            {
                var deviceDataList = sensorDevices
                    .Where(d => d.TryGetProperty("data", out _))
                    .Select(d => new { data = d.GetProperty("data") })
                    .ToArray();

                var status = await MakeEncryptedRequestAsync(host, euid, "read", new
                {
                    requestAttr = "deviceid",
                    id = deviceDataList
                });

                if (status.TryGetProperty("id", out var statusArray))
                {
                    foreach (var deviceStatus in statusArray.EnumerateArray())
                    {
                        try
                        {
                            if (!deviceStatus.TryGetProperty("data", out var data)) continue;
                            if (!data.TryGetProperty("UniID", out var uniqueIdProp)) continue;
                            
                            var uniqueId = uniqueIdProp.GetString();
                            if (string.IsNullOrEmpty(uniqueId)) continue;

                            if (!deviceStatus.TryGetProperty("sTempS", out var tempS)) continue;
                            if (!tempS.TryGetProperty("MeasuredValue_x100", out var tempValue)) continue;

                            var temperature = tempValue.GetInt32() / 100.0;
                            uniqueId = uniqueId + "_temp"; // Some sensors also measure temperature

                            var deviceName = "Unknown";
                            if (deviceStatus.TryGetProperty("sZDO", out var sZDO) &&
                                sZDO.TryGetProperty("DeviceName", out var deviceNameProp))
                            {
                                var deviceNameJson = deviceNameProp.GetString();
                                if (!string.IsNullOrEmpty(deviceNameJson))
                                {
                                    try
                                    {
                                        var nameObj = JsonSerializer.Deserialize<Dictionary<string, string>>(deviceNameJson);
                                        deviceName = nameObj?.GetValueOrDefault("deviceName", "Unknown") ?? "Unknown";
                                    }
                                    catch { }
                                }
                            }

                            var online = true;
                            if (deviceStatus.TryGetProperty("sZDOInfo", out var sZDOInfo) &&
                                sZDOInfo.TryGetProperty("OnlineStatus_i", out var onlineStatus))
                            {
                                online = onlineStatus.GetInt32() == 1;
                            }

                            var model = "Unknown";
                            if (deviceStatus.TryGetProperty("DeviceL", out var deviceL) &&
                                deviceL.TryGetProperty("ModelIdentifier_i", out var modelId))
                            {
                                model = modelId.GetString() ?? "Unknown";
                            }

                            var manufacturer = "SALUS";
                            if (deviceStatus.TryGetProperty("sBasicS", out var basicS) &&
                                basicS.TryGetProperty("ManufactureName", out var mfr))
                            {
                                manufacturer = mfr.GetString() ?? "SALUS";
                            }

                            var device = new SensorDevice
                            {
                                UniqueId = uniqueId,
                                Name = deviceName,
                                Model = model,
                                State = temperature.ToString("F1"),
                                Unit = "°C",
                                DeviceClass = "temperature",
                                Available = online,
                                Manufacturer = manufacturer
                            };

                            devices.Add(device);
                            _logger.LogInformation("Found sensor device: {Name} ({UniqueId})", device.Name, device.UniqueId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse sensor device");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sensor devices from gateway");
            throw;
        }

        return devices;
    }

    public async Task<List<SwitchDevice>> GetSwitchDevicesAsync(string host, string euid)
    {
        var devices = new List<SwitchDevice>();
        
        try
        {
            var allDevices = await MakeEncryptedRequestAsync(host, euid, "read", new
            {
                requestAttr = "readall"
            });

            // Filter switch devices
            var switchDevices = new List<JsonElement>();
            if (allDevices.TryGetProperty("id", out var idArray))
            {
                foreach (var device in idArray.EnumerateArray())
                {
                    if (device.TryGetProperty("sOnOffS", out _))
                    {
                        switchDevices.Add(device);
                    }
                }
            }

            if (switchDevices.Any())
            {
                var deviceDataList = switchDevices
                    .Where(d => d.TryGetProperty("data", out _))
                    .Select(d => new { data = d.GetProperty("data") })
                    .ToArray();

                var status = await MakeEncryptedRequestAsync(host, euid, "read", new
                {
                    requestAttr = "deviceid",
                    id = deviceDataList
                });

                if (status.TryGetProperty("id", out var statusArray))
                {
                    foreach (var deviceStatus in statusArray.EnumerateArray())
                    {
                        try
                        {
                            if (!deviceStatus.TryGetProperty("data", out var data)) continue;
                            if (!data.TryGetProperty("UniID", out var uniqueIdProp)) continue;
                            
                            var uniqueId = uniqueIdProp.GetString();
                            if (string.IsNullOrEmpty(uniqueId)) continue;

                            // Skip roller shutter endpoints in combined devices
                            if (deviceStatus.TryGetProperty("sLevelS", out _)) continue;

                            if (!deviceStatus.TryGetProperty("sOnOffS", out var onOffS)) continue;
                            if (!onOffS.TryGetProperty("OnOff", out var onOffValue)) continue;

                            var endpoint = 1;
                            if (data.TryGetProperty("Endpoint", out var endpointProp))
                            {
                                endpoint = endpointProp.GetInt32();
                            }
                            uniqueId = uniqueId + "_" + endpoint; // Double switches have different endpoints

                            var deviceName = uniqueId;
                            if (deviceStatus.TryGetProperty("sZDO", out var sZDO) &&
                                sZDO.TryGetProperty("DeviceName", out var deviceNameProp))
                            {
                                var deviceNameJson = deviceNameProp.GetString();
                                if (!string.IsNullOrEmpty(deviceNameJson))
                                {
                                    try
                                    {
                                        var nameObj = JsonSerializer.Deserialize<Dictionary<string, string>>(deviceNameJson);
                                        deviceName = nameObj?.GetValueOrDefault("deviceName", uniqueId) ?? uniqueId;
                                    }
                                    catch { }
                                }
                            }

                            var online = true;
                            if (deviceStatus.TryGetProperty("sZDOInfo", out var sZDOInfo) &&
                                sZDOInfo.TryGetProperty("OnlineStatus_i", out var onlineStatus))
                            {
                                online = onlineStatus.GetInt32() == 1;
                            }

                            var model = "Unknown";
                            if (deviceStatus.TryGetProperty("DeviceL", out var deviceL) &&
                                deviceL.TryGetProperty("ModelIdentifier_i", out var modelId))
                            {
                                model = modelId.GetString() ?? "Unknown";
                            }

                            var manufacturer = "SALUS";
                            if (deviceStatus.TryGetProperty("sBasicS", out var basicS) &&
                                basicS.TryGetProperty("ManufactureName", out var mfr))
                            {
                                manufacturer = mfr.GetString() ?? "SALUS";
                            }

                            var device = new SwitchDevice
                            {
                                UniqueId = uniqueId,
                                Name = deviceName,
                                Model = model,
                                IsOn = onOffValue.GetInt32() == 1,
                                Available = online,
                                Manufacturer = manufacturer
                            };

                            devices.Add(device);
                            _logger.LogInformation("Found switch device: {Name} ({UniqueId})", device.Name, device.UniqueId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse switch device");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching switch devices from gateway");
            throw;
        }

        return devices;
    }

    public async Task<List<BinarySensorDevice>> GetBinarySensorDevicesAsync(string host, string euid)
    {
        var devices = new List<BinarySensorDevice>();
        
        try
        {
            var allDevices = await MakeEncryptedRequestAsync(host, euid, "read", new
            {
                requestAttr = "readall"
            });

            // Filter binary sensor devices
            var binarySensorDevices = new List<JsonElement>();
            if (allDevices.TryGetProperty("id", out var idArray))
            {
                foreach (var device in idArray.EnumerateArray())
                {
                    var hasIASZS = device.TryGetProperty("sIASZS", out _);
                    var isMiniTRV = false;
                    
                    if (device.TryGetProperty("sBasicS", out var basicS) &&
                        basicS.TryGetProperty("ModelIdentifier", out var modelId))
                    {
                        var model = modelId.GetString();
                        isMiniTRV = model == "it600MINITRV" || model == "it600Receiver";
                    }
                    
                    if (hasIASZS || isMiniTRV)
                    {
                        binarySensorDevices.Add(device);
                    }
                }
            }

            if (binarySensorDevices.Any())
            {
                var deviceDataList = binarySensorDevices
                    .Where(d => d.TryGetProperty("data", out _))
                    .Select(d => new { data = d.GetProperty("data") })
                    .ToArray();

                var status = await MakeEncryptedRequestAsync(host, euid, "read", new
                {
                    requestAttr = "deviceid",
                    id = deviceDataList
                });

                if (status.TryGetProperty("id", out var statusArray))
                {
                    foreach (var deviceStatus in statusArray.EnumerateArray())
                    {
                        try
                        {
                            if (!deviceStatus.TryGetProperty("data", out var data)) continue;
                            if (!data.TryGetProperty("UniID", out var uniqueIdProp)) continue;
                            
                            var uniqueId = uniqueIdProp.GetString();
                            if (string.IsNullOrEmpty(uniqueId)) continue;

                            var deviceName = "Unknown";
                            if (deviceStatus.TryGetProperty("sZDO", out var sZDO) &&
                                sZDO.TryGetProperty("DeviceName", out var deviceNameProp))
                            {
                                var deviceNameJson = deviceNameProp.GetString();
                                if (!string.IsNullOrEmpty(deviceNameJson))
                                {
                                    try
                                    {
                                        var nameObj = JsonSerializer.Deserialize<Dictionary<string, string>>(deviceNameJson);
                                        deviceName = nameObj?.GetValueOrDefault("deviceName", "Unknown") ?? "Unknown";
                                    }
                                    catch { }
                                }
                            }

                            var online = true;
                            if (deviceStatus.TryGetProperty("sZDOInfo", out var sZDOInfo) &&
                                sZDOInfo.TryGetProperty("OnlineStatus_i", out var onlineStatus))
                            {
                                online = onlineStatus.GetInt32() == 1;
                            }

                            var model = "Unknown";
                            if (deviceStatus.TryGetProperty("sBasicS", out var basicS) &&
                                basicS.TryGetProperty("ModelIdentifier", out var modelId))
                            {
                                model = modelId.GetString() ?? "Unknown";
                            }

                            // Determine device class and state based on model
                            var isOn = false;
                            var deviceClass = "none";
                            
                            if (deviceStatus.TryGetProperty("sIASZS", out var iaszs))
                            {
                                if (iaszs.TryGetProperty("ZoneStatus", out var zoneStatus))
                                {
                                    var status_value = zoneStatus.GetInt32();
                                    isOn = (status_value & 1) != 0; // Check first bit for alarm status
                                    
                                    // Try to determine device type from model
                                    if (!string.IsNullOrEmpty(model))
                                    {
                                        if (model.Contains("Motion", StringComparison.OrdinalIgnoreCase) || 
                                            model.Contains("PS", StringComparison.OrdinalIgnoreCase))
                                            deviceClass = "motion";
                                        else if (model.Contains("Door", StringComparison.OrdinalIgnoreCase) || 
                                                 model.Contains("Window", StringComparison.OrdinalIgnoreCase) ||
                                                 model.Contains("OS", StringComparison.OrdinalIgnoreCase))
                                            deviceClass = "opening";
                                    }
                                }
                            }

                            var manufacturer = "SALUS";
                            if (deviceStatus.TryGetProperty("sBasicS", out var basicS2) &&
                                basicS2.TryGetProperty("ManufactureName", out var mfr))
                            {
                                manufacturer = mfr.GetString() ?? "SALUS";
                            }

                            var device = new BinarySensorDevice
                            {
                                UniqueId = uniqueId,
                                Name = deviceName,
                                Model = model,
                                IsOn = isOn,
                                DeviceClass = deviceClass,
                                Available = online,
                                Manufacturer = manufacturer
                            };

                            devices.Add(device);
                            _logger.LogInformation("Found binary sensor device: {Name} ({UniqueId})", device.Name, device.UniqueId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse binary sensor device");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching binary sensor devices from gateway");
            throw;
        }

        return devices;
    }

    public async Task<List<CoverDevice>> GetCoverDevicesAsync(string host, string euid)
    {
        var devices = new List<CoverDevice>();
        
        try
        {
            var allDevices = await MakeEncryptedRequestAsync(host, euid, "read", new
            {
                requestAttr = "readall"
            });

            // Filter cover devices (blinds/shutters)
            var coverDevices = new List<JsonElement>();
            if (allDevices.TryGetProperty("id", out var idArray))
            {
                foreach (var device in idArray.EnumerateArray())
                {
                    if (device.TryGetProperty("sLevelS", out _))
                    {
                        coverDevices.Add(device);
                    }
                }
            }

            if (coverDevices.Any())
            {
                var deviceDataList = coverDevices
                    .Where(d => d.TryGetProperty("data", out _))
                    .Select(d => new { data = d.GetProperty("data") })
                    .ToArray();

                var status = await MakeEncryptedRequestAsync(host, euid, "read", new
                {
                    requestAttr = "deviceid",
                    id = deviceDataList
                });

                if (status.TryGetProperty("id", out var statusArray))
                {
                    foreach (var deviceStatus in statusArray.EnumerateArray())
                    {
                        try
                        {
                            if (!deviceStatus.TryGetProperty("data", out var data)) continue;
                            if (!data.TryGetProperty("UniID", out var uniqueIdProp)) continue;
                            
                            var uniqueId = uniqueIdProp.GetString();
                            if (string.IsNullOrEmpty(uniqueId)) continue;

                            // Skip endpoints which are disabled
                            if (deviceStatus.TryGetProperty("sButtonS", out var buttonS) && 
                                buttonS.TryGetProperty("Mode", out var mode) && 
                                mode.GetInt32() == 0)
                                continue;

                            if (!deviceStatus.TryGetProperty("sLevelS", out var levelS)) continue;
                            
                            var currentPosition = 0;
                            if (levelS.TryGetProperty("CurrentLevel", out var cp))
                            {
                                currentPosition = cp.GetInt32();
                            }

                            var deviceName = "Unknown";
                            if (deviceStatus.TryGetProperty("sZDO", out var sZDO) &&
                                sZDO.TryGetProperty("DeviceName", out var deviceNameProp))
                            {
                                var deviceNameJson = deviceNameProp.GetString();
                                if (!string.IsNullOrEmpty(deviceNameJson))
                                {
                                    try
                                    {
                                        var nameObj = JsonSerializer.Deserialize<Dictionary<string, string>>(deviceNameJson);
                                        deviceName = nameObj?.GetValueOrDefault("deviceName", "Unknown") ?? "Unknown";
                                    }
                                    catch { }
                                }
                            }

                            var online = true;
                            if (deviceStatus.TryGetProperty("sZDOInfo", out var sZDOInfo) &&
                                sZDOInfo.TryGetProperty("OnlineStatus_i", out var onlineStatus))
                            {
                                online = onlineStatus.GetInt32() == 1;
                            }

                            var model = "Unknown";
                            if (deviceStatus.TryGetProperty("DeviceL", out var deviceL) &&
                                deviceL.TryGetProperty("ModelIdentifier_i", out var modelId))
                            {
                                model = modelId.GetString() ?? "Unknown";
                            }

                            var state = currentPosition == 0 ? "closed" : currentPosition == 100 ? "open" : "partially_open";

                            var manufacturer = "SALUS";
                            if (deviceStatus.TryGetProperty("sBasicS", out var basicS) &&
                                basicS.TryGetProperty("ManufactureName", out var mfr))
                            {
                                manufacturer = mfr.GetString() ?? "SALUS";
                            }

                            var device = new CoverDevice
                            {
                                UniqueId = uniqueId,
                                Name = deviceName,
                                Model = model,
                                Position = currentPosition,
                                State = state,
                                Available = online,
                                Manufacturer = manufacturer
                            };

                            devices.Add(device);
                            _logger.LogInformation("Found cover device: {Name} ({UniqueId})", device.Name, device.UniqueId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse cover device");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching cover devices from gateway");
            throw;
        }

        return devices;
    }

    /// <summary>
    /// Makes an encrypted request to the Salus iT600 Gateway
    /// </summary>
    private async Task<JsonElement> MakeEncryptedRequestAsync(string host, string euid, string command, object requestBody)
    {
        var encryptor = new SalusEncryptor(euid);
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(10);

        try
        {
            var requestUrl = $"http://{host}/deviceid/{command}";
            var requestJson = JsonSerializer.Serialize(requestBody);

            _logger.LogDebug("Gateway request: POST {Url}", requestUrl);

            // Encrypt the request
            var encryptedData = encryptor.Encrypt(requestJson);

            var content = new ByteArrayContent(encryptedData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync(requestUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new SalusConnectionException($"Gateway returned status code: {response.StatusCode}");
            }

            // Decrypt the response
            var responseBytes = await response.Content.ReadAsByteArrayAsync();
            var responseJson = encryptor.Decrypt(responseBytes);

            _logger.LogDebug("Gateway response received");

            var jsonDoc = JsonDocument.Parse(responseJson);
            var root = jsonDoc.RootElement;

            // Check if the response status is success
            if (!root.TryGetProperty("status", out var statusProp) || statusProp.GetString() != "success")
            {
                _logger.LogError("Gateway rejected command: {Command}", command);
                throw new SalusCommandException($"Gateway rejected '{command}' command");
            }

            return root;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while connecting to gateway");
            throw new SalusConnectionException("Timeout while connecting to gateway. Please check if the gateway is accessible.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connection error while communicating with gateway");
            throw new SalusConnectionException("Cannot connect to gateway. Please check the host/IP address.", ex);
        }
        catch (SalusException)
        {
            throw; // Re-throw our custom exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while communicating with gateway");
            
            // Try to determine if it's an authentication error
            if (ex.Message.Contains("decrypt", StringComparison.OrdinalIgnoreCase) || 
                ex.Message.Contains("padding", StringComparison.OrdinalIgnoreCase))
            {
                throw new SalusAuthenticationException("Authentication failed. Please check if the EUID is correct.", ex);
            }
            
            throw new SalusCommandException("Unexpected error occurred while communicating with gateway", ex);
        }
    }
}
