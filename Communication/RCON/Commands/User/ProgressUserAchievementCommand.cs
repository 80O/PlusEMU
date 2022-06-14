namespace Plus.Communication.Rcon.Commands.User;

internal class ProgressUserAchievementCommand : IRconCommand
{
    public string Description => "This command is used to progress a users achievement.";

    public string Key => "progress_user_achievement";
    public string Parameters => "%userId% %achievement% %progess%";

    public Task<bool> TryExecute(string[] parameters)
    {
        if (!int.TryParse(parameters[0], out var userId))
            return Task.FromResult(false);
        var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
        if (client == null || client.GetHabbo() == null)
            return Task.FromResult(false);

        // Validate the achievement
        if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
            return Task.FromResult(false);
        var achievement = Convert.ToString(parameters[1]);

        // Validate the progress
        if (!int.TryParse(parameters[2], out var progress))
            return Task.FromResult(false);
        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(client, achievement, progress);
        return Task.FromResult(true);
    }
}