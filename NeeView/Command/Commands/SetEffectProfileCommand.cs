using NeeView.Properties;
using System.Linq;


namespace NeeView
{
    public class SetEffectProfileCommand : CommandElement
    {
        public SetEffectProfileCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Effect");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new SetEffectProfileCommandParameter());
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var parameter = e.Parameter.Cast<SetEffectProfileCommandParameter>();
            var profile = EffectProfileCollection.Current.Profiles.FirstOrDefault(e => e.Id == parameter.Id);
            return TextResources.GetString("Word.EffectProfile") + ": " + (profile?.Name ?? parameter.Id.ToString());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            var parameter = e.Parameter.Cast<SetEffectProfileCommandParameter>();
            return Config.Current.EffectProfiles.Profiles.Any(e => e.Id == parameter.Id);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var parameter = e.Parameter.Cast<SetEffectProfileCommandParameter>();
            EffectProfileCollection.Current.SetSelectedId(parameter.Id);
        }
    }

}
