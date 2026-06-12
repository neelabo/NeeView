using NeeView.Properties;


namespace NeeView
{
    public class MoveEffectProfileCommand : CommandElement
    {
        private readonly int _offset;

        public MoveEffectProfileCommand(int offset)
        {
            _offset = offset;

            this.Group = TextResources.GetString("CommandGroup.Effect");
            this.IsShowMessage = true;
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var profile = GetNextEffectProfile();
            return TextResources.GetString("Word.EffectProfile") + ": " + profile?.Name;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            var profile = GetNextEffectProfile();
            return profile is not null;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var profile = GetNextEffectProfile();
            if (profile is null) return;

            EffectProfileCollection.Current.SelectedProfile = profile;
        }

        private EffectProfile? GetNextEffectProfile()
        {
            return EffectProfileCollection.Current.GetNext(_offset);
        }
    }


    public class NextEffectProfileCommand : MoveEffectProfileCommand
    {
        public NextEffectProfileCommand() : base(+1)
        {
        }
    }


    public class PrevEffectProfileCommand : MoveEffectProfileCommand
    {
        public PrevEffectProfileCommand() : base(-1)
        {
        }
    }
}
