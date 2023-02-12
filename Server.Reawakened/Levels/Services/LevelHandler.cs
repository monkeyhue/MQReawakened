﻿using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.XMLs.Bundles;
using System.Text.Json;
using System.Xml;
using WorldGraphDefines;

namespace Server.Reawakened.Levels.Services;

public class LevelHandler : IService
{
    private readonly ServerStaticConfig _config;
    private readonly Dictionary<int, Level> _levels;
    private readonly ILogger<LevelHandler> _logger;
    private readonly ReflectionUtils _reflection;
    private readonly IServiceProvider _services;
    private readonly EventSink _sink;
    private readonly WorldGraph _worldGraph;

    public Dictionary<string, Type> ProcessableData;

    public LevelHandler(EventSink sink, ServerStaticConfig config, WorldGraph worldGraph,
        ReflectionUtils reflection, IServiceProvider services, ILogger<LevelHandler> logger)
    {
        _sink = sink;
        _config = config;
        _worldGraph = worldGraph;
        _reflection = reflection;
        _services = services;
        _logger = logger;
        _levels = new Dictionary<int, Level>();
    }

    public void Initialize() => _sink.WorldLoad += LoadLevels;

    private void LoadLevels()
    {
        GetDirectory.OverwriteDirectory(_config.LevelDataSaveDirectory);

        ProcessableData = typeof(DataComponentAccessor).Assembly.GetServices<DataComponentAccessor>()
            .ToDictionary(x => x.Name, x => x);

        foreach (var level in _levels.Values.Where(level => level.LevelInfo.LevelId != -1))
            level.DumpPlayersToLobby();

        _levels.Clear();
    }

    public Level GetLevelFromId(int levelId)
    {
        if (_levels.TryGetValue(levelId, out var value))
            return value;

        LevelInfo levelInfo = null;
        var levelPlanes = new LevelPlanes();

        if (levelId is -1 or 0)
        {
            var name = levelId switch
            {
                -1 => "Disconnected",
                0 => "Lobby",
                _ => throw new ArgumentOutOfRangeException(nameof(levelId), levelId, null)
            };

            levelInfo = new LevelInfo(name, name, name, levelId,
                0, 0, LevelType.Unknown, TribeType._Invalid);
        }
        else
        {
            try
            {
                levelInfo = _worldGraph!.GetInfoLevel(levelId);

                if (string.IsNullOrEmpty(levelInfo.Name))
                    throw new MissingFieldException($"Level '{levelId}' does not have a valid name!");

                var levelInfoPath = Path.Join(_config.LevelSaveDirectory, $"{levelInfo.Name}.xml");
                var levelDataPath = Path.Join(_config.LevelDataSaveDirectory, $"{levelInfo.Name}.json");

                var xmlDocument = new XmlDocument();
                xmlDocument.Load(levelInfoPath);
                levelPlanes.LoadXmlDocument(xmlDocument);

                File.WriteAllText(levelDataPath,
                    JsonSerializer.Serialize(levelPlanes, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (NullReferenceException)
            {
                if (_levels.Count == 0)
                    _logger.LogCritical(
                        "Could not find any levels! Are you sure you have your cache set up correctly?");
                else
                    _logger.LogError("Could not find the required level! Are you sure your caches contain this?");
            }
        }

        var level = new Level(levelInfo, levelPlanes, _config, this, _reflection, _services, _logger);

        _levels.Add(levelId, level);

        return level;
    }

    public void RemoveLevel(int levelId) => _levels.Remove(levelId);
}
