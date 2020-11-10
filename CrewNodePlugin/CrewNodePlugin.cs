﻿using System;
using System.Threading.Tasks;
using CrewNodePlugin.Games;
using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Microsoft.Extensions.Logging;

namespace CrewNodePlugin
{
    [ImpostorPlugin(package: "crewnode", name: "CrewNode", author: "CrewNode", version: "0.0.1")]
    public class CrewNodePlugin : PluginBase
    {
        /// <summary>
        ///     A logger that works seamlessly with the server.
        /// </summary>
        private readonly ILogger<CrewNodePlugin> _logger;
        private readonly IEventManager _eventManager;
        private IDisposable[] _unregister;
        public static bool debug = true;

        /// <summary>
        ///     The constructor of the plugin. There are a few parameters you can add here and they
        ///     will be injected automatically by the server, two examples are used here.
        ///
        ///     They are not necessary but very recommended.
        /// </summary>
        /// <param name="logger">
        ///     A logger to write messages in the console.
        /// </param>
        /// <param name="eventManager">
        ///     An event manager to register event listeners.
        ///     Useful if you want your plugin to interact with the game.
        /// </param>
        public CrewNodePlugin(ILogger<CrewNodePlugin> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
            GameModeType.SetLogger(logger);
        }

        /// <summary>
        ///     This is called when your plugin is enabled by the server.
        /// </summary>
        /// <returns></returns>
        public override ValueTask EnableAsync()
        {
            _logger.LogInformation("CrewNodePlugin is being enabled.");
            _unregister = new[] {
                _eventManager.RegisterListener(new GameEventListener(_logger)),
                _eventManager.RegisterListener(new PlayerEventListener(_logger))
            };
            return default;
        }

        /// <summary>
        ///     This is called when your plugin is disabled by the server.
        ///     Most likely because it is shutting down, this is the place to clean up any managed resources.
        /// </summary>
        /// <returns></returns>
        public override ValueTask DisableAsync()
        {
            _logger.LogInformation("CrewNodePlugin is being disabled.");
            foreach (var listener in _unregister)
                listener.Dispose();

            return default;
        }
    }
}