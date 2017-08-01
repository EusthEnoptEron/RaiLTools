using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaiLTools.RScript
{
    /// <summary>
    /// Tries to convert the instructions of a GSC scenario file into sensible instruction objects.
    /// </summary>
    public class CommandTranslator
    {
        enum Commands
        {
            StartImage = 0x1a,
            CloseImageAndBlend = 0x1c, // (type: Type, steps, stepLength) Type(None, Smooth, Pattern, Flash, Dark)
            Event = 0x14, // (index, ?)
            Text = 0x51, // (?, voiceIndex, ?, ?, textIndex, ?)
            Sprite = 0xff, // (mode, position, imageNo, ?, ?), mode = 0 -> SET(position, image), mode = 1 -> CLEAR(position), mode = 2 -> ACTIVATE(position), position: [1, 6] (left -> right)
            Jump = 0x05, // (offset [from command block start])
            JumpUnless = 0x03,
            Add = 0xAA00,
            Assign = 0xF100,
            GreaterEquals = 0x5400,
            And = 0x3500
        }

        private CommandTokenizer _Tokenizer;
        private GscFile _File;
        public CommandTranslator(GscFile file)
        {
            _File = file;
            _Tokenizer = new CommandTokenizer(file.CommandSection);
        }

        public IEnumerable<ICommand> GetCommands()
        {
            bool imageContext = false;

            foreach (var token in _Tokenizer.Enumerate())
            {
                switch ((Commands)token.Value)
                {
                    case Commands.StartImage:
                        imageContext = true;
                        yield return new ImageContextCommand(token);
                        break;
                    case Commands.CloseImageAndBlend:
                        imageContext = false;
                        yield return new ImageCommitCommand(token);
                        break;
                    case Commands.Event:
                        if (imageContext)
                        {
                            yield return new BackgroundCommand(token);
                        }
                        break;
                    case Commands.Sprite:
                        if (imageContext)
                        {
                            yield return new SpriteCommand(token);
                        }
                        break;
                    case Commands.Text:
                        yield return new TextCommand(token);
                        break;
                    case Commands.Jump:
                        yield return new JumpCommand(token);
                        break;
                    case Commands.Add:
                        yield return new AddCommand(token);
                        break;
                    case Commands.And:
                        yield return new AndCommand(token);
                        break;
                    case Commands.Assign:
                        yield return new AssignCommand(token);
                        break;
                    case Commands.GreaterEquals:
                        yield return new GreaterEqualsCommand(token);
                        break;
                    case Commands.JumpUnless:
                        yield return new JumpUnlessCommand(token);
                        break;
                    default:
                        yield return new UnknownCommand(token);
                        break;
                }
            }
        }

        public string GetDebugString()
        {
            return string.Join("\n", _Tokenizer.Enumerate().Select(token =>
                string.Format("[{0}] {1}: {2}", token.Index, token.Value.ToString("x4"), TokenParamsToString(token))
            ));
        }

        public string GetTranslatedDebugString()
        {
            return string.Join("\n", GetCommands().Select(command =>
                command.ToString()
            ));
        }

        private string TokenParamsToString(CommandToken token)
        {
            string concatenation = string.Join(" ", token.Params.Select(p => "0x" + p.ToString("x")))
                        + " (" + string.Join(", ", token.Params.Select(p => p.ToString())) + ")";

            if (token.Value == (int)Commands.Text)
            {
                concatenation += "(" + _File.Strings[token.Params[4] - 1] + ")";
            }

            return concatenation;
        }
    }
}
