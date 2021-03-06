﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SupercellUilityApi.Models;

namespace SupercellUilityApi.Database
{
    public class FingerprintDatabase
    {
        private const string Name = "fingerprint";
        private static string _connectionString;

        public FingerprintDatabase()
        {
            _connectionString = new MySqlConnectionStringBuilder
            {
                Server = Resources.Configuration.MySqlServer,
                Database = Resources.Configuration.MySqlDatabase,
                UserID = Resources.Configuration.MySqlUserId,
                Password = Resources.Configuration.MySqlPassword,
                SslMode = MySqlSslMode.None,
                CharacterSet = "utf8mb4"
            }.ToString();

            Count = CountSync();

            if (Count > -1) return;

            Logger.Log($"MysqlConnection for {Name} failed [{Resources.Configuration.MySqlServer}]!");
            Program.Exit();
        }

        public static long Count { get; set; }

        public static async Task ExecuteAsync(MySqlCommand cmd)
        {
            #region ExecuteAsync

            try
            {
                cmd.Connection = new MySqlConnection(_connectionString);
                await cmd.Connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
            catch (MySqlException exception)
            {
                Logger.Log(exception, Logger.ErrorLevel.Error);
            }
            finally
            {
                if (cmd.Connection != null)
                    await cmd.Connection.CloseAsync();
            }

            #endregion
        }

        public static long CountSync()
        {
            #region CountSync

            try
            {
                long seed;

                using var connection = new MySqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new MySqlCommand($"SELECT COUNT(*) FROM {Name}", connection))
                {
                    seed = Convert.ToInt64(cmd.ExecuteScalar());
                }

                connection.Close();

                return seed;
            }
            catch (Exception exception)
            {
                Logger.Log(exception, Logger.ErrorLevel.Error);

                return -1;
            }

            #endregion
        }

        public static long MaxTimestamp(string gameName)
        {
            #region MaxTimestamp

            try
            {
                long seed;

                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                using (var cmd =
                    new MySqlCommand($"SELECT coalesce(MAX(Timestamp), 0) FROM {Name} WHERE Game = '{gameName}'",
                        connection))
                {
                    seed = Convert.ToInt64(cmd.ExecuteScalar());
                }

                connection.Close();

                return seed;
            }
            catch (Exception exception)
            {
                Logger.Log(exception, Logger.ErrorLevel.Error);
                return -1;
            }

            #endregion
        }

        public static async Task<long> CountAsync()
        {
            #region CountAsync

            try
            {
                long seed;

                await using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                await using (var cmd = new MySqlCommand($"SELECT COUNT(*) FROM {Name}", connection))
                {
                    seed = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                }

                await connection.CloseAsync();

                return seed;
            }
            catch (Exception exception)
            {
                Logger.Log(exception, Logger.ErrorLevel.Error);

                return -1;
            }

            #endregion
        }

        public static async Task SaveFingerprintLog(FingerprintLog log, string gameName, string json)
        {
            #region SaveAsync

            try
            {
                await using var cmd =
                    new MySqlCommand(
                        $"INSERT INTO {Name} (`Game`, `Sha`, `Version`, `Timestamp`, `Json`) VALUES (@game, @sha, @version, @timestamp, @json)");
#pragma warning disable 618
                cmd.Parameters?.AddWithValue("@game", gameName);
                cmd.Parameters?.AddWithValue("@sha", log.Sha);
                cmd.Parameters?.AddWithValue("@version", log.Version);
                cmd.Parameters?.AddWithValue("@timestamp", log.Timestamp);
                cmd.Parameters?.AddWithValue("@json", json);
#pragma warning restore 618

                await ExecuteAsync(cmd);
            }
            catch (Exception exception)
            {
                Logger.Log(exception, Logger.ErrorLevel.Error);
            }

            #endregion
        }

        public static async Task<List<FingerprintLog>> GetFingerprintLogs(string gameName)
        {
            #region GetAsync

            var list = new List<FingerprintLog>();

            await using var cmd =
                new MySqlCommand(
                    $"SELECT * FROM {Name} WHERE Game = '{gameName}' ORDER BY `Timestamp` DESC")
                {
                    Connection = new MySqlConnection(_connectionString)
                };

            try
            {
                await cmd.Connection.OpenAsync();

                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                    list.Add(new FingerprintLog
                    {
                        Sha = reader["Sha"].ToString(),
                        Version = reader["Version"].ToString(),
                        Timestamp = long.Parse(reader["Timestamp"].ToString() ?? "0"),
                        HasJson = !string.IsNullOrEmpty(reader["Json"].ToString())
                    });
            }
            catch (Exception exception)
            {
                Logger.Log(exception, Logger.ErrorLevel.Error);
            }
            finally
            {
                if (cmd.Connection != null)
                    await cmd.Connection.CloseAsync();
            }

            return list;

            #endregion
        }

        public static async Task<FingerprintLog> GetLatestFingerprint(string gameName)
        {
            #region GetAsync

            var timestamp = MaxTimestamp(gameName);
            if (timestamp <= -1) return null;

            await using var cmd =
                new MySqlCommand(
                    $"SELECT * FROM {Name} WHERE Timestamp = '{timestamp}'")
                {
                    Connection = new MySqlConnection(_connectionString)
                };

            try
            {
                await cmd.Connection.OpenAsync();

                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                    return new FingerprintLog
                    {
                        Sha = reader["Sha"].ToString(),
                        Version = reader["Version"].ToString(),
                        Timestamp = long.Parse(reader["Timestamp"].ToString() ?? "0"),
                        HasJson = !string.IsNullOrEmpty(reader["Json"].ToString())
                    };
            }
            catch (Exception exception)
            {
                Logger.Log(exception, Logger.ErrorLevel.Error);
            }
            finally
            {
                if (cmd.Connection != null)
                    await cmd.Connection.CloseAsync();
            }

            return null;

            #endregion
        }

        public static async Task<Fingerprint> GetFingerprintBySha(string gameName, string sha)
        {
            #region GetAsync

            await using var cmd =
                new MySqlCommand(
                    $"SELECT * FROM {Name} WHERE Game = '{gameName}' AND Sha = @sha")
                {
                    Connection = new MySqlConnection(_connectionString)
                };

            try
            {
                cmd.Parameters?.AddWithValue("@sha", sha);
                await cmd.Connection.OpenAsync();

                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                    return JsonSerializer.Deserialize<Fingerprint>(reader["Json"].ToString());
            }
            catch (Exception exception)
            {
                Logger.Log(exception, Logger.ErrorLevel.Error);
            }
            finally
            {
                if (cmd.Connection != null)
                    await cmd.Connection.CloseAsync();
            }

            return null;

            #endregion
        }
    }
}