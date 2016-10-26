﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoMapper;
using LegacyDataImporter.LegacyModels;
using LegacyDataImporter.Models;
using Contract = LegacyDataImporter.LegacyModels.Contract;
using Player = LegacyDataImporter.LegacyModels.Player;
using Round = LegacyDataImporter.LegacyModels.Round;

namespace LegacyDataImporter
{
    public class ClassMaps
    {
        public static void BuildMaps(IMapperConfigurationExpression cfg)
        {
            var playerIdMaps = new Dictionary<int, Guid>();
            var clubIdMaps = new Dictionary<int, Guid>();

            cfg.CreateMap<Round, Models.Round>();
            cfg.CreateMap<Team, Club>()
                .ConvertUsing(team =>
                {
                    var clubId = Guid.NewGuid();

                    if (!clubIdMaps.ContainsKey(team.Id))
                    {
                        clubIdMaps.Add(team.Id, clubId);
                    }

                    return new Club
                    {
                        LegacyId = team.Id,
                        CoachName = team.CoachName,
                        ClubName = team.TeamName,
                        Email = team.Email,
                        Id = clubId
                    };
                });
            cfg.CreateMap<Player, Models.Player>()
                .ConvertUsing(player =>
                {
                    var middleNames = string.IsNullOrWhiteSpace(player.MiddleNames)
                        ? String.Empty : $" {player.MiddleNames}";
                    var playerId = Guid.NewGuid();

                    if (!playerIdMaps.ContainsKey(player.Id))
                    {
                        playerIdMaps.Add(player.Id, playerId);
                    }

                    return new Models.Player
                    {
                        Name = $"{player.FirstName}{middleNames} {player.LastName}",
                        Id = playerId,
                        Active = player.Active,
                        CurrentAflClub = new Models.Player.AflClub
                        {
                            Name = player.CurrentAflTeam.Name,
                            ShortName = player.CurrentAflTeam.ShortName
                        },
                        FootywireName = player.FootywireName
                    };
                });
            cfg.CreateMap<Contract, Models.Contract>()
                .ConvertUsing(contract =>
                    new Models.Contract
                    {
                        ToRound = contract.ToRound,
                        FromRound = contract.FromRound,
                        DraftPick = contract.DraftPick,
                        PlayerId = playerIdMaps[contract.PlayerId],
                        ClubId = clubIdMaps[contract.TeamId]
                    });
        }
    }
}