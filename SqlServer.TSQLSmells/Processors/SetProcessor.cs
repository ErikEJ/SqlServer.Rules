using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class SetProcessor
    {
        private readonly Smells smells;

        public SetProcessor(Smells smells)
        {
            this.smells = smells;
        }

        private void ProcessGeneralSetCommand(GeneralSetCommand SetCommand)
        {
            switch (SetCommand.CommandType)
            {
                case GeneralSetCommandType.DateFirst:
                    smells.SendFeedBack(9, SetCommand);
                    break;
                case GeneralSetCommandType.DateFormat:
                    smells.SendFeedBack(8, SetCommand);
                    break;
            }
        }

        public void ProcessSetStatement(SetCommandStatement Fragment)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            foreach (GeneralSetCommand SetCommand in Fragment.Commands)
            {
                ProcessGeneralSetCommand(SetCommand);
            }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
        }
    }
}