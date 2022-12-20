using WDE.Module.Attributes;
using WDE.QueryGenerators.Base;
using WDE.QueryGenerators.Models;
using WDE.SqlQueryGenerator;

namespace WDE.QueryGenerators.Generators.Gameobject;

[AutoRegister]
[RequiresCore("TrinityMaster")]
internal class MasterGameObjectQueryProvider : IInsertQueryProvider<GameObjectSpawnModelEssentials>
{
    public IQuery Insert(GameObjectSpawnModelEssentials t)
    {
        return Queries.Table("gameobject").Insert(new
        {
            guid = t.Guid,
            id = t.Entry,
            map = t.Map,
            phaseId = t.PhaseId,
            position_x = t.X,
            position_y = t.Y,
            position_z = t.Z,
            state = t.State,
            rotation0 = t.Rotation0,
            rotation1 = t.Rotation1,
            rotation2 = t.Rotation2,
            rotation3 = t.Rotation3,
        });
    }

    public string TableName => "gameobject";
}