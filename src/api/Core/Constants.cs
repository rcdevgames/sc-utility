﻿namespace SupercellUilityApi.Core
{
    public class Constants
    {
        /// <summary>
        /// The amount of hours the server waits until to check the server status again
        /// </summary>
        public const int ContentUpdateTimeout = 2;

        /// <summary>
        /// The interval in which the games are checked (seconds)
        /// </summary>
        public const int StatusCheckInterval = 10;
    }
}
