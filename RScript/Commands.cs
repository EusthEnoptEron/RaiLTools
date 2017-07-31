using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaiLTools.RScript
{
    public enum TransitionType
    {
        None = 0,
        Fade = 1,
        Pattern = 2,
        Flash = 3,
        Black = 4
    }

    public enum SpriteAction
    {
        Set = 0,
        Clear = 1,
        Activate = 2
    }

    public interface ICommand
    {
        int Index { get; }
        byte[] ToBytes();
    }

    public class UnknownCommand : ICommand
    {
        public int Index { get; set; }
        public CommandToken Token { get; set; }

        public UnknownCommand(CommandToken token)
        {
            Token = token;
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }

    public class TextCommand : ICommand
    {
        public int Index { get; set; }
        public int TextIndex { get; set; }
        public int VoiceIndex { get; set; }

        public TextCommand(int textIndex, int voiceIndex = 0)
        {
            TextIndex = textIndex;
            VoiceIndex = voiceIndex;
        }

        public TextCommand(CommandToken token)
        {
            this.Index = token.Index;
            this.TextIndex = token.Params[4] - 1;
            this.VoiceIndex = token.Params[1];
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }

    public class ImageCommitCommand : ICommand
    {
        public int Index { get; set; }
        public TransitionType Transition { get; set; }
        public int StepCount { get; set; }
        public int StepDuration { get; set; }

        public ImageCommitCommand(TransitionType transition, int transitionStepCount, int transitionStepDuration)
        {
            Transition = transition;
            StepCount = transitionStepCount;
            StepDuration = transitionStepDuration;
        }

        public ImageCommitCommand(CommandToken token)
        {
            Index = token.Index;
            Transition = (TransitionType)token.Params[0];
            StepCount = token.Params[1];
            StepDuration = token.Params[2];
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }

    public class BackgroundCommand : ICommand
    {
        public int Index { get; set; }
        public int ImageIndex { get; set; }

        public BackgroundCommand(int imageIndex)
        {
            ImageIndex = imageIndex;
        }
        public BackgroundCommand(CommandToken token)
        {
            Index = token.Index;
            ImageIndex = token.Params[0];
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }

    public class SpriteCommand : ICommand
    {
        public int Index { get; set; }
        public SpriteAction Action { get; set; }
        public int Position { get; set; }
        public int SpriteIndex { get; set; }

        private SpriteCommand() { }

        public SpriteCommand(CommandToken token)
        {
            Index = token.Index;
            Action = (SpriteAction)token.Params[0];
            Position = token.Params[1];
            SpriteIndex = token.Params[2];
        }

        public static SpriteCommand AtPosition(int position, int spriteIndex)
        {
            return new SpriteCommand()
            {
                Action = SpriteAction.Set,
                Position = position,
                SpriteIndex = spriteIndex
            };
        }

        public static SpriteCommand Clear()
        {
            return new SpriteCommand()
            {
                Action = SpriteAction.Clear
            };
        }

        public static SpriteCommand Activate(int position)
        {
            return new SpriteCommand()
            {
                Action = SpriteAction.Activate,
                Position = position
            };
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }

    public class ImageContextCommand : ICommand
    {
        public int Index { get; set; }
        public ImageContextCommand() { }
        public ImageContextCommand(CommandToken token)
        {
            Index = token.Index;
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }
}
