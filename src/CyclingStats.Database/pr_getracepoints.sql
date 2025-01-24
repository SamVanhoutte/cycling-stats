CREATE PROCEDURE [dbo].[pr_getracepoints]

AS
  SELECT  rp.RaceId, rp.RiderId, rp.Points as AchievedPoints, rp.Position as AchievedPosition, rp.Picked as TimesPicked, rp.Stars as Stars,
          races.StageRace, races.Name as RaceName, races.RaceDate, races.RaceType, races.Distance, races.PointsScale, races.UciScale, races.ParcoursType,
          races.ProfileScore as raceProfileScore, races.RaceRanking, races.Elevation, races.StartlistQuality, races.DecidingMethod, 
          races.Classification as RaceClassification, races.Category as RaceCategory,
          rs.Position as RacePosition, rs.Gap as TimeGap
  FROM    RacePoints rp 
          INNER JOIN Races on rp.RaceId = races.id
          INNER JOIN RaceResults rs on rp.RaceId = rs.Raceid AND rp.RiderId = rs.RiderId
