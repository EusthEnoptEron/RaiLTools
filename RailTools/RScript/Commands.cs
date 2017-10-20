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

        public override string ToString()
        {
            return string.Format("??? ({0})", Token.Value.ToString("x2"));
        }
    }

    public class JumpCommand : ICommand
    {
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets at which offset the program continues.
        /// </summary>
        public int Offset { get; set; }
        public JumpCommand(CommandToken token)
        {
            Index = token.Index;
            Offset = token.Params[0];
        }
            
        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("GOTO #{0};", Offset);
        }
    }

    public class AndCommand : ICommand
    {
        public int Index { get; set; }
        public int ResultVariable { get; set; }
        public int LhsVariable { get; set; }
        public int RhsVariable { get; set; }

        public AndCommand(CommandToken token)
        {
            Index = token.Index;
            ResultVariable = token.Params[0];
            LhsVariable = token.Params[1];
            RhsVariable = token.Params[2];
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("V[{0}] = V[{1}] && V[{2}];", ResultVariable, LhsVariable, RhsVariable);
        }
    }

    public class JumpUnlessCommand : ICommand
    {
        public int Index { get; set; }
        public int Offset { get; set; }
        public int ConditionVariable { get; set; }

        public JumpUnlessCommand(CommandToken token)
        {
            Index = token.Index;
            ConditionVariable = 0;
            Offset = token.Params[0];
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("if(! V[{0}]) {{\n  GOTO #{1}\n}}", ConditionVariable, Offset);
        }
    }


    public class GreaterEqualsCommand : ICommand
    {
        public int Index { get; set; }
        public int ResultVariable { get; set; }
        public int LhsVariable { get; set; }
        public int RhsVariable { get; set; }

        public GreaterEqualsCommand(CommandToken token)
        {
            Index = token.Index;
            ResultVariable = token.Params[0];
            LhsVariable = token.Params[1];
            RhsVariable = token.Params[2];
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("V[{0}] = V[{1}] >= V[{2}];", ResultVariable, LhsVariable, RhsVariable);
        }
    }


    public class AddCommand : ICommand
    {
        public int Index { get; set; }
        public int ResultVariable { get; set; }
        public int LhsVariable { get; set; }
        public int RhsVariable { get; set; }

        public AddCommand(CommandToken token)
        {
            Index = token.Index;
            ResultVariable = token.Params[0];
            LhsVariable = token.Params[1];
            RhsVariable = token.Params[2];
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("V[{0}] = V[{1}] + V[{2}];", ResultVariable, LhsVariable, RhsVariable);
        }
    }

    public class AssignCommand : ICommand
    {
        public int Index { get; set; }
        public int ResultVariable { get; set; }
        public int Variable { get; set; }

        public AssignCommand(CommandToken token)
        {
            Index = token.Index;
            ResultVariable = token.Params[0];
            Variable = token.Params[1];
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("V[{0}] = V[{1}];", ResultVariable, Variable   );
        }
    }

    public class TextCommand : ICommand
    {
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the index of the text to play. <see cref="GscFile.Strings"/>
        /// </summary>
        public int TextIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the voice file to play.
        /// </summary>
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

        public override string ToString()
        {
            return string.Format("Text({0}, {1})", TextIndex, VoiceIndex);
        }
    }

    public class ImageCommitCommand : ICommand
    {
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the type of transition. Always has to preceded by a <see cref="ImageContextCommand"/>.
        /// </summary>
        public TransitionType Transition { get; set; }

        /// <summary>
        /// Number of steps the transition should take.
        /// </summary>
        public int StepCount { get; set; }

        /// <summary>
        /// Duration of each step in the transition. (ms?)
        /// </summary>
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

        public override string ToString()
        {
            return string.Format("}} [{0}, {1}, {2}]", Transition, StepCount, StepDuration);
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

        public override string ToString()
        {
            return string.Format("  BACKGROUND({0});", ImageIndex);
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

        public override string ToString()
        {
            return string.Format("  {0} SPRITE {1} {2}", Enum.GetName(typeof(SpriteAction), Action).ToUpper(), Position, SpriteIndex);
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

        public override string ToString()
        {
            return string.Format("IMAGE {{");
        }
    }
}
