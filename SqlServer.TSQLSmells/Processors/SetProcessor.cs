using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class SetProcessor
    {
        private readonly Smells _smells;

        public SetProcessor(Smells smells)
        {
            _smells = smells;
        }

        private void ProcessGeneralSetCommand(GeneralSetCommand SetCommand)
        {
            switch (SetCommand.CommandType)
            {
                case GeneralSetCommandType.DateFirst:
                    _smells.SendFeedBack(9, SetCommand);
                    break;
                case GeneralSetCommandType.DateFormat:
                    _smells.SendFeedBack(8, SetCommand);
                    break;
            }
        }

        public void ProcessSetStatement(SetCommandStatement Fragment)
        {
            foreach (GeneralSetCommand SetCommand in Fragment.Commands)
            {
                ProcessGeneralSetCommand(SetCommand);
            }
        }
    }
}